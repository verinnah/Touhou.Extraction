using System.Runtime.CompilerServices;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Touhou.Common;

namespace Touhou.Extraction.Crypto;

/// <summary>
/// Writes primitive data types into an RSA-encrypted stream. This class cannot be inherited.
/// </summary>
internal sealed class RsaWriter
{
	private readonly Stream _stream;
	private readonly Pkcs1Encoding _cipher;

	private const int BLOCK_SIZE = 32;

	private static readonly RsaKeyParameters s_privateKeyTh135 = new(isPrivate: true, modulus: new BigInteger(sign: 1,
	[
		0xC7, 0x9A, 0x9E, 0x9B, 0xFB, 0xC2, 0x0C, 0xB0,
		0xC3, 0xE7, 0xAE, 0x27, 0x49, 0x67, 0x62, 0x8A, 
		0x78, 0xBB, 0xD1, 0x2C, 0xB2, 0x4D, 0xF4, 0x87,
		0xC7, 0x09, 0x35, 0xF7, 0x01, 0xF8, 0x2E, 0xE5, 
		0x49, 0x3B, 0x83, 0x6B, 0x84, 0x26, 0xAA, 0x42,
		0x9A, 0xE1, 0xCC, 0xEE, 0x08, 0xA2, 0x15, 0x1C, 
		0x42, 0xE7, 0x48, 0xB1, 0x9C, 0xCE, 0x7A, 0xD9,
		0x40, 0x1A, 0x4D, 0xD4, 0x36, 0x37, 0x5C, 0x89
	]), exponent: new BigInteger(sign: 1,
	[
		// Computed by Riatre
		0x34, 0x78, 0x84, 0xF1, 0x64, 0x41, 0x22, 0xAC,
		0xE5, 0x12, 0xE6, 0x49, 0x15, 0x96, 0xC3, 0xE4,
		0xBA, 0xD0, 0x44, 0xB0, 0x87, 0x3E, 0xCE, 0xE5,
		0x52, 0x81, 0x2D, 0x5A, 0x7D, 0x7E, 0x0C, 0x75,
		0x6A, 0x96, 0x7C, 0xE7, 0x5F, 0xDF, 0x7A, 0x21,
		0x86, 0x40, 0x5B, 0x10, 0x43, 0xFD, 0x47, 0xDA,
		0x7B, 0xA7, 0xA4, 0xAC, 0x89, 0x20, 0xA6, 0x93,
		0x91, 0x1C, 0x63, 0x5A, 0x83, 0x8E, 0x08, 0x01
	]));
	private static readonly RsaKeyParameters s_privateKeyTh145 = new(isPrivate: true, modulus: new BigInteger(sign: 1,
	[
		0xC6, 0x43, 0xE0, 0x9D, 0x35, 0x5E, 0x98, 0x1D,
		0xBE, 0x63, 0x6D, 0x3A, 0x5F, 0x84, 0x0F, 0x49,
		0xB8, 0xE8, 0x53, 0xF5, 0x42, 0x06, 0x37, 0x3B,
		0x36, 0x25, 0xCB, 0x65, 0xCE, 0xDD, 0x68, 0x8C,
		0xF7, 0x5D, 0x72, 0x0A, 0xC0, 0x47, 0xBD, 0xFA,
		0x3B, 0x10, 0x4C, 0xD2, 0x2C, 0xFE, 0x72, 0x03,
		0x10, 0x4D, 0xD8, 0x85, 0x15, 0x35, 0x55, 0xA3,
		0x5A, 0xAF, 0xC3, 0x4A, 0x3B, 0xF3, 0xE2, 0x37
	]), exponent: new BigInteger(sign: 1,
	[
		0x47, 0xB8, 0xEE, 0x5C, 0x70, 0x9E, 0x13, 0xB1,
		0x4E, 0xDA, 0x70, 0xFD, 0x18, 0xE8, 0x91, 0x0F,
		0x3E, 0x50, 0xED, 0x6E, 0x5F, 0xC0, 0x17, 0xE2,
		0xD7, 0xA7, 0xBC, 0x78, 0xCB, 0xE8, 0xD4, 0x94,
		0xE6, 0x29, 0x95, 0xDA, 0x55, 0xF7, 0xFF, 0xB0,
		0x04, 0x2F, 0x1A, 0x25, 0xDA, 0x8F, 0x50, 0x59,
		0xE4, 0x70, 0x59, 0x90, 0x23, 0x4A, 0x3E, 0x34,
		0xCB, 0x86, 0xD0, 0x4F, 0xDE, 0xEF, 0x5C, 0xA1
	]));
	private static readonly RsaKeyParameters s_privateKeyTh155 = new(isPrivate: true, modulus: new BigInteger(sign: 1,
	[
		0xC4, 0x4D, 0x6A, 0x2F, 0x05, 0x78, 0x2C, 0x0F,
		0xD7, 0x5C, 0x82, 0x97, 0x17, 0x60, 0x91, 0xDD,
		0x6F, 0x83, 0x61, 0x81, 0xD1, 0x4E, 0x06, 0x9B,
		0x94, 0x37, 0xD2, 0x98, 0x4D, 0xE4, 0x7B, 0xBF,
		0x42, 0x60, 0xA7, 0x8F, 0x88, 0xD6, 0xFD, 0xFE,
		0xE1, 0xF5, 0x6A, 0x0B, 0x29, 0xCF, 0x0B, 0xED,
		0x66, 0xF0, 0xAC, 0x4E, 0xD7, 0xEF, 0x96, 0x06,
		0x8B, 0xFA, 0x8E, 0x33, 0x48, 0xA3, 0x02, 0x7D
	]), exponent: new BigInteger(sign: 1,
	[
		0x43, 0x4F, 0x68, 0x41, 0x87, 0x56, 0x95, 0x85,
		0x39, 0x0B, 0x77, 0x9B, 0xE7, 0x60, 0x2D, 0xA4,
		0x08, 0x40, 0xBC, 0x86, 0x8F, 0x06, 0x7A, 0x2D,
		0x8E, 0xBD, 0x35, 0x71, 0x14, 0x04, 0x6C, 0x89,
		0xA6, 0xAE, 0x01, 0xD0, 0xF0, 0x62, 0x7A, 0x6A,
		0xA9, 0x77, 0xF5, 0x61, 0xAC, 0x4C, 0xC5, 0xE0,
		0x5F, 0x1D, 0x49, 0x3B, 0x3D, 0xD9, 0x7D, 0xEC,
		0x1A, 0xFC, 0x53, 0xA0, 0x0C, 0xB1, 0x54, 0xE1
	]));

