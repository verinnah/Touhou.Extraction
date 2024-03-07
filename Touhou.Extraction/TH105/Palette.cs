using System.Buffers;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Touhou.Extraction.TH105;

/// <summary>
/// Provides static methods to handle extraction and packaging of data from ".pal" (Bgra5551 palette data) files from Touhou 10.5. This class cannot be inherited.
/// </summary>
/// <remarks>After extraction the palette is converted to Rgba32.</remarks>
public static class Palette
{
	private const int PALETTE_SIZE = 513;

	/// <summary>
	/// Extracts the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to extract.</param>
	/// <returns>A span containing the extracted data.</returns>
	/// <exception cref="InvalidDataException"><paramref name="data"/> is not 513 bytes.</exception>
	public static Span<byte> Extract(ReadOnlySpan<byte> data)
	{
		if (data.Length != PALETTE_SIZE)
		{
			throw new InvalidDataException("The palette must be 513 bytes.");
		}
		else if (data[0] != 16)
		{
			throw new InvalidDataException($"The palette must use 16 bpp colors.");
		}

		Span<byte> paletteData = new byte[1024];
		PixelOperations<Rgba32>.Instance.FromBgra5551Bytes(Configuration.Default, data[1..], MemoryMarshal.Cast<byte, Rgba32>(paletteData), 256);

		return paletteData;
	}

	/// <summary>
	/// Extracts the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to extract.</param>
	/// <param name="outputStream">The stream to which the content's should be written.</param>
	/// <exception cref="InvalidDataException"><paramref name="inputStream"/> is not 513 bytes.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable, or <paramref name="outputStream"/> is not writable.</exception>
	public static void Extract(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (inputStream.Length != PALETTE_SIZE)
		{
			throw new InvalidDataException("The palette must be 513 bytes.");
		}

		Span<byte> data = stackalloc byte[PALETTE_SIZE];

		inputStream.Seek(0, SeekOrigin.Begin);
		inputStream.ReadExactly(data);

		if (data[0] != 16)
		{
			throw new InvalidDataException($"The palette must use 16 bpp colors.");
		}

		Span<byte> paletteData = stackalloc byte[1024];
		PixelOperations<Rgba32>.Instance.FromBgra5551Bytes(Configuration.Default, data[1..], MemoryMarshal.Cast<byte, Rgba32>(paletteData), 256);

		outputStream.Write(paletteData);
	}

	/// <inheritdoc cref="Extract(Stream, Stream)"/>
	public static async ValueTask ExtractAsync(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (inputStream.Length != PALETTE_SIZE)
		{
			throw new InvalidDataException("The palette must be 513 bytes.");
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(PALETTE_SIZE + 1024);

		inputStream.Seek(0, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(buffer.AsMemory(0, PALETTE_SIZE)).ConfigureAwait(false);

		if (buffer[0] != 16)
		{
			throw new InvalidDataException($"The palette must use 16 bpp colors.");
		}

		PixelOperations<Rgba32>.Instance.FromBgra5551Bytes(Configuration.Default, buffer.AsSpan(1, PALETTE_SIZE), MemoryMarshal.Cast<byte, Rgba32>(buffer.AsSpan(PALETTE_SIZE, 1024)), 256);

		await outputStream.WriteAsync(buffer.AsMemory(PALETTE_SIZE, 1024)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <summary>
	/// Packages the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to package.</param>
	/// <returns>A span containing the packaged data.</returns>
	/// <exception cref="InvalidDataException"><paramref name="data"/> is not 1024 bytes.</exception>
	public static Span<byte> Pack(ReadOnlySpan<byte> data)
	{
		if (data.Length != 1024)
		{
			throw new InvalidDataException("A palette must be 1024 bytes.");
		}

		using MemoryStream outputStream = new(PALETTE_SIZE);

		PackCore(data, outputStream);

		return outputStream.GetBuffer().AsSpan(0, PALETTE_SIZE);
	}

	/// <summary>
	/// Packages the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to package.</param>
	/// <param name="outputStream">The stream that will contain the packaged data.</param>
	/// <exception cref="InvalidDataException"><paramref name="inputStream"/> is not 1024 bytes.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not writable or is too big.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	public static void Pack(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (inputStream.Length != 1024)
		{
			throw new InvalidDataException("A palette must be 1024 bytes.");
		}

		Span<byte> data = stackalloc byte[1024];

		inputStream.Seek(0, SeekOrigin.Begin);
		inputStream.ReadExactly(data);

		PackCore(data, outputStream);
	}

	private static void PackCore(ReadOnlySpan<byte> data, Stream outputStream)
	{
		Span<byte> packedData = stackalloc byte[PALETTE_SIZE];
		// bpp
		packedData[0] = 16;

		PixelOperations<Bgra5551>.Instance.FromRgba32Bytes(Configuration.Default, data, MemoryMarshal.Cast<byte, Bgra5551>(packedData[1..]), 256);

		outputStream.Write(packedData);
	}

	/// <inheritdoc cref="Pack(Stream, Stream)"/>
	public static async ValueTask PackAsync(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		if (inputStream.Length != 1024)
		{
			throw new InvalidDataException("A palette must be 1024 bytes.");
		}

		inputStream.Seek(0, SeekOrigin.Begin);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(1024 + PALETTE_SIZE);
		// bpp
		buffer[1024] = 16;

		await inputStream.ReadExactlyAsync(buffer.AsMemory(0, 1024)).ConfigureAwait(false);

		PixelOperations<Bgra5551>.Instance.FromRgba32Bytes(Configuration.Default, buffer.AsSpan(0, 1024), MemoryMarshal.Cast<byte, Bgra5551>(buffer.AsSpan(1024 + sizeof(byte), PALETTE_SIZE - 1)), 256);

		await outputStream.WriteAsync(buffer.AsMemory(1024, PALETTE_SIZE)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}
}
