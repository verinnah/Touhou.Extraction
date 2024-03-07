using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Touhou.Common;
using Touhou.Extraction.Crypto;

namespace Touhou.Extraction.TH135;

/// <summary>
/// Serves as the basis for the manipulation of TFPK archives. This is an <see langword="abstract"/> class.
/// </summary>
/// <remarks>An instance of <see cref="CodePagesEncodingProvider"/> must be registered with <see cref="Encoding.RegisterProvider"/> before using this class.</remarks>
public abstract partial class TFPK : Archive
{
	/// <inheritdoc/>
	public sealed override IEnumerable<Entry> Entries
	{
		get
		{
			ObjectDisposedException.ThrowIf(_isDisposed, this);

			return _entries!;
		}
	}

	/// <summary>
	/// Gets the TFPK format version being used.
	/// </summary>
	private protected abstract TfpkVersion Version { get; }

	private int _offset;
	private Stream? _stream;
	private bool _isDisposed;
	private List<EntryTh135>? _entries;
	private readonly ArchiveInitializationMode _initializationMode;

	private static readonly ReadOnlyMemory<byte> s_magic = "TFPK"u8.ToArray();

	/// <summary>
	/// Initializes a new instance of the archive in the specified mode.
	/// </summary>
	/// <param name="offset">The offset in which the archive is.</param>
	/// <param name="entries">The list of entries in the archive.</param>
	/// <param name="stream">The stream that contains the archive's data.</param>
	/// <param name="initializationMode">The mode in which the archive will be initialized.</param>
	private protected TFPK(int offset, List<EntryTh135>? entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_stream = stream;
		_offset = offset;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares a <see cref="TFPK"/> archive for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="game">The game this archive is from.</param>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <param name="entriesBasePath">The path to the base directory where the entries are stored in disk.</param>
	/// <returns>An instance of the <see cref="TFPK"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	public static Archive Create(Game game, int entryCount, Stream outputStream, string[] entryFileNames, string entriesBasePath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentException.ThrowIfNullOrWhiteSpace(entriesBasePath);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		TfpkVersion version = game switch
		{
			Game.HM => TfpkVersion.HM,
			Game.ULiL or Game.AoCF => TfpkVersion.ULiL,
			_ => throw new ArgumentOutOfRangeException(nameof(game), "The game must be either Touhou 13.5, 14.5 or 15.5.")
		};

		FnList fnList = FnList.Create(version, loadFileNames: false);
		fnList.AddEntries(entryFileNames);

		uint GetFileNameHash(string path) => fnList.GetFileNameHash(path);

		// Write headers
		int offset = 5;

		outputStream.Seek(offset, SeekOrigin.Begin);

		RsaWriter rsaWriter = new(game, outputStream);

		DirList dirList = new();
		dirList.AddEntries(entriesBasePath, entryFileNames, GetFileNameHash);
		dirList.Write(rsaWriter);

		fnList.Write(rsaWriter);

		List<EntryTh135> entries = new(entryCount);

		EntryList entriesList = EntryList.Create(version, entries);
		entriesList.AddEntries(entriesBasePath, entryFileNames, GetFileNameHash);
		entriesList.Write(rsaWriter);

		offset = (int)outputStream.Position;

		TFPK archive = version switch
		{
			TfpkVersion.HM => new TFPK0(offset, entries, outputStream, ArchiveInitializationMode.Creation),
			_ => new TFPK1(offset, entries, outputStream, ArchiveInitializationMode.Creation)
		};

		Parallel.For(0, entryCount, entryIndex => archive.GetValidatedEntryName(entries[entryIndex].FileName));

		return archive;
	}

