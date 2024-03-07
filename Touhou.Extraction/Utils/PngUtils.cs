using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Touhou.Extraction.Utils;

/// <summary>
/// Provides static methods to handle the creation of PNG files. This class cannot be inherited.
/// </summary>
internal static class PngUtils
{
	/// <summary>
	/// Gets a <see cref="PngEncoder"/> instance configured to encode PNG images without metadata, unnecessary ancillary chunks, no interlacing, no filter, no compression, and RGBA32 pixels.
	/// </summary>
	internal static PngEncoder DefaultPngEncoder { get; } = new()
	{
		Quantizer = null,
		SkipMetadata = true,
		FilterMethod = PngFilterMethod.None,
		ColorType = PngColorType.RgbWithAlpha,
		ChunkFilter = PngChunkFilter.ExcludeAll,
		InterlaceMethod = PngInterlaceMode.None,
		CompressionLevel = PngCompressionLevel.NoCompression,
		TransparentColorMode = PngTransparentColorMode.Preserve,
	};

	private static readonly QuantizerOptions s_nonDitherQuantizer = new()
	{
		Dither = null,
		DitherScale = 0
	};

	/// <summary>
	/// Creates a <see cref="PngEncoder"/> instance that uses the specified <paramref name="palette"/>.
	/// </summary>
	/// <param name="palette">The palette of colors to use for quantization.</param>
	/// <returns>The <see cref="PngEncoder"/> instance with the <paramref name="palette"/>.</returns>
	internal static PngEncoder Get8bbpPngEncoder(ReadOnlyMemory<Color> palette) => new()
	{
		SkipMetadata = true,
		ColorType = PngColorType.Palette,
		FilterMethod = PngFilterMethod.None,
		ChunkFilter = PngChunkFilter.ExcludeAll,
		InterlaceMethod = PngInterlaceMode.None,
		CompressionLevel = PngCompressionLevel.NoCompression,
		TransparentColorMode = PngTransparentColorMode.Preserve,
		Quantizer = new PaletteQuantizer(palette, s_nonDitherQuantizer)
	};
}
