using System.Runtime.InteropServices;

namespace Touhou.Extraction.Utils;

/// <summary>
/// Provides static methods to handle the creation of wave files. This class cannot be inherited.
/// </summary>
internal static class WaveUtils
{
	private const uint RIFF_ID = 0x46464952;
	private const uint DATA_ID = 0x61746164;
	private const ulong WAVEFMT_ID = 0x20746d66_45564157;

	/// <summary>
	/// Writes a PCM wave file using <paramref name="formatData"/> and <paramref name="data"/>.
	/// </summary>
	/// <param name="formatData">The span containing the WAVEFORMATEX data.</param>
	/// <param name="data">The PCM data of the wave.</param>
	/// <param name="checkIfMagicExists">Whether it should be checked if <paramref name="data"/> has the wave magic.</param>
	/// <param name="shouldUseInputData">Whether <paramref name="data"/> should be used instead of the method's returning value.</param>
	/// <returns>An array containing the wave file, or an empty array if <paramref name="shouldUseInputData"/> is <see langword="true"/>.</returns>
	/// <exception cref="ArgumentException"><paramref name="formatData"/> has a size other than 16 or 18.</exception>
	internal static byte[] WriteWave(ReadOnlySpan<byte> formatData, ReadOnlySpan<byte> data, bool checkIfMagicExists, out bool shouldUseInputData)
	{
		if (checkIfMagicExists && data[..4].SequenceEqual("RIFF"u8))
		{
			shouldUseInputData = true;

			return [];
		}

		if (formatData.Length is not 16 or 18)
		{
			throw new ArgumentException("The format data (WAVEFORMATEX) must be 16 or 18 bytes in size.", nameof(formatData));
		}

		shouldUseInputData = false;

		byte[] buffer = new byte[44 + data.Length];

		// Write WAVE header
		MemoryMarshal.Write(buffer.AsSpan(0x0, sizeof(uint)), RIFF_ID);          // chunkID @ 0x0 = "RIFF"
		MemoryMarshal.Write(buffer.AsSpan(0x4, sizeof(uint)), data.Length + 36); // chunkSize @ 0x4 = 36 (size of the rest of the chunk) + subChunk2Size
		MemoryMarshal.Write(buffer.AsSpan(0x8, sizeof(ulong)), WAVEFMT_ID);      // format @ 0x8 = "WAVE" & subChunk1ID @ 0xC = "fmt "
		MemoryMarshal.Write(buffer.AsSpan(0x10, sizeof(uint)), 0x10u);           // subChunk1Size @ 0x10 = 16 for PCM
		formatData.CopyTo(buffer.AsSpan(0x14, 16));                              // WAVEFORMATEX @ 0x14 (skip reading cbSize as in PCM data it's always 0)
		MemoryMarshal.Write(buffer.AsSpan(0x24, sizeof(uint)), DATA_ID);         // subChunk2ID @ 0x24 = "data"
		MemoryMarshal.Write(buffer.AsSpan(0x28, sizeof(uint)), data.Length);     // subChunk2Size @ 0x28 = numSamples * numChannels * (bitsPerSample / 8) = wave data size

		// Write audio data if there's any
		if (!data.IsEmpty)
		{
			data.CopyTo(buffer.AsSpan(0x2c, data.Length));
		}

		return buffer;
	}
}
