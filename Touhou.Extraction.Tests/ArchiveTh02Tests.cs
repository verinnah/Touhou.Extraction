using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh02Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "BOMB2.PI", 0x529f095dc160515e },
		{ "BOMB1.PI", 0xe4a96c74e7eefb6 },
		{ "ALL.PI", 0x1d4164c0c19b3951 },
		{ "BOSS2.MMD", 0xb1ef6809ae7dc343 },
		{ "BOSS2.M", 0xd15817a5a4d39bbc },
		{ "BOSS1.MMD", 0x3f6f5d1b0e96aa2c },
		{ "BOSS1.M", 0x698af56cf47efbc2 },
		{ "BOMB1.BFT", 0x8160257085187bd0 },
		{ "BOMB3.PI", 0xb0e2913816a8184f },
		{ "BOSS3.MMD", 0xf78487f85f1f0b04 },
		{ "BOMBS.BFT", 0x110967edb54edb6b },
		{ "BOSS4.M", 0xf3ca6dd187287332 },
		{ "BOSS5.M", 0x6ff15ea56099d93d },
		{ "BOSS3.M", 0x58e0172f9ccaddbe },
		{ "DEMO2.REC", 0xe0a51e798b8976a4 },
		{ "BOSS5.MMD", 0x3acfe70c686b57a8 },
		{ "BUT.PI", 0xc16e909141855449 },
		{ "DEMO1.REC", 0x8777465f6487ede9 },
		{ "BOSS4.MMD", 0x5b17c1c4a944bf24 },
		{ "ED03.PI", 0x4f4576ec897ec6b },
		{ "DEMO3.REC", 0x3736eadd17249212 },
		{ "ED02.PI", 0x9ba3d68da9267c6c },
		{ "ED06A.RGB", 0x7e7e5a2f1ba9d15c },
		{ "ED01.PI", 0x82437e93af499a61 },
		{ "ED06.PI", 0x9487f6449f4f3f6b },
		{ "ED04.PI", 0x3a516385b5b14a30 },
		{ "ED05.PI", 0x816a32913af03aaf },
		{ "ED07.PI", 0x37a30d1eff7ecaa4 },
		{ "ED07B.RGB", 0xb529b32134e613a6 },
		{ "ED08.PI", 0xae27b9b36a24f124 },
		{ "ED08B.RGB", 0xa35bbbc22f334e7f },
		{ "ED06B.RGB", 0xd2e12c994d96c99 },
		{ "ED07A.RGB", 0x1cce7e525cb9de6c },
		{ "ED09.PI", 0xe09f6fcce183e05 },
		{ "END1.TXT", 0xed5434b0dbebe2e },
		{ "ED08A.RGB", 0xd46fd69d7a108ec2 },
		{ "END3.TXT", 0xf6aae93d8d374fc8 },
		{ "ENDFT.BFT", 0xf489810de01defd6 },
		{ "EXTRA.PI", 0xc9c56a3414c78e03 },
		{ "ED03A.RGB", 0x8fcba4d74c064c68 },
		{ "ED08C.RGB", 0xcf5985135a661b3d },
		{ "END1.M", 0x247ad31f063f2004 },
		{ "GMINIT.MMD", 0xeae79c8e7c7f0e85 },
		{ "MIKO.BFT", 0x1d4d3efbc80459b5 },
		{ "ED06C.RGB", 0x1cce7e525cb9de6c },
		{ "ENDING.M", 0x89e1a62238e20c70 },
		{ "ENDING.MMD", 0x54bcc7db47a7d3de },
		{ "MIKOFT.BFT", 0x931f139a7524b39c },
		{ "MIKO32.BFT", 0x25b231ff3434220e },
		{ "END2.TXT", 0x2b0b4efd1c315332 },
		{ "MIMA.M", 0xfa9b06b5da3ec8e4 },
		{ "MIKO16.BFT", 0x1a2d0e5cf264bbf2 },
		{ "MIMA.MMD", 0x2c29955714f7e591 },
		{ "END1.MMD", 0xdff5dea66b5015cb },
		{ "MIMA1.BFT", 0x4277a93500780ca1 },
		{ "MIMA.BFT", 0x2bbdb772c20e7c17 },
		{ "MIMA2.BFT", 0x195e1a489b23571 },
		{ "EYE.PI", 0x2c70506077a7bc5c },
		{ "OP.RGB", 0x406ab34f54b91ece },
		{ "MUSIC.TXT", 0x5cbf9d9a95915eca },
		{ "OPA.PI", 0xe8ddbc3c8a9f351 },
		{ "OP.M", 0xe2cdb19e432ca623 },
		{ "OPB.PI", 0x94d68a960405e870 },
		{ "OPC.PI", 0xf080ce7d58036d89 },
		{ "OP_H.BFT", 0x538bc55976dac7a0 },
		{ "OP.PI", 0x5508804985569e2b },
		{ "MIKO_K.MPN", 0xe2c84334347be86c },
		{ "OP2.PI", 0x96d3ac182134726b },
		{ "OP3.PI", 0x60f87c8972e9d93c },
		{ "STAGE0.MAP", 0x18b5d81adbc348e0 },
		{ "HUUMA.EFC", 0xb9f8835b08b584fa },
		{ "STAGE0.M", 0xdd4d1cd31f72a295 },
		{ "STAGE0.MMD", 0xb1df8b760ea0d65e },
		{ "STAGE0.MPN", 0x4a75835f46ef9a9e },
		{ "STAGE0.BBT", 0x93018b2eb8f28be4 },
		{ "STAGE0.BMT", 0xb8760a9589dcb19a },
		{ "STAGE0.TXT", 0x1e7cd4664666dca2 },
		{ "OP.MMD", 0xff534e9791a9f482 },
		{ "STAGE1.BMT", 0x5785bef455b5e36f },
		{ "OP_H.RGB", 0x75b11ffdb9049984 },
		{ "STAGE1.M", 0xe8ae05913a999997 },
		{ "STAGE0.BFT", 0xcb00f91a8b96f33a },
		{ "STAGE1.MMD", 0x96be3e76effb4b0 },
		{ "STAGE0.DT1", 0x2c04b4806e2f5572 },
		{ "STAGE1.MPN", 0xd7c39d8580ceaa05 },
		{ "STAGE1.BFT", 0x4e6dbc6d692d1ad8 },
		{ "STAGE2.BBT", 0xaf9af8c917c277eb },
		{ "STAGE1.BBT", 0xf1ad8bdcb3bd7dcf },
		{ "STAGE2.BFT", 0xe7ef33a1a5deb78e },
		{ "STAGE2.BMT", 0xfc7d93bfa09d7c21 },
		{ "STAGE2.DT1", 0x6d975afb34bb4019 },
		{ "STAGE1.DT1", 0xb61565e649ca62e6 },
		{ "STAGE2.MMD", 0xf384e106b1f0af7a },
		{ "STAGE1.TXT", 0x154dc248cf85bb48 },
		{ "STAGE1.MAP", 0x64f7d62028a8593b },
		{ "STAGE2.M", 0xc9854bef26cb9c18 },
		{ "STAGE2_B.BBT", 0xfda1b0f0e99741ad },
		{ "STAGE2.MAP", 0x8cfe3f40807c0866 },
		{ "STAGE3.BFT", 0x1ab68388372ba5b },
		{ "STAGE3.DT1", 0xf4713bee798af5f },
		{ "STAGE2.TXT", 0x9cc5ee6884aa484b },
		{ "STAGE3.MPN", 0xa1f653ce84689a69 },
		{ "STAGE3.TXT", 0x91236aaf290e4414 },
		{ "STAGE3.BMT", 0xcf055e4706a1db0f },
		{ "STAGE3_B.BTT", 0x50c69f66486d6d0d },
		{ "STAGE3_B.BFT", 0xf0d9ffbd595d84a6 },
		{ "STAGE3.BBT", 0x9b9669bc90eacc18 },
		{ "STAGE4.BBT", 0xc1965f2b7111febf },
		{ "STAGE3.M", 0x26d80022c9b9d727 },
		{ "STAGE4.BFT", 0x868fcae72312fcad },
		{ "STAGE3.MAP", 0x5b5842299a195533 },
		{ "STAGE4.MAP", 0x33baf4e8baa33203 },
		{ "STAGE3.MMD", 0x5468d4a97688f34c },
		{ "STAGE4.MMD", 0x81b3ea25d0308d46 },
		{ "STAGE4.M", 0xdf2055f44e351870 },
		{ "STAGE4.BMT", 0x4b0dce0a28964273 },
		{ "STAGE4.MPN", 0x3f466ad37ffad0fd },
		{ "STAGE5.BBT", 0xc1965f2b7111febf },
		{ "STAGE4.DT1", 0xb0300016aea8416e },
		{ "STAGE5.BFT", 0xaf7fa72939e21a83 },
		{ "STAGE4.TXT", 0xdc47ae3f50dc85ec },
		{ "STAGE5.DT1", 0x84fdbf010d2c02f5 },
		{ "STAGE5.BMT", 0x4b0dce0a28964273 },
		{ "STAGE5.MMD", 0x5e08d9797610e449 },
		{ "STAGE5.MAP", 0x1f3fa08daa743602 },
		{ "STAGE5.MPN", 0x21bb40bd1f141dab },
		{ "STAGE5.M", 0x1710ba44411863ec },
		{ "STAGE5B1.BFT", 0x830be88da58337e8 },
		{ "STAGE5B2.BFT", 0x59409a53171cdb7f },
		{ "TS2.PI", 0x3ab6cae5c89fdc3a },
		{ "TS1.PI", 0x5fad45f285502d19 },
		{ "TS3.PI", 0x432418cdc8b787d1 },
		{ "STAGE5.TXT", 0x4b8363f6a2533630 },
		{ "TSELECT.PI", 0xd1d89c07674c1129 },
		{ "STAGE2.MPN", 0xf629d9b9686fd41 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th02";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th02-test.dat";

	public ArchiveTh02Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\東方封魔.録")]
	public void ReadArchiveTh02(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.SoEW, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\東方封魔.録", true)]
	public async Task ReadArchiveTh02Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.SoEW, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh02(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.SoEW, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.SoEW, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh02Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.SoEW, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.SoEW, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
