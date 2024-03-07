using System.Runtime.CompilerServices;

namespace Touhou.Extraction.TH75;

/// <summary>
/// Provides static methods to handle encryption and decryption of data from Touhou 7.5. This class cannot be inherited.
/// </summary>
internal static class Crypto
{
	internal static void Crypt(Span<byte> data, byte a, byte b, byte c)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] ^= a;
			a += b;
			b += c;
		}
	}

	/// <summary>
	/// Decrypts or encrypts the entry headers <paramref name="data"/>.
	/// </summary>
	/// <param name="data">The data to encrypt/decrypt.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void CryptEntryHeaders(Span<byte> data) => Crypt(data, 0x64, 0x64, 0x4D);
}
