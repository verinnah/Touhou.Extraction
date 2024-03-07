using System.Buffers;
using System.Runtime.CompilerServices;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Touhou.Common;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.Crypto;

/// <summary>
/// Reads primitive data types from a stream of RSA-encrypted data. This class cannot be inherited.
/// </summary>
internal sealed class RsaReader
{
	private readonly Stream _stream;
	private readonly Pkcs1Encoding _cipher;

	private static readonly RsaKeyParameters s_publicKeyTh135 = new(isPrivate: false, modulus: new BigInteger(sign: 1,
	[
		0xC7, 0x9A, 0x9E, 0x9B, 0xFB, 0xC2, 0x0C, 0xB0,
		0xC3, 0xE7, 0xAE, 0x27, 0x49, 0x67, 0x62, 0x8A,
		0x78, 0xBB, 0xD1, 0x2C, 0xB2, 0x4D, 0xF4, 0x87,
		0xC7, 0x09, 0x35, 0xF7, 0x01, 0xF8, 0x2E, 0xE5,
		0x49, 0x3B, 0x83, 0x6B, 0x84, 0x26, 0xAA, 0x42,
		0x9A, 0xE1, 0xCC, 0xEE, 0x08, 0xA2, 0x15, 0x1C,
		0x42, 0xE7, 0x48, 0xB1, 0x9C, 0xCE, 0x7A, 0xD9,
		0x40, 0x1A, 0x4D, 0xD4, 0x36, 0x37, 0x5C, 0x89
	]), exponent: BigInteger.ValueOf(65537));
	private static readonly RsaKeyParameters s_publicKeyTh145 = new(isPrivate: false, modulus: new BigInteger(sign: 1,
	[
		0xC6, 0x43, 0xE0, 0x9D, 0x35, 0x5E, 0x98, 0x1D,
		0xBE, 0x63, 0x6D, 0x3A, 0x5F, 0x84, 0x0F, 0x49,
		0xB8, 0xE8, 0x53, 0xF5, 0x42, 0x06, 0x37, 0x3B,
		0x36, 0x25, 0xCB, 0x65, 0xCE, 0xDD, 0x68, 0x8C,
		0xF7, 0x5D, 0x72, 0x0A, 0xC0, 0x47, 0xBD, 0xFA,
		0x3B, 0x10, 0x4C, 0xD2, 0x2C, 0xFE, 0x72, 0x03,
		0x10, 0x4D, 0xD8, 0x85, 0x15, 0x35, 0x55, 0xA3,
		0x5A, 0xAF, 0xC3, 0x4A, 0x3B, 0xF3, 0xE2, 0x37
	]), exponent: BigInteger.ValueOf(65537));
	private static readonly RsaKeyParameters s_publicKeyTh155 = new(isPrivate: false, modulus: new BigInteger(sign: 1,
	[
		0xC4, 0x4D, 0x6A, 0x2F, 0x05, 0x78, 0x2C, 0x0F,
		0xD7, 0x5C, 0x82, 0x97, 0x17, 0x60, 0x91, 0xDD,
		0x6F, 0x83, 0x61, 0x81, 0xD1, 0x4E, 0x06, 0x9B,
		0x94, 0x37, 0xD2, 0x98, 0x4D, 0xE4, 0x7B, 0xBF,
		0x42, 0x60, 0xA7, 0x8F, 0x88, 0xD6, 0xFD, 0xFE,
		0xE1, 0xF5, 0x6A, 0x0B, 0x29, 0xCF, 0x0B, 0xED,
		0x66, 0xF0, 0xAC, 0x4E, 0xD7, 0xEF, 0x96, 0x06,
		0x8B, 0xFA, 0x8E, 0x33, 0x48, 0xA3, 0x02, 0x7D
	]), exponent: BigInteger.ValueOf(65537));

	private const int BLOCK_SIZE = 32;
	private const int RSA_BLOCK_SIZE = 64;

	/// <summary>
	/// Initializes a new instance of the <see cref="RsaReader"/> using the specified <paramref name="stream"/> as the underlying stream.
	/// </summary>
	/// <param name="game">The game from which the data is from.</param>
	/// <param name="stream">The stream from which data will be read from.</param>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidDataException">No valid RSA key was found to decrypt the data from <paramref name="stream"/>.</exception>
	internal RsaReader(Game game, Stream stream)
	{
		Guard.ThrowIfNullOrNotReadableAndSeekable(stream);

		RsaKeyParameters key = game switch
		{
			Game.HM => s_publicKeyTh135,
			Game.ULiL => s_publicKeyTh145,
			Game.AoCF => s_publicKeyTh155,
			_ => throw new ArgumentOutOfRangeException($"The game must be either Touhou 13.5, 14.5 or 15.5 (is {game}).")
		};

		_stream = stream;

		_cipher = new Pkcs1Encoding(new RsaEngine());
		_cipher.Init(forEncryption: false, key);
	}

