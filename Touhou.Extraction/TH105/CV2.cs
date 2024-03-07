using System.Buffers;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Touhou.Extraction.Helpers;
using Touhou.Extraction.Utils;

namespace Touhou.Extraction.TH105;

/// <summary>
/// Provides static methods to handle extraction and packaging of data from ".cv2" (PNG data) files from Touhou 10.5. This class cannot be inherited.
/// </summary>
public static class CV2
{
	private const int PALETTE_SIZE = 1024;
	// Account at least for the size of the PNG 8 byte signature, the IHDR chunk, and one IDAT and the IEND's chunk headers
	private const int MINIMUM_PNG_HEADERS_SIZE = 57;

	/// <summary>
	/// Extracts the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to extract.</param>
	/// <param name="paletteData">The Rgba32 palette data for indexed images.</param>
	/// <returns>A span containing the extracted data.</returns>
	/// <exception cref="ArgumentException"><paramref name="data"/> is empty, or <paramref name="paletteData"/> is empty when a palette is needed.</exception>
	public static Span<byte> Extract(ReadOnlySpan<byte> data, ReadOnlySpan<byte> paletteData)
	{
		if (data.Length <= 0)
		{
			throw new ArgumentException("The input data is empty.", nameof(data));
		}

		using MemoryStream outputStream = new(data.Length + MINIMUM_PNG_HEADERS_SIZE);

		using Image<Bgra32> image = ConvertImage(data, paletteData, out PngEncoder pngEncoder);
		image.SaveAsPng(outputStream, pngEncoder);

		return outputStream.GetBuffer().AsSpan(0, (int)outputStream.Length);
	}

