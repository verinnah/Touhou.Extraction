using System.Buffers;
using System.Runtime.InteropServices;
using Touhou.Extraction.Crypto;

namespace Touhou.Extraction.TH135;

internal sealed class DirList
{
	private readonly List<DirEntry> _entries = [];

	private const int DIR_ENTRY_SIZE = sizeof(uint) * 2;

	/// <summary>
	/// Adds directory entries based on the <paramref name="filePaths"/> provided.
	/// </summary>
	/// <param name="entriesBasePath">The path to the base directory of the entries in <paramref name="filePaths"/>.</param>
	/// <param name="filePaths">The file paths of the entries.</param>
	/// <param name="hashCallback">The function used to generate hashes of the paths.</param>
	internal void AddEntries(string entriesBasePath, string[] filePaths, Func<string, uint> hashCallback)
	{
		Dictionary<string, uint> dirs = [];

		foreach (string filePath in filePaths)
		{
			string dirPath = Path.GetDirectoryName(Path.GetRelativePath(entriesBasePath, filePath))!;

			ref uint dirEntryCount = ref CollectionsMarshal.GetValueRefOrAddDefault(dirs, dirPath, out bool exists);
			dirEntryCount++;
		}

		_entries.Clear();
		_entries.Capacity = dirs.Count;

		foreach ((string dirPath, uint entryCount) in dirs)
		{
			_entries.Add(new DirEntry
			{
				PathHash = hashCallback(dirPath),
				EntryCount = entryCount
			});
		}
	}

	/// <summary>
	/// Reads the specified number of directories from the archive's list.
	/// </summary>
	/// <param name="rsaReader">The RSA reader to decrypt the list's data.</param>
	/// <param name="dirCount">The number of directories to read.</param>
	internal void Read(RsaReader rsaReader, uint dirCount)
	{
		for (uint dirIndex = 0; dirIndex < dirCount; dirIndex++)
		{
			_entries.Add(MemoryMarshal.Read<DirEntry>(rsaReader.Read(DIR_ENTRY_SIZE)));
		}
	}

	/// <summary>
	/// Reads the specified number of directories from the archive's list.
	/// </summary>
	/// <param name="rsaReader">The <see cref="RsaReader"/> that will be used to decrypt the list's data.</param>
	/// <param name="dirCount">The number of directories to read.</param>
	internal async ValueTask ReadAsync(RsaReader rsaReader, uint dirCount)
	{
		for (uint dirIndex = 0; dirIndex < dirCount; dirIndex++)
		{
			_entries.Add(MemoryMarshal.Read<DirEntry>(await rsaReader.ReadAsync(DIR_ENTRY_SIZE).ConfigureAwait(false)));
		}
	}

	/// <summary>
	/// Writes the list of directories of the archive.
	/// </summary>
	/// <param name="rsaWriter">The RSA writer to encrypt the list's data.</param>
	internal void Write(RsaWriter rsaWriter)
	{
		rsaWriter.WriteUInt32((uint)_entries.Count);

		byte[] dirEntryBuffer = ArrayPool<byte>.Shared.Rent(DIR_ENTRY_SIZE);

		foreach (DirEntry entry in _entries)
		{
			MemoryMarshal.Write(dirEntryBuffer.AsSpan(0, DIR_ENTRY_SIZE), in entry);

			rsaWriter.Write(dirEntryBuffer);
		}

		ArrayPool<byte>.Shared.Return(dirEntryBuffer);
	}

	/// <summary>
	/// Writes the list of directories of the archive.
	/// </summary>
	/// <param name="rsaWriter">The RSA writer to encrypt the list's data.</param>
	internal async ValueTask WriteAsync(RsaWriter rsaWriter)
	{
		await rsaWriter.WriteUInt32Async((uint)_entries.Count).ConfigureAwait(false);

		byte[] dirEntryBuffer = ArrayPool<byte>.Shared.Rent(DIR_ENTRY_SIZE);

		foreach (DirEntry entry in _entries)
		{
			MemoryMarshal.Write(dirEntryBuffer.AsSpan(0, DIR_ENTRY_SIZE), in entry);

			await rsaWriter.WriteAsync(dirEntryBuffer).ConfigureAwait(false);
		}

		ArrayPool<byte>.Shared.Return(dirEntryBuffer);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private readonly struct DirEntry
	{
		/// <summary>
		/// Gets the hash of the directory path.
		/// </summary>
		internal readonly required uint PathHash { get; init; }
		/// <summary>
		/// Gets the number of entries inside the directory.
		/// </summary>
		internal readonly required uint EntryCount { get; init; }
	}
}
