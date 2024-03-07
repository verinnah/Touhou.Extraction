using System.Buffers;
using System.Runtime.InteropServices;
using Touhou.Extraction.Helpers;
using Touhou.Extraction.Utils;

namespace Touhou.Extraction.TH105;

/// <summary>
/// Provides static methods to handle extraction and packaging of data from ".cv3" (wave data) files from Touhou 10.5. This class cannot be inherited.
/// </summary>
public static class CV3
{
	private static readonly ReadOnlyMemory<byte> s_zeroUInt16 = new byte[2] { 0, 0 };

	private const int WAVEFORMATEX_SIZE = (sizeof(uint) * 2) + (sizeof(ushort) * 4);

	/// <summary>
	/// Extracts the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to extract.</param>
	/// <returns>A span containing the extracted data.</returns>
	/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
	public static Span<byte> Extract(ReadOnlySpan<byte> data)
	{
		if (data.Length <= 0)
		{
			throw new ArgumentException("The input data is empty.", nameof(data));
		}

		using MemoryStream outputStream = new(22 + data.Length);

		ExtractCore(data, outputStream);

		return outputStream.GetBuffer().AsSpan(0, (int)outputStream.Length);
	}

	private static void ExtractCore(ReadOnlySpan<byte> data, Stream outputStream)
	{
		Span<byte> waveData = WaveUtils.WriteWave(formatData: data[..WAVEFORMATEX_SIZE], data.Slice(22, SpanHelpers.ReadInt32(data, 18)), checkIfMagicExists: true, out bool shouldUseInputData);

		outputStream.Write(shouldUseInputData ? data : waveData);
	}

	/// <summary>
	/// Extracts the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to extract.</param>
	/// <param name="outputStream">The stream that will contain the extracted data.</param>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable, is too big, or is empty; or <paramref name="outputStream"/> is not writable.</exception>
	public static void Extract(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size == 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
		Span<byte> data = buffer.AsSpan(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		inputStream.ReadExactly(data);

		ExtractCore(data, outputStream);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc cref="Extract(Stream, Stream)"/>
	public static async ValueTask ExtractAsync(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size == 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
		Memory<byte> data = buffer.AsMemory(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

		Memory<byte> waveData = WaveUtils.WriteWave(formatData: data.Span[..WAVEFORMATEX_SIZE], data.Span.Slice(22, MemoryHelpers.ReadInt32(data, 18)), checkIfMagicExists: true, out bool shouldUseInputData);

		await outputStream.WriteAsync(shouldUseInputData ? data : waveData).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <summary>
	/// Packages the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to package.</param>
	/// <returns>A span containing the packaged data.</returns>
	/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
	public static Span<byte> Pack(ReadOnlySpan<byte> data)
	{
		if (data.Length <= 0)
		{
			throw new ArgumentException("The input data is empty.", nameof(data));
		}

		byte[] outputData = new byte[data.Length];
		using MemoryStream outputStream = new(outputData);

		PackCore(data, outputStream);

		return outputData;
	}

	/// <summary>
	/// Packages the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to package.</param>
	/// <param name="outputStream">The stream that will contain the packaged data.</param>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable, is too big, or is empty; or <paramref name="outputStream"/> is not writable.</exception>
	public static void Pack(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size == 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
		Span<byte> data = buffer.AsSpan(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		inputStream.ReadExactly(data);

		PackCore(data, outputStream);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc cref="Pack(Stream, Stream)"/>
	public static async ValueTask PackAsync(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size == 0)
		{
			throw new ArgumentException("The input stream is empty.", nameof(inputStream));
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
		Memory<byte> data = buffer.AsMemory(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

		// WaveFormatEx
		await outputStream.WriteAsync(data.Slice(20, WAVEFORMATEX_SIZE)).ConfigureAwait(false);

		// cbSize (0x0)
		await outputStream.WriteAsync(s_zeroUInt16).ConfigureAwait(false);

		// Wave size
		int waveSize = data.Length - 44;

		await outputStream.WriteAsync(data.Slice(40, sizeof(uint))).ConfigureAwait(false);

		// Wave data
		await outputStream.WriteAsync(data.Slice(44, waveSize)).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	private static void PackCore(ReadOnlySpan<byte> data, Stream outputStream)
	{
		// WaveFormatEx
		outputStream.Write(data.Slice(20, WAVEFORMATEX_SIZE));

		// cbSize (0x0)
		outputStream.Write([0, 0]);

		// Wave size
		int waveSize = data.Length - 44;

		outputStream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref waveSize, 1)));

		// Wave data
		outputStream.Write(data.Slice(44, waveSize));
	}
}