	/// <summary>
	/// Extracts the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to extract.</param>
	/// <param name="paletteStream">The stream containing the Rgba32 palette data for indexed images.</param>
	/// <param name="outputStream">The stream to which the content's should be written.</param>
	/// <exception cref="InvalidDataException"><paramref name="paletteStream"/> is not 1024 bytes.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>, or <paramref name="paletteStream"/> is null when a palette is needed.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable, is too big, or is empty; <paramref name="paletteStream"/> is empty when a palette is needed, or <paramref name="outputStream"/> is not writable.</exception>
	public static void Extract(Stream inputStream, Stream? paletteStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size <= 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		byte[] dataBuffer = ArrayPool<byte>.Shared.Rent(size);
		Span<byte> data = dataBuffer.AsSpan(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		inputStream.ReadExactly(data);

		byte[]? paletteDataBuffer = null;
		Span<byte> paletteData;

		if (paletteStream is not null)
		{
			paletteDataBuffer = ArrayPool<byte>.Shared.Rent(PALETTE_SIZE);
			paletteData = paletteDataBuffer.AsSpan(0, PALETTE_SIZE);

			paletteStream.Seek(0, SeekOrigin.Begin);
			paletteStream.ReadExactly(paletteData);
		}
		else
		{
			paletteData = [];
		}

		using Image<Bgra32> image = ConvertImage(data, paletteData, out PngEncoder pngEncoder);
		image.SaveAsPng(outputStream, pngEncoder);

		ArrayPool<byte>.Shared.Return(dataBuffer);

		if (paletteDataBuffer is not null)
		{
			ArrayPool<byte>.Shared.Return(paletteDataBuffer);
		}
	}

	/// <inheritdoc cref="Extract(Stream, Stream, Stream)"/>
	public static async ValueTask ExtractAsync(Stream inputStream, Stream? paletteStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size <= 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		byte[] dataBuffer = ArrayPool<byte>.Shared.Rent(size);
		Memory<byte> data = dataBuffer.AsMemory(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

		byte[]? paletteDataBuffer = null;
		Memory<byte> paletteData;

		if (paletteStream is not null)
		{
			paletteDataBuffer = ArrayPool<byte>.Shared.Rent(PALETTE_SIZE);
			paletteData = paletteDataBuffer.AsMemory(0, PALETTE_SIZE);

			paletteStream.Seek(0, SeekOrigin.Begin);
			await paletteStream.ReadExactlyAsync(paletteData).ConfigureAwait(false);
		}
		else
		{
			paletteData = Memory<byte>.Empty;
		}

		using Image<Bgra32> image = ConvertImage(data.Span, paletteData.Span, out PngEncoder pngEncoder);
		await image.SaveAsPngAsync(outputStream, pngEncoder).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(dataBuffer);

		if (paletteDataBuffer is not null)
		{
			ArrayPool<byte>.Shared.Return(paletteDataBuffer);
		}
	}

	/// <summary>
	/// Packages the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to package.</param>
	/// <returns>A span containing the packaged data.</returns>
	/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
	/// <exception cref="NotSupportedException">The method is called on an image with other than 24 or 32 bpp and RGB(A) colors.</exception>
	public static Span<byte> Pack(ReadOnlySpan<byte> data)
	{
		if (data.Length <= 0)
		{
			throw new ArgumentException("The input data is empty.", nameof(data));
		}

		using Image<Bgra32> image = Image.Load<Bgra32>(data);

		using MemoryStream outputStream = new(data.Length - MINIMUM_PNG_HEADERS_SIZE);

		PackCore(image, outputStream);

		return outputStream.GetBuffer().AsSpan(0, (int)outputStream.Length);
	}

	/// <summary>
	/// Packages the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to package.</param>
	/// <param name="outputStream">The stream that will contain the packaged data.</param>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not writable or is too big.</exception>
	/// <exception cref="NotSupportedException">The method is called on an image with other than 24 or 32 bpp and RGB(A) colors.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	public static void Pack(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		long length = inputStream.Length;

		if (length > int.MaxValue)
		{
			throw new ArgumentException($"The stream is too big (is {length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (length <= 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		using Image<Bgra32> image = Image.Load<Bgra32>(inputStream);

		PackCore(image, outputStream);
	}

	private static void PackCore(Image<Bgra32> image, Stream outputStream)
	{
		byte bpp = image.Metadata.GetPngMetadata().ColorType switch
		{
			PngColorType.Rgb or PngColorType.RgbWithAlpha => 32,
			_ => throw new NotSupportedException("Only packaging of 24 and 32 bpp images with RGB(A) colors are supported.")
		};
		uint width = (uint)image.Width;
		uint height = (uint)image.Height;
		uint pixelDataSize = width * height * 4;
		int outputDataSize = (sizeof(uint) * 4) + sizeof(byte) + (int)pixelDataSize;

		byte[] buffer = ArrayPool<byte>.Shared.Rent(outputDataSize);

		// bpp
		buffer[0] = bpp;
		int ptr = 1;

		// width2
		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), width);
		ptr += sizeof(uint);

		// height
		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), height);
		ptr += sizeof(uint);

