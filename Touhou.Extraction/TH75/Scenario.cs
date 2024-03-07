using System.Buffers;
using System.Runtime.CompilerServices;

namespace Touhou.Extraction.TH75;

/// <summary>
/// Provides static methods to handle encryption and decryption of data from ".sce" (text data) files from Touhou 7.5. This class cannot be inherited.
/// </summary>
public static class Scenario
{
	/// <inheritdoc cref="CardList.Decrypt(ReadOnlySpan{byte})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<byte> Decrypt(ReadOnlySpan<byte> data) => Encrypt(data);

	/// <inheritdoc cref="CardList.Decrypt(Stream, Stream)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Decrypt(Stream inputStream, Stream outputStream) => Encrypt(inputStream, outputStream);

	/// <inheritdoc cref="CardList.Decrypt(Stream, Stream)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async ValueTask DecryptAsync(Stream inputStream, Stream outputStream) => await EncryptAsync(inputStream, outputStream).ConfigureAwait(false);

	/// <inheritdoc cref="CardList.Encrypt(ReadOnlySpan{byte})"/>
	public static Span<byte> Encrypt(ReadOnlySpan<byte> data)
	{
		if (data.Length < 0)
		{
			throw new ArgumentException($"The input data is empty.", nameof(data));
		}

		Span<byte> outputData = new byte[data.Length];

		data.CopyTo(outputData);

		Crypto.Crypt(outputData, 0x63, 0x62, 0x42);

		return outputData;
	}

	/// <inheritdoc cref="CardList.Encrypt(Stream, Stream)"/>
	public static void Encrypt(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size == 0)
		{
			throw new ArgumentException($"The input stream is empty.", nameof(inputStream));
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
		Span<byte> data = buffer.AsSpan(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		inputStream.ReadExactly(data);

		Crypto.Crypt(data, 0x63, 0x62, 0x42);

		outputStream.Write(data);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc cref="CardList.Encrypt(Stream, Stream)"/>
	public static async ValueTask EncryptAsync(Stream inputStream, Stream outputStream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(inputStream);
		Guard.ThrowIfNullOrNotWritable(outputStream);

		int size = (int)inputStream.Length;

		if (size == int.MinValue)
		{
			throw new ArgumentException($"The input stream is too big in size (is {inputStream.Length} bytes, {int.MaxValue} max).", nameof(inputStream));
		}
		else if (size == 0)
		{
			throw new ArgumentException($"The input stream is empty.", nameof(inputStream));
		}

		byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
		Memory<byte> data = buffer.AsMemory(0, size);

		inputStream.Seek(0, SeekOrigin.Begin);
		await inputStream.ReadExactlyAsync(data).ConfigureAwait(false);

		Crypto.Crypt(data.Span, 0x63, 0x62, 0x42);

		await outputStream.WriteAsync(data).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}
}
