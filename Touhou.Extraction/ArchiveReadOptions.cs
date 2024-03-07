namespace Touhou.Extraction;

/// <summary>
/// Specifies options for reading archives.
/// </summary>
[Flags]
public enum ArchiveReadOptions : int
{
	/// <summary>
	/// Uses the default options when reading an archive.
	/// </summary>
	None = 0,
	/// <summary>
	/// Excludes entries with unknown names from reading.
	/// </summary>
	ExcludeUnknownEntries = 1
}
