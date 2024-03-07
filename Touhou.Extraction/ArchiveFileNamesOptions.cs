namespace Touhou.Extraction;

[Flags]
internal enum ArchiveFileNamesOptions
{
	/// <summary>
	/// No special handling needed.
	/// </summary>
	None = 0,
	/// <summary>
	/// Strip path names.
	/// </summary>
	BaseName = 1,
	/// <summary>
	/// Force uppercase filenames.
	/// </summary>
	Uppercase = 2,
	/// <summary>
	/// Check for 8.3 (short) filenames.
	/// </summary>
	ShortFilename = 4,
}
