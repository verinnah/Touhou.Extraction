using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Extraction.Tests.Utils;
using Touhou.Extraction.TH75;

namespace Touhou.Extraction.Tests;

public sealed class WaveArchiveTests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "00.wav", 0xee281178e56db730 },
		{ "01.wav", 0x5812a278d01b0531 },
		{ "02.wav", 0xa74bdcee56d52790 },
		{ "03.wav", 0xca3484e055ceee03 },
		{ "04.wav", 0x5dff329d0ad56295 },
		{ "05.wav", 0xc625a9c0814c713c },
		{ "06.wav", 0x41a786cd84ac2140 },
		{ "07.wav", 0x8a2714a3785f73d1 },
		{ "08.wav", 0x9d050009b76e378 },
		{ "09.wav", 0x9f7f2b7c2e362b43 },
		{ "10.wav", 0x1fa1d0db3be47c8 },
		{ "11.wav", 0xe5de9f74f6c8409e },
		{ "12.wav", 0xf47d37373c04d190 },
		{ "13.wav", 0x1e2abfde927aba8a },
		{ "14.wav", 0x554844144f96b025 },
		{ "15.wav", 0xe2dac34713e40cba },
		{ "16.wav", 0xa89a0255b6cba399 },
		{ "17.wav", 0x14dc2c49e6cfebce },
		{ "18.wav", 0x14dc2c49e6cfebce },
		{ "19.wav", 0x14dc2c49e6cfebce },
		{ "20.wav", 0x14dc2c49e6cfebce },
		{ "21.wav", 0x14dc2c49e6cfebce },
		{ "22.wav", 0x14dc2c49e6cfebce },
		{ "23.wav", 0x14dc2c49e6cfebce },
		{ "24.wav", 0x14dc2c49e6cfebce },
		{ "25.wav", 0x14dc2c49e6cfebce },
		{ "26.wav", 0x14dc2c49e6cfebce },
		{ "27.wav", 0x14dc2c49e6cfebce },
		{ "28.wav", 0x14dc2c49e6cfebce },
		{ "29.wav", 0x14dc2c49e6cfebce },
		{ "30.wav", 0x14dc2c49e6cfebce },
		{ "31.wav", 0x14dc2c49e6cfebce },
		{ "32.wav", 0x14dc2c49e6cfebce },
		{ "33.wav", 0x14dc2c49e6cfebce },
		{ "34.wav", 0x14dc2c49e6cfebce },
		{ "35.wav", 0x14dc2c49e6cfebce },
		{ "36.wav", 0x14dc2c49e6cfebce },
		{ "37.wav", 0x14dc2c49e6cfebce },
		{ "38.wav", 0x14dc2c49e6cfebce },
		{ "39.wav", 0x14dc2c49e6cfebce },
		{ "40.wav", 0x14dc2c49e6cfebce },
		{ "41.wav", 0x14dc2c49e6cfebce },
		{ "42.wav", 0x14dc2c49e6cfebce },
		{ "43.wav", 0x14dc2c49e6cfebce },
		{ "44.wav", 0x14dc2c49e6cfebce },
		{ "45.wav", 0x14dc2c49e6cfebce },
		{ "46.wav", 0x14dc2c49e6cfebce },
		{ "47.wav", 0x14dc2c49e6cfebce },
		{ "48.wav", 0x14dc2c49e6cfebce },
		{ "49.wav", 0x14dc2c49e6cfebce }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th075";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries-wave";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\se03-test.dat";

	public WaveArchiveTests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\se03.dat")]
	public void ReadWaveArchive(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using WaveArchive archive = WaveArchive.Read(new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset > 0);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\se03.dat", true)]
	public async Task ReadWaveArchiveAsync(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using WaveArchive archive = await WaveArchive.ReadAsync(new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset > 0);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData.Span));

			if (writeEntriesToDisk)
			{
				string entryPath = Path.Combine(ENTRIES_PATH, entry.FileName);

				if (!File.Exists(entryPath))
				{
					await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
					await entryStream.WriteAsync(entryData);
				}
			}
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public void WriteWaveArchive(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		using (FileStream archiveStream = new(ARCHIVE_OUTPUT_PATH, FileUtils.OpenWriteFileStreamOptions))
		{
			int entryCount = GetFilePaths(entriesPath, out string[] entryFileNames);

			using WaveArchive waveArchive = WaveArchive.Create(entryCount, archiveStream, entryFileNames);

			foreach (Entry entry in waveArchive.Entries)
			{
				using FileStream entryStream = new(Path.Combine(entriesPath, entry.FileName), FileUtils.OpenReadFileStreamOptions);
				waveArchive.Pack(entry, entryStream);
			}
		}

		using WaveArchive archive = WaveArchive.Read(new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset > 0);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public async Task WriteWaveArchiveAsync(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await using (FileStream archiveStream = new(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenWriteFileStreamOptions))
		{
			int entryCount = GetFilePaths(entriesPath, out string[] entryFileNames);

			await using WaveArchive waveArchive = WaveArchive.Create(entryCount, archiveStream, entryFileNames);

			foreach (Entry entry in waveArchive.Entries)
			{
				await using FileStream entryStream = new(Path.Combine(entriesPath, entry.FileName), FileUtils.AsyncOpenReadFileStreamOptions);
				await waveArchive.PackAsync(entry, entryStream);
			}
		}

		await using WaveArchive archive = await WaveArchive.ReadAsync(new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset > 0);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData.Span));
		});
	}

	private static int GetFilePaths(string path, out string[] result)
	{
		if (!Directory.Exists(path))
		{
			result = [path];

			return 1;
		}

		result = Directory.GetFiles(path, "*", FileUtils.RecursiveEnumerationOptions);

		return result.Length;
	}

	public void Dispose() => File.Delete(ARCHIVE_OUTPUT_PATH);
}
