using System.Buffers;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Touhou.Extraction.Crypto;
using Touhou.Extraction.Properties;

namespace Touhou.Extraction.TH135;

internal abstract partial class FnList
{
	private readonly Dictionary<uint, string> _hashMap = [];

	private static readonly Encoding s_sjisEncoding = Encoding.GetEncoding(932);

	private const int RSA_BLOCK_SIZE = 32;
	private const int HEADER_SIZE = sizeof(uint) * 3;

	/// <summary>
	/// Creates the list of file names for the specified <paramref name="version"/>.
	/// </summary>
	/// <param name="version">The archive format version to create the list for.</param>
	/// <param name="loadFileNames">Whether to load the file names from memory.</param>
	/// <returns>The list of file names for the given <paramref name="version"/>.</returns>
	internal static FnList Create(TfpkVersion version, bool loadFileNames = true)
	{
		FnList list = CreateFromVersion(version);

		if (loadFileNames)
		{
			list.LoadFileNames();
		}

		return list;
	}

	/// <inheritdoc cref="Create(TfpkVersion, bool)"/>
	internal static async Task<FnList> CreateAsync(TfpkVersion version)
	{
		FnList list = CreateFromVersion(version);
		await list.LoadFileNamesAsync().ConfigureAwait(false);

		return list;
	}

	private static FnList CreateFromVersion(TfpkVersion version) => version switch
	{
		TfpkVersion.HM => new FnList0(),
		TfpkVersion.ULiL => new FnList1(),
		_ => throw new ArgumentOutOfRangeException(nameof(version), "The archive format version provided must be either Touhou 13.5 (0) or Touhou 14.5 (1).")
	};

	/// <summary>
	/// Adds the <paramref name="fileNames"/> provided and their hashes.
	/// </summary>
	/// <param name="fileNames">The file names of the entries.</param>
	internal void AddEntries(string[] fileNames)
	{
		_hashMap.EnsureCapacity(fileNames.Length);

		foreach (string fileName in fileNames)
		{
			Add(fileName, fromShiftJIS: false);
		}
	}

	/// <summary>
	/// Initializes the hash map of file names.
	/// </summary>
	private void LoadFileNames()
	{
		using StringReader fileNamesReader = new(Resources.fileslist);

		while (true)
		{
			string? fileName = fileNamesReader.ReadLine();

			if (string.IsNullOrWhiteSpace(fileName))
			{
				break;
			}

			Add(fileName.Replace('\\', '/'), fromShiftJIS: true);
		}
	}

