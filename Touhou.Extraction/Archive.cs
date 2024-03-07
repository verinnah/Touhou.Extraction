using System.Text;
using Touhou.Common;
using Touhou.Extraction.Utils;

namespace Touhou.Extraction;

/// <summary>
/// Represents an archive file containing data from a Touhou game. This is an <see langword="abstract"/> class.
/// </summary>
/// <remarks>An instance of <see cref="CodePagesEncodingProvider"/> must be registered with <see cref="Encoding.RegisterProvider"/> before working with Touhou 10.5, 12.3, 13.5, 14.5 and 15.5.</remarks>
public abstract class Archive : IAsyncDisposable, IDisposable
{
	/// <summary>
	/// Gets the entries of the files contained in the archive.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The property is accessed after the object was disposed of.</exception>
	public abstract IEnumerable<Entry> Entries { get; }
	/// <summary>
	/// Gets the flags specified for the archive when special handling is needed.
	/// </summary>
	private protected virtual ArchiveFileNamesOptions Flags { get; } = ArchiveFileNamesOptions.None;

	private bool _isDisposed;

	private static readonly EnumerationOptions s_entryFilesEnumerationOptions = new()
	{
		RecurseSubdirectories = true,
		ReturnSpecialDirectories = false
	};

	/// <summary>
	/// Creates an <see cref="Archive"/> for the specified <paramref name="game"/> containing all the files found in the <paramref name="entriesPath"/> provided.
	/// </summary>
	/// <param name="game">The game the archive is from.</param>
	/// <param name="outputPath">The file path the archive will be written into.</param>
	/// <param name="entriesPath">The path to the directory that contains the files to be included in the archive.</param>
	/// <exception cref="NotSupportedException"><paramref name="game"/> specifies an unsupported game.</exception>
	/// <exception cref="ArgumentException"><paramref name="outputPath"/> or <paramref name="entriesPath"/> is empty or consists only of white-space characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputPath"/> or <paramref name="entriesPath"/> is <see langword="null"/>.</exception>
	public static void Create(Game game, string outputPath, string entriesPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

		using FileStream outputStream = new(outputPath, FileUtils.OpenWriteFileStreamOptions);
		Create(game, outputStream, entriesPath);
	}

	/// <summary>
	/// Creates an <see cref="Archive"/> for the specified <paramref name="game"/> containing all the files found in the <paramref name="entriesPath"/> provided.
	/// </summary>
	/// <param name="game">The game the archive is from.</param>
	/// <param name="outputStream">The stream the archive will be written into.</param>
	/// <param name="entriesPath">The path to the directory that contains the files to be included in the archive.</param>
	/// <exception cref="NotSupportedException"><paramref name="game"/> specifies an unsupported game.</exception>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> or <paramref name="entriesPath"/> is empty or consists only of white-space characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="outputStream"/> or <paramref name="entriesPath"/> is <see langword="null"/>.</exception>
	public static void Create(Game game, Stream outputStream, string entriesPath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentException.ThrowIfNullOrWhiteSpace(entriesPath);

		int entryCount = GetFilePaths(entriesPath, out string[] entryPaths);

		outputStream.Seek(0, SeekOrigin.Begin);

		using Archive archive = game switch
		{
			Game.HRtP or Game.SoEW or Game.PoDD or Game.LLS or Game.MS => TH01.Archive.Create(game, entryCount, outputStream, entryPaths),
			Game.EoSD or Game.PCB => TH06.Archive.Create(game, entryCount, outputStream, entryPaths),
			Game.IN or Game.PoFV => TH08.PBGZ.Create(game, entryCount, outputStream, entryPaths),
			Game.IaMP => TH75.Archive.Create(entryCount, outputStream, entryPaths, entriesPath),
			Game.SWR or Game.Hiso => TH105.Archive.Create(entryCount, outputStream, entryPaths, entriesPath),
			Game.HM or Game.ULiL or Game.AoCF => TH135.TFPK.Create(game, entryCount, outputStream, entryPaths, entriesPath),
			Game.GI => TH175.CGX.Create(entryCount, outputStream, entryPaths, entriesPath),
			_ => TH95.THA1.Create(game, entryCount, outputStream, entryPaths)
		};

		foreach (Entry entry in archive.Entries)
		{
			if (string.IsNullOrWhiteSpace(entry.FileName))
			{
				continue;
			}

			archive.Pack(entry, Path.Combine(entriesPath, entry.FileName));
		}

		archive.Close();
	}

