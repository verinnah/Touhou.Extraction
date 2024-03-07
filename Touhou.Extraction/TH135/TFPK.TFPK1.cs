using System.Buffers;
using System.Runtime.InteropServices;

namespace Touhou.Extraction.TH135;

public abstract partial class TFPK
{
	private sealed class TFPK1(int offset, List<EntryTh135>? entries, Stream stream, ArchiveInitializationMode initializationMode) : TFPK(offset, entries, stream, initializationMode)
	{
		/// <inheritdoc/>
		private protected override TfpkVersion Version { get; } = TfpkVersion.ULiL;

		/// <inheritdoc/>
		private protected override void Decrypt(Span<byte> data, ReadOnlySpan<uint> key)
		{
			if (key.IsEmpty)
			{
				throw new ArgumentException("The key cannot be empty.", nameof(key));
			}

			ReadOnlySpan<byte> keyData = MemoryMarshal.AsBytes(key);

			Span<byte> aux = stackalloc byte[4];
			keyData[..4].CopyTo(aux);

			for (int c = 0; c < data.Length; c++)
			{
				byte tmp = data[c];
				data[c] = (byte)(data[c] ^ keyData[c % 16] ^ aux[c % 4]);
				aux[c % 4] = tmp;
			}
		}

		/// <inheritdoc/>
		private protected override void Encrypt(Span<byte> data, ReadOnlySpan<uint> key)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(data.Length);
			Span<byte> tmp = buffer.AsSpan(0, data.Length);
			data.CopyTo(tmp);

			// This seems to give the correct value for aux
			uint aux = Encrypt(tmp, key, key[0]);

			ArrayPool<byte>.Shared.Return(buffer);

			Encrypt(data, key, aux);
		}

		private static uint Encrypt(Span<byte> data, ReadOnlySpan<uint> key, uint aux)
		{
			ReadOnlySpan<byte> keyData = MemoryMarshal.AsBytes(key);
			Span<byte> auxData = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref aux, 1));

			for (int c = data.Length - 1; c >= 0; c--)
			{
				byte unencryptedByte = data[c];
				byte encryptedByte = auxData[c % 4];

				data[c] = encryptedByte;
				auxData[c % 4] = (byte)(unencryptedByte ^ encryptedByte ^ keyData[c % 16]);
			}

			return aux;
		}
	}
}
