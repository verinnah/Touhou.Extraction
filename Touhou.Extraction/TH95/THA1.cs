using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Touhou.Common;
using Touhou.Common.Compression;
using Touhou.Extraction.Crypto;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH95;

/// <summary>
/// Represents an archive file containing data from Touhou 9.5-19 (only ZUN-made). This class cannot be inherited.
/// </summary>
public sealed class THA1 : Archive
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
	private protected override ArchiveFileNamesOptions Flags { get; } = ArchiveFileNamesOptions.BaseName;

	private int _offset;
	private Stream? _stream;
	private bool _isDisposed;
	private readonly Game _game;
	private List<ZunEntry>? _entries;
	private readonly ArchiveInitializationMode _initializationMode;

	private static readonly CryptParams[] s_cryptParamsTh95 =
	[
		new CryptParams { Key = 0x1b, Step = 0x37, Block = 0x40,  Limit = 0x2800 },
		new CryptParams { Key = 0x51, Step = 0xe9, Block = 0x40,  Limit = 0x3000 },
		new CryptParams { Key = 0xc1, Step = 0x51, Block = 0x80,  Limit = 0x3200 },
		new CryptParams { Key = 0x03, Step = 0x19, Block = 0x400, Limit = 0x7800 },
		new CryptParams { Key = 0xab, Step = 0xcd, Block = 0x200, Limit = 0x2800 },
		new CryptParams { Key = 0x12, Step = 0x34, Block = 0x80,  Limit = 0x3200 },
		new CryptParams { Key = 0x35, Step = 0x97, Block = 0x80,  Limit = 0x2800 },
		new CryptParams { Key = 0x99, Step = 0x37, Block = 0x400, Limit = 0x2000 }
	];
	private static readonly CryptParams[] s_cryptParamsTh12 =
	[
		new CryptParams { Key = 0x1b, Step = 0x73, Block = 0x40,  Limit = 0x3800 },
		new CryptParams { Key = 0x51, Step = 0x9e, Block = 0x40,  Limit = 0x4000 },
		new CryptParams { Key = 0xc1, Step = 0x15, Block = 0x400, Limit = 0x2c00 },
		new CryptParams { Key = 0x03, Step = 0x91, Block = 0x80,  Limit = 0x6400 },
		new CryptParams { Key = 0xab, Step = 0xdc, Block = 0x80,  Limit = 0x6e00 },
		new CryptParams { Key = 0x12, Step = 0x43, Block = 0x200, Limit = 0x3c00 },
		new CryptParams { Key = 0x35, Step = 0x79, Block = 0x400, Limit = 0x3c00 },
		new CryptParams { Key = 0x99, Step = 0x7d, Block = 0x80,  Limit = 0x2800 }
	];
	private static readonly CryptParams[] s_cryptParamsTh13 =
	[
		new CryptParams { Key = 0x1b, Step = 0x73, Block = 0x100, Limit = 0x3800 },
		new CryptParams { Key = 0x12, Step = 0x43, Block = 0x200, Limit = 0x3e00 },
		new CryptParams { Key = 0x35, Step = 0x79, Block = 0x400, Limit = 0x3c00 },
		new CryptParams { Key = 0x03, Step = 0x91, Block = 0x80,  Limit = 0x6400 },
		new CryptParams { Key = 0xab, Step = 0xdc, Block = 0x80,  Limit = 0x6e00 },
		new CryptParams { Key = 0x51, Step = 0x9e, Block = 0x100, Limit = 0x4000 },
		new CryptParams { Key = 0xc1, Step = 0x15, Block = 0x400, Limit = 0x2c00 },
		new CryptParams { Key = 0x99, Step = 0x7d, Block = 0x80,  Limit = 0x4400 }
	];
	private static readonly CryptParams[] s_cryptParamsTh14 =
	[
		new CryptParams { Key = 0x1b, Step = 0x73, Block = 0x100, Limit = 0x3800 },
		new CryptParams { Key = 0x12, Step = 0x43, Block = 0x200, Limit = 0x3e00 },
		new CryptParams { Key = 0x35, Step = 0x79, Block = 0x400, Limit = 0x3c00 },
		new CryptParams { Key = 0x03, Step = 0x91, Block = 0x80,  Limit = 0x6400 },
		new CryptParams { Key = 0xab, Step = 0xdc, Block = 0x80,  Limit = 0x7000 },
		new CryptParams { Key = 0x51, Step = 0x9e, Block = 0x100, Limit = 0x4000 },
		new CryptParams { Key = 0xc1, Step = 0x15, Block = 0x400, Limit = 0x2c00 },
		new CryptParams { Key = 0x99, Step = 0x7d, Block = 0x80,  Limit = 0x4400 }
	];

	private const int HEADER_SIZE = sizeof(uint) * 4;
	private const uint HEADER_MAGIC = 0x31414854 /* 1AHT */;

	private THA1(Game game, int offset, List<ZunEntry> entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_game = game;
		_offset = offset;
		_stream = stream;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares an <see cref="THA1"/> for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="game">The game this archive is from.</param>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <returns>An instance of the <see cref="THA1"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	public static THA1 Create(Game game, int entryCount, Stream outputStream, string[] entryFileNames)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		int offset = 16;
		outputStream.Seek(offset, SeekOrigin.Begin);

		List<ZunEntry> entries = new(entryCount);

		THA1 archive = new(game, offset, entries, outputStream, ArchiveInitializationMode.Creation);

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
	/// <returns>An instance of the <see cref="THA1"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="inputStream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> is <see langword="null"/>.</exception>
	public static new THA1 Read(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		// Read archive header
		Span<byte> data = stackalloc byte[HEADER_SIZE];

		inputStream.ReadExactly(data);

		ZunCrypt.Decrypt(data, key: 0x1b, step: 0x37, block: HEADER_SIZE, limit: HEADER_SIZE);

		ref Header header = ref MemoryMarshal.AsRef<Header>(data);

		if (header.Magic != HEADER_MAGIC)
		{
			throw new InvalidDataException($"Magic string not recognized (expected THA1, read {Encoding.UTF8.GetString(BitConverter.GetBytes(header.Magic))}).");
		}

		header.EntryCount -= 135792468;
		int entryCount = (int)header.EntryCount;

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		header.Size -= 123456789;
		header.CompressedSize -= 987654321;

		// Reads entry headers
		inputStream.Seek(-header.CompressedSize, SeekOrigin.End);

		byte[] compressedEntryHeadersData = ArrayPool<byte>.Shared.Rent((int)header.CompressedSize);
		data = compressedEntryHeadersData.AsSpan(0, (int)header.CompressedSize);

		inputStream.ReadExactly(data);

		ZunCrypt.Decrypt(data, key: 0x3e, step: 0x9b, block: 0x80, limit: header.CompressedSize);

		using MemoryStream entryHeadersStream = new((int)header.Size);
		using MemoryStream compressedEntryHeadersStream = new(compressedEntryHeadersData, writable: false);
		LZSS.Decompress(compressedEntryHeadersStream, entryHeadersStream, (int)header.Size);

		ArrayPool<byte>.Shared.Return(compressedEntryHeadersData);

		List<ZunEntry> entries = ReadEntryHeaders((int)inputStream.Length, (int)header.CompressedSize, entryCount, entryHeadersStream.GetBuffer().AsSpan(0, (int)header.Size), extensionFilters);

		return new THA1(game, offset: 0, entries, inputStream, ArchiveInitializationMode.Extraction);
	}

	/// <inheritdoc cref="Read(Game, Stream, ArchiveReadOptions, string[])"/>
	public static new async Task<THA1> ReadAsync(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		// Read archive header
		byte[] buffer = ArrayPool<byte>.Shared.Rent(HEADER_SIZE);
		Memory<byte> headerData = buffer.AsMemory(0, HEADER_SIZE);

		await inputStream.ReadExactlyAsync(headerData).ConfigureAwait(false);

		ZunCrypt.Decrypt(headerData.Span, key: 0x1b, step: 0x37, block: HEADER_SIZE, limit: HEADER_SIZE);

		Header header = MemoryMarshal.Read<Header>(headerData.Span);

		ArrayPool<byte>.Shared.Return(buffer);

		if (header.Magic != HEADER_MAGIC)
		{
			throw new InvalidDataException($"Magic string not recognized (expected THA1, read {Encoding.UTF8.GetString(BitConverter.GetBytes(header.Magic))}).");
		}

		header.EntryCount -= 135792468;
		int entryCount = (int)header.EntryCount;

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		header.Size -= 123456789;
		header.CompressedSize -= 987654321;

		// Reads entry headers
		inputStream.Seek(-header.CompressedSize, SeekOrigin.End);

		buffer = ArrayPool<byte>.Shared.Rent((int)header.CompressedSize);

		await inputStream.ReadExactlyAsync(buffer.AsMemory(0, (int)header.CompressedSize)).ConfigureAwait(false);

		ZunCrypt.Decrypt(buffer.AsSpan(0, (int)header.CompressedSize), key: 0x3e, step: 0x9b, block: 0x80, limit: header.CompressedSize);

		MemoryStream entryHeadersStream = new((int)header.Size);
		await using (entryHeadersStream.ConfigureAwait(false))
		{
			MemoryStream compressedEntryHeadersStream = new(buffer, writable: false);
			await using (compressedEntryHeadersStream.ConfigureAwait(false))
			{
				LZSS.Decompress(compressedEntryHeadersStream, entryHeadersStream, (int)header.Size);
			}

			ArrayPool<byte>.Shared.Return(buffer);

			List<ZunEntry> entries = ReadEntryHeaders((int)inputStream.Length, (int)header.CompressedSize, entryCount, entryHeadersStream.GetBuffer().AsSpan(0, (int)header.Size), extensionFilters);

			return new THA1(game, offset: 0, entries, inputStream, ArchiveInitializationMode.Extraction);
		}
	}

	private static List<ZunEntry> ReadEntryHeaders(int inputLength, int compressedSize, int entryCount, ReadOnlySpan<byte> entryHeadersData, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		ZunEntry? previousEntry = null;
		List<ZunEntry> entries = new(entryCount);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entryCount; entryIndex++)
		{
			// File name
			ZunEntry entry = new()
			{
				FileName = SpanHelpers.ReadString(entryHeadersData[entryHeadersDataPtr..])
			};
			entryHeadersDataPtr += entry.FileName.Length + (4 - (entry.FileName.Length % 4));

			// Data offset in the archive
			entry.Offset = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			if (entry.Offset < 0)
			{
				throw new InvalidDataException($"The entry \"{entry.FileName}\" has an invalid offset ({entry.Offset}).");
			}

			if (previousEntry is not null)
			{
				previousEntry.CompressedSize = entry.Offset - previousEntry.Offset;
			}

			previousEntry = entry;

			if (extensionFilters?.Contains(Path.GetExtension(entry.FileName)) is false)
			{
				entryHeadersDataPtr += sizeof(uint) * 2;

				continue;
			}

			// Uncompressed size
			entry.Size = SpanHelpers.ReadInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			if (entry.Size < 0)
			{
				throw new InvalidDataException($"The entry \"{entry.FileName}\" has an invalid size ({entry.Size}).");
			}

			// Zero
			entry.Extra = SpanHelpers.ReadUInt32(entryHeadersData, entryHeadersDataPtr);
			entryHeadersDataPtr += sizeof(uint);

			entries.Add(entry);
		}

		previousEntry!.CompressedSize = inputLength - compressedSize - previousEntry.Offset;

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

		return stream.GetBuffer().AsSpan(0, zunEntry.Size);
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

			return stream.GetBuffer().AsMemory(0, zunEntry.Size);
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
		byte[] compressedData = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);

		_stream!.Seek(entry.Offset, SeekOrigin.Begin);
		_stream.ReadExactly(compressedData.AsSpan(0, entry.CompressedSize));

		CryptParams cryptParams = GetCryptParams(entry.FileName);
		ZunCrypt.Decrypt(compressedData.AsSpan(0, entry.CompressedSize), cryptParams.Key, cryptParams.Step, cryptParams.Block, cryptParams.Limit);

		Span<byte> data;

		if (entry.CompressedSize == entry.Size)
		{
			data = compressedData.AsSpan(0, entry.CompressedSize);
		}
		else
		{
			using MemoryStream dataStream = new(entry.Size);
			using MemoryStream compressedDataStream = new(compressedData, 0, entry.CompressedSize, writable: false);
			LZSS.Decompress(compressedDataStream, dataStream, entry.Size);

			ArrayPool<byte>.Shared.Return(compressedData);

			data = dataStream.GetBuffer().AsSpan(0, entry.Size);
		}

		outputStream.Write(data);
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
		byte[] compressedData = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);

		_stream!.Seek(entry.Offset, SeekOrigin.Begin);
		await _stream.ReadExactlyAsync(compressedData.AsMemory(0, entry.CompressedSize)).ConfigureAwait(false);

		CryptParams cryptParams = GetCryptParams(entry.FileName);
		ZunCrypt.Decrypt(compressedData.AsSpan(0, entry.CompressedSize), cryptParams.Key, cryptParams.Step, cryptParams.Block, cryptParams.Limit);

		Memory<byte> data;

		if (entry.CompressedSize == entry.Size)
		{
			data = compressedData.AsMemory(0, entry.CompressedSize);
		}
		else
		{
			MemoryStream dataStream = new(entry.Size);
			await using (dataStream.ConfigureAwait(false))
			{
				MemoryStream compressedDataStream = new(compressedData, 0, entry.CompressedSize, writable: false);
				await using (compressedDataStream.ConfigureAwait(false))
				{
					LZSS.Decompress(compressedDataStream, dataStream, entry.Size);

					ArrayPool<byte>.Shared.Return(compressedData);

					data = dataStream.GetBuffer().AsMemory(0, entry.Size);
				}
			}
		}

		await outputStream.WriteAsync(data).ConfigureAwait(false);
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
		long dataOffset = inputStream.Position;
		using MemoryStream compressedDataStream = new();
		entry.CompressedSize = LZSS.Compress(inputStream, entry.Size, compressedDataStream);

		byte[] buffer;
		Span<byte> data;

		if (entry.CompressedSize >= entry.Size)
		{
			inputStream.Seek(dataOffset, SeekOrigin.Begin);

			buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
			data = buffer.AsSpan(0, entry.Size);

			inputStream.ReadExactly(data);

			entry.CompressedSize = entry.Size;
		}
		else
		{
			buffer = compressedDataStream.GetBuffer();
			data = buffer.AsSpan(0, entry.CompressedSize);
		}

		CryptParams cryptParams = GetCryptParams(entry.FileName);
		ZunCrypt.Encrypt(data, cryptParams.Key, cryptParams.Step, cryptParams.Block, cryptParams.Limit);

		_stream!.Write(data);

		if (entry.CompressedSize == entry.Size)
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		entry.Offset = _offset;
		_offset += entry.CompressedSize;
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
			long dataOffset = inputStream.Position;
			entry.CompressedSize = LZSS.Compress(inputStream, entry.Size, compressedDataStream);

			byte[] data;

			if (entry.CompressedSize >= entry.Size)
			{
				inputStream.Seek(dataOffset, SeekOrigin.Begin);

				data = ArrayPool<byte>.Shared.Rent(entry.Size);
				await inputStream.ReadExactlyAsync(data.AsMemory(0, entry.Size)).ConfigureAwait(false);

				entry.CompressedSize = entry.Size;
			}
			else
			{
				data = compressedDataStream.GetBuffer();
			}

			CryptParams cryptParams = GetCryptParams(entry.FileName);
			ZunCrypt.Encrypt(data.AsSpan(0, entry.CompressedSize), cryptParams.Key, cryptParams.Step, cryptParams.Block, cryptParams.Limit);

			await _stream!.WriteAsync(data.AsMemory(0, entry.CompressedSize)).ConfigureAwait(false);

			if (entry.CompressedSize == entry.Size)
			{
				ArrayPool<byte>.Shared.Return(data);
			}
		}

		entry.Offset = _offset;
		_offset += entry.CompressedSize;
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

		int entryHeadersSize = 0;
		Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + (sizeof(uint) * 3) + entry.FileName.Length + (4 - (entry.FileName.Length % 4)), sum => Interlocked.Add(ref entryHeadersSize, sum));

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);

		WriteEntryHeaders(buffer.AsSpan(0, entryHeadersSize));

		using MemoryStream compressedEntryHeadersStream = new();
		using MemoryStream entryHeadersStream = new(buffer, 0, entryHeadersSize, false);

		int compressedEntryHeadersSize = LZSS.Compress(entryHeadersStream, entryHeadersSize, compressedEntryHeadersStream);
		Span<byte> compressedEntryHeadersData = compressedEntryHeadersStream.GetBuffer().AsSpan(0, compressedEntryHeadersSize);

		ZunCrypt.Encrypt(compressedEntryHeadersData, key: 0x3e, step: 0x9b, block: 0x80, limit: (uint)entryHeadersSize);

		_stream!.Write(compressedEntryHeadersData);

		// Write archive header
		_stream.Seek(0, SeekOrigin.Begin);

		Span<byte> headerData = buffer.AsSpan(0, sizeof(uint) * 4);
		Span<uint> header = MemoryMarshal.Cast<byte, uint>(headerData);

		// Magic
		header[0] = HEADER_MAGIC;
		// Entry headers size
		header[1] = (uint)entryHeadersSize + 123456789;
		// Entry headers compressed size
		header[2] = (uint)compressedEntryHeadersSize + 987654321;
		// Entry count
		header[3] = (uint)_entries.Count + 135792468;

		ZunCrypt.Encrypt(headerData, key: 0x1b, step: 0x37, block: (uint)headerData.Length, limit: (uint)headerData.Length);

		_stream.Write(headerData);

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

		int entryHeadersSize = 0;
		Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + (sizeof(uint) * 3) + entry.FileName.Length + (4 - (entry.FileName.Length % 4)), sum => Interlocked.Add(ref entryHeadersSize, sum));

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);

		WriteEntryHeaders(buffer.AsSpan(0, entryHeadersSize));

		int compressedEntryHeadersSize;
		MemoryStream compressedEntryHeadersStream = new();
		await using (compressedEntryHeadersStream.ConfigureAwait(false))
		{
			MemoryStream entryHeadersStream = new(buffer, 0, entryHeadersSize, writable: false);
			await using (entryHeadersStream.ConfigureAwait(false))
			{
				compressedEntryHeadersSize = LZSS.Compress(entryHeadersStream, entryHeadersSize, compressedEntryHeadersStream);
			}

			Memory<byte> compressedEntryHeadersData = compressedEntryHeadersStream.GetBuffer().AsMemory(0, compressedEntryHeadersSize);

			ZunCrypt.Encrypt(compressedEntryHeadersData.Span, key: 0x3e, step: 0x9b, block: 0x80, limit: (uint)entryHeadersSize);

			await _stream!.WriteAsync(compressedEntryHeadersData).ConfigureAwait(false);
		}

		// Write archive header
		_stream.Seek(0, SeekOrigin.Begin);

		Header header = new()
		{
			Magic = HEADER_MAGIC,
			Size = (uint)entryHeadersSize + 123456789,
			EntryCount = (uint)_entries.Count + 135792468,
			CompressedSize = (uint)compressedEntryHeadersSize + 987654321
		};

		Memory<byte> headerData = buffer.AsMemory(0, HEADER_SIZE);
		MemoryMarshal.Write(headerData.Span, in header);

		ZunCrypt.Encrypt(headerData.Span, key: 0x1b, step: 0x37, block: (uint)headerData.Length, limit: (uint)headerData.Length);

		await _stream.WriteAsync(headerData).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private void WriteEntryHeaders(Span<byte> entryHeadersData)
	{
		ReadOnlySpan<ZunEntry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0, entryHeadersDataPtr = 0; entryIndex < entries.Length; entryIndex++)
		{
			ZunEntry entry = entries[entryIndex];

			// File name
			if (Utf8.FromUtf16(entry.FileName, entryHeadersData.Slice(entryHeadersDataPtr, entry.FileName.Length), out _, out _) != OperationStatus.Done)
			{
				throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
			}

			entryHeadersDataPtr += entry.FileName.Length;

			int fileNamePaddingLength = 4 - (entry.FileName.Length % 4);
			entryHeadersData.Slice(entryHeadersDataPtr, fileNamePaddingLength).Clear();
			entryHeadersDataPtr += fileNamePaddingLength;

			// Data offset in the archive
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Offset);
			entryHeadersDataPtr += sizeof(uint);

			// Uncompressed size
			MemoryMarshal.Write(entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)), (uint)entry.Size);
			entryHeadersDataPtr += sizeof(uint);

			// Extra (zero)
			entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)).Clear();
			entryHeadersDataPtr += sizeof(uint);
		}
	}

	private CryptParams GetCryptParams(ReadOnlySpan<char> fileName)
	{
		uint index = 0;
		for (int c = 0; c < fileName.Length; c++)
		{
			index += (byte)fileName[c];
		}

		index &= 7;

		return _game switch
		{
			Game.StB or Game.MoF or Game.SA => s_cryptParamsTh95[index],
			Game.UFO or Game.DS or Game.GFW => s_cryptParamsTh12[index],
			Game.TD => s_cryptParamsTh13[index],
			_ => s_cryptParamsTh14[index],
		};
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
	private struct Header
	{
		internal readonly required uint Magic { get; init; }
		// value - 123456789
		internal required uint Size { get; set; }
		// value - 987654321
		internal required uint CompressedSize { get; set; }
		// value - 135792468
		internal required uint EntryCount { get; set; }
	}

	private readonly struct CryptParams
	{
		internal readonly required byte Key { get; init; }
		internal readonly required byte Step { get; init; }
		internal readonly required uint Block { get; init; }
		internal readonly required uint Limit { get; init; }
	}
}