	/// <summary>
	/// Reads and decrypts the specified amount of bytes from the underlying stream.
	/// </summary>
	/// <param name="size">The number of bytes to read from the stream.</param>
	/// <returns>The decrypted data.</returns>
	internal byte[] Read(int size)
	{
		using MemoryStream tmpStream = new(size);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(RSA_BLOCK_SIZE);
		Span<byte> tmp = buffer.AsSpan(0, RSA_BLOCK_SIZE);

		while (size > BLOCK_SIZE)
		{
			_stream.ReadExactly(tmp);

			tmpStream.Write(_cipher.ProcessBlock(buffer, 0, RSA_BLOCK_SIZE).AsSpan(0, BLOCK_SIZE));

			size -= BLOCK_SIZE;
		}

		_stream.ReadExactly(tmp);

		tmpStream.Write(_cipher.ProcessBlock(buffer, 0, RSA_BLOCK_SIZE).AsSpan(0, size));

		ArrayPool<byte>.Shared.Return(buffer);

		return tmpStream.ToArray();
	}

	/// <inheritdoc cref="Read(int)"/>
	internal async Task<byte[]> ReadAsync(int size)
	{
		MemoryStream tmpStream = new(size);
		await using (tmpStream.ConfigureAwait(false))
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(RSA_BLOCK_SIZE);
			Memory<byte> tmp = buffer.AsMemory(0, RSA_BLOCK_SIZE);

			while (size > BLOCK_SIZE)
			{
				await _stream.ReadExactlyAsync(tmp).ConfigureAwait(false);

				await tmpStream.WriteAsync(_cipher.ProcessBlock(buffer, 0, RSA_BLOCK_SIZE).AsMemory(0, BLOCK_SIZE)).ConfigureAwait(false);

				size -= BLOCK_SIZE;
			}

			await _stream.ReadExactlyAsync(tmp).ConfigureAwait(false);

			await tmpStream.WriteAsync(_cipher.ProcessBlock(buffer, 0, RSA_BLOCK_SIZE).AsMemory(0, size)).ConfigureAwait(false);

			ArrayPool<byte>.Shared.Return(buffer);

			return tmpStream.ToArray();
		}
	}

	/// <summary>
	/// Reads an unsigned <see cref="byte"/> from the stream.
	/// </summary>
	/// <returns>The read unsigned <see cref="byte"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal byte ReadByte() => Read(sizeof(byte))[0];

	/// <inheritdoc cref="ReadByte"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<byte> ReadByteAsync() => (await ReadAsync(sizeof(byte)).ConfigureAwait(false))[0];

	/// <summary>
	/// Reads a signed <see cref="short"/> from the stream.
	/// </summary>
	/// <returns>The read signed <see cref="short"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal short ReadInt16() => SpanHelpers.ReadInt16(Read(sizeof(short)));

	/// <inheritdoc cref="ReadInt16"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<short> ReadInt16Async() => SpanHelpers.ReadInt16(await ReadAsync(sizeof(short)).ConfigureAwait(false));

	/// <summary>
	/// Reads an unsigned <see cref="short"/> from the stream.
	/// </summary>
	/// <returns>The read unsigned <see cref="short"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ushort ReadUInt16() => SpanHelpers.ReadUInt16(Read(sizeof(ushort)));

	/// <inheritdoc cref="ReadUInt16"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<ushort> ReadUInt16Async() => SpanHelpers.ReadUInt16(await ReadAsync(sizeof(ushort)).ConfigureAwait(false));

	/// <summary>
	/// Reads a signed <see cref="int"/> from the stream.
	/// </summary>
	/// <returns>The read signed <see cref="int"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int ReadInt32() => SpanHelpers.ReadInt32(Read(sizeof(int)));

	/// <inheritdoc cref="ReadInt32"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<int> ReadInt32Async() => SpanHelpers.ReadInt32(await ReadAsync(sizeof(int)).ConfigureAwait(false));

	/// <summary>
	/// Reads an unsigned <see cref="int"/> from the stream.
	/// </summary>
	/// <returns>The read unsigned <see cref="int"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal uint ReadUInt32() => SpanHelpers.ReadUInt32(Read(sizeof(uint)));

	/// <inheritdoc cref="ReadUInt32"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<uint> ReadUInt32Async() => SpanHelpers.ReadUInt32(await ReadAsync(sizeof(uint)).ConfigureAwait(false));

	/// <summary>
	/// Reads a signed <see cref="long"/> from the stream.
	/// </summary>
	/// <returns>The read signed <see cref="long"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal long ReadInt64() => SpanHelpers.ReadInt64(Read(sizeof(long)));

	/// <inheritdoc cref="ReadInt64"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<long> ReadInt64Async() => SpanHelpers.ReadInt64(await ReadAsync(sizeof(long)).ConfigureAwait(false));

	/// <summary>
	/// Reads an unsigned <see cref="long"/> from the stream.
	/// </summary>
	/// <returns>The read unsigned <see cref="long"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ulong ReadUInt64() => SpanHelpers.ReadUInt64(Read(sizeof(ulong)));

	/// <inheritdoc cref="ReadUInt64"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async Task<ulong> ReadUInt64Async() => SpanHelpers.ReadUInt64(await ReadAsync(sizeof(ulong)).ConfigureAwait(false));
}
