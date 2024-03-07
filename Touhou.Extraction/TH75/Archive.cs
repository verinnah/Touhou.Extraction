using System.Buffers;
using System.Runtime.InteropServices;
using System.Text.Unicode;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH75;

/// <summary>
/// Represents an archive with the Touhou 7.5 format. This class cannot be inherited.
/// </summary>
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

	private const int ENTRY_SIZE = 0x6C;

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
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="entryCount"/> is less than or equal to zero.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	public static Archive Create(int entryCount, Stream outputStream, string[] entryFileNames, string entriesBasePath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentException.ThrowIfNullOrWhiteSpace(entriesBasePath);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		int offset = 2 + (ENTRY_SIZE * entryCount);
		outputStream.Seek(offset, SeekOrigin.Begin);

		List<Entry> entries = new(entryCount);

		Archive archive = new(offset, entries, outputStream, ArchiveInitializationMode.Creation);

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
		{
			entries.Add(new Entry(-1, -1, archive.GetValidatedEntryName(Path.GetRelativePath(entriesBasePath, entryFileNames[entryIndex])).Replace('/', '\\')));
		}

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

		Span<byte> data = stackalloc byte[sizeof(ushort)];

		stream.ReadExactly(data);

		ushort entryCount = SpanHelpers.ReadUInt16(data);

		if (entryCount == 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		int entryHeadersSize = ENTRY_SIZE * entryCount;

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
		data = buffer.AsSpan(0, entryHeadersSize);

		stream.ReadExactly(data);

		Crypto.CryptEntryHeaders(data);

		Archive archive = new(offset: 0, new List<Entry>(entryCount), stream, ArchiveInitializationMode.Extraction);
		archive.ReadEntryHeaders(data, extensionFilters);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	/// <inheritdoc cref="Read(Stream, ArchiveReadOptions, string[])"/>
	public static async Task<Archive> ReadAsync(Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ushort));
		Memory<byte> data = buffer.AsMemory(0, sizeof(ushort));

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		ushort entryCount = MemoryHelpers.ReadUInt16(data);

		if (entryCount == 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		int entryHeadersSize = ENTRY_SIZE * entryCount;

		ArrayPool<byte>.Shared.Return(buffer);

		buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
		data = buffer.AsMemory(0, entryHeadersSize);

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		Crypto.CryptEntryHeaders(data.Span);

		Archive archive = new(offset: 0, new List<Entry>(entryCount), stream, ArchiveInitializationMode.Extraction);
		archive.ReadEntryHeaders(data.Span, extensionFilters);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	private void ReadEntryHeaders(ReadOnlySpan<byte> entryHeadersData, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < _entries!.Capacity; entryIndex++)
		{
			// File name
			string filePath = SpanHelpers.ReadString(entryHeadersData.Slice(entryHeadersDataPtr, 100));
			entryHeadersDataPtr += 100;

			// Uncompressed size
			int size = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			if (size < 0)
			{
				throw new InvalidDataException($"The entry \"{filePath}\" has an invalid size ({size}).");
			}

			// Data offset in the archive
			int offset = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			if (offset < 0)
			{
				throw new InvalidDataException($"The entry \"{filePath}\" has an invalid offset ({offset}).");
			}

			Entry entry = new(size, offset, filePath);

			string extension = Path.GetExtension(filePath);

			if (extensionFilters?.Contains(extension) is false)
			{
				continue;
			}

			_entries.Add(entry);
		}

		_entries.TrimExcess();
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
		Span<byte> data;

		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		data = new byte[entry.Size];

		_stream.ReadExactly(data);

		return data;
	}

	/// <inheritdoc/>
	public override async Task<Memory<byte>> ExtractAsync(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull((object?)entry);

		return entry.Size == 0 ? Memory<byte>.Empty : await ExtractAsyncCore(entry).ConfigureAwait(false);
	}

	private async Task<Memory<byte>> ExtractAsyncCore(Entry entry)
	{
		Memory<byte> data;

		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		data = new byte[entry.Size];

		await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

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

		_entries.Sort(static (x, y) => (int)(x.Offset - y.Offset));

		int entryHeadersSize = sizeof(ushort) + (_entries.Count * ENTRY_SIZE);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
		Span<byte> entryHeadersData = buffer.AsSpan(sizeof(ushort), entryHeadersSize - sizeof(ushort));

		WriteEntryHeaders(entryHeadersData);

		Crypto.CryptEntryHeaders(entryHeadersData);

		MemoryMarshal.Write(buffer.AsSpan(0, sizeof(ushort)), (ushort)_entries.Count);

		_stream!.Seek(0, SeekOrigin.Begin);

		_stream.Write(buffer.AsSpan(0, entryHeadersSize));

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

		_entries.Sort(static (x, y) => (int)(x.Offset - y.Offset));

		int entryHeadersSize = sizeof(ushort) + (_entries.Count * ENTRY_SIZE);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);

		WriteEntryHeaders(buffer.AsSpan(sizeof(ushort), entryHeadersSize - sizeof(ushort)));

		Crypto.CryptEntryHeaders(buffer.AsSpan(sizeof(ushort), entryHeadersSize - sizeof(ushort)));

		MemoryMarshal.Write(buffer.AsSpan(0, sizeof(ushort)), (ushort)_entries.Count);

		_stream!.Seek(0, SeekOrigin.Begin);

		await _stream.WriteAsync(buffer.AsMemory(0, entryHeadersSize)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private void WriteEntryHeaders(Span<byte> entryHeadersData)
	{
		ReadOnlySpan<Entry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entries.Length; entryIndex++)
		{
			Entry entry = entries[entryIndex];

			// File name
			if (Utf8.FromUtf16(entry.FileName, entryHeadersData.Slice(entryHeadersDataPtr, entry.FileName.Length), out _, out _) != OperationStatus.Done)
			{
				throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
			}

			entryHeadersDataPtr += entry.FileName.Length;

			int fileNamePaddingLength = 100 - entry.FileName.Length;

			if (fileNamePaddingLength > 0)
			{
				entryHeadersData.Slice(entryHeadersDataPtr, fileNamePaddingLength).Clear();

				entryHeadersDataPtr += fileNamePaddingLength;
			}

			// Uncompressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Size);
			entryHeadersDataPtr += sizeof(uint);

			// Data offset in the archive
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), entry.Offset);
			entryHeadersDataPtr += sizeof(uint);
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
}