	/// <inheritdoc cref="Create(Game, string, string)"/>
	public static async ValueTask CreateAsync(Game game, string outputPath, string entriesPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

		FileStream outputStream = new(outputPath, FileUtils.AsyncOpenWriteFileStreamOptions);
		await using (outputStream.ConfigureAwait(false))
		{
			await CreateAsync(game, outputStream, entriesPath).ConfigureAwait(false);
		}
	}

	/// <inheritdoc cref="Create(Game, Stream, string)"/>
	public static async ValueTask CreateAsync(Game game, Stream outputStream, string entriesPath)
	{
		Guard.ThrowIfNullOrNotWritable(outputStream);

		ArgumentException.ThrowIfNullOrWhiteSpace(entriesPath);

		int entryCount = GetFilePaths(entriesPath, out string[] entryPaths);

		outputStream.Seek(0, SeekOrigin.Begin);

		Archive archive = game switch
		{
			Game.HRtP or Game.SoEW or Game.PoDD or Game.LLS or Game.MS => TH01.Archive.Create(game, entryCount, outputStream, entryPaths),
			Game.EoSD or Game.PCB => TH06.Archive.Create(game, entryCount, outputStream, entryPaths),
			Game.IN or Game.PoFV => TH08.PBGZ.Create(game, entryCount, outputStream, entryPaths),
			Game.IaMP => TH75.Archive.Create(entryCount, outputStream, entryPaths, entriesPath),
			Game.SWR or Game.Hiso => TH105.Archive.Create(entryCount, outputStream, entryPaths, entriesPath),
			Game.HM or Game.ULiL or Game.AoCF => await TH135.TFPK.CreateAsync(game, entryCount, outputStream, entryPaths, entriesPath).ConfigureAwait(false),
			Game.GI => TH175.CGX.Create(entryCount, outputStream, entryPaths, entriesPath),
			_ => TH95.THA1.Create(game, entryCount, outputStream, entryPaths)
		};
		await using (archive.ConfigureAwait(false))
		{
			foreach (Entry entry in archive.Entries)
			{
				if (string.IsNullOrWhiteSpace(entry.FileName))
				{
					continue;
				}

				await archive.PackAsync(entry, Path.Combine(entriesPath, entry.FileName)).ConfigureAwait(false);
			}

			await archive.CloseAsync().ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Reads the contents of the archive from the specified <paramref name="game"/> from the <paramref name="stream"/>.
	/// </summary>
	/// <param name="game">The game the archive is from.</param>
	/// <param name="stream">The stream that contains the arechive's data.</param>
	/// <param name="options">The options with which to read the archive.</param>
	/// <param name="extensionFilters">If any is given, only these extensions will be included.</param>
	/// <returns>A new instance of the <see cref="Archive"/> class initialized for extraction.</returns>
	/// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	/// <exception cref="NotSupportedException"><paramref name="game"/> specifies a non-supported game.</exception>
	public static Archive Read(Game game, Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null) => game switch
	{
		Game.HRtP or Game.SoEW or Game.PoDD or Game.LLS or Game.MS => TH01.Archive.Read(game, stream, options, extensionFilters),
		Game.EoSD or Game.PCB => TH06.Archive.Read(game, stream, options, extensionFilters),
		Game.IN or Game.PoFV => TH08.PBGZ.Read(game, stream, options, extensionFilters),
		Game.IaMP => TH75.Archive.Read(stream, options, extensionFilters),
		Game.SWR or Game.Hiso => TH105.Archive.Read(stream, options, extensionFilters),
		Game.HM or Game.ULiL or Game.AoCF => TH135.TFPK.Read(game, stream, options, extensionFilters),
		Game.GI => TH175.CGX.Read(stream, options, extensionFilters),
		_ => TH95.THA1.Read(game, stream, options, extensionFilters)
	};

	/// <inheritdoc cref="Read(Game, Stream, ArchiveReadOptions, string[])"/>
	public static async Task<Archive> ReadAsync(Game game, Stream stream, ArchiveReadOptions options = ArchiveReadOptions.None, string[]? extensionFilters = null) => game switch
	{
		Game.HRtP or Game.SoEW or Game.PoDD or Game.LLS or Game.MS => await TH01.Archive.ReadAsync(game, stream, options, extensionFilters).ConfigureAwait(false),
		Game.EoSD or Game.PCB => await TH06.Archive.ReadAsync(game, stream, options, extensionFilters).ConfigureAwait(false),
		Game.IN or Game.PoFV => await TH08.PBGZ.ReadAsync(game, stream, options, extensionFilters).ConfigureAwait(false),
		Game.IaMP => await TH75.Archive.ReadAsync(stream, options, extensionFilters).ConfigureAwait(false),
		Game.SWR or Game.Hiso => await TH105.Archive.ReadAsync(stream, options, extensionFilters).ConfigureAwait(false),
		Game.HM or Game.ULiL or Game.AoCF => await TH135.TFPK.ReadAsync(game, stream, options, extensionFilters).ConfigureAwait(false),
		Game.GI => await TH175.CGX.ReadAsync(stream, options, extensionFilters).ConfigureAwait(false),
		_ => await TH95.THA1.ReadAsync(game, stream, options, extensionFilters).ConfigureAwait(false)
	};

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> from the archive into the specified <paramref name="outputPath"/>.
	/// </summary>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <param name="outputPath">The path in which a file containing the <paramref name="entry"/> data will be created.</param>
	/// <exception cref="ArgumentException"><paramref name="entry"/> has a file name that is either <see langword="null"/>, empty or consists only of white-space characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="outputPath"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	public void Extract(Entry entry, string outputPath)
	{
		ArgumentNullException.ThrowIfNull(entry);

		if (string.IsNullOrWhiteSpace(entry.FileName))
		{
			throw new ArgumentException("The entry has an invalid file name.", nameof(entry));
		}

		string entryName = Path.Combine(outputPath, entry.FileName);

		string? dirName = Path.GetDirectoryName(entryName);
		if (!string.IsNullOrEmpty(dirName))
		{
			Directory.CreateDirectory(dirName);
		}

		using FileStream entryStream = new(entryName, FileUtils.OpenWriteFileStreamOptions);

		Extract(entry, entryStream);
	}

	/// <inheritdoc cref="Extract(Entry, string)"/>
	public async ValueTask ExtractAsync(Entry entry, string outputPath)
	{
		ArgumentNullException.ThrowIfNull(entry);

		if (string.IsNullOrWhiteSpace(entry.FileName))
		{
			throw new ArgumentException("The entry has an invalid file name.", nameof(entry));
		}

		string entryName = Path.Combine(outputPath, entry.FileName);

		string? dirName = Path.GetDirectoryName(entryName);
		if (!string.IsNullOrEmpty(dirName))
		{
			Directory.CreateDirectory(dirName);
		}

		FileStream entryStream = new(entryName, FileUtils.AsyncOpenWriteFileStreamOptions);
		await using (entryStream.ConfigureAwait(false))
		{
			await ExtractAsync(entry, entryStream).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> from the archive.
	/// </summary>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <returns>A span containing the data extracted from the given <paramref name="entry"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	public abstract Span<byte> Extract(Entry entry);

	/// <inheritdoc cref="Extract(Entry)"/>
	public abstract Task<Memory<byte>> ExtractAsync(Entry entry);

	/// <summary>
	/// Extracts the contents of the specified <paramref name="entry"/> into the <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="entry">The entry which contents should be extracted from the archive.</param>
	/// <param name="outputStream">The stream to which the content's should be written.</param>
	/// <exception cref="ArgumentException"><paramref name="outputStream"/> is not writable.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for extraction.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="outputStream"/> is <see langword="null"/>.</exception>
	public abstract void Extract(Entry entry, Stream outputStream);

	/// <inheritdoc cref="Extract(Entry, Stream)"/>
	public abstract ValueTask ExtractAsync(Entry entry, Stream outputStream);

	/// <summary>
	/// Reads the entry data from the file pointed at by <paramref name="entryFilePath"/> and writes it into the archive.
	/// </summary>
	/// <param name="entry">The entry in the archive.</param>
	/// <param name="entryFilePath">The path pointing to the file from which the entry data will be read.</param>
	/// <exception cref="ArgumentException"><paramref name="entry"/> is not a valid entry for this archive, or <paramref name="entryFilePath"/> is empty or consists only of white-space characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="entryFilePath"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for packaging.</exception>
	public void Pack(Entry entry, string entryFilePath)
	{
		ArgumentNullException.ThrowIfNull(entry);

		if (string.IsNullOrWhiteSpace(entry.FileName))
		{
			throw new ArgumentException("The entry has an invalid file name.", nameof(entry));
		}

		using FileStream entryStream = new(entryFilePath, FileUtils.OpenReadFileStreamOptions);

		Pack(entry, entryStream);
	}

	/// <inheritdoc cref="Pack(Entry, string)"/>
	public async ValueTask PackAsync(Entry entry, string entryFilePath)
	{
		ArgumentNullException.ThrowIfNull(entry);

		if (string.IsNullOrWhiteSpace(entry.FileName))
		{
			throw new ArgumentException("The entry has an invalid file name.", nameof(entry));
		}

		FileStream entryStream = new(entryFilePath, FileUtils.AsyncOpenReadFileStreamOptions);
		await using (entryStream.ConfigureAwait(false))
		{
			await PackAsync(entry, entryStream).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Reads the <paramref name="entryData"/> and writes it into the archive.
	/// </summary>
	/// <param name="entry">The entry in the archive.</param>
	/// <param name="entryData">The array from which the entry data will be read.</param>
	/// <exception cref="ArgumentException"><paramref name="entry"/> is not a valid entry for this archive.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="entryData"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for packaging.</exception>
	public abstract void Pack(Entry entry, byte[] entryData);

	/// <inheritdoc cref="Pack(Entry, byte[])"/>
	public abstract ValueTask PackAsync(Entry entry, byte[] entryData);

	/// <summary>
	/// Reads the entry data from <paramref name="inputStream"/> and writes it into the archive.
	/// </summary>
	/// <param name="entry">The entry in the archive.</param>
	/// <param name="inputStream">The stream from which the entry data will be read.</param>
	/// <exception cref="ArgumentException"><paramref name="inputStream"/> is not writable or <paramref name="entry"/> is not a valid entry for this archive.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> or <paramref name="inputStream"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	/// <exception cref="InvalidOperationException">The archive has not been initialized for packaging.</exception>
	public abstract void Pack(Entry entry, Stream inputStream);

	/// <inheritdoc cref="Pack(Entry, Stream)"/>
	public abstract ValueTask PackAsync(Entry entry, Stream inputStream);

	/// <summary>
	/// Writes the remaining data to the archive and ends archive creation.
	/// </summary>
	public abstract void Close();

	/// <inheritdoc cref="Close"/>
	public abstract ValueTask CloseAsync();

	/// <summary>
	/// Validates <paramref name="fileName"/> and returns a valid file name that can be used in the archive.
	/// </summary>
	/// <remarks>This method only validates against the <see cref="ArchiveFileNamesOptions"/> specified in <see cref="Flags"/>; other validation is up to implementers.</remarks>
	/// <param name="fileName">The file name to be validated.</param>
	/// <returns>The validated file name following the specified <see cref="Flags"/>.</returns>
	/// <exception cref="ArgumentException"><paramref name="fileName"/> is not a valid 8.3 file name, is empty or consists only of white-space characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
	/// <exception cref="ObjectDisposedException">The method is called after the object was disposed of.</exception>
	private protected string GetValidatedEntryName(string fileName)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

		if (Flags != ArchiveFileNamesOptions.None)
		{
			if (Flags.HasFlag(ArchiveFileNamesOptions.BaseName))
			{
				fileName = Path.GetFileName(fileName);
			}

			if (Flags.HasFlag(ArchiveFileNamesOptions.Uppercase))
			{
				fileName = fileName.ToUpperInvariant();
			}

			if (Flags.HasFlag(ArchiveFileNamesOptions.ShortFilename))
			{
				bool hasExtension = Path.HasExtension(fileName);
				int fileNameLength = (hasExtension ? Path.GetFileNameWithoutExtension(fileName) : Path.GetFileName(fileName)).Length;
				int extensionLength = hasExtension ? Path.GetExtension(fileName).Length - 1 : 0;

				if (fileNameLength > 8 || extensionLength > 3)
				{
					throw new ArgumentException($"{fileName} is not a valid 8.3 file name.");
				}
			}
		}

		return fileName;
	}

	/// <summary>
	/// Gets the paths of the files inside <paramref name="path"/>.
	/// </summary>
	/// <remarks>If <paramref name="path"/> is a file, <paramref name="result"/> contains 1 entry with <paramref name="path"/>.</remarks>
	/// <param name="path">The path of the directory to examine for files.</param>
	/// <param name="result">After returning, contains the paths retrieved from examining <paramref name="path"/>.</param>
	/// <returns>The number of paths retrieved from examining <paramref name="path"/>.</returns>
	private static int GetFilePaths(string path, out string[] result)
	{
		if (!Directory.Exists(path))
		{
			result = [path];

			return 1;
		}

		result = Directory.GetFiles(path, "*", s_entryFilesEnumerationOptions);

		return result.Length;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);

		GC.SuppressFinalize(this);
	}

	/// <inheritdoc cref="Dispose()"/>
	/// <param name="disposing">Whether managed resources should be disposed of.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed)
		{
			return;
		}

		_isDisposed = true;
	}

	/// <inheritdoc cref="DisposeAsync"/>
	protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
}