	/// <inheritdoc cref="Create(Game, int, Stream, string[], string)"/>
	public static async Task<Archive> CreateAsync(Game game, int entryCount, Stream outputStream, string[] entryFileNames, string entriesBasePath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentException.ThrowIfNullOrWhiteSpace(entriesBasePath);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		TfpkVersion version = game switch
		{
			Game.HM => TfpkVersion.HM,
			Game.ULiL or Game.AoCF => TfpkVersion.ULiL,
			_ => throw new ArgumentOutOfRangeException(nameof(game), "The game must be either Touhou 13.5, 14.5 or 15.5.")
		};

		FnList fnList = FnList.Create(version, loadFileNames: false);
		fnList.AddEntries(entryFileNames);

		uint GetFileNameHash(string path) => fnList.GetFileNameHash(path);

		// Write headers
		int offset = 5;

		outputStream.Seek(offset, SeekOrigin.Begin);

		RsaWriter rsaWriter = new(game, outputStream);

		DirList dirList = new();
		dirList.AddEntries(entriesBasePath, entryFileNames, GetFileNameHash);
		await dirList.WriteAsync(rsaWriter).ConfigureAwait(false);

		await fnList.WriteAsync(rsaWriter).ConfigureAwait(false);

		List<EntryTh135> entries = new(entryCount);

		EntryList entriesList = EntryList.Create(version, entries);
		entriesList.AddEntries(entriesBasePath, entryFileNames, GetFileNameHash);
		await entriesList.WriteAsync(rsaWriter).ConfigureAwait(false);

		offset = (int)outputStream.Position;

		TFPK archive = version switch
		{
			TfpkVersion.HM => new TFPK0(offset, entries, outputStream, ArchiveInitializationMode.Creation),
			_ => new TFPK1(offset, entries, outputStream, ArchiveInitializationMode.Creation)
		};

		Parallel.For(0, entryCount, entryIndex => archive.GetValidatedEntryName(entries[entryIndex].FileName));

		return archive;
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="stream"/> and prepares for extraction.
	/// </summary>
	/// <param name="game">The game this archive is from.</param>
	/// <param name="stream">The stream that contains the archive's data.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only these extensions will be included.</param>
	/// <returns>An instance of the <see cref="TFPK"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="stream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	public static new TFPK Read(Game game, Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		stream.Seek(0, SeekOrigin.Begin);

		Span<byte> magic = stackalloc byte[4];

		stream.ReadExactly(magic);

		if (!magic.SequenceEqual("TFPK"u8))
		{
			throw new InvalidDataException($"Magic string not recognized (expected TFPK, read {Encoding.UTF8.GetString(magic)}).");
		}

		TfpkVersion version = (TfpkVersion)stream.ReadByte();

		TFPK archive = version switch
		{
			TfpkVersion.HM => new TFPK0(offset: 0, null, stream, ArchiveInitializationMode.Extraction),
			TfpkVersion.ULiL => new TFPK1(offset: 0, null, stream, ArchiveInitializationMode.Extraction),
			_ => throw new InvalidDataException($"The version of the archive ({(byte)version}) does not correspond to a valid and supported format version."),
		};
		archive.ReadHeaders(game, options, extensionFilters);

		return archive;
	}

	/// <inheritdoc cref="Read(Game, Stream, ArchiveReadOptions, string[])"/>
	public static new async Task<TFPK> ReadAsync(Game game, Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		stream.Seek(0, SeekOrigin.Begin);

		Memory<byte> magic = new byte[4];

		await stream.ReadExactlyAsync(magic).ConfigureAwait(false);

		if (!magic.Span.SequenceEqual("TFPK"u8))
		{
			throw new InvalidDataException($"Magic string not recognized (expected TFPK, read {Encoding.UTF8.GetString(magic.Span)}).");
		}

		TfpkVersion version = (TfpkVersion)stream.ReadByte();

		TFPK archive = version switch
		{
			TfpkVersion.HM => new TFPK0(offset: 0, null, stream, ArchiveInitializationMode.Extraction),
			TfpkVersion.ULiL => new TFPK1(offset: 0, null, stream, ArchiveInitializationMode.Extraction),
			_ => throw new InvalidDataException($"The version of the archive ({(byte)version}) does not correspond to a valid and supported format version."),
		};
		await archive.ReadHeadersAsync(game, options, extensionFilters).ConfigureAwait(false);

		return archive;
	}

	private void ReadHeaders(Game game, ArchiveReadOptions options, string[]? extensionFilters)
	{
		RsaReader rsaReader = new(game, _stream!);
		uint dirCount = rsaReader.ReadUInt32();

		DirList dirList = new();
		dirList.Read(rsaReader, dirCount);

		FnList fileNames = FnList.Create(Version);
		fileNames.Read(rsaReader, dirCount);

		uint fileCount = rsaReader.ReadUInt32();
		_entries = new List<EntryTh135>((int)fileCount);

		EntryList entries = EntryList.Create(Version, _entries, options, extensionFilters);
		entries.Read(rsaReader, fileCount, fileNames);

		_entries.TrimExcess();

		_offset = (int)_stream!.Position;
	}