	/// <summary>
	/// Initializes a new instance of the <see cref="RsaWriter"/> using the specified <paramref name="stream"/> as the underlying stream.
	/// </summary>
	/// <param name="game">The game from which the data is from.</param>
	/// <param name="stream">The stream into which data will be written.</param>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not writable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	internal RsaWriter(Game game, Stream stream)
	{
		Guard.ThrowIfNullOrNotWritable(stream);

		_stream = stream;

		_cipher = new Pkcs1Encoding(new RsaEngine());
		_cipher.Init(forEncryption: true, game switch
		{
			Game.HM => s_privateKeyTh135,
			Game.ULiL => s_privateKeyTh145,
			Game.AoCF => s_privateKeyTh155,
			_ => throw new ArgumentOutOfRangeException($"The game must be either Touhou 13.5, 14.5 or 15.5 (is {game}).")
		});
	}

	/// <summary>
	/// Writes and encrypts the specified <paramref name="data"/> into the underlying stream.
	/// </summary>
	/// <param name="data">The data to write into the stream.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Write(byte[] data) => Write(data, data.Length);

	/// <summary>
	/// Writes and encrypts the specified <paramref name="data"/> into the underlying stream.
	/// </summary>
	/// <param name="data">The data to write into the stream.</param>
	/// <param name="size">The size of the <paramref name="data"/> to write.</param>
	internal void Write(byte[] data, int size)
	{
		ArgumentNullException.ThrowIfNull(data);
		ArgumentOutOfRangeException.ThrowIfNegative(size);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(size, data.Length);

		int ptr = 0;

		while (size > BLOCK_SIZE)
		{
			_stream.Write(_cipher.ProcessBlock(data, ptr, BLOCK_SIZE));

			ptr += BLOCK_SIZE;
			size -= BLOCK_SIZE;
		}

		_stream.Write(_cipher.ProcessBlock(data, ptr, size));
	}