	/// <inheritdoc cref="LoadFileNames"/>
	private async ValueTask LoadFileNamesAsync()
	{
		using StringReader fileNamesReader = new(Resources.fileslist);

		while (true)
		{
			string? fileName = await fileNamesReader.ReadLineAsync().ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(fileName))
			{
				break;
			}

			Add(fileName.Replace('\\', '/'), fromShiftJIS: true);
		}
	}

	/// <summary>
	/// Reads the list of file names from the archive.
	/// </summary>
	/// <param name="rsaReader">The <see cref="RsaReader"/> instance that will be used to decrypt the list's data.</param>
	/// <param name="dirCount">The number of directories in the archive.</param>
	internal void Read(RsaReader rsaReader, uint dirCount)
	{
		if (dirCount == 0)
		{
			return;
		}

		ref readonly FnHeader header = ref MemoryMarshal.AsRef<FnHeader>(rsaReader.Read(HEADER_SIZE));

		byte[] fileNamesDataBuffer = ArrayPool<byte>.Shared.Rent((int)(header.Size + 1));
		byte[] compressedFileNamesData = rsaReader.Read((int)(header.BlockCount * RSA_BLOCK_SIZE));

		using (ZLibStream decompressorStream = new(new MemoryStream(compressedFileNamesData, writable: false), CompressionMode.Decompress, leaveOpen: false))
		{
			using MemoryStream tmpStream = new(fileNamesDataBuffer);
			decompressorStream.CopyTo(tmpStream);
			fileNamesDataBuffer[header.Size] = 0;
		}

		string[] fileNames = Encoding.UTF8.GetString(fileNamesDataBuffer.AsSpan(0, (int)(header.Size + 1))).Split('\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		ArrayPool<byte>.Shared.Return(fileNamesDataBuffer);

		foreach (string fileName in fileNames)
		{
			Add(fileName, fromShiftJIS: true);
		}
	}

	/// <inheritdoc cref="Read(RsaReader, uint)"/>
	internal async ValueTask ReadAsync(RsaReader rsaReader, uint dirCount)
	{
		if (dirCount == 0)
		{
			return;
		}

		FnHeader header = MemoryMarshal.Read<FnHeader>(await rsaReader.ReadAsync(HEADER_SIZE).ConfigureAwait(false));

		byte[] fileNamesDataBuffer = ArrayPool<byte>.Shared.Rent((int)(header.Size + 1));
		byte[] compressedFileNamesData = await rsaReader.ReadAsync((int)(header.BlockCount * RSA_BLOCK_SIZE)).ConfigureAwait(false);

		ZLibStream decompressorStream = new(new MemoryStream(compressedFileNamesData, writable: false), CompressionMode.Decompress, leaveOpen: false);
		await using (decompressorStream.ConfigureAwait(false))
		{
			MemoryStream tmpStream = new(fileNamesDataBuffer);
			await using (tmpStream.ConfigureAwait(false))
			{
				await decompressorStream.CopyToAsync(tmpStream).ConfigureAwait(false);
				fileNamesDataBuffer[header.Size] = 0;
			}
		}

		string[] fileNames = Encoding.UTF8.GetString(fileNamesDataBuffer.AsSpan(0, (int)(header.Size + 1))).Split('\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		ArrayPool<byte>.Shared.Return(fileNamesDataBuffer);

		foreach (string fileName in fileNames)
		{
			Add(fileName, fromShiftJIS: true);
		}
	}

	/// <summary>
	/// Writes the list of file names of the archive.
	/// </summary>
	/// <param name="rsaWriter">The <see cref="RsaWriter"/> instance that will be used to encrypt the list's data.</param>
	internal void Write(RsaWriter rsaWriter)
	{
		List<byte> uncompressedFileNames = new(_hashMap.Count * 6);

		foreach ((_, string fileName) in _hashMap)
		{
			uncompressedFileNames.AddRange(Encoding.Convert(Encoding.UTF8, s_sjisEncoding, Encoding.UTF8.GetBytes(fileName)));
			uncompressedFileNames.Add(0x0);
		}

		uint blockCount;
		byte[] compressedFileNamesData;
		Span<byte> uncompressedFileNamesData = CollectionsMarshal.AsSpan(uncompressedFileNames)[..uncompressedFileNames.Count];

		using (MemoryStream compressedFileNamesStream = new())
		{
			using (ZLibStream compressorStream = new(compressedFileNamesStream, CompressionLevel.SmallestSize, leaveOpen: true))
			{
				compressorStream.Write(uncompressedFileNamesData);
			}

			compressedFileNamesStream.Seek(0, SeekOrigin.Begin);

			blockCount = (uint)(compressedFileNamesStream.Length / RSA_BLOCK_SIZE);

			if (compressedFileNamesStream.Length % RSA_BLOCK_SIZE != 0)
			{
				blockCount++;
			}

			compressedFileNamesData = new byte[compressedFileNamesStream.Length + RSA_BLOCK_SIZE];
			compressedFileNamesStream.ReadExactly(compressedFileNamesData.AsSpan(0, (int)compressedFileNamesStream.Length));
		}

		FnHeader header = new()
		{
			BlockCount = blockCount,
			Size = (uint)uncompressedFileNamesData.Length,
			CompressedSize = (uint)compressedFileNamesData.Length
		};
		byte[] headerData = new byte[HEADER_SIZE];
		MemoryMarshal.Write(headerData, in header);

		rsaWriter.Write(headerData);
		rsaWriter.Write(compressedFileNamesData, (int)(blockCount * RSA_BLOCK_SIZE));
	}

	/// <inheritdoc cref="Write(RsaWriter)"/>
	internal async ValueTask WriteAsync(RsaWriter rsaWriter)
	{
		List<byte> uncompressedFileNames = new(_hashMap.Count * 6);

		foreach ((_, string fileName) in _hashMap)
		{
			uncompressedFileNames.AddRange(Encoding.Convert(Encoding.UTF8, s_sjisEncoding, Encoding.UTF8.GetBytes(fileName)));
			uncompressedFileNames.Add(0x0);
		}

		uint blockCount;
		byte[] compressedFileNamesData;

		MemoryStream compressedFileNamesStream = new();
		await using (compressedFileNamesStream.ConfigureAwait(false))
		{
			ZLibStream compressorStream = new(compressedFileNamesStream, CompressionLevel.SmallestSize, leaveOpen: true);
			await using (compressorStream.ConfigureAwait(false))
			{
				await compressorStream.WriteAsync(uncompressedFileNames.ToArray()).ConfigureAwait(false);
			}

			compressedFileNamesStream.Seek(0, SeekOrigin.Begin);

			blockCount = (uint)(compressedFileNamesStream.Length / RSA_BLOCK_SIZE);

			if (compressedFileNamesStream.Length % RSA_BLOCK_SIZE != 0)
			{
				blockCount++;
			}

			compressedFileNamesData = new byte[compressedFileNamesStream.Length + RSA_BLOCK_SIZE];
			await compressedFileNamesStream.ReadExactlyAsync(compressedFileNamesData.AsMemory(0, (int)compressedFileNamesStream.Length)).ConfigureAwait(false);
		}

		FnHeader header = new()
		{
			BlockCount = blockCount,
			Size = (uint)uncompressedFileNames.Count,
			CompressedSize = (uint)compressedFileNamesData.Length
		};
		byte[] headerData = new byte[HEADER_SIZE];
		MemoryMarshal.Write(headerData, in header);

		await rsaWriter.WriteAsync(headerData).ConfigureAwait(false);
		await rsaWriter.WriteAsync(compressedFileNamesData, (int)(blockCount * RSA_BLOCK_SIZE)).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets the file name that corresponds with the specified <paramref name="hash"/>.
	/// </summary>
	/// <param name="hash">The hash of the file name to be retrieved.</param>
	/// <param name="isUnknown">After returning contains whether the file name is unknown.</param>
	/// <returns>The file name that corresponds with the specified <paramref name="hash"/>, or a string in the form of "unk/XXXXXXXX" (where XXXXXXXX is the <paramref name="hash"/>) if the file name is unknown.</returns>
	internal string GetFileNameFromHash(uint hash, out bool isUnknown) => (isUnknown = !_hashMap.ContainsKey(hash)) ? $"unk/{hash:X8}" : _hashMap[hash];

	/// <summary>
	/// Calculates the hash of the given <paramref name="fileName"/>.
	/// </summary>
	/// <param name="fileName">The file name of an entry relative to the archive.</param>
	/// <param name="initialHash">The starting hash to start calculating from.</param>
	/// <returns>The hash of the specified <paramref name="fileName"/>.</returns>
	/// <exception cref="ArgumentException"><paramref name="fileName"/> is <see langword="null"/>, empty or consists only of white-space characters.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal uint GetFileNameHash(string fileName, uint initialHash = 0x811C9DC5u)
	{
		return fileName.Length > 3 && fileName.StartsWith("unk", StringComparison.OrdinalIgnoreCase)
			? uint.Parse(Path.GetFileNameWithoutExtension(fileName)!, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
			: GetFileNameHashCore(fileName, initialHash);
	}

	/// <inheritdoc cref="GetFileNameHash(string, uint)"/>
	protected abstract uint GetFileNameHashCore(string fileName, uint initialHash = 0x811C9DC5u);

	/// <summary>
	/// Adds the specified <paramref name="fileName"/> to the list after calculating its hash.
	/// </summary>
	/// <param name="fileName">The file name of an entry relative to the archive.</param>
	/// <param name="fromShiftJIS">Whether to convert the <paramref name="fileName"/> to or from Shift-JIS.</param>
	/// <exception cref="ArgumentException"><paramref name="fileName"/> is <see langword="null"/>, empty or consists only of white-space characters.</exception>
	private void Add(string fileName, bool fromShiftJIS)
	{
		if (fromShiftJIS)
		{
			_hashMap[GetFileNameHash(fileName)] = Encoding.UTF8.GetString(Encoding.Convert(s_sjisEncoding, Encoding.UTF8, s_sjisEncoding.GetBytes(fileName)));
		}
		else
		{
			_hashMap[GetFileNameHash(s_sjisEncoding.GetString(Encoding.Convert(Encoding.UTF8, s_sjisEncoding, Encoding.UTF8.GetBytes(fileName))))] = fileName;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private readonly struct FnHeader
	{
		/// <summary>
		/// Gets the compressed file name list size.
		/// </summary>
		internal readonly uint CompressedSize { get; init; }
		/// <summary>
		/// Gets the file name list size.
		/// </summary>
		internal readonly uint Size { get; init; }
		/// <summary>
		/// Gets the number of RSA blocks used by the file name list.
		/// </summary>
		internal readonly uint BlockCount { get; init; }
	}
}
