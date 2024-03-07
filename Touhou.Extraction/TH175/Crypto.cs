using System.Globalization;
using System.Runtime.InteropServices;

namespace Touhou.Extraction.TH175;

internal static class Crypto
{
	/// <summary>
	/// Encrypts or decrypts the given <paramref name="data"/> at the specified <paramref name="offset"/> in the archive.
	/// </summary>
	/// <param name="data">The data to encrypt or decrypt.</param>
	/// <param name="offset">The offset of the <paramref name="data"/> in the archive.</param>
	internal static void Crypt(Span<byte> data, uint offset)
	{
		int ptr = 0;
		uint key = (uint)data.Length ^ offset;

		while (ptr < data.Length)
		{
			uint xor = 0;
			uint tmpKey = key++;

			for (int c = 0; c < 4; c++)
			{
				long a = tmpKey * 0x5E4789C9L;
				uint b = (uint)((a >> 0x2E) + (a >> 0x3F));
				tmpKey = ((tmpKey - (b * 0xADC8)) * 0xBC8F) + (b * 0xFFFFF2B9);

				if ((int)tmpKey <= 0)
				{
					tmpKey += 0x7FFFFFFF;
				}

				xor = (xor << 8) | (tmpKey & 0xFF);
			}

			if (ptr + 4 <= data.Length)
			{
				ref uint tmp = ref MemoryMarshal.AsRef<uint>(data.Slice(ptr, 4));
				tmp ^= xor;

				ptr += 4;
			}
			else
			{
				ReadOnlySpan<byte> xorBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref xor, 1));

				int left = data.Length - ptr;

				for (int c = 0; c < left; c++)
				{
					data[ptr++] ^= xorBytes[c];
				}
			}
		}
	}

	/// <summary>
	/// Calculates the hash for the specified <paramref name="fileName"/>.
	/// </summary>
	/// <param name="fileName">The file name to calculate the hash of.</param>
	/// <returns>The hash of the specified <paramref name="fileName"/>.</returns>
	internal static uint GetFileNameHash(string fileName)
	{
		if (fileName.StartsWith("unk", StringComparison.OrdinalIgnoreCase))
		{
			return uint.Parse(Path.GetFileNameWithoutExtension(fileName), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		long hash = 0x811C9DC5;

		foreach (char c in fileName)
		{
			hash = ((hash ^ c) * 0x1000193) & 0xFFFFFFFF;
		}

		return (uint)hash;
	}
}
