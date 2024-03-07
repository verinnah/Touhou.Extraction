using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Touhou.Extraction.Helpers;
using Touhou.Extraction.Properties;

namespace Touhou.Extraction.TH175;

/// <summary>
/// Represents an archive with the Touhou 17.5 format. This class cannot be inherited.
/// </summary>
public sealed class CGX : Archive
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

	private const byte FOOTER_SIZE = 0x20;
	private const byte ENTRY_HEADER_SIZE = 0x18;

	private CGX(int offset, List<Entry> entries, Stream stream, ArchiveInitializationMode initializationMode)
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
	public static CGX Create(int entryCount, Stream outputStream, string[] entryFileNames, string entriesBasePath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentException.ThrowIfNullOrWhiteSpace(entriesBasePath);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		outputStream.Seek(0, SeekOrigin.Begin);

		List<Entry> entries = new(entryCount);

		CGX archive = new(offset: 0, entries, outputStream, ArchiveInitializationMode.Creation);

		Entry? payloaderEntry = null;
		int payloaderEntryIndex = -1;

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
		{
			string fileName = archive.GetValidatedEntryName(Path.GetRelativePath(entriesBasePath, entryFileNames[entryIndex])).Replace('\\', '/');

			Entry entry = new(-1, -1, fileName);

			entries.Add(entry);

			if (fileName == "payloader.exe")
			{
				payloaderEntry = entry;
				payloaderEntryIndex = entryIndex;
			}
		}

		if (payloaderEntryIndex != -1)
		{
			entries.RemoveAt(payloaderEntryIndex);
			entries.Insert(0, payloaderEntry!);
		}

		return archive;
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="stream"/> and prepares for extraction.
	/// </summary>
	/// <param name="stream">The stream that contains the archive's data.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only these extensions will be included.</param>
	/// <returns>An instance of the <see cref="CGX"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="stream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	public static CGX Read(Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		uint footerOffset = (uint)stream.Length - FOOTER_SIZE;

		stream.Seek(footerOffset, SeekOrigin.Begin);

		Span<byte> data = stackalloc byte[FOOTER_SIZE];

		stream.ReadExactly(data);

		Crypto.Crypt(data, footerOffset);

		ref readonly Footer footer = ref MemoryMarshal.AsRef<Footer>(data);

		int entryCount = (int)footer.EntryCount;

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		if (footer.FooterSize != FOOTER_SIZE || footer.EntryHeaderSize != ENTRY_HEADER_SIZE)
		{
			throw new InvalidDataException($"The archive footer contains invalid data.");
		}

		uint entryHeadersSize = ENTRY_HEADER_SIZE * footer.EntryCount;
		uint entryHeadersOffset = footerOffset - entryHeadersSize;

		stream.Seek(entryHeadersOffset, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)entryHeadersSize);
		data = buffer.AsSpan(0, (int)entryHeadersSize);

		stream.ReadExactly(data);

		List<Entry> entries = ReadEntryHeaders(data, entryCount, entryHeadersOffset, options, extensionFilters);
		CGX archive = new(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	/// <inheritdoc cref="Read(Stream, ArchiveReadOptions, string[])"/>
	public static async Task<CGX> ReadAsync(Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		uint footerOffset = (uint)stream.Length - FOOTER_SIZE;

		stream.Seek(footerOffset, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(FOOTER_SIZE);
		Memory<byte> data = buffer.AsMemory(0, FOOTER_SIZE);

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		Crypto.Crypt(data.Span, footerOffset);

		Footer footer = MemoryMarshal.Read<Footer>(data.Span);

		int entryCount = (int)footer.EntryCount;

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		if (footer.FooterSize != FOOTER_SIZE || footer.EntryHeaderSize != ENTRY_HEADER_SIZE)
		{
			throw new InvalidDataException($"The archive footer contains invalid data.");
		}

		uint entryHeadersSize = ENTRY_HEADER_SIZE * footer.EntryCount;
		uint entryHeadersOffset = footerOffset - entryHeadersSize;

		stream.Seek(entryHeadersOffset, SeekOrigin.Begin);

		ArrayPool<byte>.Shared.Return(buffer);

		buffer = ArrayPool<byte>.Shared.Rent((int)entryHeadersSize);
		data = buffer.AsMemory(0, (int)entryHeadersSize);

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		List<Entry> entries = ReadEntryHeaders(data.Span, entryCount, entryHeadersOffset, options, extensionFilters);
		CGX archive = new(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	private static List<Entry> ReadEntryHeaders(Span<byte> entryHeadersData, int entryCount, uint offset, ArchiveReadOptions options, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		Crypto.Crypt(entryHeadersData, offset);

		string[] fileNamesJson = JsonConvert.DeserializeObject<string[]>(Resources.th175)!;
		Dictionary<uint, string> fileNames = new(fileNamesJson.Length);

		foreach (string fileName in fileNamesJson)
		{
			fileNames[Crypto.GetFileNameHash(fileName)] = fileName;
		}

		List<Entry> entries = new(entryCount);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entryCount; entryIndex++)
		{
			// File name
			uint fileNameHash = (uint)SpanHelpers.ReadInt64(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(long);

			bool shouldSkipEntry = false;
			bool isUnknownFileName = !fileNames.TryGetValue(fileNameHash, out string? fileName);

			if (isUnknownFileName)
			{
				if (options.HasFlag(ArchiveReadOptions.ExcludeUnknownEntries))
				{
					shouldSkipEntry = true;
				}
				else
				{
					fileName = $"unk/{fileNameHash:x}";
				}
			}
			else if (extensionFilters?.Contains(Path.GetExtension(fileName)) is false)
			{
				shouldSkipEntry = true;
			}

			if (shouldSkipEntry)
			{
				entryHeadersDataPtr += sizeof(long) * 2;

				continue;
			}

			// Data offset in the archive
			int entryOffset = (int)SpanHelpers.ReadInt64(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(long);

			if (entryOffset < 0)
			{
				throw new InvalidDataException($"The entry \"{fileName}\" has an invalid offset ({entryOffset}).");
			}

			// Uncompressed size
			int entrySize = (int)SpanHelpers.ReadInt64(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(long);

			if (entrySize < 0)
			{
				throw new InvalidDataException($"The entry \"{fileName}\" has an invalid size ({entrySize}).");
			}

			entries.Add(new Entry(entrySize, entryOffset, fileName!));
		}

		entries.TrimExcess();

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

		if (entry.FileName != "payloader.exe")
		{
			Crypto.Crypt(data, (uint)entry.Offset);
		}

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

		if (entry.FileName != "payloader.exe")
		{
			Crypto.Crypt(data.Span, (uint)entry.Offset);
		}

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

		if (entry.FileName != "payloader.exe")
		{
			Crypto.Crypt(data, (uint)entry.Offset);
		}
		else if (entry.Offset != 0)
		{
			throw new InvalidOperationException("payloader.exe must be the first entry to be written in the archive.");
		}

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

		if (entry.FileName != "payloader.exe")
		{
			Crypto.Crypt(data.Span, (uint)entry.Offset);
		}
		else if (entry.Offset != 0)
		{
			throw new InvalidOperationException("payloader.exe must be the first entry to be written in the archive.");
		}

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

		int entryHeadersSize = sizeof(ulong) * 3 * _entries.Count;

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize + FOOTER_SIZE);

		WriteEntryHeaders(buffer.AsSpan(0, entryHeadersSize));

		_offset += entryHeadersSize;

		Footer footer = new((uint)_entries.Count);
		MemoryMarshal.Write(buffer.AsSpan(entryHeadersSize, FOOTER_SIZE), in footer);

		Crypto.Crypt(buffer.AsSpan(entryHeadersSize, FOOTER_SIZE), (uint)_offset);

		_stream!.Write(buffer.AsSpan(0, entryHeadersSize + FOOTER_SIZE));

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

		int entryHeadersSize = sizeof(ulong) * 3 * _entries.Count;

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize + FOOTER_SIZE);

		WriteEntryHeaders(buffer.AsSpan(0, entryHeadersSize));

		_offset += entryHeadersSize;

		Footer footer = new((uint)_entries.Count);
		MemoryMarshal.Write(buffer.AsSpan(entryHeadersSize, FOOTER_SIZE), in footer);

		Crypto.Crypt(buffer.AsSpan(entryHeadersSize, FOOTER_SIZE), (uint)_offset);

		await _stream!.WriteAsync(buffer.AsMemory(0, entryHeadersSize + FOOTER_SIZE)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private void WriteEntryHeaders(Span<byte> entryHeadersData)
	{
		ReadOnlySpan<Entry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entries.Length; entryIndex++)
		{
			Entry entry = entries[entryIndex];

			// File name
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(ulong)), (ulong)Crypto.GetFileNameHash(entry.FileName));
			entryHeadersDataPtr += sizeof(ulong);

			// Data offset in the archive
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(ulong)), (ulong)entry.Offset);
			entryHeadersDataPtr += sizeof(ulong);

			// Uncompressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(ulong)), (ulong)entry.Size);
			entryHeadersDataPtr += sizeof(ulong);
		}

		Crypto.Crypt(entryHeadersData, (uint)_offset);
	}

	/// <summary>
	/// Tries to guess <paramref name="data"/>'s extension based on its contents.
	/// </summary>
	/// <param name="data">The data of the entry to guess.</param>
	/// <returns>The guessed extension of the entry, or <see langword="null"/> if an extension could not be determined.</returns>
	public static string? GuessExtension(ReadOnlySpan<byte> data)
	{
		if (data.IsEmpty)
		{
			return null;
		}

		// "\xFA\xFARIQS"
		ReadOnlySpan<byte> nutMagic = [0xFA, 0xFA, 82, 73, 81, 83];
		// "\x89PNG"
		ReadOnlySpan<byte> pngMagic = [0x89, 80, 78, 71];
		ReadOnlySpan<byte> txtMagic = [0xEF, 0xBB, 0xBF];

		if (data.SequenceEqual(nutMagic))
		{
			return ".nut";
		}
		else if (data.SequenceEqual(pngMagic))
		{
			return ".png";
		}
		else if (data.SequenceEqual("OggS"u8))
		{
			return ".ogg";
		}
		else if (data.SequenceEqual("RIFF"u8))
		{
			return ".wav";
		}
		else if (data.SequenceEqual("OTTO"u8))
		{
			return ".otf";
		}
		else if (data.SequenceEqual("GIF"u8))
		{
			return ".gif";
		}
		else if (data.SequenceEqual(txtMagic))
		{
			return ".txt";
		}

		return null;
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
	private readonly struct Footer
	{
		private readonly uint _unk1 = 0;
		private readonly uint _unk2 = 0;
		// Non-zero in original archives (4 bytes filled)
		private readonly uint _unk3 = 0;
		private readonly uint _unk4 = 0;
		private readonly uint _unk5 = 0;

		/// <summary>
		/// Gets the size of an entry header in the archive.
		/// </summary>
		/// <remarks>This must be equal to 24.</remarks>
		internal readonly uint EntryHeaderSize { get; } = 0x18;
		/// <summary>
		/// Gets the number of entries in the archive.
		/// </summary>
		internal readonly required uint EntryCount { get; init; }
		/// <summary>
		/// Gets the size of an archive's footer.
		/// </summary>
		/// <remarks>This must be equal to <see langword="sizeof"/>(<see cref="Footer"/>) (32).</remarks>
		internal readonly uint FooterSize { get; } = 0x20;

		[SetsRequiredMembers]
		internal Footer(uint entryCount) => EntryCount = entryCount;
	}
}
