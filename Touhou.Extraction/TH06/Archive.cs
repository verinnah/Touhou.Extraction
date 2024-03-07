using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Touhou.Common;
using Touhou.Common.Compression;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH06;

/// <summary>
/// Represents an archive file containing data from Touhou 6/7. This class cannot be inherited.
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
	private protected override ArchiveFileNamesOptions Flags { get; } = ArchiveFileNamesOptions.BaseName;

	private int _offset;
	private Stream? _stream;
	private bool _isDisposed;
	private readonly Game _game;
	private List<ZunEntry>? _entries;
	private readonly ArchiveInitializationMode _initializationMode;

	private static readonly ReadOnlyMemory<byte> s_magicTh06 = Encoding.UTF8.GetBytes("PBG3");
	private static readonly ReadOnlyMemory<byte> s_magicTh07 = Encoding.UTF8.GetBytes("PBG4");

	private const int TH07_HEADER_SIZE = sizeof(uint) * 3;

	private Archive(Game game, int offset, List<ZunEntry> entries, Stream stream, ArchiveInitializationMode initializationMode)
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

		int offset = game == Game.EoSD ? 13 : 16;
		outputStream.Seek(offset, SeekOrigin.Begin);

		List<ZunEntry> entries = new(entryCount);

		Archive archive = new(game, offset, entries, outputStream, ArchiveInitializationMode.Creation);

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
		{
			string fileName = archive.GetValidatedEntryName(entryFileNames[entryIndex]);

			if ((game == Game.EoSD && fileName.Length >= 255) || (game == Game.PCB && fileName.Length >= 260))
			{
				throw new ArgumentException($"The file name \"{fileName}\" is too long (max {(game == Game.EoSD ? "254" : "259")} characters)", nameof(entryFileNames));
			}

			entries.Add(new ZunEntry(fileName, 0, -1, -1, -1));
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

		Span<byte> headerData = stackalloc byte[TH07_HEADER_SIZE];
		Span<byte> magic = headerData[..4];

		inputStream.ReadExactly(magic);

		int offset = 0;
		List<ZunEntry> entries;

		if (game == Game.EoSD)
		{
			if (magic.SequenceEqual("PBG3"u8))
			{
				BitStream stream = new(inputStream);

				int entryCount = (int)stream.ReadTh06UInt32();

				if (entryCount == 0)
				{
					throw new InvalidDataException("There are no entries in the archive.");
				}

				offset = (int)stream.ReadTh06UInt32();

				inputStream.Seek(offset, SeekOrigin.Begin);

				stream = new BitStream(inputStream);

				entries = ReadEntryHeadersTh06(stream, (int)inputStream.Length, entryCount, extensionFilters);
			}
			else
			{
				throw new InvalidDataException($"Magic string \"{Encoding.UTF8.GetString(magic)}\" not recognized (expected \"PBG3\").");
			}
		}
		else if (game == Game.PCB)
		{
			if (magic.SequenceEqual("PBG4"u8))
			{
				inputStream.ReadExactly(headerData);

				ref readonly HeaderTh07 header = ref MemoryMarshal.AsRef<HeaderTh07>((ReadOnlySpan<byte>)headerData);

				int entryCount = (int)header.EntryCount;

				if (entryCount == 0)
				{
					throw new InvalidDataException("There are no entries in the archive.");
				}

				inputStream.Seek(header.Offset, SeekOrigin.Begin);

				using MemoryStream entryHeadersStream = new((int)header.Size);
				LZSS.Decompress(inputStream, entryHeadersStream, (int)header.Size);

				ReadOnlySpan<byte> entryHeadersData = entryHeadersStream.GetBuffer().AsSpan(0, (int)header.Size);

				entries = ReadEntryHeadersTh07((int)inputStream.Length, entryCount, entryHeadersData, extensionFilters);
			}
			else
			{
				throw new InvalidDataException($"Magic string \"{Encoding.UTF8.GetString(magic)}\" not recognized (expected \"PBG4\").");
			}
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(game), $"The game specified ({game}) is not supported by this archive format.");
		}

		return new Archive(game, offset, entries, inputStream, ArchiveInitializationMode.Extraction);
	}

	/// <inheritdoc cref="Read(Game, Stream, ArchiveReadOptions, string[])"/>
	public static new async Task<Archive> ReadAsync(Game game, Stream inputStream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(TH07_HEADER_SIZE);
		Memory<byte> magic = buffer.AsMemory(0, 4);

		await inputStream.ReadExactlyAsync(magic).ConfigureAwait(false);

		int offset = 0;
		List<ZunEntry> entries;

		if (game == Game.EoSD)
		{
			if (magic.Span.SequenceEqual("PBG3"u8))
			{
				ArrayPool<byte>.Shared.Return(buffer);

				BitStream stream = new(inputStream);

				int entryCount = (int)stream.ReadTh06UInt32();

				if (entryCount <= 0)
				{
					throw new InvalidDataException("There are no entries in the archive.");
				}

				offset = (int)stream.ReadTh06UInt32();

				inputStream.Seek(offset, SeekOrigin.Begin);

				stream = new BitStream(inputStream);

				entries = ReadEntryHeadersTh06(stream, (int)inputStream.Length, entryCount, extensionFilters);
			}
			else
			{
				ArrayPool<byte>.Shared.Return(buffer);

				throw new InvalidDataException($"Magic string \"{Encoding.UTF8.GetString(magic.Span)}\" not recognized (expected \"PBG3\").");
			}
		}
		else if (game == Game.PCB)
		{
			if (magic.Span.SequenceEqual("PBG4"u8))
			{
				Memory<byte> headerData = buffer.AsMemory(0, TH07_HEADER_SIZE);

				await inputStream.ReadExactlyAsync(headerData).ConfigureAwait(false);

				HeaderTh07 header = MemoryMarshal.Read<HeaderTh07>(headerData.Span);

				ArrayPool<byte>.Shared.Return(buffer);

				int entryCount = (int)header.EntryCount;

				if (entryCount <= 0)
				{
					throw new InvalidDataException("There are no entries in the archive.");
				}

				inputStream.Seek(header.Offset, SeekOrigin.Begin);

				MemoryStream entryHeadersStream = new((int)header.Size);
				await using (entryHeadersStream.ConfigureAwait(false))
				{
					LZSS.Decompress(inputStream, entryHeadersStream, (int)header.Size);

					ReadOnlyMemory<byte> entryHeadersData = entryHeadersStream.GetBuffer().AsMemory(0, (int)header.Size);

					entries = ReadEntryHeadersTh07((int)inputStream.Length, entryCount, entryHeadersData.Span, extensionFilters);
				}
			}
			else
			{
				ArrayPool<byte>.Shared.Return(buffer);

				throw new InvalidDataException($"Magic string \"{Encoding.UTF8.GetString(magic.Span)}\" not recognized (expected \"PBG4\").");
			}
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(game), $"The game specified ({game}) is not supported by this archive format.");
		}

		return new Archive(game, offset, entries, inputStream, ArchiveInitializationMode.Extraction);
	}

	private static List<ZunEntry> ReadEntryHeadersTh06(BitStream entryHeadersStream, int inputLength, int entryCount, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		ZunEntry? previousEntry = null;
		List<ZunEntry> entries = new(entryCount);
		Span<byte> fileNameBuffer = stackalloc byte[255];

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
		{
			ZunEntry entry = new();

			// These values are unknown, but it seems they can be ignored
			entryHeadersStream.ReadTh06UInt32(); // The same for all entries in an archive
			entryHeadersStream.ReadTh06UInt32(); // Starts at a high value; increases by a random multiple of a thousand per entry

			// Checksum
			entry.Extra = entryHeadersStream.ReadTh06UInt32();

			// Data offset in the archive
			entry.Offset = (int)entryHeadersStream.ReadTh06UInt32();

			if (entry.Offset < 0)
			{
				throw new InvalidDataException($"The entry \"{entry.FileName}\" has an invalid offset ({entry.Offset}).");
			}

			// Uncompressed size
			entry.Size = (int)entryHeadersStream.ReadTh06UInt32();

			if (entry.Size < 0)
			{
				throw new InvalidDataException($"The entry \"{entry.FileName}\" has an invalid size ({entry.Size}).");
			}

			// File name
			entryHeadersStream.ReadTh06String(255, fileNameBuffer);
			entry.FileName = SpanHelpers.ReadString(fileNameBuffer);

			if (previousEntry is not null)
			{
				previousEntry.CompressedSize = entry.Offset - previousEntry.Offset;
			}

			previousEntry = entry;

			if (extensionFilters?.Contains(Path.GetExtension(entry.FileName)) is false)
			{
				continue;
			}

			entries.Add(entry);
		}

		previousEntry!.CompressedSize = inputLength - previousEntry.Offset;

		entries.TrimExcess();

		return entries;
	}

	private static List<ZunEntry> ReadEntryHeadersTh07(int inputLength, int entryCount, ReadOnlySpan<byte> entryHeadersData, string[]? extensionFilters)
	{
		if (extensionFilters is { Length: 0 })
		{
			extensionFilters = null;
		}

		int entryHeadersDataPtr = 0;
		ZunEntry? previousEntry = null;
		List<ZunEntry> entries = new(entryCount);

		for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
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

		previousEntry!.CompressedSize = inputLength - previousEntry.Offset;

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

		byte[] compressedData = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);

		_stream.ReadExactly(compressedData.AsSpan(0, entry.CompressedSize));

		using MemoryStream compressedDataStream = new(compressedData, 0, entry.CompressedSize, writable: false);

		LZSS.Decompress(compressedDataStream, outputStream, entry.Size);

		ArrayPool<byte>.Shared.Return(compressedData);
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

		byte[] compressedData = ArrayPool<byte>.Shared.Rent(entry.CompressedSize);

		await _stream.ReadExactlyAsync(compressedData.AsMemory(0, entry.CompressedSize)).ConfigureAwait(false);

		MemoryStream compressedDataStream = new(compressedData, 0, entry.CompressedSize, writable: false);
		await using (compressedDataStream.ConfigureAwait(false))
		{
			LZSS.Decompress(compressedDataStream, outputStream, entry.Size);
		}

		ArrayPool<byte>.Shared.Return(compressedData);
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
		using MemoryStream compressedStream = new();

		// One of the games might support uncompressed data
		entry.CompressedSize = LZSS.Compress(inputStream, entry.Size, compressedStream);

		ReadOnlySpan<byte> compressedData = compressedStream.GetBuffer().AsSpan(0, entry.CompressedSize);

		if (_game == Game.EoSD)
		{
			CalculateChecksum(entry, compressedData);
		}

		entry.Offset = _offset;

		_stream!.Write(compressedData);
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
		MemoryStream compressedStream = new();
		await using (compressedStream.ConfigureAwait(false))
		{
			// One of the games might support uncompressed data
			entry.CompressedSize = LZSS.Compress(inputStream, entry.Size, compressedStream);

			ReadOnlyMemory<byte> compressedData = compressedStream.GetBuffer().AsMemory(0, entry.CompressedSize);

			if (_game == Game.EoSD)
			{
				CalculateChecksum(entry, compressedData.Span);
			}

			entry.Offset = _offset;

			await _stream!.WriteAsync(compressedData).ConfigureAwait(false);
			_offset += entry.CompressedSize;
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

		if (_game == Game.EoSD)
		{
			BitStream headersStream = new(_stream!);

			WriteEntryHeadersTh06(headersStream);

			// Write header
			_stream!.Seek(0, SeekOrigin.Begin);

			_stream.Write("PBG3"u8);

			headersStream.WriteTh06UInt32((uint)_entries.Count);
			headersStream.WriteTh06UInt32((uint)_offset);
			headersStream.FinishByte();
		}
		else
		{
			int entryHeadersSize = sizeof(uint);
			Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + entry.FileName.Length + 1 + (sizeof(uint) * 3), sum => Interlocked.Add(ref entryHeadersSize, sum));

			Span<byte> buffer = stackalloc byte[260];
			ReadOnlySpan<byte> zeroUInt32 = [0x0, 0x0, 0x0, 0x0];
			ReadOnlySpan<ZunEntry> entries = CollectionsMarshal.AsSpan(_entries);

			using MemoryStream entryHeadersDataStream = new(entryHeadersSize);

			for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
			{
				ZunEntry entry = entries[entryIndex];

				// File name
				if (Utf8.FromUtf16(entry.FileName, buffer[..entry.FileName.Length], out _, out _, replaceInvalidSequences: false) != OperationStatus.Done)
				{
					throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
				}

				buffer[entry.FileName.Length] = 0x0;
				entryHeadersDataStream.Write(buffer[..(entry.FileName.Length + 1)]);

				// Data offset in the archive
				entryHeadersDataStream.Write(BitConverter.GetBytes((uint)entry.Offset));
				// Uncompressed size
				entryHeadersDataStream.Write(BitConverter.GetBytes((uint)entry.Size));
				// Extra (zero)
				entryHeadersDataStream.Write(zeroUInt32);
			}

			entryHeadersDataStream.Write(zeroUInt32);

			entryHeadersDataStream.Seek(0, SeekOrigin.Begin);

			LZSS.Compress(entryHeadersDataStream, entryHeadersSize, _stream!);

			// Write header
			_stream!.Seek(0, SeekOrigin.Begin);

			_stream.Write("PBG4"u8);

			Span<byte> headerData = buffer[..TH07_HEADER_SIZE];
			Span<uint> header = MemoryMarshal.Cast<byte, uint>(headerData);

			// Entry count
			header[0] = (uint)_entries.Count;
			// Entry headers offset
			header[1] = (uint)_offset;
			// Entry headers size
			header[2] = (uint)entryHeadersSize;

			_stream.Write(headerData);
		}
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

		if (_game == Game.EoSD)
		{
			BitStream headersStream = new(_stream!);

			WriteEntryHeadersTh06(headersStream);

			// Write header
			_stream!.Seek(0, SeekOrigin.Begin);

			await _stream.WriteAsync(s_magicTh06).ConfigureAwait(false);

			headersStream.WriteTh06UInt32((uint)_entries.Count);
			headersStream.WriteTh06UInt32((uint)_offset);
			headersStream.FinishByte();
		}
		else
		{
			int entryHeadersSize = sizeof(uint);
			Parallel.ForEach(_entries, static () => 0, static (entry, _, sum) => sum + entry.FileName.Length + 1 + (sizeof(uint) * 3), sum => Interlocked.Add(ref entryHeadersSize, sum));

			byte[] buffer = ArrayPool<byte>.Shared.Rent(260);

			MemoryStream entryHeadersDataStream = new(entryHeadersSize);
			await using (entryHeadersDataStream.ConfigureAwait(false))
			{
				Memory<byte> fileNameBuffer = buffer.AsMemory(0, 260);
				ReadOnlyMemory<byte> zeroUInt32 = new byte[] { 0x0, 0x0, 0x0, 0x0 };

				foreach (ZunEntry entry in _entries)
				{
					// File name
					if (Utf8.FromUtf16(entry.FileName, fileNameBuffer.Span[..entry.FileName.Length], out _, out _, replaceInvalidSequences: false) != OperationStatus.Done)
					{
						throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
					}

					fileNameBuffer.Span[entry.FileName.Length] = 0x0;
					await entryHeadersDataStream.WriteAsync(fileNameBuffer[..(entry.FileName.Length + 1)]).ConfigureAwait(false);

					// Data offset in the archive
					await entryHeadersDataStream.WriteAsync(BitConverter.GetBytes((uint)entry.Offset)).ConfigureAwait(false);
					// Uncompressed size
					await entryHeadersDataStream.WriteAsync(BitConverter.GetBytes((uint)entry.Size)).ConfigureAwait(false);
					// Extra (zero)
					await entryHeadersDataStream.WriteAsync(zeroUInt32).ConfigureAwait(false);
				}

				await entryHeadersDataStream.WriteAsync(zeroUInt32).ConfigureAwait(false);

				entryHeadersDataStream.Seek(0, SeekOrigin.Begin);

				LZSS.Compress(entryHeadersDataStream, entryHeadersSize, _stream!);
			}

			// Write header
			_stream!.Seek(0, SeekOrigin.Begin);

			await _stream.WriteAsync(s_magicTh07).ConfigureAwait(false);

			HeaderTh07 header = new()
			{
				Offset = (uint)_offset,
				Size = (uint)entryHeadersSize,
				EntryCount = (uint)_entries.Count
			};

			Memory<byte> headerData = buffer.AsMemory(0, TH07_HEADER_SIZE);
			MemoryMarshal.Write(headerData.Span, in header);

			await _stream.WriteAsync(headerData).ConfigureAwait(false);

			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	private void WriteEntryHeadersTh06(BitStream stream)
	{
		Span<byte> fileNameBuffer = stackalloc byte[255];
		ReadOnlySpan<ZunEntry> entries = CollectionsMarshal.AsSpan(_entries);

		for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
		{
			ZunEntry entry = entries[entryIndex];

			// These values are unknown, but it seems they can be ignored
			stream.WriteTh06UInt32(0); // The same for all entries in an archive
			stream.WriteTh06UInt32(0); // Starts at a high value; increases by a random multiple of a thousand per entry

			// Extra (checksum)
			stream.WriteTh06UInt32(entry.Extra);
			// Data offset in the archive
			stream.WriteTh06UInt32((uint)entry.Offset);
			// Uncompressed size
			stream.WriteTh06UInt32((uint)entry.Size);

			// File name
			if (Utf8.FromUtf16(entry.FileName, fileNameBuffer[..entry.FileName.Length], out _, out _, replaceInvalidSequences: false) != OperationStatus.Done)
			{
				throw new InvalidOperationException("The entry's file name is not a valid UTF-8 sequence.");
			}

			fileNameBuffer[entry.FileName.Length] = 0x0;
			stream.WriteTh06String(fileNameBuffer[..(entry.FileName.Length + 1)]);
		}

		stream.FinishByte();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CalculateChecksum(ZunEntry entry, ReadOnlySpan<byte> data)
	{
		entry.Extra = 0;

		for (int c = 0; c < entry.CompressedSize; c++)
		{
			entry.Extra += data[c];
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
	private readonly struct HeaderTh07
	{
		internal readonly required uint EntryCount { get; init; }
		internal readonly required uint Offset { get; init; }
		internal readonly required uint Size { get; init; }
	}
}

file static class BitStreamExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadTh06UInt32(this BitStream stream)
	{
		uint size = stream.Read(2);

		return stream.Read((size + 1) * 8);
	}

	internal static void ReadTh06String(this BitStream stream, uint length, Span<byte> data)
	{
		for (int c = 0; length != 0; c++, length--)
		{
			data[c] = (byte)stream.Read(8);

			if (data[c] == 0x0)
			{
				break;
			}
		}
	}

	internal static void WriteTh06UInt32(this BitStream stream, uint value)
	{
		int size = 1;

		if ((value & 0xffffff00) != 0)
		{
			size = 2;

			if ((value & 0xffff0000) != 0)
			{
				size = 3;

				if ((value & 0xff000000) != 0)
				{
					size = 4;
				}
			}
		}

		stream.Write(2, (uint)(size - 1));
		stream.Write(size * 8, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void WriteTh06String(this BitStream stream, ReadOnlySpan<byte> data)
	{
		foreach (byte c in data)
		{
			stream.Write(8, c);
		}
	}
}
