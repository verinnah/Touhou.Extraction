using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Touhou.Extraction.TH135;

public abstract partial class TFPK
{
	private sealed class TFPK0(int offset, List<EntryTh135>? entries, Stream stream, ArchiveInitializationMode initializationMode) : TFPK(offset, entries, stream, initializationMode)
	{
		/// <inheritdoc/>
		private protected override TfpkVersion Version { get; } = TfpkVersion.HM;

		/// <inheritdoc/>
		private protected override void Decrypt(Span<byte> data, ReadOnlySpan<uint> key)
		{
			if (key.IsEmpty)
			{
				throw new ArgumentException("The key cannot be empty.", nameof(key));
			}

			ReadOnlySpan<byte> keyData = MemoryMarshal.AsBytes(key);

			for (int c = 0; c < data.Length; c++)
			{
				data[c] ^= keyData[c % 16];
			}
		}

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected override void Encrypt(Span<byte> data, ReadOnlySpan<uint> key) => Decrypt(data, key);
	}
}
