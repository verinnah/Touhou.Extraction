using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Touhou.Common;
using Touhou.Common.Compression;
using Touhou.Extraction.Crypto;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH08;

/// <summary>
/// Represents an archive file containing data from Touhou 8/9. This class cannot be inherited.
/// </summary>
public sealed class PBGZ : Archive
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

	private static readonly CryptParams[] s_cryptParamsTh08 =
	[
		new CryptParams { Type = 'M', Key = 0x1b, Step = 0x37, Block =   0x40, Limit = 0x2000 }, // .msg
		new CryptParams { Type = 'T', Key = 0x51, Step = 0xe9, Block =   0x40, Limit = 0x3000 }, // .txt
		new CryptParams { Type = 'A', Key = 0xc1, Step = 0x51, Block = 0x1400, Limit = 0x2000 }, // .anm
		new CryptParams { Type = 'J', Key = 0x03, Step = 0x19, Block = 0x1400, Limit = 0x7800 }, // .jpg
		new CryptParams { Type = 'E', Key = 0xab, Step = 0xcd, Block =  0x200, Limit = 0x1000 }, // .ecl
		new CryptParams { Type = 'W', Key = 0x12, Step = 0x34, Block =  0x400, Limit = 0x2800 }, // .wav
		new CryptParams { Type = '-', Key = 0x35, Step = 0x97, Block =   0x80, Limit = 0x2800 }, // .*
		new CryptParams { Type = '*', Key = 0x99, Step = 0x37, Block =  0x400, Limit = 0x1000 }  // .* (not present in original archives)
	];
	private static readonly CryptParams[] s_cryptParamsTh09 =
	[
		new CryptParams { Type = 'M', Key = 0x1b, Step = 0x37, Block =  0x40, Limit = 0x2800 }, // .msg
		new CryptParams { Type = 'T', Key = 0x51, Step = 0xe9, Block =  0x40, Limit = 0x3000 }, // .txt
		new CryptParams { Type = 'A', Key = 0xc1, Step = 0x51, Block = 0x400, Limit =  0x400 }, // .anm
		new CryptParams { Type = 'J', Key = 0x03, Step = 0x19, Block = 0x400, Limit =  0x400 }, // .jpg
		new CryptParams { Type = 'E', Key = 0xab, Step = 0xcd, Block = 0x200, Limit = 0x1000 }, // .ecl
		new CryptParams { Type = 'W', Key = 0x12, Step = 0x34, Block = 0x400, Limit =  0x400 }, // .wav
		new CryptParams { Type = '-', Key = 0x35, Step = 0x97, Block =  0x80, Limit = 0x2800 }, // .*
		new CryptParams { Type = '*', Key = 0x99, Step = 0x37, Block = 0x400, Limit = 0x1000 }  // .* (not present in original archives; required for th09e.dat)
	];

	private const int HEADER_SIZE = sizeof(uint) * 4;
	private const uint HEADER_MAGIC = 0x5a474250 /* ZGBP */;

	private const uint CRYPT_TYPE_MSG = 0;
	private const uint CRYPT_TYPE_TXT = 1;
	private const uint CRYPT_TYPE_ANM = 2;
	private const uint CRYPT_TYPE_JPG = 3;
	private const uint CRYPT_TYPE_ECL = 4;
	private const uint CRYPT_TYPE_WAV = 5;
	private const uint CRYPT_TYPE_ETC = 6;
	//private const uint CRYPT_TYPE_UNK = 7;

	private PBGZ(Game game, int offset, List<ZunEntry> entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_game = game;
		_offset = offset;
		_stream = stream;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares an <see cref="PBGZ"/> for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="game">The game this archive is from.</param>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <returns>An instance of the <see cref="PBGZ"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	public static PBGZ Create(Game game, int entryCount, Stream outputStream, string[] entryFileNames)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		outputStream.Seek(offset: HEADER_SIZE, SeekOrigin.Begin);

		List<ZunEntry> entries = new(entryCount);

		PBGZ archive = new(game, offset: HEADER_SIZE, entries, outputStream, ArchiveInitializationMode.Creation);

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
	/// <returns>An instance of the <see cref="PBGZ"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="inputStream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> is <see langword="null"/>.</exception>
	public static new PBGZ Read(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		int fileSize = (int)inputStream.Length;

		inputStream.Seek(0, SeekOrigin.Begin);

		// Read archive header
		Span<byte> headerData = stackalloc byte[HEADER_SIZE];

		inputStream.ReadExactly(headerData);

		ZunCrypt.Decrypt(headerData[4..], key: 0x1b, step: 0x37, block: HEADER_SIZE - 4, limit: 0x400);

		ref Header header = ref MemoryMarshal.AsRef<Header>(headerData);

		if (header.Magic != HEADER_MAGIC)
		{
			throw new InvalidDataException($"Magic string not recognized (expected PBGZ, read {Encoding.UTF8.GetString(BitConverter.GetBytes(header.Magic))}).");
		}

		header.EntryCount -= 123456;
		int entryCount = (int)header.EntryCount;

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		header.Offset -= 345678;
		header.Size -= 567891;

		// Reads entry headers
		inputStream.Seek(header.Offset, SeekOrigin.Begin);

		int compressedSize = (int)(fileSize - header.Offset);
		byte[] compressedData = ArrayPool<byte>.Shared.Rent(compressedSize);
		Span<byte> compressedDataSpan = compressedData.AsSpan(0, compressedSize);

		inputStream.ReadExactly(compressedDataSpan);

		ZunCrypt.Decrypt(compressedDataSpan, key: 0x3e, step: 0x9b, block: 0x80, limit: 0x400);

		using MemoryStream entryHeadersDataStream = new((int)header.Size);
		using MemoryStream compressedDataStream = new(compressedData, 0, compressedSize, writable: false);
		LZSS.Decompress(compressedDataStream, entryHeadersDataStream, (int)header.Size);

		ArrayPool<byte>.Shared.Return(compressedData);

		List<ZunEntry> entries = ReadEntryHeaders(entryCount, compressedSize, fileSize, entryHeadersDataStream.GetBuffer().AsSpan(0, (int)header.Size), extensionFilters);

		return new PBGZ(game, offset: 0, entries, inputStream, ArchiveInitializationMode.Extraction);
	}

	/// <inheritdoc cref="Read(Game, Stream, ArchiveReadOptions, string[])"/>
	public static new async Task<PBGZ> ReadAsync(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		int fileSize = (int)inputStream.Length;

		inputStream.Seek(0, SeekOrigin.Begin);

		// Read archive header
		byte[] buffer = ArrayPool<byte>.Shared.Rent(HEADER_SIZE);
		Memory<byte> headerData = buffer.AsMemory(0, HEADER_SIZE);

		await inputStream.ReadExactlyAsync(headerData).ConfigureAwait(false);

		ZunCrypt.Decrypt(headerData.Span[4..], key: 0x1b, step: 0x37, block: HEADER_SIZE - 4, limit: 0x400);

		Header header = MemoryMarshal.Read<Header>(headerData.Span);

		ArrayPool<byte>.Shared.Return(buffer);

		if (header.Magic != HEADER_MAGIC)
		{
			throw new InvalidDataException($"Magic string not recognized (expected PBGZ, read {Encoding.UTF8.GetString(BitConverter.GetBytes(header.Magic))}).");
		}

		header.EntryCount -= 123456;
		int entryCount = (int)header.EntryCount;

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		header.Offset -= 345678;
		header.Size -= 567891;

		// Reads entry headers
		inputStream.Seek(header.Offset, SeekOrigin.Begin);

		int compressedSize = (int)(fileSize - header.Offset);
		buffer = ArrayPool<byte>.Shared.Rent(compressedSize);

		await inputStream.ReadExactlyAsync(buffer.AsMemory(0, compressedSize)).ConfigureAwait(false);

		ZunCrypt.Decrypt(buffer.AsSpan(0, compressedSize), key: 0x3e, step: 0x9b, block: 0x80, limit: 0x400);

		MemoryStream entryHeadersDataStream = new((int)header.Size);
		await using (entryHeadersDataStream.ConfigureAwait(false))
		{
			MemoryStream compressedDataStream = new(buffer, 0, compressedSize, writable: false);
			await using (compressedDataStream.ConfigureAwait(false))
			{
				LZSS.Decompress(compressedDataStream, entryHeadersDataStream, (int)header.Size);
			}

			ArrayPool<byte>.Shared.Return(buffer);

			List<ZunEntry> entries = ReadEntryHeaders(entryCount, compressedSize, fileSize, entryHeadersDataStream.GetBuffer().AsSpan(0, (int)header.Size), extensionFilters);

			return new PBGZ(game, offset: 0, entries, inputStream, ArchiveInitializationMode.Extraction);
		}
	}

	private static List<ZunEntry> ReadEntryHeaders(int entryCount, int compressedSize, int fileSize, ReadOnlySpan<byte> entryHeadersData, string[]? extensionFilters)
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
			entryHeadersDataPtr += entry.FileName.Length + 1;

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

		previousEntry!.CompressedSize = fileSize - compressedSize - previousEntry.Offset;

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
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);
		_stream.ReadExactly(buffer.AsSpan(0, entry.CompressedSize));

		using MemoryStream entryDataStream = new(entry.Size);
		using MemoryStream compressedDataStream = new(buffer, 0, entry.CompressedSize, writable: false);

		LZSS.Decompress(compressedDataStream, entryDataStream, entry.Size);

		entryDataStream.Seek(0, SeekOrigin.Begin);

		Span<byte> magic = buffer.AsSpan(0, 3);
		entryDataStream.ReadExactly(magic);

		if (!magic.SequenceEqual("edz"u8))
		{
			outputStream.Write(entryDataStream.GetBuffer().AsSpan(0, entry.Size));

			return;
		}

		ArrayPool<byte>.Shared.Return(buffer);

		entry.Size -= 4;

		int cryptParamsIndex = -1;
		char entryType = (char)entryDataStream.ReadByte();
		CryptParams[] cryptParams = _game == Game.IN ? s_cryptParamsTh08 : s_cryptParamsTh09;

		for (int c = 0; c < 8; c++)
		{
			if (cryptParams[c].Type == entryType)
			{
				cryptParamsIndex = c;

				break;
			}
		}

		if (cryptParamsIndex == -1)
		{
			outputStream.Write(entryDataStream.GetBuffer().AsSpan(0, entry.Size));

			return;
		}

		Span<byte> data = entryDataStream.GetBuffer().AsSpan(4, entry.Size);

		ZunCrypt.Decrypt(data, cryptParams[cryptParamsIndex].Key, cryptParams[cryptParamsIndex].Step, cryptParams[cryptParamsIndex].Block, cryptParams[cryptParamsIndex].Limit);

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
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);
		await _stream.ReadExactlyAsync(buffer.AsMemory(0, entry.CompressedSize)).ConfigureAwait(false);

		MemoryStream entryDataStream = new(entry.Size);
		await using (entryDataStream.ConfigureAwait(false))
		{
			MemoryStream compressedDataStream = new(buffer, 0, entry.CompressedSize, writable: false);
			await using (compressedDataStream.ConfigureAwait(false))
			{
				LZSS.Decompress(compressedDataStream, entryDataStream, entry.Size);
			}

			entryDataStream.Seek(0, SeekOrigin.Begin);

			Memory<byte> magic = buffer.AsMemory(0, 3);
			await entryDataStream.ReadExactlyAsync(magic).ConfigureAwait(false);

			if (!magic.Span.SequenceEqual("edz"u8))
			{
				await outputStream.WriteAsync(entryDataStream.GetBuffer().AsMemory(0, entry.Size)).ConfigureAwait(false);

				return;
			}

			ArrayPool<byte>.Shared.Return(buffer);

			entry.Size -= 4;

			int cryptParamsIndex = -1;
			char entryType = (char)entryDataStream.ReadByte();
			CryptParams[] cryptParams = _game == Game.IN ? s_cryptParamsTh08 : s_cryptParamsTh09;

			for (int c = 0; c < 8; c++)
			{
				if (cryptParams[c].Type == entryType)
				{
					cryptParamsIndex = c;

					break;
				}
			}

			if (cryptParamsIndex == -1)
			{
				await outputStream.WriteAsync(entryDataStream.GetBuffer().AsMemory(0, entry.Size)).ConfigureAwait(false);

				return;
			}

			Memory<byte> data = entryDataStream.GetBuffer().AsMemory(4, entry.Size);

			ZunCrypt.Decrypt(data.Span, cryptParams[cryptParamsIndex].Key, cryptParams[cryptParamsIndex].Step, cryptParams[cryptParamsIndex].Block, cryptParams[cryptParamsIndex].Limit);

			await outputStream.WriteAsync(data).ConfigureAwait(false);
		}
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

		int entrySize = entryData.Length;

		if (entrySize == 0)
		{
			throw new ArgumentException($"The entry data is empty.", nameof(entryData));
		}

		using MemoryStream stream = new(entryData, writable: false);
		PackCore(zunEntry, entrySize, stream);
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

		int entrySize = entryData.Length;

		if (entrySize == 0)
		{
			throw new ArgumentException($"The entry data is empty.", nameof(entryData));
		}

		MemoryStream stream = new(entryData, writable: false);
		await using (stream.ConfigureAwait(false))
		{
			await PackAsyncCore(zunEntry, entrySize, stream).ConfigureAwait(false);
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

		int entrySize = (int)inputStream.Length;

		if (entrySize is int.MinValue or > int.MaxValue - 4)
		{
			throw new ArgumentException($"The stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue - 4} max).", nameof(inputStream));
		}
		else if (entrySize == 0)
		{
			throw new ArgumentException($"The stream is empty.", nameof(inputStream));
		}

		PackCore(zunEntry, entrySize, inputStream);
	}

	private void PackCore(ZunEntry entry, int entrySize, Stream inputStream)
	{
		entry.Size = entrySize + 4;

		byte[] data = ArrayPool<byte>.Shared.Rent(entry.Size);
		CryptParams cryptParams = GetCryptParams(entry.FileName);

		"edz"u8.CopyTo(data.AsSpan(0, 3));
		data[3] = (byte)cryptParams.Type;
		inputStream.ReadExactly(data.AsSpan(4, entrySize));

		ZunCrypt.Encrypt(data.AsSpan(4, entrySize), cryptParams.Key, cryptParams.Step, cryptParams.Block, cryptParams.Limit);

		using MemoryStream compressedEntryDataStream = new();
		using MemoryStream entryDataStream = new(data, 0, entry.Size, writable: false);
		entry.CompressedSize = LZSS.Compress(entryDataStream, entry.Size, compressedEntryDataStream);

		ArrayPool<byte>.Shared.Return(data);

		_stream!.Write(compressedEntryDataStream.GetBuffer().AsSpan(0, entry.CompressedSize));

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

		int entrySize = (int)inputStream.Length;

		if (entrySize is int.MinValue or > int.MaxValue - 4)
		{
			throw new ArgumentException($"The stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue - 4} max).", nameof(inputStream));
		}
		else if (entrySize == 0)
		{
			throw new ArgumentException($"The stream is empty.", nameof(inputStream));
		}

		await PackAsyncCore(zunEntry, entrySize, inputStream).ConfigureAwait(false);
	}

	private async ValueTask PackAsyncCore(ZunEntry entry, int entrySize, Stream inputStream)
	{
		entry.Size = entrySize + 4;

		byte[] data = ArrayPool<byte>.Shared.Rent(entry.Size);
		CryptParams cryptParams = GetCryptParams(entry.FileName);

		"edz"u8.CopyTo(data.AsSpan(0, 3));
		data[3] = (byte)cryptParams.Type;
		await inputStream.ReadExactlyAsync(data.AsMemory(4, entrySize)).ConfigureAwait(false);

		ZunCrypt.Encrypt(data.AsSpan(4, entrySize), cryptParams.Key, cryptParams.Step, cryptParams.Block, cryptParams.Limit);

		MemoryStream compressedEntryDataStream = new();
		await using (compressedEntryDataStream.ConfigureAwait(false))
		{
			MemoryStream entryDataStream = new(data, 0, entry.Size, writable: false);
			await using (entryDataStream.ConfigureAwait(false))
			{
				entry.CompressedSize = LZSS.Compress(entryDataStream, entry.Size, compressedEntryDataStream);
			}

			ArrayPool<byte>.Shared.Return(data);

			await _stream!.WriteAsync(compressedEntryDataStream.GetBuffer().AsMemory(0, entry.CompressedSize)).ConfigureAwait(false);
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

		// Padding to satisfy pbgzmlt (not necessary for the games)
		int entryHeadersSize = 4;
		Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + entry.FileName.Length + 1 + (sizeof(uint) * 3), sum => Interlocked.Add(ref entryHeadersSize, sum));

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);

		WriteEntryHeaders(buffer.AsSpan(0, entryHeadersSize));

		using MemoryStream commpressedEntryHeadersStream = new();
		using MemoryStream entryHeadersStream = new(buffer, 0, entryHeadersSize, writable: false);
		int commpressedEntryHeadersSize = LZSS.Compress(entryHeadersStream, entryHeadersSize, commpressedEntryHeadersStream);

		Span<byte> compressedEntryHeadersData = commpressedEntryHeadersStream.GetBuffer().AsSpan(0, commpressedEntryHeadersSize);

		ZunCrypt.Encrypt(compressedEntryHeadersData, key: 0x3e, step: 0x9b, block: 0x80, limit: 0x400);

		_stream!.Write(compressedEntryHeadersData);

		// Write archive header
		Span<byte> headerData = buffer.AsSpan(0, sizeof(uint) * 4);
		Span<uint> header = MemoryMarshal.Cast<byte, uint>(headerData);

		// Magic
		header[0] = HEADER_MAGIC;
		// Entry count
		header[1] = (uint)(_entries.Count + 123456);
		// Entry headers offset
		header[2] = (uint)(_offset + 345678);
		// Entry headers size
		header[3] = (uint)(entryHeadersSize + 567891);

		ZunCrypt.Encrypt(headerData[sizeof(uint)..], key: 0x1b, step: 0x37, block: sizeof(uint) * 3, limit: 0x400);

		_stream.Seek(0, SeekOrigin.Begin);

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

		// Padding to satisfy pbgzmlt (not necessary for the games)
		int entryHeadersSize = 4;
		Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + entry.FileName.Length + 1 + (sizeof(uint) * 3), sum => Interlocked.Add(ref entryHeadersSize, sum));

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entryHeadersSize);

		WriteEntryHeaders(buffer.AsSpan(0, entryHeadersSize));

		using MemoryStream commpressedEntryHeadersStream = new();
		using MemoryStream entryHeadersStream = new(buffer, 0, entryHeadersSize, writable: false);
		int commpressedEntryHeadersSize = LZSS.Compress(entryHeadersStream, entryHeadersSize, commpressedEntryHeadersStream);

		Memory<byte> compressedEntryHeadersData = commpressedEntryHeadersStream.GetBuffer().AsMemory(0, commpressedEntryHeadersSize);

		ZunCrypt.Encrypt(compressedEntryHeadersData.Span, key: 0x3e, step: 0x9b, block: 0x80, limit: 0x400);

		await _stream!.WriteAsync(compressedEntryHeadersData).ConfigureAwait(false);

		// Write archive header
		Header header = new()
		{
			Magic = HEADER_MAGIC,
			Offset = (uint)(_offset + 345678),
			Size = (uint)(entryHeadersSize + 567891),
			EntryCount = (uint)(_entries.Count + 123456)
		};

		Memory<byte> headerData = buffer.AsMemory(0, HEADER_SIZE);
		MemoryMarshal.Write(headerData.Span, in header);

		ZunCrypt.Encrypt(headerData.Span[sizeof(uint)..], key: 0x1b, step: 0x37, block: sizeof(uint) * 3, limit: 0x400);

		_stream.Seek(0, SeekOrigin.Begin);

		await _stream.WriteAsync(headerData).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private void WriteEntryHeaders(Span<byte> entryHeadersData)
	{
		int entryHeadersDataPtr = 0;
		ReadOnlySpan<ZunEntry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
		{
			ZunEntry entry = entries[entryIndex];

			// File name
			if (Utf8.FromUtf16(entry.FileName, entryHeadersData.Slice(entryHeadersDataPtr, entry.FileName.Length), out _, out _, replaceInvalidSequences: false) != OperationStatus.Done)
			{
				throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
			}

			entryHeadersDataPtr += entry.FileName.Length;
			entryHeadersData[entryHeadersDataPtr] = 0x0;
			entryHeadersDataPtr++;

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

		entryHeadersData.Slice(entryHeadersDataPtr, sizeof(uint)).Clear();
	}

	private CryptParams GetCryptParams(string fileName)
	{
		string extension = fileName.Length < 4 ? string.Empty : Path.GetExtension(fileName).ToUpperInvariant();

		return (_game == Game.IN ? s_cryptParamsTh08 : s_cryptParamsTh09)[extension switch
		{
			".ANM" => CRYPT_TYPE_ANM,
			".ECL" => CRYPT_TYPE_ECL,
			".JPG" => CRYPT_TYPE_JPG,
			".MSG" => CRYPT_TYPE_MSG,
			".TXT" => CRYPT_TYPE_TXT,
			".WAV" => CRYPT_TYPE_WAV,
			_ => CRYPT_TYPE_ETC,
		}];
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
		// "PBGZ"
		internal required uint Magic { get; init; }
		// value + 123456
		internal required uint EntryCount { get; set; }
		// value + 345678
		internal required uint Offset { get; set; }
		// value + 567891
		internal required uint Size { get; set; }
	}

	private readonly struct CryptParams
	{
		internal readonly required char Type { get; init; }
		internal readonly required byte Key { get; init; }
		internal readonly required byte Step { get; init; }
		internal readonly required uint Block { get; init; }
		internal readonly required uint Limit { get; init; }
	}
}
