using Touhou.Extraction.Crypto;

namespace Touhou.Extraction.TH105;

/// <summary>
/// Provides static methods to handle encryption and decryption of data from Touhou 10.5. This class cannot be inherited.
/// </summary>
internal static class Crypto
{
	/// <summary>
	/// Decrypts or encrypts the specified entry data.
	/// </summary>
	/// <param name="data">The entry data.</param>
	/// <param name="offset">The offset of the entry in the archive.</param>
	internal static void CryptEntry(Span<byte> data, uint offset)
	{
		byte key = (byte)((offset >> 1) | 0x23);

		for (int c = 0; c < data.Length; c++)
		{
			data[c] ^= key;
		}
	}

	internal static void Crypt(Span<byte> data, byte key1, byte key2, byte key3)
	{
		for (int c = 0; c < data.Length; c++)
		{
			data[c] ^= key1;
			key1 += key2;
			key2 += key3;
		}
	}

	internal static void CryptEntryHeaders(Span<byte> data, uint size, byte key, byte step1, byte step2)
	{
		MersenneTwister mt = new(seed: 6 + size);

		// Progressive XOR decryption
		for (int c = 0; c < data.Length; c++)
		{
			int ti = c - 1;

			data[c] ^= (byte)(key + (c * step1) + (((ti * ti) + ti) / 2 * step2));
			data[c] ^= (byte)mt.NextUInt32();
		}
	}
}
