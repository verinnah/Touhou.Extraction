namespace Touhou.Extraction.TH135;

/// <summary>
/// Represents an entry from a Touhou 13.5 / 14.5 / 15.5 <see cref="TFPK"/>. This class cannot be inherited.
/// </summary>
internal sealed class EntryTh135 : Entry
{
	/// <summary>
	/// Gets the key used to decrypt this entry's contents.
	/// </summary>
	internal uint[] Key { get; init; }
	/// <summary>
	/// Gets or sets the hash of this entry's file name.
	/// </summary>
	internal uint FileNameHash { get; set; }

	/// <summary>
	/// Instantiates a new archive entry with the specified information.
	/// </summary>
	/// <param name="size">The size of the entry in the archive; may be -1 if unspecified.</param>
	/// <param name="offset">The offset of the entry in the archive.</param>
	/// <param name="hash">The hash of the entry's path.</param>
	/// <param name="key">The key that must be used to decrypt the entry's data.</param>
	/// <param name="fileName">The path of this entry relative to the archive.</param>
	internal EntryTh135(int size, int offset, uint hash, uint[] key, string fileName) : base(size, offset, fileName)
	{
		Key = key;
		FileNameHash = hash;
	}
}
