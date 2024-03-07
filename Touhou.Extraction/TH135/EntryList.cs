using Touhou.Extraction.Crypto;

namespace Touhou.Extraction.TH135;

internal abstract partial class EntryList
{
	/// <summary>
	/// Gets the list of entries.
	/// </summary>
	internal List<EntryTh135> Entries { get; }
	/// <summary>
	/// Gets the file extension filters applied.
	/// </summary>
	protected string[]? ExtensionFilters { get; }
	/// <summary>
	/// Gets the options the archive was open with.
	/// </summary>
	protected ArchiveReadOptions Options { get; }

	/// <summary>
	/// Initializes a new list of entries.
	/// </summary>
	/// <param name="entries">The backing list of entries.</param>
	protected EntryList(List<EntryTh135> entries)
	{
		Entries = entries;
	}

	/// <summary>
	/// Initializes a new list of entries.
	/// </summary>
	/// <param name="entries">The backing list of entries.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only entries with these extensions will be included.</param>
	protected EntryList(List<EntryTh135> entries, ArchiveReadOptions options, string[]? extensionFilters)
	{
		Options = options;
		Entries = entries;
		ExtensionFilters = extensionFilters;
	}

	/// <summary>
	/// Creates a tailored list of entries for the specified <paramref name="version"/>.
	/// </summary>
	/// <param name="version">The archive format version to create the list for.</param>
	/// <param name="entries">The list of entries to use as backing storage.</param>
	/// <returns>The specialised list tailored for the given <paramref name="version"/>.</returns>
	internal static EntryList Create(TfpkVersion version, List<EntryTh135> entries) => version switch
	{
		TfpkVersion.HM => new EntryList0(entries),
		_ => new EntryList1(entries),
	};

	/// <summary>
	/// Creates a tailored list of entries for the specified <paramref name="version"/>.
	/// </summary>
	/// <param name="version">The archive format version to create the list for.</param>
	/// <param name="entries">The list of entries to use as backing storage.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only these extensions will be included.</param>
	/// <returns>The specialised list tailored for the given <paramref name="version"/>.</returns>
	internal static EntryList Create(TfpkVersion version, List<EntryTh135> entries, ArchiveReadOptions options, string[]? extensionFilters) => version switch
	{
		TfpkVersion.HM => new EntryList0(entries, options, extensionFilters),
		_ => new EntryList1(entries, options, extensionFilters),
	};

	/// <summary>
	/// Adds the provided <paramref name="fileNames"/> and their hashes to the list.
	/// </summary>
	/// <param name="entriesBasePath">The path to the base directory of the entries in <paramref name="fileNames"/>.</param>
	/// <param name="fileNames">The file names of the entries.</param>
	/// <param name="hashCallback">The function used to generate hashes for the paths.</param>
	internal void AddEntries(string entriesBasePath, string[] fileNames, Func<string, uint> hashCallback)
	{
		int offset = 0;

		foreach (string filePath in fileNames)
		{
			FileInfo entryFileInfo = new(filePath);

			int size = (int)entryFileInfo.Length;

			if (size == int.MinValue)
			{
				throw new ArgumentException($"The file \"{filePath}\" is too big in size (is {entryFileInfo.Length} bytes, {int.MaxValue} max).", nameof(fileNames));
			}
			else if (size == 0)
			{
				throw new ArgumentException($"The file \"{filePath}\" is empty.", nameof(fileNames));
			}

			string fileName = Path.GetRelativePath(entriesBasePath, filePath);
			Entries.Add(new EntryTh135(size, offset, hashCallback(fileName), key: new uint[4], fileName));

			offset += size;
		}
	}

	/// <summary>
	/// Reads the list of entries from the archive.
	/// </summary>
	/// <param name="rsaReader">The <see cref="RsaReader"/> instance that will be used to decrypt the list's data.</param>
	/// <param name="entryCount">The number of entries in the list.</param>
	/// <param name="fnList">The list of file names.</param>
	internal abstract void Read(RsaReader rsaReader, uint entryCount, FnList fnList);

	/// <inheritdoc cref="Read(RsaReader, uint, FnList)"/>
	internal abstract ValueTask ReadAsync(RsaReader rsaReader, uint entryCount, FnList fnList);

	/// <summary>
	/// Writes the list of entries into the archive.
	/// </summary>
	/// <param name="rsaWriter">The <see cref="RsaWriter"/> instance that will be used to encrypt the list's data.</param>
	internal abstract void Write(RsaWriter rsaWriter);

	/// <inheritdoc cref="Write(RsaWriter)"/>
	internal abstract ValueTask WriteAsync(RsaWriter rsaWriter);
}