		// width
		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), width);
		ptr += sizeof(uint);

		// unk (always 0x0?)
		buffer.AsSpan(ptr, sizeof(uint)).Clear();
		ptr += sizeof(uint);

		// pixel data
		image.CopyPixelDataTo(buffer.AsSpan(ptr, (int)pixelDataSize));

		outputStream.Write(buffer.AsSpan(0, outputDataSize));

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc cref="Pack(Stream, Stream)"/>
	public static async ValueTask PackAsync(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		long length = inputStream.Length;

		if (length > int.MaxValue)
		{
			throw new ArgumentException($"The stream is too big in size (is {length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (length <= 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		inputStream.Seek(0, SeekOrigin.Begin);

		using Image<Bgra32> image = await Image.LoadAsync<Bgra32>(inputStream).ConfigureAwait(false);

		byte bpp = image.Metadata.GetPngMetadata().ColorType switch
		{
			PngColorType.Rgb or PngColorType.RgbWithAlpha => 32,
			_ => throw new NotSupportedException("Only packaging of 24 and 32 bpp images with RGB(A) colors are supported.")
		};
		uint width = (uint)image.Width;
		uint height = (uint)image.Height;
		uint pixelDataSize = width * height * 4;
		int outputDataSize = (sizeof(uint) * 4) + sizeof(byte) + (int)pixelDataSize;

		byte[] buffer = ArrayPool<byte>.Shared.Rent(outputDataSize);

		// bpp
		buffer[0] = bpp;
		int ptr = 1;

		// width2
		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), width);
		ptr += sizeof(uint);

		// height
		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), height);
		ptr += sizeof(uint);

		// width
		MemoryMarshal.Write(buffer.AsSpan(ptr, sizeof(uint)), width);
		ptr += sizeof(uint);

		// unk (always 0x0?)
		buffer.AsSpan(ptr, sizeof(uint)).Clear();
		ptr += sizeof(uint);

		// pixel data
		image.CopyPixelDataTo(buffer.AsSpan(ptr, (int)pixelDataSize));

		await outputStream.WriteAsync(buffer.AsMemory(0, outputDataSize)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private static Image<Bgra32> ConvertImage(ReadOnlySpan<byte> data, ReadOnlySpan<byte> paletteData, out PngEncoder pngEncoder)
	{
		byte bpp = data[0];
		int width2 = SpanHelpers.ReadInt32(data, 1);
		int height = SpanHelpers.ReadInt32(data, 5);
		int width = SpanHelpers.ReadInt32(data, 9);

		if (height == 0 || width2 == 0)
		{
			throw new InvalidDataException("The width or the height cannot be zero.");
		}

		Image<Bgra32> image;

		switch (bpp)
		{
			case 32 or 24:
				if (width != width2)
				{
					Span<Bgra32> pixelData = new Bgra32[height * width2];

					for (int c = 0, ptr = 17; c < height * width2; c++)
					{
						pixelData[c] = MemoryMarshal.AsRef<Bgra32>(data.Slice(ptr, sizeof(uint)));
						ptr += 4;

						if ((c + 1) % width2 == 0)
						{
							ptr += 4 * (width - width2);
						}
					}

					image = Image.LoadPixelData<Bgra32>(pixelData, width2, height);
				}
				else
				{
					image = Image.LoadPixelData<Bgra32>(data[17..], width2, height);
				}

				pngEncoder = PngUtils.DefaultPngEncoder;

				break;
			case 8:
				{
					if (paletteData.IsEmpty)
					{
						throw new ArgumentException("The palette of colors cannot be empty in an 8 bpp image.", nameof(paletteData));
					}
					else if (paletteData.Length != PALETTE_SIZE)
					{
						throw new InvalidDataException("The palette of colors must be 1024 bytes.");
					}

					Color[] palette = new Color[256];
					ReadOnlySpan<Rgba32> palettePixels = MemoryMarshal.Cast<byte, Rgba32>(paletteData);

					for (int colorIndex = 0; colorIndex < palette.Length; colorIndex++)
					{
						palette[colorIndex] = palettePixels[colorIndex];
					}

					Span<byte> pixelData = new byte[height * width2];

					for (int pixelDataPtr = 0, dataPtr = 17; pixelDataPtr < height * width2; pixelDataPtr += width2, dataPtr += width)
					{
						data.Slice(dataPtr, width2).CopyTo(pixelData[pixelDataPtr..]);
					}

					Bgra32[] colors = new Bgra32[height * width2];

					for (int c = 0; c < colors.Length; c++)
					{
						colors[c] = palette[pixelData[c]];
					}

					image = Image.LoadPixelData<Bgra32>(colors, width2, height);
					pngEncoder = PngUtils.Get8bbpPngEncoder(palette);

					break;
				}
			default:
				throw new InvalidDataException("Only images with 32, 24 or 8 bpp are supported.");
		}

		return image;
	}
}
