using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Touhou.Extraction.Helpers;
using Touhou.Extraction.Utils;

namespace Touhou.Extraction.TH75;

/// <summary>
/// Represents an archive file containing PNG data from Touhou 7.5. This class cannot be inherited.
/// </summary>
public sealed class PngArchive : IAsyncDisposable, IDisposable
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

	private static readonly DecoderOptions s_contiguousBufferDecoderOptions = new()
	{
		SkipMetadata = true,
		Configuration = new Configuration(new PngConfigurationModule())
		{
			PreferContiguousImageBuffers = true
		}
	};

	private const int PALETTE_SIZE = 512;

	private PngArchive(int offset, List<Entry> entries, Stream stream, ArchiveInitializationMode initializationMode)
	{
		_offset = offset;
		_stream = stream;
		_entries = entries;
		_initializationMode = initializationMode;
	}

	/// <summary>
	/// Prepares an <see cref="PngArchive"/> for packaging using the <paramref name="outputStream"/>.
	/// </summary>
	/// <remarks>The file names of the entries in <paramref name="entryFileNames"/> must be in form of "XX.png", where XX is a number that has at least 2 digits (i.e.: 01.png)</remarks>
	/// <param name="entryCount">The number of entries that will be included in the archive.</param>
	/// <param name="outputStream">The stream that will contain the archive's data.</param>
	/// <param name="entryFileNames">The array containing the file names of the entries to be included in the archive.</param>
	/// <returns>An instance of the <see cref="PngArchive"/> class prepared to package entries into the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entryFileNames"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="entryCount"/> is less than or equal to zero, or is not equal to <paramref name="entryFileNames"/>.Length.</exception>
	public static PngArchive Create(int entryCount, Stream outputStream, string[] entryFileNames)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentNullException.ThrowIfNull(entryFileNames);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
		ArgumentOutOfRangeException.ThrowIfNotEqual(entryCount, entryFileNames.Length);

		List<Entry> entries = new(entryCount);
		CollectionsMarshal.SetCount(entries, entryCount);
		Span<Entry> entriesSpan = CollectionsMarshal.AsSpan(entries);

		byte paletteCount = 0;
		int offset = sizeof(byte);

		foreach (string filePath in entryFileNames)
		{
			string fileName = Path.GetFileName(filePath);
			Entry entry = new()
			{
				FileName = Path.GetFileName(fileName)
			};

			if (fileName.StartsWith("palette", StringComparison.OrdinalIgnoreCase))
			{
				paletteCount++;
				offset += PALETTE_SIZE;

				entriesSpan[^paletteCount] = entry;
			}
			else
			{
				int entryIndex = int.Parse(Path.GetFileNameWithoutExtension(fileName), NumberStyles.None, CultureInfo.InvariantCulture);

				entriesSpan[entryIndex] = entry;
			}
		}

		outputStream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref paletteCount, length: 1)));
		outputStream.Seek(offset, SeekOrigin.Begin);

		return new PngArchive(offset, entries, outputStream, ArchiveInitializationMode.Creation);
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="stream"/> and prepares for extraction.
	/// </summary>
	/// <param name="stream">The stream that contains the archive's data.</param>
	/// <returns>An instance of the <see cref="PngArchive"/> class prepared to extract contents from the archive.</returns>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidDataException">The data from <paramref name="stream"/> is invalid, <paramref name="stream"/> is too big, or it does not have enough data.</exception>
	public static PngArchive Read(Stream stream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		stream.Seek(0, SeekOrigin.Begin);

		Span<byte> data = stackalloc byte[sizeof(uint)];

		int length = (int)stream.Length;

		if (length < ((sizeof(uint) * 4) + (sizeof(byte) * 2)))
		{
			throw new InvalidDataException("The stream does not have enough data to read.");
		}
		else if (length == int.MinValue)
		{
			throw new InvalidDataException($"The stream is too big (is {stream.Length} bytes, {int.MaxValue} max).");
		}

		int ptr = 1;
		List<Entry> entries = [];
		byte paletteCount = (byte)stream.ReadByte();

		for (int paletteIndex = 0; paletteIndex < paletteCount; paletteIndex++, ptr += PALETTE_SIZE)
		{
			entries.Add(new Entry(PALETTE_SIZE, ptr, $"palette{paletteIndex:D3}.pal"));
		}

		for (int entryIndex = 0; ptr < length; entryIndex++)
		{
			int entryOffset = ptr;

			ptr += (sizeof(uint) * 3) + sizeof(byte);

			stream.Seek(ptr, SeekOrigin.Begin);
			stream.ReadExactly(data);

			int pixelDataSize = SpanHelpers.ReadInt32(data);
			ptr += sizeof(uint) + pixelDataSize;

			entries.Add(new Entry(-1, entryOffset, $"{entryIndex:D2}.png"));
		}

		return new PngArchive(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);
	}

	/// <inheritdoc cref="Read(Stream)"/>
	public static async Task<PngArchive> ReadAsync(Stream stream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		stream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(uint));
		Memory<byte> data = buffer.AsMemory(0, sizeof(uint));

		int length = (int)stream.Length;

		if (length < ((sizeof(uint) * 4) + (sizeof(byte) * 2)))
		{
			throw new InvalidDataException("The stream does not have enough data to read.");
		}
		else if (length == int.MinValue)
		{
			throw new InvalidDataException($"The stream is too big (is {stream.Length} bytes, {int.MaxValue} max).");
		}

		int ptr = 1;
		List<Entry> entries = [];
		byte paletteCount = (byte)stream.ReadByte();

		for (int paletteIndex = 0; paletteIndex < paletteCount; paletteIndex++, ptr += PALETTE_SIZE)
		{
			entries.Add(new Entry(PALETTE_SIZE, ptr, $"palette{paletteIndex:D3}.pal"));
		}

		for (int entryIndex = 0; ptr < length; entryIndex++)
		{
			int entryOffset = ptr;

			ptr += (sizeof(uint) * 3) + sizeof(byte);

			stream.Seek(ptr, SeekOrigin.Begin);
			await stream.ReadExactlyAsync(data).ConfigureAwait(false);

			int pixelDataSize = MemoryHelpers.ReadInt32(data);
			ptr += sizeof(uint) + pixelDataSize;

			entries.Add(new Entry(-1, entryOffset, $"{entryIndex:D2}.png"));
		}

		ArrayPool<byte>.Shared.Return(buffer);

		return new PngArchive(offset: 0, entries, stream, ArchiveInitializationMode.Extraction);
	}

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> from the archive.
	/// </summary>
	/// <remarks>Palettes extracted by this method are converted from Bgra5551 to Rgba32.</remarks>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <param name="paletteData">The Rgba32 palette data for indexed images.</param>
	/// <returns>A span containing the data extracted from the given <paramref name="entry"/>.</returns>
	/// <exception cref="InvalidDataException">The pixel data size has invalid value.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	/// <exception cref="ArgumentException"><paramref name="paletteData"/> is empty when a palette is needed.</exception>
	public Span<byte> Extract(Entry entry, ReadOnlySpan<byte> paletteData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		return ExtractCore(entry, paletteData);
	}

	private Span<byte> ExtractCore(Entry entry, ReadOnlySpan<byte> paletteData)
	{
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		if (Path.GetExtension(entry.FileName) == ".pal")
		{
			Span<byte> data = stackalloc byte[PALETTE_SIZE];

			_stream.ReadExactly(data);

			byte[] outputData = new byte[1024];

			PixelOperations<Rgba32>.Instance.FromBgra5551Bytes(Configuration.Default, data, MemoryMarshal.Cast<byte, Rgba32>(outputData), 256);

			return outputData;
		}
		else
		{
			Span<byte> data = stackalloc byte[(sizeof(uint) * 4) + sizeof(byte)];

			_stream.ReadExactly(data);

			uint width = SpanHelpers.ReadUInt32(data);
			int offset = sizeof(uint);

			uint height = SpanHelpers.ReadUInt32(data, offset);
			// Skip width2
			offset += sizeof(uint) * 2;

			byte bpp = data[offset++];

			int pixelDataSize = (int)SpanHelpers.ReadUInt32(data, offset);
			offset += sizeof(uint);

			if (pixelDataSize < 0)
			{
				throw new InvalidDataException($"The data is too big (is {(uint)pixelDataSize} bytes, {int.MaxValue} max).");
			}

			if (bpp == 8 && paletteData.Length != 1024)
			{
				throw new ArgumentException("The palette must be 1024 bytes.", nameof(paletteData));
			}

			byte[] buffer = ArrayPool<byte>.Shared.Rent(pixelDataSize);
			data = buffer.AsSpan(0, pixelDataSize);

			_stream.ReadExactly(data);

			Span<byte> imageData = ConvertImage(data, width, height, bpp, paletteData);

			ArrayPool<byte>.Shared.Return(buffer);

			entry.Size = imageData.Length;

			return imageData;
		}
	}

	/// <inheritdoc cref="Extract(Entry, ReadOnlySpan{byte})"/>
	public async Task<Memory<byte>> ExtractAsync(Entry entry, ReadOnlyMemory<byte> paletteData)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		return await ExtractAsyncCore(entry, paletteData).ConfigureAwait(false);
	}

	private async Task<Memory<byte>> ExtractAsyncCore(Entry entry, ReadOnlyMemory<byte> paletteData)
	{
		_stream!.Seek(entry.Offset, SeekOrigin.Begin);

		if (Path.GetExtension(entry.FileName) == ".pal")
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(PALETTE_SIZE);

			await _stream.ReadExactlyAsync(buffer.AsMemory(0, PALETTE_SIZE)).ConfigureAwait(false);

			byte[] data = new byte[1024];

			PixelOperations<Rgba32>.Instance.FromBgra5551Bytes(Configuration.Default, buffer.AsSpan(0, PALETTE_SIZE), MemoryMarshal.Cast<byte, Rgba32>(data), 256);

			ArrayPool<byte>.Shared.Return(buffer);

			return data;
		}
		else
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent((sizeof(uint) * 4) + sizeof(byte));
			Memory<byte> data = buffer.AsMemory(0, (sizeof(uint) * 4) + sizeof(byte));

			await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

			uint width = MemoryHelpers.ReadUInt32(data);
			int offset = sizeof(uint);

			uint height = MemoryHelpers.ReadUInt32(data, offset);
			// Skip width2
			offset += sizeof(uint) * 2;

			byte bpp = buffer[offset++];

			int pixelDataSize = MemoryHelpers.ReadInt32(data, offset);
			offset += sizeof(uint);

			ArrayPool<byte>.Shared.Return(buffer);

			if (pixelDataSize < 0)
			{
				throw new InvalidDataException($"The data is too big (is {(uint)pixelDataSize} bytes, {int.MaxValue} max).");
			}

			if (bpp == 8 && paletteData.Length != 1024)
			{
				throw new ArgumentException("The palette must be 1024 bytes.", nameof(paletteData));
			}

			buffer = ArrayPool<byte>.Shared.Rent(pixelDataSize);
			data = buffer.AsMemory(0, pixelDataSize);

			await _stream.ReadExactlyAsync(data).ConfigureAwait(false);

			Memory<byte> imageData = ConvertImage(data.Span, width, height, bpp, paletteData.Span);

			ArrayPool<byte>.Shared.Return(buffer);

			entry.Size = imageData.Length;

			return imageData;
		}
	}

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> into the <paramref name="outputStream"/>.
	/// </summary>
	/// <remarks>Palettes extracted by this method are converted from Bgra5551 to Rgba32.</remarks>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <param name="paletteData">The Rgba32 palette data for indexed images.</param>
	/// <param name="outputStream">The stream to which the content's should be written.</param>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable, or <paramref name="paletteData"/> is empty when a palette is needed.</exception>
	public void Extract(Entry entry, ReadOnlySpan<byte> paletteData, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		outputStream.Write(ExtractCore(entry, paletteData));
	}

	/// <inheritdoc cref="Extract(Entry, ReadOnlySpan{byte}, Stream)"/>
	public async ValueTask ExtractAsync(Entry entry, ReadOnlyMemory<byte> paletteData, Stream outputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotExtractionInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		await outputStream.WriteAsync(await ExtractAsyncCore(entry, paletteData).ConfigureAwait(false)).ConfigureAwait(false);
	}

	/// <summary>
	/// Reads the <paramref name="data"/> and writes it into the archive.
	/// </summary>
	/// <param name="entry">The entry in the archive.</param>
	/// <param name="data">The array from which the entry data will be read.</param>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for packaging.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="data"/> is <see langword="null"/>.</exception>
	/// <exception cref="NotSupportedException">The method is called on an image with other than 24 or 32 bpp and RGB(A) colors.</exception>
	/// <exception cref="InvalidDataException"><paramref name="data"/> is not 1024 bytes when <paramref name="entry"/> is a palette.</exception>
	public void Pack(Entry entry, byte[] data)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(data);

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
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not writable or is too big.</exception>
	/// <exception cref="NotSupportedException">The method is called on an image with other than 24 or 32 bpp and RGB(A) colors.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="inputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidDataException"><paramref name="inputStream"/> is not 1024 bytes when <paramref name="entry"/> is a palette.</exception>
	public void Pack(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		long length = inputStream.Length;

		if (length > int.MaxValue)
		{
			throw new ArgumentException($"The stream is too big (is {length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}

		PackCore(entry, inputStream);
	}

	private void PackCore(Entry entry, Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);

		if (Path.GetExtension(entry.FileName) == ".pal")
		{
			if (inputStream.Length != 1024)
			{
				throw new InvalidDataException("A palette must be 1024 bytes.");
			}

			int paletteIndex = int.Parse(entry.FileName.AsSpan(7, 3), NumberStyles.None, CultureInfo.InvariantCulture);

			entry.Size = PALETTE_SIZE;
			entry.Offset = 1 + (PALETTE_SIZE * paletteIndex);

			Span<byte> paletteData = stackalloc byte[1024];
			Span<byte> packedData = stackalloc byte[PALETTE_SIZE];

			inputStream.ReadExactly(paletteData);

			PixelOperations<Bgra5551>.Instance.FromBgra32Bytes(Configuration.Default, paletteData, MemoryMarshal.Cast<byte, Bgra5551>(packedData), 256);

			_stream!.Seek(entry.Offset, SeekOrigin.Begin);
			_stream.Write(packedData);
		}
		else
		{
			_offset = (int)_stream!.Seek(0, SeekOrigin.End);
			entry.Offset = _offset;

			using Image<Bgra32> image = Image.Load<Bgra32>(s_contiguousBufferDecoderOptions, inputStream);

			byte bpp = image.Metadata.GetPngMetadata().ColorType switch
			{
				PngColorType.Rgb or PngColorType.RgbWithAlpha => 32,
				_ => throw new NotSupportedException("Only packaging of 24 and 32 bpp images with RGB(A) colors are supported.")
			};

			image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> pixelMemory);
			ReadOnlySpan<Bgra32> pixelData = pixelMemory.Span;

			uint width = (uint)image.Width;
			uint height = (uint)image.Height;
			uint pixelDataSize = GetPixelDataSize(pixelData);
			int outputDataSize = (sizeof(uint) * 4) + sizeof(byte) + (int)pixelDataSize;

			byte[] buffer = ArrayPool<byte>.Shared.Rent(outputDataSize);

			// width
			MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), width);
			int ptr = sizeof(uint);

			// height
			MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), height);
			ptr += sizeof(uint);

			// width2
			MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), width);
			ptr += sizeof(uint);

			// bpp
			buffer[ptr++] = bpp;

			// pixelDataSize
			MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), pixelDataSize);
			ptr += sizeof(uint);

			// pixel data
			PackPixels(pixelData, buffer.AsSpan(ptr, (int)pixelDataSize));

			_stream.Write(buffer.AsSpan(0, outputDataSize));

			ArrayPool<byte>.Shared.Return(buffer);

			entry.Size = outputDataSize;
			_offset += outputDataSize;
		}
	}

	/// <inheritdoc cref="Pack(Entry, Stream)"/>
	public async ValueTask PackAsync(Entry entry, Stream inputStream)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		Guard.ThrowIfNotCreationInitialized(_initializationMode);

		ArgumentNullException.ThrowIfNull(entry);

		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);

		long length = inputStream.Length;

		if (length > int.MaxValue)
		{
			throw new ArgumentException($"The stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}

		await PackAsyncCore(entry, inputStream).ConfigureAwait(false);
	}

	private async ValueTask PackAsyncCore(Entry entry, Stream inputStream)
	{
		inputStream.Seek(0, SeekOrigin.Begin);

		if (Path.GetExtension(entry.FileName) == ".pal")
		{
			if (inputStream.Length != 1024)
			{
				throw new InvalidDataException("A palette must be 1024 bytes.");
			}

			int paletteIndex = int.Parse(entry.FileName.AsSpan(7, 3), NumberStyles.None, CultureInfo.InvariantCulture);

			entry.Size = PALETTE_SIZE;
			entry.Offset = 1 + (PALETTE_SIZE * paletteIndex);

			byte[] buffer = ArrayPool<byte>.Shared.Rent(1024 + PALETTE_SIZE);

			await inputStream.ReadExactlyAsync(buffer.AsMemory(0, 1024)).ConfigureAwait(false);

			PixelOperations<Bgra5551>.Instance.FromBgra32Bytes(Configuration.Default, buffer.AsSpan(0, 1024), MemoryMarshal.Cast<byte, Bgra5551>(buffer.AsSpan(1024, PALETTE_SIZE)), 256);

			_stream!.Seek(entry.Offset, SeekOrigin.Begin);
			await _stream.WriteAsync(buffer.AsMemory(1024, PALETTE_SIZE)).ConfigureAwait(false);
		}
		else
		{
			_offset = (int)_stream!.Seek(0, SeekOrigin.End);
			entry.Offset = _offset;

			using Image<Bgra32> image = await Image.LoadAsync<Bgra32>(s_contiguousBufferDecoderOptions, inputStream).ConfigureAwait(false);

			byte bpp = image.Metadata.GetPngMetadata().ColorType switch
			{
				PngColorType.Rgb or PngColorType.RgbWithAlpha => 32,
				_ => throw new NotSupportedException("Only packaging of 24 and 32 bpp images with RGB(A) colors are supported.")
			};

			image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> pixelData);

			uint width = (uint)image.Width;
			uint height = (uint)image.Height;
			uint pixelDataSize = GetPixelDataSize(pixelData.Span);
			int outputDataSize = (sizeof(uint) * 4) + sizeof(byte) + (int)pixelDataSize;

			byte[] buffer = ArrayPool<byte>.Shared.Rent(outputDataSize);

			// width
			MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), width);
			int ptr = sizeof(uint);

			// height
			MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), height);
			ptr += sizeof(uint);

			// width2
			MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), width);
			ptr += sizeof(uint);

			// bpp
			buffer[ptr++] = bpp;

			// pixelDataSize
			MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), pixelDataSize);
			ptr += sizeof(uint);

			// pixel data
			PackPixels(pixelData.Span, buffer.AsSpan(ptr, (int)pixelDataSize));

			await _stream.WriteAsync(buffer.AsMemory(0, outputDataSize)).ConfigureAwait(false);

			ArrayPool<byte>.Shared.Return(buffer);

			entry.Size = outputDataSize;
			_offset += outputDataSize;
		}
	}

	private static byte[] ConvertImage(ReadOnlySpan<byte> data, uint width, uint height, byte bpp, ReadOnlySpan<byte> paletteData)
	{
		PngEncoder pngEncoder;
		Span<Bgra32> pixelData;
		uint pixelsCount = width * height;

		if (pixelsCount == 0)
		{
			throw new InvalidDataException("The width or the height cannot be zero.");
		}
		else
		{
			pixelData = new Bgra32[pixelsCount];

			switch (bpp)
			{
				case 32 or 24:
					for (int c = 0, offset = 0; c < pixelsCount; offset += 4)
					{
						uint length = SpanHelpers.ReadUInt32(data, offset);
						offset += sizeof(uint);

						if (length > pixelsCount)
						{
							throw new InvalidDataException("The number of repeating pixels cannot be higher than the total number of pixels.");
						}

						Bgra32 color = new(b: data[offset + 0], g: data[offset + 1], r: data[offset + 2], a: bpp == 32 ? data[offset + 3] : (byte)0xFF);

						for (uint i = 0; i < length; i++)
						{
							pixelData[c++] = color;
						}
					}

					pngEncoder = PngUtils.DefaultPngEncoder;

					break;
				case 16:
					for (int c = 0, offset = 0; c < pixelsCount; offset += 2)
					{
						ushort length = SpanHelpers.ReadUInt16(data, offset);
						offset += sizeof(ushort);

						if (length > pixelsCount)
						{
							throw new InvalidDataException("The number of repeating pixels cannot be higher than the total number of pixels.");
						}

						Bgra32 color = new
						(
							b: (byte)((data[offset + 0] & 0x1F) << 3),
							g: (byte)(((data[offset + 0] & 0xE0) >> 2) + ((data[offset + 1] & 0x03) << 6)),
							r: (byte)((data[offset + 1] & 0x7c) << 1),
							a: (byte)(((data[offset + 1] & 0x80) >> 7) * 0xFF)
						);

						for (uint i = 0; i < length; i++)
						{
							pixelData[c++] = color;
						}
					}

					pngEncoder = PngUtils.DefaultPngEncoder;

					break;
				case 8:
					Color[] palette = new Color[256];
					ReadOnlySpan<Rgba32> palettePixels = MemoryMarshal.Cast<byte, Rgba32>(paletteData);

					for (int colorIndex = 0; colorIndex < palette.Length; colorIndex++)
					{
						palette[colorIndex] = palettePixels[colorIndex];
					}

					for (int c = 0, offset = 0; c < pixelsCount; offset++)
					{
						byte length = data[offset++];

						if (length > pixelsCount)
						{
							throw new InvalidDataException("The number of repeating pixels cannot be higher than the total number of pixels.");
						}

						for (int i = 0; i < length; i++)
						{
							pixelData[c++] = palette[data[offset]];
						}
					}

					pngEncoder = PngUtils.Get8bbpPngEncoder(palette);

					break;
				default:
					throw new InvalidDataException("Only images with 32, 24, 16 or 8 bpp are supported.");
			}
		}

		using MemoryStream imageStream = new();
		using Image<Bgra32> image = Image.LoadPixelData<Bgra32>(pixelData, (int)width, (int)height);
		image.SaveAsPng(imageStream, pngEncoder);

		return imageStream.ToArray();
	}

	private static uint GetPixelDataSize(ReadOnlySpan<Bgra32> pixelData)
	{
		uint pixelDataSize = 0;
		Unsafe.SkipInit(out Bgra32 lastColor);

		for (int pixelIndex = 0, length = 0; pixelIndex <= pixelData.Length; pixelIndex++)
		{
			if (pixelIndex == 0)
			{
				lastColor = pixelData[pixelIndex];

				length++;
			}
			else if (pixelIndex < pixelData.Length && length < int.MaxValue && lastColor == pixelData[pixelIndex])
			{
				length++;
			}
			else
			{
				pixelDataSize += sizeof(uint) * 2;

				if (pixelIndex < pixelData.Length)
				{
					lastColor = pixelData[pixelIndex];
					length = 1;
				}
			}
		}

		return pixelDataSize;
	}

	private static void PackPixels(ReadOnlySpan<Bgra32> pixelData, Span<byte> packedData)
	{
		Unsafe.SkipInit(out Bgra32 lastColor);

		for (int pixelIndex = 0, offset = 0, length = 0; pixelIndex <= pixelData.Length; pixelIndex++)
		{
			if (pixelIndex == 0)
			{
				lastColor = pixelData[pixelIndex];

				length++;
			}
			else if (pixelIndex < pixelData.Length && length < int.MaxValue && lastColor == pixelData[pixelIndex])
			{
				length++;
			}
			else
			{
				MemoryMarshal.Write(packedData.Slice(offset, sizeof(uint)), length);
				offset += sizeof(uint);

				MemoryMarshal.Write(packedData.Slice(offset, sizeof(uint)), in lastColor);
				offset += sizeof(uint);

				if (pixelIndex < pixelData.Length)
				{
					lastColor = pixelData[pixelIndex];
					length = 1;
				}
			}
		}
	}

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
