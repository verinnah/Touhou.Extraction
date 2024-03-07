using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Touhou.Common;
using Touhou.Extraction.Compression;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH01;

/// <summary>
/// Represents an archive file containing data from Touhou 1-5. This class cannot be inherited.
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
	/// <inheritdoc/>
	private protected override ArchiveFileNamesOptions Flags { get; } = ArchiveFileNamesOptions.BaseName | ArchiveFileNamesOptions.Uppercase | ArchiveFileNamesOptions.ShortFilename;

	private uint _offset;
	private Stream? _stream;
	private bool _isDisposed;
	private readonly Game _game;
	private List<ZunEntry>? _entries;
	private readonly ArchiveInitializationMode _initializationMode;

	private static readonly byte[] s_keys = [/* TH01 */ 0x76, /* TH02 */ 0x12];

	private const byte ENTRY_KEY = 0x34;
	private const byte ARCHIVE_KEY = 0x12;

	private const ushort COMPRESSED_ENTRIES_MAGIC = 0x9595;
	private const ushort UNCOMPRESSED_ENTRIES_MAGIC = 0xf388;

	private const int TH03_ARCHIVE_HEADER_SIZE = (sizeof(ushort) * 3) + (sizeof(byte) * 10);
	private const int TH02_ENTRY_HEADER_SIZE = (sizeof(uint) * 4) + (sizeof(byte) * 14) + sizeof(ushort);
	private const int TH03_ENTRY_HEADER_SIZE = (sizeof(uint) * 3) + (sizeof(ushort) * 3) + (sizeof(byte) * 14);

	private Archive(Game game, uint offset, List<ZunEntry> entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_game = game;
		_offset = offset;
		_stream = stream;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares an <see cref="Archive"/> for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="game">The game this archive is from.</param>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <returns>An instance of the <see cref="Archive"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	public static Archive Create(Game game, int entryCount, Stream outputStream, string[] entryFileNames)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		if (game >= Game.PoDD)
		{
			ArgumentOutOfRangeException.ThrowIfGreaterThan(entryCount, ushort.MaxValue);
		}

		uint offset = (uint)(game <= Game.SoEW ? (entryCount + 1) * TH02_ENTRY_HEADER_SIZE : TH03_ARCHIVE_HEADER_SIZE + ((entryCount + 1) * TH03_ENTRY_HEADER_SIZE));
		outputStream.Seek(offset, SeekOrigin.Begin);

		List<ZunEntry> entries = new(entryCount);

		Archive archive = new(game, offset, entries, outputStream, ArchiveInitializationMode.Creation);

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
		{
			entries.Add(new ZunEntry(archive.GetValidatedEntryName(entryFileNames[entryIndex]), 0, -1, -1, -1));
		}

		return archive;
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="inputStream"/> and prepares for extraction.
	/// </summary>
	/// <param name="game">The game this archive is from.</param>
	/// <param name="inputStream">The stream that contains the archive's data.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only these extensions will be included.</param>
	/// <returns>An instance of the <see cref="Archive"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="inputStream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> is <see langword="null"/>.</exception>
	public static new Archive Read(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		List<ZunEntry> entries;

		if (game <= Game.SoEW)
		{
			inputStream.Seek(sizeof(ushort) + (sizeof(byte) * 14) + (sizeof(uint) * 2), SeekOrigin.Begin);

			Span<byte> data = stackalloc byte[sizeof(uint)];

			inputStream.ReadExactly(data);

			uint firstEntryOffset = SpanHelpers.ReadUInt32(data);

			if ((firstEntryOffset % TH02_ENTRY_HEADER_SIZE) != 0)
			{
				throw new InvalidDataException($"The offset of the first entry is invalid ({firstEntryOffset} must be aligned to {TH02_ENTRY_HEADER_SIZE}).");
			}

			int entryCount = (int)((firstEntryOffset / TH02_ENTRY_HEADER_SIZE) - 1);

			if (entryCount <= 0)
			{
				throw new InvalidDataException("There are no entries in the archive");
			}

			inputStream.Seek(0, SeekOrigin.Begin);

			int entryHeadersSize = entryCount * TH02_ENTRY_HEADER_SIZE;
			byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsSpan(0, entryHeadersSize);

			inputStream.ReadExactly(data);

			entries = ReadEntryHeadersTh01(game, entryCount, data, extensionFilters);

			ArrayPool<byte>.Shared.Return(buffer);
		}
		else
		{
			inputStream.Seek(sizeof(ushort) * 2, SeekOrigin.Begin);

			Span<byte> data = stackalloc byte[sizeof(ushort) + sizeof(byte)];

			inputStream.ReadExactly(data);

			int entryCount = SpanHelpers.ReadUInt16(data);

			if (entryCount <= 0)
			{
				throw new InvalidDataException("There are no entries in the archive");
			}

			inputStream.Seek(sizeof(byte) * 9, SeekOrigin.Current);

			byte key = data[sizeof(ushort)];

			int entryHeadersSize = entryCount * TH03_ENTRY_HEADER_SIZE;
			byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsSpan(0, entryHeadersSize);

			inputStream.ReadExactly(data);

			entries = ReadEntryHeadersTh03(key, entryCount, data, extensionFilters);

			ArrayPool<byte>.Shared.Return(buffer);
		}

		return new Archive(game, offset: 0, entries, inputStream, ArchiveInitializationMode.Extraction);
	}

	/// <inheritdoc cref="Read(Game, Stream, ArchiveReadOptions, string[])"/>
	public static new async Task<Archive> ReadAsync(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		List<ZunEntry> entries;

		if (game <= Game.SoEW)
		{
			inputStream.Seek(sizeof(ushort) + (sizeof(byte) * 14) + (sizeof(uint) * 2), SeekOrigin.Begin);

			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(uint));
			Memory<byte> data = buffer.AsMemory(0, sizeof(uint));

			await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

			uint firstEntryOffset = MemoryHelpers.ReadUInt32(data);

			ArrayPool<byte>.Shared.Return(buffer);

			if ((firstEntryOffset % TH02_ENTRY_HEADER_SIZE) != 0)
			{
				throw new InvalidDataException($"The offset of the first entry is invalid ({firstEntryOffset} must be aligned to {TH02_ENTRY_HEADER_SIZE}).");
			}

			int entryCount = (int)((firstEntryOffset / TH02_ENTRY_HEADER_SIZE) - 1);

			if (entryCount <= 0)
			{
				throw new InvalidDataException("There are no entries in the archive");
			}

			inputStream.Seek(0, SeekOrigin.Begin);

			int entryHeadersSize = entryCount * TH02_ENTRY_HEADER_SIZE;
			buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsMemory(0, entryHeadersSize);

			await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

			entries = ReadEntryHeadersTh01(game, entryCount, data.Span, extensionFilters);

			ArrayPool<byte>.Shared.Return(buffer);
		}
		else
		{
			inputStream.Seek(sizeof(ushort) * 2, SeekOrigin.Begin);

			byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(ushort) + sizeof(byte));
			Memory<byte> data = buffer.AsMemory(0, sizeof(ushort) + sizeof(byte));

			await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

			int entryCount = MemoryHelpers.ReadUInt16(data);

			if (entryCount <= 0)
			{
				throw new InvalidDataException("There are no entries in the archive");
			}

			byte key = data.Span[sizeof(ushort)];

			ArrayPool<byte>.Shared.Return(buffer);

			inputStream.Seek(sizeof(byte) * 9, SeekOrigin.Current);

			int entryHeadersSize = entryCount * TH03_ENTRY_HEADER_SIZE;
			buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsMemory(0, entryHeadersSize);

			await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

			entries = ReadEntryHeadersTh03(key, entryCount, data.Span, extensionFilters);

			ArrayPool<byte>.Shared.Return(buffer);
		}

		return new Archive(game, offset: 0, entries, inputStream, ArchiveInitializationMode.Extraction);
	}

	private static List<ZunEntry> ReadEntryHeadersTh01(Game game, int entryCount, Span<byte> entryHeadersData, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		List<ZunEntry> entries = new(entryCount);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entryCount; entryIndex++)
		{
			ZunEntry entry = new();

			// Skip entry magic
			entryHeadersDataPtr += sizeof(ushort);

			// Key inside entry header seems unused
			entry.Extra = s_keys[(int)game - 1];
			entryHeadersDataPtr += sizeof(byte);

			// File name
			Span<byte> fileName = entryHeadersData.Slice(entryHeadersDataPtr, 13);
			entryHeadersDataPtr += 13;

			int c;
			for (c = 0; c < 13 && fileName[c] != 0x0; c++)
			{
				fileName[c] ^= 0xff;
			}

			entry.FileName = Encoding.UTF8.GetString(fileName[..c]);

			if (extensionFilters?.Contains(Path.GetExtension(entry.FileName)) is false)
			{
				entryHeadersDataPtr += sizeof(uint) * 4;

				continue;
			}

			// Compressed size
			entry.CompressedSize = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			// Uncompressed size
			entry.Size = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			// Data offset in the archive
			entry.Offset = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			// Skip 0u
			entryHeadersDataPtr += sizeof(uint) * 2;

			entries.Add(entry);
		}

		entries.TrimExcess();

		return entries;
	}

	private static List<ZunEntry> ReadEntryHeadersTh03(byte key, int entryCount, Span<byte> entryHeadersData, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		for (int c = 0; c < entryHeadersData.Length; c++)
		{
			entryHeadersData[c] ^= key;
			key -= entryHeadersData[c];
		}

		List<ZunEntry> entries = new(entryCount);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entryCount; entryIndex++)
		{
			ZunEntry entry = new();

			// Skip entry magic
			entryHeadersDataPtr += sizeof(ushort);

			// Key
			entry.Extra = entryHeadersData[entryHeadersDataPtr];
			entryHeadersDataPtr += sizeof(byte);

			// File name
			entry.FileName = SpanHelpers.ReadString(entryHeadersData.Slice(entryHeadersDataPtr, 13));
			entryHeadersDataPtr += 13;

			if (extensionFilters?.Contains(Path.GetExtension(entry.FileName)) is false)
			{
				entryHeadersDataPtr += (sizeof(ushort) * 2) + (sizeof(uint) * 3);

				continue;
			}

			// Compressed size
			entry.CompressedSize = SpanHelpers.ReadUInt16(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(ushort);

			// Uncompressed size
			entry.Size = SpanHelpers.ReadUInt16(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(ushort);

			// Data offset in the archive
			entry.Offset = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			// Skip [0u, 0u]
			entryHeadersDataPtr += sizeof(uint) * 3;

			entries.Add(entry);
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

		if (entry.Size == 0)
		{
			return [];
		}

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		using MemoryStream stream = new(zunEntry.Size);
		ExtractCore(zunEntry, stream);

		return stream.GetBuffer().AsSpan(0, (int)stream.Length);
	}

	/// <inheritdoc/>
	public override async Task<Memory<byte>> ExtractAsync(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		if (entry.Size == 0)
		{
			return Memory<byte>.Empty;
		}

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		MemoryStream stream = new(zunEntry.Size);
		await using (stream.ConfigureAwait(false))
		{
			await ExtractAsyncCore(zunEntry, stream).ConfigureAwait(false);

			return stream.GetBuffer().AsMemory(0, (int)stream.Length);
		}
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

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		ExtractCore(zunEntry, outputStream);
	}

	private void ExtractCore(ZunEntry entry, Stream outputStream)
	{
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);
		Span<byte> data = buffer.AsSpan(0, entry.CompressedSize);

		_stream.ReadExactly(data);

		for (int c = 0; c < entry.CompressedSize; c++)
		{
			data[c] ^= (byte)entry.Extra;
		}

		if (entry.Size == entry.CompressedSize)
		{
			outputStream.Write(data);
		}
		else
		{
			ThRLE.Decompress(data, outputStream);
		}

		ArrayPool<byte>.Shared.Return(buffer);
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

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		await ExtractAsyncCore(zunEntry, outputStream).ConfigureAwait(false);
	}

	private async ValueTask ExtractAsyncCore(ZunEntry entry, Stream outputStream)
	{
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);
		Memory<byte> data = buffer.AsMemory(0, entry.CompressedSize);

		await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

		for (int c = 0; c < entry.CompressedSize; c++)
		{
			data.Span[c] ^= (byte)entry.Extra;
		}

		if (entry.Size == entry.CompressedSize)
		{
			await outputStream.WriteAsync(data).ConfigureAwait(false);
		}
		else
		{
			ThRLE.Decompress(data.Span, outputStream);
		}

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public override void Pack(Entry entry, byte[] entryData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(entryData);

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		zunEntry.Size = entryData.Length;

		if (zunEntry.Size == 0)
		{
			throw new ArgumentException($"The entry data is empty.", nameof(entryData));
		}

		using MemoryStream stream = new(entryData, writable: false);
		PackCore(zunEntry, stream);
	}

	/// <inheritdoc/>
	public override async ValueTask PackAsync(Entry entry, byte[] entryData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(entryData);

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		zunEntry.Size = entryData.Length;

		if (zunEntry.Size == 0)
		{
			throw new ArgumentException($"The entry data is empty.", nameof(entryData));
		}

		MemoryStream stream = new(entryData, writable: false);
		await using (stream.ConfigureAwait(false))
		{
			await PackAsyncCore(zunEntry, stream).ConfigureAwait(false);
		}
	}

	/// <inheritdoc/>
	public override void Pack(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		zunEntry.Size = (int)inputStream.Length;

		if (zunEntry.Size == int.MinValue)
		{
			throw new ArgumentException($"The stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (zunEntry.Size == 0)
		{
			throw new ArgumentException($"The stream is empty.", nameof(inputStream));
		}

		PackCore(zunEntry, inputStream);
	}

	private void PackCore(ZunEntry entry, Stream inputStream)
	{
		using MemoryStream compressedDataStream = new();

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
		Span<byte> entryData = buffer.AsSpan(0, entry.Size);

		inputStream.ReadExactly(entryData);

		entry.CompressedSize = ThRLE.Compress(entryData, compressedDataStream);

		Span<byte> compressedData;

		if (entry.CompressedSize >= entry.Size)
		{
			entry.CompressedSize = entry.Size;

			compressedData = entryData;
		}
		else
		{
			compressedData = compressedDataStream.GetBuffer().AsSpan(0, entry.CompressedSize);
		}

		for (int c = 0; c < entry.CompressedSize; c++)
		{
			compressedData[c] ^= _game <= Game.SoEW ? s_keys[(int)_game - 1] : ENTRY_KEY;
		}

		entry.Offset = (int)_stream!.Position;

		if (entry.Offset == int.MinValue)
		{
			throw new InvalidOperationException($"The archive is too big to add more entries.");
		}

		_stream.Write(compressedData);
		_offset += (uint)entry.CompressedSize;

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public override async ValueTask PackAsync(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		if (entry is not ZunEntry zunEntry)
		{
			throw new ArgumentException($"The entry must be from a Touhou {(int)_game} archive.", nameof(entry));
		}

		zunEntry.Size = (int)inputStream.Length;

		if (zunEntry.Size == int.MinValue)
		{
			throw new ArgumentException($"The stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (zunEntry.Size == 0)
		{
			throw new ArgumentException($"The stream is empty.", nameof(inputStream));
		}

		await PackAsyncCore(zunEntry, inputStream).ConfigureAwait(false);
	}

	private async ValueTask PackAsyncCore(ZunEntry entry, Stream inputStream)
	{
		MemoryStream compressedDataStream = new();
		await using (compressedDataStream.ConfigureAwait(false))
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
			Memory<byte> entryData = buffer.AsMemory(0, entry.Size);

			await inputStream.ReadExactlyAsync(entryData).ConfigureAwait(false);

			entry.CompressedSize = await ThRLE.CompressAsync(entryData, compressedDataStream).ConfigureAwait(false);

			Memory<byte> compressedData;

			if (entry.CompressedSize >= entry.Size)
			{
				entry.CompressedSize = entry.Size;

				compressedData = entryData;
			}
			else
			{
				compressedData = compressedDataStream.GetBuffer().AsMemory(0, entry.CompressedSize);
			}

			for (int c = 0; c < entry.CompressedSize; c++)
			{
				compressedData.Span[c] ^= _game <= Game.SoEW ? s_keys[(int)_game - 1] : ENTRY_KEY;
			}

			entry.Offset = (int)_stream!.Position;

			if (entry.Offset == int.MinValue)
			{
				throw new InvalidOperationException($"The archive is too big to add more entries.");
			}

			await _stream.WriteAsync(compressedData).ConfigureAwait(false);
			_offset += (uint)entry.CompressedSize;

			ArrayPool<byte>.Shared.Return(buffer);
		}
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

		_stream!.Seek(0, SeekOrigin.Begin);

		byte[] buffer;
		Span<byte> data;

		if (_game <= Game.SoEW)
		{
			int entryHeadersSize = (_entries.Count + 1) * TH02_ENTRY_HEADER_SIZE;

			buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsSpan(0, entryHeadersSize);

			WriteEntryHeadersTh01(data);
		}
		else
		{
			// Archive header
			buffer = ArrayPool<byte>.Shared.Rent(TH03_ARCHIVE_HEADER_SIZE);
			data = buffer.AsSpan(0, TH03_ARCHIVE_HEADER_SIZE);

			ushort entryHeadersSize = WriteArchiveHeaderTh03(data);

			_stream.Write(data);

			ArrayPool<byte>.Shared.Return(buffer);

			// Entry headers
			buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsSpan(0, entryHeadersSize);

			WriteEntryHeadersTh03(data);
		}

		_stream.Write(data);

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

		_stream!.Seek(0, SeekOrigin.Begin);

		byte[] buffer;
		Memory<byte> data;

		if (_game <= Game.SoEW)
		{
			int entryHeadersSize = (_entries.Count + 1) * TH02_ENTRY_HEADER_SIZE;

			buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsMemory(0, entryHeadersSize);

			WriteEntryHeadersTh01(data.Span);
		}
		else
		{
			// Archive header
			buffer = ArrayPool<byte>.Shared.Rent(TH03_ARCHIVE_HEADER_SIZE);
			data = buffer.AsMemory(0, TH03_ARCHIVE_HEADER_SIZE);

			ushort entryHeadersSize = WriteArchiveHeaderTh03(data.Span);

			await _stream.WriteAsync(data).ConfigureAwait(false);

			ArrayPool<byte>.Shared.Return(buffer);

			// Entry headers
			buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);
			data = buffer.AsMemory(0, entryHeadersSize);

			WriteEntryHeadersTh03(data.Span);
		}

		await _stream.WriteAsync(data).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private ushort WriteArchiveHeaderTh03(Span<byte> archiveHeaderData)
	{
		int archiveHeaderDataPtr = 0;
		ushort entryCount = (ushort)_entries!.Count;

		// Entry headers size
		ushort entryHeadersSize = (ushort)((entryCount + 1) * TH03_ENTRY_HEADER_SIZE);
		MemoryMarshal.Write(archiveHeaderData.Slice(archiveHeaderDataPtr, sizeof(ushort)), entryHeadersSize);
		archiveHeaderDataPtr += sizeof(ushort);

		// Unknown = 2
		MemoryMarshal.Write<ushort>(archiveHeaderData.Slice(archiveHeaderDataPtr, sizeof(ushort)), 2);
		archiveHeaderDataPtr += sizeof(ushort);

		// Entries count
		MemoryMarshal.Write(archiveHeaderData.Slice(archiveHeaderDataPtr, sizeof(ushort)), entryCount);
		archiveHeaderDataPtr += sizeof(ushort);

		// Key
		archiveHeaderData[archiveHeaderDataPtr] = ARCHIVE_KEY;
		archiveHeaderDataPtr += sizeof(byte);

		// Zero
		archiveHeaderData.Slice(archiveHeaderDataPtr, sizeof(byte) * 9).Clear();

		return entryHeadersSize;
	}

	private void WriteEntryHeadersTh01(Span<byte> entryHeadersData)
	{
		ReadOnlySpan<ZunEntry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entries.Length; entryIndex++)
		{
			ZunEntry entry = entries[entryIndex];

			// Magic
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(ushort)), entry.CompressedSize == entry.Size ? UNCOMPRESSED_ENTRIES_MAGIC : COMPRESSED_ENTRIES_MAGIC);
			entryHeadersDataPtr += sizeof(ushort);

			// Extra (key (seems unused))
			entryHeadersData[entryHeadersDataPtr] = 3;
			entryHeadersDataPtr += sizeof(byte);

			// File name
			Span<byte> fileName = entryHeadersData.Slice(entryHeadersDataPtr, entry.FileName.Length);

			if (Utf8.FromUtf16(entry.FileName, fileName, out _, out _, replaceInvalidSequences: false) != OperationStatus.Done)
			{
				throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
			}

			for (int c = 0; c < entry.FileName.Length; c++)
			{
				fileName[c] ^= 0xff;
			}

			entryHeadersDataPtr += entry.FileName.Length;

			if (entry.FileName.Length < 13)
			{
				int bytesLeftCount = 13 - entry.FileName.Length;

				entryHeadersData.Slice(entryHeadersDataPtr, bytesLeftCount).Clear();
				entryHeadersDataPtr += bytesLeftCount;
			}

			// Compressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.CompressedSize);
			entryHeadersDataPtr += sizeof(uint);

			// Uncompressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Size);
			entryHeadersDataPtr += sizeof(uint);

			// Data offset in the archive
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Offset);
			entryHeadersDataPtr += sizeof(uint);

			// Zero
			entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)).Clear();
			entryHeadersDataPtr += sizeof(uint);
		}
	}

	private void WriteEntryHeadersTh03(Span<byte> entryHeadersData)
	{
		ReadOnlySpan<ZunEntry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entries.Length; entryIndex++)
		{
			ZunEntry entry = entries[entryIndex];

			// Magic
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), entry.CompressedSize == entry.Size ? UNCOMPRESSED_ENTRIES_MAGIC : COMPRESSED_ENTRIES_MAGIC);
			entryHeadersDataPtr += sizeof(ushort);

			// Extra (key)
			entryHeadersData[entryHeadersDataPtr] = ENTRY_KEY;
			entryHeadersDataPtr += sizeof(byte);

			// File name
			Span<byte> fileName = entryHeadersData.Slice(entryHeadersDataPtr, entry.FileName.Length);

			if (Utf8.FromUtf16(entry.FileName, fileName, out _, out _, replaceInvalidSequences: false) != OperationStatus.Done)
			{
				throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
			}

			entryHeadersDataPtr += entry.FileName.Length;

			if (entry.FileName.Length < 13)
			{
				int bytesLeftCount = 13 - entry.FileName.Length;

				entryHeadersData.Slice(entryHeadersDataPtr, bytesLeftCount).Clear();
				entryHeadersDataPtr += bytesLeftCount;
			}

			// Compressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(ushort)), (ushort)entry.CompressedSize);
			entryHeadersDataPtr += sizeof(ushort);

			// Uncompressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(ushort)), (ushort)entry.Size);
			entryHeadersDataPtr += sizeof(ushort);

			// Data offset in the archive
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Offset);
			entryHeadersDataPtr += sizeof(uint);

			// Zero
			entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint) * 2).Clear();
			entryHeadersDataPtr += sizeof(uint) * 2;
		}

		uint dataKey = ARCHIVE_KEY;

		for (ushort c = 0; c < entryHeadersData.Length; c++)
		{
			byte tmp = entryHeadersData[c];
			entryHeadersData[c] ^= (byte)dataKey;
			dataKey -= tmp;
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
