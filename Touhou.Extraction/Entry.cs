using System.Runtime.CompilerServices;

namespace Touhou.Extraction;

/// <summary>
/// Represents an entry from an <see cref="Archive"/>.
/// </summary>
public class Entry
{
	/// <summary>
	/// Gets the size of the entry.
	/// </summary>
	/// <remarks>This may be -1 when the size could not be determined.</remarks>
	public int Size { get; internal set; }
	/// <summary>
	/// Gets the offset of the entry in the archive.
	/// </summary>
	public int Offset { get; internal set; }
	/// <summary>
	/// Gets the file name of the entry relative to the archive.
	/// </summary>
	/// <remarks>This may not represent the actual file name and instead use a hash.</remarks>
	public string FileName { get; internal set; }

	/// <summary>
	/// Instantiates a new archive entry.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Entry() : this(-1, -1, string.Empty) { }

	/// <summary>
	/// Instantiates a new archive entry with the specified information.
	/// </summary>
	/// <param name="size">The size of the entry in the archive; may be -1 if unspecified.</param>
	/// <param name="offset">The offset of the entry in the archive; may be -1 if unspecified.</param>
	/// <param name="fileName">The file name of the entry relative to the archive.</param>
	internal Entry(int size, int offset, string fileName)
	{
		Size = size;
		Offset = offset;
		FileName = fileName;
	}
}
