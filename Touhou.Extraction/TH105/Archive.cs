using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH105;

/// <summary>
/// Represents an archive with the Touhou 10.5 / 12.3 format.
/// </summary>
/// <remarks>An instance of <see cref="CodePagesEncodingProvider"/> must be registered with <see cref="Encoding.RegisterProvider"/> before using this class.</remarks>
public sealed class Archive : Extraction.Archive
{
	/// <inheritdoc/>
	public override IEnumerable<Entry> Entries
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);

			return _entries!;
		}
	}

	private int _offset;
	private Stream? _stream;
	private bool _isDisposed;
	private List<Entry>? _entries;
	private readonly ArchiveInitializationMode _initializationMode;

	private static readonly Decoder s_sjisDecoder = Encoding.GetEncoding(932).GetDecoder();
	private static readonly Encoder s_sjisEncoder = Encoding.GetEncoding(932).GetEncoder();

	private const int HEADER_SIZE = sizeof(uint) + sizeof(ushort);

	private Archive(int offset, List<Entry> entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_offset = offset;
		_stream = stream;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares an <see cref="Archive"/> for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <param name="entriesBasePath">The path to the base directory where the entries are stored in disk.</param>
	/// <returns>An instance of the <see cref="Archive"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	public static Archive Create(int entryCount, Stream outputStream, string[] entryFileNames, string entriesBasePath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(entryCount, ushort.MaxValue);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		int offset = HEADER_SIZE;
		List<Entry> entries = new(entryCount);

		Archive archive = new(offset, entries, outputStream, ArchiveInitializationMode.Creation);

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
		{
			string fileName = Path.GetRelativePath(entriesBasePath, entryFileNames[entryIndex]).Replace('\\', '/');

			Entry entry = new(-1, -1, archive.GetValidatedEntryName(fileName));

			offset += (sizeof(uint) * 2) + 1 + s_sjisEncoder.GetByteCount(fileName, flush: true);

			entries.Add(entry);
		}

		outputStream.Seek(offset, SeekOrigin.Begin);

		archive._offset = offset;

		return archive;
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="stream"/> and prepares for extraction.
	/// </summary>
	/// <param name="stream">The stream that contains the archive's data.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only these extensions will be included.</param>
	/// <returns>An instance of the <see cref="Archive"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="stream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	public static Archive Read(Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		Span<byte> data = stackalloc byte[6];

		stream.ReadExactly(data);

		ref readonly Header header = ref MemoryMarshal.AsRef<Header>(data);

		if (header.EntryCount == 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)header.EntryHeadersSize);
		data = buffer.AsSpan(0, (int)header.EntryHeadersSize);

		stream.ReadExactly(data);

		List<Entry> entries = ReadEntryHeaders(data, in header, extensionFilters);
		Archive archive = new(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	/// <inheritdoc cref="Read(Stream, ArchiveReadOptions, string[])"/>
	public static async Task<Archive> ReadAsync(Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(6);
		Memory<byte> data = buffer.AsMemory(0, 6);

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		Header header = MemoryMarshal.Read<Header>(data.Span);

		if (header.EntryCount == 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		ArrayPool<byte>.Shared.Return(buffer);

		buffer = ArrayPool<byte>.Shared.Rent((int)header.EntryHeadersSize);
		data = buffer.AsMemory(0, (int)header.EntryHeadersSize);

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		List<Entry> entries = ReadEntryHeaders(data.Span, in header, extensionFilters);
		Archive archive = new(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	private static List<Entry> ReadEntryHeaders(Span<byte> entryHeadersData, in Header header, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		Crypto.CryptEntryHeaders(entryHeadersData, header.EntryHeadersSize, 0xc5, 0x83, 0x53);

		List<Entry> entries = new(header.EntryCount);

		char[] buffer = ArrayPool<char>.Shared.Rent(100);
		Span<char> fileNameBuffer = buffer;

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < header.EntryCount; entryIndex++)
		{
			// Data offset in the archive
			int offset = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			// Uncompressed size
			int size = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			// File name
			byte fileNameLength = entryHeadersData[entryHeadersDataPtr++];

			if (fileNameLength > fileNameBuffer.Length)
			{
				ArrayPool<char>.Shared.Return(buffer);

				buffer = ArrayPool<char>.Shared.Rent(fileNameLength);
				fileNameBuffer = buffer;
			}

			s_sjisDecoder.Convert(entryHeadersData.Slice(entryHeadersDataPtr, fileNameLength), fileNameBuffer, true, out _, out int charsUsed, out _);
			entryHeadersDataPtr += fileNameLength;

			string fileName = fileNameBuffer[..charsUsed].ToString();

			if (offset < 0)
			{
				throw new InvalidDataException($"The entry \"{fileName}\" has an invalid offset ({offset}).");
			}

			if (size < 0)
			{
				throw new InvalidDataException($"The entry \"{fileName}\" has an invalid size ({size}).");
			}

			string extension = Path.GetExtension(fileName);

			Entry entry = new(size, offset, fileName);

			if (extensionFilters?.Contains(extension) is false)
			{
				continue;
			}

			entries.Add(entry);
		}

		entries.TrimExcess();

		ArrayPool<char>.Shared.Return(buffer);

		return entries;
	}

	/// <inheritdoc/>
	public override Span<byte> Extract(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		return entry.Size == 0 ? [] : ExtractCore(entry);
	}

	private Span<byte> ExtractCore(Entry entry)
	{
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		Span<byte> data = new byte[entry.Size];

		_stream.ReadExactly(data);

		Crypto.CryptEntry(data, (uint)entry.Offset);

		return data;
	}

	/// <inheritdoc/>
	public override async Task<Memory<byte>> ExtractAsync(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		return entry.Size == 0 ? Memory<byte>.Empty : await ExtractAsyncCore(entry).ConfigureAwait(false);
	}

	private async Task<Memory<byte>> ExtractAsyncCore(Entry entry)
	{
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		Memory<byte> data = new byte[entry.Size];

		await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

		Crypto.CryptEntry(data.Span, (uint)entry.Offset);

		return data;
	}

	/// <inheritdoc/>
	public override void Extract(Entry entry, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (entry.Size == 0)
		{
			return;
		}

		outputStream.Write(ExtractCore(entry));
	}

	/// <inheritdoc/>
	public override async ValueTask ExtractAsync(Entry entry, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (entry.Size == 0)
		{
			return;
		}

		await outputStream.WriteAsync(await ExtractAsyncCore(entry).ConfigureAwait(false)).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public override void Pack(Entry entry, byte[] entryData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(entryData);

		entry.Size = entryData.Length;

		if (entry.Size == 0)
		{
			throw new ArgumentException($"The entry data is empty.", nameof(entryData));
		}

		using MemoryStream stream = new(entryData, writable: false);
		PackCore(entry, stream);
	}

	/// <inheritdoc/>
	public override async ValueTask PackAsync(Entry entry, byte[] entryData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(entryData);

		entry.Size = entryData.Length;

		if (entry.Size == 0)
		{
			throw new ArgumentException($"The entry data is empty.", nameof(entryData));
		}

		MemoryStream stream = new(entryData, writable: false);
		await using (stream.ConfigureAwait(false))
		{
			await PackAsyncCore(entry, stream).ConfigureAwait(false);
		}
	}

	/// <inheritdoc/>
	public override void Pack(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		entry.Size = (int)inputStream.Length;

		if (entry.Size == int.MinValue)
		{
			throw new ArgumentException($"The stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (entry.Size == 0)
		{
			throw new ArgumentException($"The stream is empty.", nameof(inputStream));
		}

		PackCore(entry, inputStream);
	}

	private void PackCore(Entry entry, Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
		Span<byte> data = buffer.AsSpan(0, entry.Size);

		inputStream.ReadExactly(data);

		entry.Offset = _offset;
		_offset += entry.Size;

		Crypto.CryptEntry(data, (uint)entry.Offset);

		_stream!.Write(data);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public override async ValueTask PackAsync(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		entry.Size = (int)inputStream.Length;

		if (entry.Size == int.MinValue)
		{
			throw new ArgumentException($"The stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (entry.Size == 0)
		{
			throw new ArgumentException($"The stream is empty.", nameof(inputStream));
		}

		await PackAsyncCore(entry, inputStream).ConfigureAwait(false);
	}

	private async ValueTask PackAsyncCore(Entry entry, Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
		Memory<byte> data = buffer.AsMemory(0, entry.Size);

		await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

		entry.Offset = _offset;
		_offset += entry.Size;

		Crypto.CryptEntry(data.Span, (uint)entry.Offset);

		await _stream!.WriteAsync(data).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public override void Close()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		if (_entries!.Count == 0)
		{
			throw new InvalidOperationException("There are no entries in the archive.");
		}

		_entries.Sort(static (x, y) => x.Offset - y.Offset);

		int entryHeadersSize = sizeof(uint) * 2 * _entries.Count;
		Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + s_sjisEncoder.GetByteCount(entry.FileName, flush: true) + 1, sum => Interlocked.Add(ref entryHeadersSize, sum));

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize + HEADER_SIZE);
		Span<byte> entryHeadersData = buffer.AsSpan(HEADER_SIZE, entryHeadersSize);

		WriteEntryHeaders(entryHeadersData);

		Crypto.CryptEntryHeaders(entryHeadersData, (uint)entryHeadersSize, 0xc5, 0x83, 0x53);

		MemoryMarshal.Write(buffer.AsSpan(0, sizeof(ushort)), (ushort)_entries.Count);
		MemoryMarshal.Write(buffer.AsSpan(sizeof(ushort), sizeof(uint)), (uint)entryHeadersSize);

		_stream!.Seek(0, SeekOrigin.Begin);

		_stream.Write(buffer.AsSpan(0, entryHeadersSize + HEADER_SIZE));

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public override async ValueTask CloseAsync()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		if (_entries!.Count == 0)
		{
			throw new InvalidOperationException("There are no entries in the archive.");
		}

		_entries.Sort(static (x, y) => x.Offset - y.Offset);

		int entryHeadersSize = sizeof(uint) * 2 * _entries.Count;
		Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + s_sjisEncoder.GetByteCount(entry.FileName, flush: true) + 1, sum => Interlocked.Add(ref entryHeadersSize, sum));

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize + HEADER_SIZE);

		WriteEntryHeaders(buffer.AsSpan(HEADER_SIZE, entryHeadersSize));

		Crypto.CryptEntryHeaders(buffer.AsSpan(HEADER_SIZE, entryHeadersSize), (uint)entryHeadersSize, 0xc5, 0x83, 0x53);

		MemoryMarshal.Write(buffer.AsSpan(0, sizeof(ushort)), (ushort)_entries.Count);
		MemoryMarshal.Write(buffer.AsSpan(sizeof(ushort), sizeof(uint)), (uint)entryHeadersSize);

		_stream!.Seek(0, SeekOrigin.Begin);

		await _stream.WriteAsync(buffer.AsMemory(0, entryHeadersSize + HEADER_SIZE)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private void WriteEntryHeaders(Span<byte> entryHeadersData)
	{
		ReadOnlySpan<Entry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entries.Length; entryIndex++)
		{
			Entry entry = entries[entryIndex];

			// Data offset in the archive
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), entry.Offset);
			entryHeadersDataPtr += sizeof(uint);

			// Uncompressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Size);
			entryHeadersDataPtr += sizeof(uint);

			// File name
			s_sjisEncoder.Convert(entry.FileName, entryHeadersData[(entryHeadersDataPtr + 1)..], flush: true, out _, out int bytesUsed, out _);
			entryHeadersData[entryHeadersDataPtr] = (byte)bytesUsed;

			entryHeadersDataPtr += bytesUsed + 1;
		}
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (_isDisposed)
		{
			return;
		}

		if (disposing)
		{
			_stream!.Dispose();

			_stream = null;
			_entries = null;
		}

		_isDisposed = true;
	}

	/// <inheritdoc/>
	protected override async ValueTask DisposeAsyncCore()
	{
		if (_isDisposed)
		{
			return;
		}

		await _stream!.DisposeAsync().ConfigureAwait(false);

		_stream = null;
		_entries = null;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private readonly struct Header
	{
		/// <summary>
		/// Gets the number of entries in the archive.
		/// </summary>
		internal readonly ushort EntryCount { get; init; }
		/// <summary>
		/// Gets the size of the entry headers.
		/// </summary>
		internal readonly uint EntryHeadersSize { get; init; }
	}
}