	/// <inheritdoc cref="Write(byte[])"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteAsync(byte[] data) => await WriteAsync(data, data.Length).ConfigureAwait(false);

	/// <inheritdoc cref="Write(byte[], int)"/>
	internal async ValueTask WriteAsync(byte[] data, int size)
	{
		ArgumentNullException.ThrowIfNull(data);
		ArgumentOutOfRangeException.ThrowIfNegative(size);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(size, data.Length);

		int ptr = 0;

		while (size > BLOCK_SIZE)
		{
			await _stream.WriteAsync(_cipher.ProcessBlock(data, ptr, BLOCK_SIZE)).ConfigureAwait(false);

			ptr += BLOCK_SIZE;
			size -= BLOCK_SIZE;
		}

		await _stream.WriteAsync(_cipher.ProcessBlock(data, ptr, size)).ConfigureAwait(false);
	}

	/// <summary>
	/// Writes an unsigned <see cref="byte"/> into the stream.
	/// </summary>
	/// <param name="data">The unsigned <see cref="byte"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteByte(byte data) => Write([data]);

	/// <inheritdoc cref="WriteByte"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteByteAsync(byte data) => await WriteAsync([data]).ConfigureAwait(false);

	/// <summary>
	/// Writes a signed <see cref="short"/> into the stream.
	/// </summary>
	/// <param name="data">The signed <see cref="short"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteInt16(short data) => Write(BitConverter.GetBytes(data));

	/// <inheritdoc cref="WriteInt16"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteInt16Async(short data) => await WriteAsync(BitConverter.GetBytes(data)).ConfigureAwait(false);

	/// <summary>
	/// Writes an unsigned <see cref="short"/> into the stream.
	/// </summary>
	/// <param name="data">The unsigned <see cref="short"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteUInt16(ushort data) => Write(BitConverter.GetBytes(data));

	/// <inheritdoc cref="WriteUInt16"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteUInt16Async(ushort data) => await WriteAsync(BitConverter.GetBytes(data)).ConfigureAwait(false);

	/// <summary>
	/// Writes a signed <see cref="int"/> into the stream.
	/// </summary>
	/// <param name="data">The signed <see cref="int"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteInt32(int data) => Write(BitConverter.GetBytes(data));

	/// <inheritdoc cref="WriteInt32"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteInt32Async(int data) => await WriteAsync(BitConverter.GetBytes(data)).ConfigureAwait(false);

	/// <summary>
	/// Writes an unsigned <see cref="int"/> into the stream.
	/// </summary>
	/// <param name="data">The unsigned <see cref="int"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteUInt32(uint data) => Write(BitConverter.GetBytes(data));

	/// <inheritdoc cref="WriteUInt32"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteUInt32Async(uint data) => await WriteAsync(BitConverter.GetBytes(data)).ConfigureAwait(false);

	/// <summary>
	/// Writes a signed <see cref="long"/> into the stream.
	/// </summary>
	/// <param name="data">The signed <see cref="long"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteInt64(long data) => Write(BitConverter.GetBytes(data));

	/// <inheritdoc cref="WriteInt64"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteInt64Async(long data) => await WriteAsync(BitConverter.GetBytes(data)).ConfigureAwait(false);

	/// <summary>
	/// Writes an unsigned <see cref="long"/> into the stream.
	/// </summary>
	/// <param name="data">The unsigned <see cref="long"/> to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteUInt64(ulong data) => Write(BitConverter.GetBytes(data));

	/// <inheritdoc cref="WriteUInt64"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal async ValueTask WriteUInt64Async(ulong data) => await WriteAsync(BitConverter.GetBytes(data)).ConfigureAwait(false);
}
