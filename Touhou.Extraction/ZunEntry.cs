namespace Touhou.Extraction;

/// <summary>
/// Represents a entry from a ZUN <see cref="Archive"/>. This class cannot be inherited.
/// </summary>
internal sealed class ZunEntry : Entry
{
	/// <summary>
	/// Gets or sets format-specific data.
	/// </summary>
	internal uint Extra { get; set; }
	/// <summary>
	/// Gets or sets the compressed entry size.
	/// </summary>
	/// <remarks>Initialized to -1 before being filled out.</remarks>
	internal int CompressedSize { get; set; } = -1;

	/// <summary>
	/// Instantiates a new ZUN archive entry.
	/// </summary>
	internal ZunEntry() : base(-1, -1, string.Empty) { }

	/// <summary>
	/// Instantiates a new ZUN archive entry with the specified information.
	/// </summary>
	/// <param name="fileName">The path of this entry relative to the archive.</param>
	/// <param name="extra">The format-specific data of the entry.</param>
	/// <param name="size">The original (uncompressed) size of the entry; may be -1 if unspecified.</param>
	/// <param name="compressedSize">The size of the entry compressed in the archive; may be -1 if unspecified.</param>
	/// <param name="offset">The offset of the entry in the archive; may be -1 if unspecified.</param>
	internal ZunEntry(string fileName, uint extra, int size, int compressedSize, int offset) : base(size, offset, fileName)
	{
		Size = size;
		Extra = extra;
		CompressedSize = compressedSize;
	}
}
