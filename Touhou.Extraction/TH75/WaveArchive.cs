using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Touhou.Extraction.Helpers;
using Touhou.Extraction.Utils;

namespace Touhou.Extraction.TH75;

/// <summary>
/// Represents an archive file containing wave data from Touhou 7.5. This class cannot be inherited.
/// </summary>
public sealed class WaveArchive : IAsyncDisposable, IDisposable
{
	/// <summary>
	/// Gets the entries of the files contained in the archive.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The property is accessed after the object was disposed of.</exception>
	public IEnumerable<Entry> Entries
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

	private WaveArchive(int offset, List<Entry> entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_offset = offset;
		_stream = stream;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares an <see cref="WaveArchive"/> for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <remarks>The file names of the entries in <paramref name="entryFileNames"/> must be in form of "XX.wav", where XX is a number that has at least 2 digits (i.e.: 01.wav)</remarks>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <returns>An instance of the <see cref="WaveArchive"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="entryCount"/> is less than or equal to zero, or is not equal to <paramref name="entryFileNames"/>.Length.</exception>
	public static WaveArchive Create(int entryCount, Stream outputStream, string[] entryFileNames)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		List<Entry> entries = new(entryCount);
		CollectionsMarshal.SetCount(entries, entryCount);
		Span<Entry> entriesSpan = CollectionsMarshal.AsSpan(entries);

		foreach (string fileName in entryFileNames)
		{
			int entryIndex = int.Parse(Path.GetFileNameWithoutExtension(fileName), NumberStyles.None, CultureInfo.InvariantCulture);

			entriesSpan[entryIndex] = new Entry()
			{
				FileName = Path.GetFileName(fileName)
			};
		}

		outputStream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref entryCount, length: 1)));

		return new WaveArchive(offset: sizeof(uint), entries, outputStream, ArchiveInitializationMode.Creation);
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="stream"/> and prepares for extraction.
	/// </summary>
	/// <param name="stream">The stream that contains the archive's data.</param>
	/// <returns>An instance of the <see cref="WaveArchive"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="stream"/> is invalid.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	public static WaveArchive Read(Stream stream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		stream.Seek(0, SeekOrigin.Begin);

		Span<byte> data = stackalloc byte[sizeof(uint)];

		stream.ReadExactly(data);

		int entryCount = SpanHelpers.ReadInt32(data);

		if (entryCount <= 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		List<Entry> entries = new(entryCount);

		for (int entryIndex = 0, ptr = sizeof(uint); entryIndex < entryCount; entryIndex++)
		{
			int waveSize = 0;
			int entryOffset = ptr++;

			if (stream.ReadByte() == 1)
			{
				stream.ReadExactly(data);

				waveSize = SpanHelpers.ReadInt32(data);

				ptr += 22 + waveSize;

				stream.Seek(ptr, SeekOrigin.Begin);
			}

			entries.Add(new Entry(44 + waveSize, entryOffset, $"{entryIndex:D2}.wav"));
		}

		WaveArchive archive = new(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);

		return archive;
	}

	/// <inheritdoc cref="Read(Stream)"/>
	public static async Task<WaveArchive> ReadAsync(Stream stream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		stream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(uint));
		Memory<byte> data = buffer.AsMemory(0, sizeof(uint));

		await stream.ReadExactlyAsync(data).ConfigureAwait(false);

		int entryCount = MemoryHelpers.ReadInt32(data);

		if (entryCount == 0)
		{
			throw new InvalidDataException("There are no entries in the archive.");
		}

		List<Entry> entries = new(entryCount);

		for (int entryIndex = 0, ptr = sizeof(uint); entryIndex < entryCount; entryIndex++)
		{
			int waveSize = 0;
			int entryOffset = ptr++;

			if (stream.ReadByte() == 1)
			{
				await stream.ReadExactlyAsync(data).ConfigureAwait(false);

				waveSize = MemoryHelpers.ReadInt32(data);

				ptr += 22 + waveSize;

				stream.Seek(ptr, SeekOrigin.Begin);
			}

			entries.Add(new Entry(44 + waveSize, entryOffset, $"{entryIndex:D2}.wav"));
		}

		WaveArchive archive = new(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);

		ArrayPool<byte>.Shared.Return(buffer);

		return archive;
	}

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> from the archive.
	/// </summary>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <returns>A span containing the data extracted from the given <paramref name="entry"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	public Span<byte> Extract(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		return ExtractCore(entry);
	}

	private Span<byte> ExtractCore(Entry entry)
	{
		if (entry.Size <= 44)
		{
			byte[] emptyWaveData =
			[
				// RIFF header
				0x52, 0x49, 0x46, 0x46, // ChunkID = "RIFF"
				0x24, 0x00, 0x00, 0x00, // ChunkSize = 36
				0x57, 0x41, 0x56, 0x45, // Format = "WAVE"
				// fmt chunk
				0x66, 0x6d, 0x74, 0x20, // Subchunk1ID = "fmt "
				0x10, 0x00, 0x00, 0x00, // Subchunk1Size = 16
				0x01, 0x00,             // wFormatTag = 1
				0x01, 0x00,             // nChannels = 1
				0x44, 0xAC, 0x00, 0x00, // nSamplesPerSec = 44100
				0x88, 0x58, 0x01, 0x00, // nAvgBytesPerSec = 88200
				0x02, 0x00,             // nBlockAlign = 2
				0x10, 0x00,             // wBitsPerSample = 16
				// data subchunk
				0x64, 0x61, 0x74, 0x61, // Subchunk2ID = "data"
				0x00, 0x00, 0x00, 0x00, // Subchunk2Size = 0
			];

			return emptyWaveData;
		}

		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		Span<byte> data = new byte[entry.Size];

		_stream.ReadExactly(data);

		return ConvertWave(entry, data);
	}

	/// <inheritdoc cref="Extract(Entry)"/>
	public async Task<Memory<byte>> ExtractAsync(Entry entry)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		return await ExtractAsyncCore(entry).ConfigureAwait(false);
	}

	private async Task<Memory<byte>> ExtractAsyncCore(Entry entry)
	{
		if (entry.Size <= 44)
		{
			byte[] emptyWaveData =
			[
				// RIFF header
				0x52, 0x49, 0x46, 0x46, // ChunkID = "RIFF"
				0x24, 0x00, 0x00, 0x00, // ChunkSize = 36
				0x57, 0x41, 0x56, 0x45, // Format = "WAVE"
				// fmt chunk
				0x66, 0x6d, 0x74, 0x20, // Subchunk1ID = "fmt "
				0x10, 0x00, 0x00, 0x00, // Subchunk1Size = 16
				0x01, 0x00,             // wFormatTag = 1
				0x01, 0x00,             // nChannels = 1
				0x44, 0xAC, 0x00, 0x00, // nSamplesPerSec = 44100
				0x88, 0x58, 0x01, 0x00, // nAvgBytesPerSec = 88200
				0x02, 0x00,             // nBlockAlign = 2
				0x10, 0x00,             // wBitsPerSample = 16
				// data subchunk
				0x64, 0x61, 0x74, 0x61, // Subchunk2ID = "data"
				0x00, 0x00, 0x00, 0x00, // Subchunk2Size = 0
			];

			return emptyWaveData;
		}

		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		Memory<byte> data = new byte[entry.Size];

		await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

		return ConvertWave(entry, data.Span);
	}

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> into the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <param name="outputStream">The stream to which the content's should be written.</param>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	public void Extract(Entry entry, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		outputStream.Write(ExtractCore(entry));
	}

	/// <inheritdoc cref="Extract(Entry, Stream)"/>
	public async ValueTask ExtractAsync(Entry entry, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		await outputStream.WriteAsync(await ExtractAsyncCore(entry).ConfigureAwait(false)).ConfigureAwait(false);
	}

	/// <summary>
	/// Reads the <paramref name="data"/> and writes it into the archive.
	/// </summary>
	/// <param name="entry">The entry in the archive.</param>
	/// <param name="data">The array from which the entry data will be read.</param>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for packaging.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="data"/> is <see langword="null"/>.</exception>
	public void Pack(Entry entry, byte[] data)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(data);

		entry.Size = data.Length;

		using MemoryStream stream = new(data, writable: false);
		PackCore(entry, stream);
	}

	/// <inheritdoc cref="Pack(Entry, byte[])"/>
	public async ValueTask PackAsync(Entry entry, byte[] data)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(data);

		entry.Size = data.Length;

		MemoryStream stream = new(data, writable: false);
		await using (stream.ConfigureAwait(false))
		{
			await PackAsyncCore(entry, stream).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Reads the entry data from <paramref name="inputStream"/> and writes it into the archive.
	/// </summary>
	/// <param name="entry">The entry in the archive.</param>
	/// <param name="inputStream">The stream from which the entry data will be read.</param>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for packaging.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not writable or is too big in size.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="inputStream"/> is <see langword="null"/>.</exception>
	public void Pack(Entry entry, Stream inputStream)
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

		PackCore(entry, inputStream);
	}

	private void PackCore(Entry entry, Stream inputStream)
	{
		entry.Offset = _offset;
		_offset += entry.Size;

		if (entry.Size <= 44)
		{
			// Flag (0x0)
			_stream!.WriteByte(0x0);

			return;
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size - 21);

		// Flag (0x1)
		buffer[0] = 0x1;
		int ptr = sizeof(byte);

		// Wave size
		int waveSize = entry.Size - 44;

		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), waveSize);
		ptr += sizeof(uint);

		// WaveFormatEx
		const int WAVEFORMATEX_SIZE = (sizeof(uint) * 2) + (sizeof(ushort) * 4);

		inputStream.Seek(20, SeekOrigin.Begin);
		inputStream.ReadExactly(buffer.AsSpan(ptr, WAVEFORMATEX_SIZE));

		ptr += WAVEFORMATEX_SIZE;

		// cbSize (0x0)
		buffer.AsSpan(ptr, sizeof(ushort)).Clear();
		ptr += sizeof(ushort);

		// Wave data
		inputStream.Seek(44, SeekOrigin.Begin);
		inputStream.ReadExactly(buffer.AsSpan(ptr, waveSize));

		_stream!.Write(buffer.AsSpan(0, entry.Size - 21));

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc cref="Pack(Entry, Stream)"/>
	public async ValueTask PackAsync(Entry entry, Stream inputStream)
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

		await PackAsyncCore(entry, inputStream).ConfigureAwait(false);
	}

	private async ValueTask PackAsyncCore(Entry entry, Stream inputStream)
	{
		entry.Offset = _offset;
		_offset += entry.Size;

		if (entry.Size <= 44)
		{
			// Flag (0x0)
			_stream!.WriteByte(0x0);

			return;
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(entry.Size - 21);

		// Flag (0x1)
		buffer[0] = 0x1;
		int ptr = sizeof(byte);

		// Wave size
		int waveSize = entry.Size - 44;

		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), waveSize);
		ptr += sizeof(uint);

		// WaveFormatEx
		const int WAVEFORMATEX_SIZE = (sizeof(uint) * 2) + (sizeof(ushort) * 4);

		inputStream.Seek(20, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(buffer.AsMemory(ptr, WAVEFORMATEX_SIZE)).ConfigureAwait(false);

		ptr += WAVEFORMATEX_SIZE;

		// cbSize (0x0)
		buffer.AsSpan(ptr, sizeof(ushort)).Clear();
		ptr += sizeof(ushort);

		// Wave data
		inputStream.Seek(44, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(buffer.AsMemory(ptr, waveSize)).ConfigureAwait(false);

		await _stream!.WriteAsync(buffer.AsMemory(0, entry.Size - 21)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte[] ConvertWave(Entry entry, ReadOnlySpan<byte> data) => WaveUtils.WriteWave(formatData: data.Slice(5, 16), data.Slice(23, entry.Size - 44), checkIfMagicExists: false, out _);

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

	/// <inheritdoc cref="Dispose()"/>
	/// <param name="disposing">Whether managed resources should be disposed of.</param>
	private void Dispose(bool disposing)
	{
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
	public async ValueTask DisposeAsync()
	{
		if (!_isDisposed)
		{
			await _stream!.DisposeAsync().ConfigureAwait(false);

			_stream = null;
			_entries = null;
		}

		Dispose(false);

		GC.SuppressFinalize(this);
	}
}