	private async ValueTask ReadHeadersAsync(Game game, ArchiveReadOptions options, string[]? extensionFilters)
	{
		RsaReader rsaReader = new(game, _stream!);
		uint dirCount = await rsaReader.ReadUInt32Async().ConfigureAwait(false);

		DirList dirList = new();
		await dirList.ReadAsync(rsaReader, dirCount).ConfigureAwait(false);

		FnList fileNames = await FnList.CreateAsync(Version).ConfigureAwait(false);
		await fileNames.ReadAsync(rsaReader, dirCount).ConfigureAwait(false);

		uint fileCount = await rsaReader.ReadUInt32Async().ConfigureAwait(false);
		_entries = new List<EntryTh135>((int)fileCount);

		EntryList entries = EntryList.Create(Version, _entries, options, extensionFilters);
		await entries.ReadAsync(rsaReader, fileCount, fileNames).ConfigureAwait(false);

		_entries.TrimExcess();

		_offset = (int)_stream!.Position;
	}

	/// <inheritdoc/>
	public sealed override Span<byte> Extract(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		if (entry.Size == 0)
		{
			return [];
		}

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		Span<byte> data = ExtractCore(entryTh135);

		return data;
	}

	private Span<byte> ExtractCore(EntryTh135 entry)
	{
		_stream!.Seek(_offset + entry.Offset, SeekOrigin.Begin);

		byte[] data = new byte[entry.Size];

		_stream.ReadExactly(data);

		Decrypt(data, entry.Key);

		return data;
	}

	/// <inheritdoc/>
	public sealed override async Task<Memory<byte>> ExtractAsync(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		if (entry.Size == 0)
		{
			return Memory<byte>.Empty;
		}

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		Memory<byte> data = await ExtractAsyncCore(entryTh135).ConfigureAwait(false);

		return data;
	}

	private async Task<Memory<byte>> ExtractAsyncCore(EntryTh135 entry)
	{
		_stream!.Seek(_offset + entry.Offset, SeekOrigin.Begin);

		byte[] data = new byte[entry.Size];

		await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

		Decrypt(data, entry.Key);

		return data;
	}

	/// <inheritdoc/>
	public sealed override void Extract(Entry entry, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (entry.Size == 0)
		{
			return;
		}

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		outputStream.Write(ExtractCore(entryTh135));
	}

	/// <inheritdoc/>
	public sealed override async ValueTask ExtractAsync(Entry entry, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (entry.Size == 0)
		{
			return;
		}

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		await outputStream.WriteAsync(await ExtractAsyncCore(entryTh135).ConfigureAwait(false)).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public sealed override void Pack(Entry entry, byte[] entryData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(entryData);

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		using MemoryStream stream = new(entryData, writable: false);
		PackCore(entryTh135, stream);
	}

	/// <inheritdoc/>
	public sealed override async ValueTask PackAsync(Entry entry, byte[] entryData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(entryData);

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		MemoryStream stream = new(entryData, writable: false);
		await using (stream.ConfigureAwait(false))
		{
			await PackAsyncCore(entryTh135, stream).ConfigureAwait(false);
		}
	}

	/// <inheritdoc/>
	public sealed override void Pack(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		PackCore(entryTh135, inputStream);
	}

	private void PackCore(EntryTh135 entry, Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
		Span<byte> data = buffer.AsSpan(0, entry.Size);

		inputStream.ReadExactly(data);

		long offset = _offset + entry.Offset;

		_stream!.Seek(0, SeekOrigin.End);

		if (_stream.Position < offset)
		{
			int fillDataSize = (int)(offset - _stream.Position);

			byte[] tmpBuffer = ArrayPool<byte>.Shared.Rent(fillDataSize);
			Span<byte> fillData = tmpBuffer.AsSpan(0, fillDataSize);

			fillData.Clear();
			_stream.Write(fillData);

			ArrayPool<byte>.Shared.Return(tmpBuffer);
		}

		_stream.Seek(offset, SeekOrigin.Begin);

		Encrypt(data, entry.Key);

		_stream.Write(data);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public sealed override async ValueTask PackAsync(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		if (entry is not EntryTh135 entryTh135)
		{
			throw new ArgumentException("The entry must be from a TFPK (Touhou 13.5 / 14.5 / 15.5) archive.", nameof(entry));
		}

		await PackAsyncCore(entryTh135, inputStream).ConfigureAwait(false);
	}

	private async ValueTask PackAsyncCore(EntryTh135 entry, Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size);
		Memory<byte> data = buffer.AsMemory(0, entry.Size);

		await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

		long offset = _offset + entry.Offset;

		_stream!.Seek(0, SeekOrigin.End);

		if (_stream.Position < offset)
		{
			int fillDataSize = (int)(offset - _stream.Position);

			byte[] tmpBuffer = ArrayPool<byte>.Shared.Rent(fillDataSize);
			Memory<byte> fillData = tmpBuffer.AsMemory(0, fillDataSize);

			fillData.Span.Clear();
			await _stream.WriteAsync(fillData).ConfigureAwait(false);

			ArrayPool<byte>.Shared.Return(tmpBuffer);
		}

		_stream.Seek(offset, SeekOrigin.Begin);

		Encrypt(data.Span, entry.Key);

		await _stream!.WriteAsync(data).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc/>
	public sealed override void Close()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		_stream!.Seek(0, SeekOrigin.Begin);

		_stream.Write("TFPK"u8);
		_stream.WriteByte((byte)Version);
	}

	/// <inheritdoc/>
	public sealed override async ValueTask CloseAsync()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		_stream!.Seek(0, SeekOrigin.Begin);

		await _stream.WriteAsync(s_magic).ConfigureAwait(false);
		_stream.WriteByte((byte)Version);
	}

