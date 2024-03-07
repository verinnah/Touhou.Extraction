using System.Buffers;
using System.Runtime.CompilerServices;

namespace Touhou.Extraction.TH105;

/// <summary>
/// Provides static methods to handle encryption and decryption of data from ".cv0" (text data) and ".cv1" (CSV data) files from Touhou 10.5. This class cannot be inherited.
/// </summary>
public static class CV01
{
	/// <summary>
	/// Decrypts the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to decrypt.</param>
	/// <returns>A span containing the decrypted data.</returns>
	/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<byte> Decrypt(ReadOnlySpan<byte> data) => Encrypt(data);

	/// <summary>
	/// Decrypts the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to decrypt.</param>
	/// <param name="outputStream">The stream that will contain the decrypted data.</param>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable, or <paramref name="outputStream"/> is not writable.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Decrypt(Stream inputStream, Stream outputStream) => Encrypt(inputStream, outputStream);

	/// <inheritdoc cref="Decrypt(Stream, Stream)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async ValueTask DecryptAsync(Stream inputStream, Stream outputStream) => await EncryptAsync(inputStream, outputStream).ConfigureAwait(false);

	/// <summary>
	/// Encrypts the contents of the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to encrypt.</param>
	/// <returns>A span containing the encrypted data.</returns>
	/// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
	public static Span<byte> Encrypt(ReadOnlySpan<byte> data)
	{
		if (data.Length < 0)
		{
			throw new ArgumentException($"The input data is empty.", nameof(data));
		}

		Span<byte> outputData = new byte[data.Length];

		data.CopyTo(outputData);

		Crypto.Crypt(outputData, 0x8B, 0x71, 0x95);

		return outputData;
	}

	/// <summary>
	/// Encrypts the contents from <paramref name="inputStream"/> into <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputStream">The stream containing the data to encrypt.</param>
	/// <param name="outputStream">The stream that will contain the encrypted data.</param>
	/// <exception cref="ArgumentNullException"><paramref name="inputStream"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not readable or seekable, or <paramref name="outputStream"/> is not writable.</exception>
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

		Crypto.Crypt(data, 0x8B, 0x71, 0x95);

		outputStream.Write(data);

		ArrayPool<byte>.Shared.Return(buffer);
	}

	/// <inheritdoc cref="Encrypt(Stream, Stream)"/>
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

		Crypto.Crypt(data.Span, 0x8B, 0x71, 0x95);

		await outputStream.WriteAsync(data).ConfigureAwait(false);

		ArrayPool<byte>.Shared.Return(buffer);
	}
}
