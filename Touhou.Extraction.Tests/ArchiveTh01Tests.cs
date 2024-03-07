using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh01Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "BOSS1.BOS", 0xf95b247c4e53b27f },
		{ "BOSS5_GR.GRC", 0xe9cb5ce351ac97c1 },
		{ "BOSS1_2.BOS", 0x3d60e5e94e1eeb8b },
		{ "BOSS5_3.BOS", 0x8482dac9d3ffb4d7 },
		{ "BOSS5_2.BOS", 0xd9b6baa2a5f95df4 },
		{ "BOSS1_3.BOS", 0xf935668cba109f89 },
		{ "BOSS3_M.PTN", 0x89225b79359cf0c8 },
		{ "BOSS5.BOS", 0xb3ad379193bb85ac },
		{ "BOSS2.BOS", 0x3082c923e50762f2 },
		{ "BOSS6_1.BOS", 0x5157144ac53b4e7b },
		{ "BOSS3_2.BOS", 0xfe10b92d8dbb993f },
		{ "BOSS3_1.BOS", 0x43a4da24436d6486 },
		{ "BOSS3.BOS", 0x6a801394cbc739a0 },
		{ "BOSS6GR1.GRC", 0xdf91eb7e205cff21 },
		{ "BOSS6GR4.GRC", 0x10aeff47dff580e },
		{ "BOSS6GR3.GRC", 0xdc33a4890aeceee1 },
		{ "BOSS6GR2.GRC", 0x4e624c1810b2b198 },
		{ "BOSS8_E2.BOS", 0xc0faf2fd480a8998 },
		{ "BOSS8_E1.BOS", 0xf6b24357a27eb346 },
		{ "BOSS6_2.BOS", 0x7ba92dd0f2b917ab },
		{ "BOSS6_3.BOS", 0x65fc4615d9782f02 },
		{ "BOSS8_1.BOS", 0x993bb4ca2c0b0ce8 },
		{ "KUZI1.GRC", 0x43111511d344468 },
		{ "MIKO_AC.BOS", 0xa591bb4ca874579 },
		{ "MIKO.PTN", 0x208be5cfccecaad6 },
		{ "MIKO_AC2.BOS", 0xfa6544b1dc53e4ef },
		{ "STAGE3.DAT", 0xbcd0b4d544c3f0d9 },
		{ "STAGE0.DAT", 0xe0b6fe509164c4ed },
		{ "KUZI2.GRC", 0x44235fa02ecba2fd },
		{ "STAGE1.DAT", 0x4012a5f35fbf6ad0 },
		{ "NUMB.PTN", 0x82fba2c5387f9f03 },
		{ "STG.PTN", 0x2c4ddf2a5171c3b9 },
		{ "TAMASII.BOS", 0x7ad6e1e341193f6b },
		{ "TAMASII2.BOS", 0x2b3ec13682bdb9b4 },
		{ "STAGE7.DAT", 0xa6ea0a234588e731 },
		{ "STAGE4.DAT", 0x43dffddad5eef969 },
		{ "STAGE6.DAT", 0xdf69918de34f5fe4 },
		{ "STAGE5.DAT", 0x95e1b02e9e22c4ce },
		{ "STG_B.PTN", 0x436acf740966d486 },
		{ "STAGE2.DAT", 0xe0b762fb29cf8353 },
		{ "TAMAYEN.PTN", 0x1a644b6f1a531ed2 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th01";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th01-test.dat";

	public ArchiveTh01Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\東方靈異.伝")]
	public void ReadArchiveTh01(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.HRtP, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH01.Archive>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.True(entryData.Length >= entry.Size);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\東方靈異.伝", true)]
	public async Task ReadArchiveTh01Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.HRtP, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH01.Archive>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.True(entryData.Length >= entry.Size);
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
	public void WriteArchiveTh01(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.HRtP, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.HRtP, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public async Task WriteArchiveTh01Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.HRtP, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.HRtP, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData.Span));
		});
	}

	public void Dispose() => File.Delete(ARCHIVE_OUTPUT_PATH);
}