	/// <summary>
	/// Tries to guess <paramref name="data"/>'s extension based on its contents.
	/// </summary>
	/// <param name="data">The data of the entry to guess.</param>
	/// <returns>The guessed extension of the entry, or <see langword="null"/> if an extension could not be determined.</returns>
	public static string? GuessFileExtension(ReadOnlySpan<byte> data)
	{
		ReadOnlySpan<byte> pngMagic = [0x89, 80, 78, 71]; // "\x89PNG"
		ReadOnlySpan<byte> nutMagic = [0xFA, 0xFA, 82, 73, 81, 83]; // "\xFA\xFARIQS"
		ReadOnlySpan<byte> nhtexHeader = MemoryMarshal.AsBytes((ReadOnlySpan<uint>)
		[
			0x20, 0, 0x10, 0,
			0x20, 0, (uint)(data.Length - 0x30), 0,
			0, 0, 0, 0
		]);

		return data.Length switch
		{
			>= 0x34 when data[..0x30].SequenceEqual(nhtexHeader) && (data[0x30..0x34].SequenceEqual(pngMagic) || data[0x30..0x34].SequenceEqual("DDS "u8)) => ".nhtex",
			>= 12 when data[..4].SequenceEqual("RIFF"u8) && data[8..12].SequenceEqual("SFPL"u8) => ".sfl",
			>= 73 when data[..73].SequenceEqual("#========================================================================"u8) => ".pl",
			>= 6 when data[..6].SequenceEqual(nutMagic) => ".nut",
			>= 4 when data[..4].SequenceEqual("TFBM"u8) || data[..4].SequenceEqual(pngMagic) => ".png",
			>= 4 when data[..4].SequenceEqual("TFCS"u8) => ".csv",
			>= 4 when data[..4].SequenceEqual("DDS "u8) => ".dds",
			>= 4 when data[..4].SequenceEqual("OggS"u8) => ".ogg",
			>= 4 when data[..4].SequenceEqual("eft$"u8) => ".eft",
			>= 4 when data[..4].SequenceEqual("TFWA"u8) => ".wav",
			>= 4 when data[..4].SequenceEqual("IBMB"u8) => ".bmb",
			>= 4 when data[..4].SequenceEqual("ACT1"u8) => ".act",
			>= 4 when data[..4].SequenceEqual("TFPA"u8) => ".bmp",
			>= 2 when data[..2].SequenceEqual("BM"u8) => ".bmp",
			>= 2 when data[..2].SequenceEqual("MZ"u8) => ".dll",
			>= 1 when data[0] is 0x11 or (byte)'{' => ".pat",
			_ => null
		};
	}

	/// <summary>
	/// Decrypts the given <paramref name="data"/> using the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="data">The data to decrypt.</param>
	/// <param name="key">The key to use for decryption.</param>
	/// <exception cref="ArgumentException"><paramref name="key"/> is empty.</exception>
	private protected abstract void Decrypt(Span<byte> data, ReadOnlySpan<uint> key);

	/// <summary>
	/// Encrypts the given <paramref name="data"/> using the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="data">The data to encrypt.</param>
	/// <param name="key">The key to use for encryption.</param>
	/// <exception cref="ArgumentException"><paramref name="key"/> is empty.</exception>
	private protected abstract void Encrypt(Span<byte> data, ReadOnlySpan<uint> key);

	/// <inheritdoc/>
	protected sealed override void Dispose(bool disposing)
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
	protected sealed override async ValueTask DisposeAsyncCore()
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
