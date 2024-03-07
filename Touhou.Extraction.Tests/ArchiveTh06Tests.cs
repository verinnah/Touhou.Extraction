using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh06Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ecldata1.ecl", 0xbdf10e89c2d48f58 },
		{ "ecldata2.ecl", 0xaf34518dadf6efc8 },
		{ "ecldata3.ecl", 0xd1a42a64e2b91a52 },
		{ "ecldata4.ecl", 0xafb16cf4ff772b62 },
		{ "ecldata5.ecl", 0x7cbfff927959bfac },
		{ "ecldata6.ecl", 0xadf7577b77b6d4cf },
		{ "ecldata7.ecl", 0x2fc09414a17054cf },
		{ "eff01.anm", 0x7aa62bb9fa21f69c },
		{ "eff01.png", 0x3669f452120b1312 },
		{ "eff02.anm", 0xdc027f30fdf4cd99 },
		{ "eff02.png", 0x66e4a39c8f45978c },
		{ "eff03.anm", 0x8c82558ab5d2ff4c },
		{ "eff03.png", 0x785b036edea47cb8 },
		{ "eff04.anm", 0xd3907a65aa425ef7 },
		{ "eff04.png", 0xd5ecd22ff3ee460c },
		{ "eff05.anm", 0xa06bce7cb726c0db },
		{ "eff05.png", 0x6cb26b6425c0a440 },
		{ "eff06.anm", 0x282c5134066c7345 },
		{ "eff06.png", 0xbba58906fad142a5 },
		{ "eff07.anm", 0xe5f671be93b238de },
		{ "eff07.png", 0x324f83a6cda69c38 },
		{ "face03a.anm", 0xe6b2970e2cca6959 },
		{ "face03a.png", 0x7d68e3d434103bcd },
		{ "face03a_a.png", 0x1a05463701015d7a },
		{ "face03b.anm", 0x783dd32d41d6027f },
		{ "face03b.png", 0x89c2cf0144f00246 },
		{ "face03b_a.png", 0x21437277909d0664 },
		{ "face05a.anm", 0xc9ef674026c4474a },
		{ "face05a.png", 0x7a5bfb6950de07d6 },
		{ "face05a_a.png", 0x9b182994091d7cd1 },
		{ "face06a.anm", 0xa45ddecc5fd6d2ad },
		{ "face06a.png", 0x41112a3250abed72 },
		{ "face06a_a.png", 0xb4296c23e7cc006a },
		{ "face06b.anm", 0x17d4ffd05035bcef },
		{ "face06b.png", 0xb4dff1c37a6f7f7f },
		{ "face06b_a.png", 0xbcf5483875745c27 },
		{ "face08a.anm", 0xb060c08dabb2745f },
		{ "face08a.png", 0xbd002f88de8f01a5 },
		{ "face08a_a.png", 0x4eb58c44cfe9a8fc },
		{ "face08b.anm", 0x117c19559171ac58 },
		{ "face08b.png", 0x514eaef1ee5f637d },
		{ "face08b_a.png", 0x9eb0d66fbd257980 },
		{ "face09a.anm", 0x3eefe898644dc2c },
		{ "face09a.png", 0xc047d8ac8b792065 },
		{ "face09a_a.png", 0x9b8582a50b6bb809 },
		{ "face09b.anm", 0x17fbacbc9028b54b },
		{ "face09b.png", 0xb24f6343f9733a70 },
		{ "face09b_a.png", 0xb37beae264b7bee4 },
		{ "face10a.anm", 0x97cd200045dd54fb },
		{ "face10a.png", 0x1eb6595b52e178c9 },
		{ "face10a_a.png", 0xe16b57124329a3c9 },
		{ "face10b.anm", 0xc3b147b51b5e45e5 },
		{ "face10b.png", 0xeb971a6b68ced617 },
		{ "face12a.anm", 0xdbeb9f413ff34889 },
		{ "face12a.png", 0x4c44c583fc152b68 },
		{ "face12a_a.png", 0x3246388f0494253c },
		{ "face12b.anm", 0xb21fb76d67122dd1 },
		{ "face12b.png", 0x540e11b6b865d424 },
		{ "face12c.anm", 0x742474c7460ae7f9 },
		{ "face12c.png", 0x4a9326c911acd8b0 },
		{ "face12c_a.png", 0x47325159e8c4a473 },
		{ "msg1.dat", 0x713d50a0e41c7a76 },
		{ "msg2.dat", 0x4851a68a47abd00e },
		{ "msg3.dat", 0xa5a63de24f1e8896 },
		{ "msg4.dat", 0x95cb079446884bd5 },
		{ "msg5.dat", 0xc923c03187b4c217 },
		{ "msg6.dat", 0x8be9b187eb1b9bc0 },
		{ "msg7.dat", 0xf06f3aba10bad384 },
		{ "stage1.std", 0x686932868d5cb732 },
		{ "stage2.std", 0xb77f33cd9ec8d2a4 },
		{ "stage3.std", 0x682031088f16525c },
		{ "stage4.std", 0x66e67909110e5224 },
		{ "stage5.std", 0xc77ecc5fa36aa286 },
		{ "stage6.std", 0x4524f78541aa1ece },
		{ "stage7.std", 0xa59b2afbd00bbb45 },
		{ "stg1bg.anm", 0x83c5733ff2742b5e },
		{ "stg1bg.png", 0x2bdea19dfd4b0e6d },
		{ "stg1bg_a.png", 0x1962e10e5859bf39 },
		{ "stg1enm.anm", 0xb6bc547c553a3b02 },
		{ "stg1enm.png", 0x76ceb607fd464a34 },
		{ "stg1enm2.anm", 0x5903de0b0d5a64c5 },
		{ "stg1enm2.png", 0x46c138caf96bef6b },
		{ "stg1enm2_a.png", 0xda79f70e1dbfe0bb },
		{ "stg1enm_a.png", 0x40d729dc9c8326ce },
		{ "stg2bg.anm", 0x90d6aa0084a6a55 },
		{ "stg2bg.png", 0xb1c80a72da48f79c },
		{ "stg2enm.anm", 0x97595988dfee7fe1 },
		{ "stg2enm.png", 0xb86ab973f1ab439 },
		{ "stg2enm2.anm", 0x6e4befb347efe1a },
		{ "stg2enm2.png", 0x75bef68f365a38f6 },
		{ "stg2enm2_a.png", 0x812f28baa53bbce6 },
		{ "stg2enm_a.png", 0x5a9ff5740f7deedf },
		{ "stg3bg.anm", 0xfeb276c64d3ba16a },
		{ "stg3bg.png", 0x1c157d4fdeffce30 },
		{ "stg3bg_a.png", 0x47614a3c1184dac2 },
		{ "stg3enm.anm", 0xfd443668771f8e2f },
		{ "stg3enm.png", 0x54b99bc997193496 },
		{ "stg3enm_a.png", 0xc72ed84b04f0ad84 },
		{ "stg4bg.anm", 0xc425c7e5bce82aaa },
		{ "stg4bg.png", 0xff87fe455a554cd4 },
		{ "stg4bg_a.png", 0x4435b6f075735b49 },
		{ "stg4enm.anm", 0xa96757a32dce8ae0 },
		{ "stg4enm.png", 0x8e0aed53fb7daf9d },
		{ "stg4enm_a.png", 0xb353fba94aabdef6 },
		{ "stg5bg.anm", 0x5e309c34e9ab1ba6 },
		{ "stg5bg.png", 0x403c5350979ffbd7 },
		{ "stg5bg_a.png", 0x3672f41460a1642 },
		{ "stg5enm.anm", 0x5cec2b25d729b9d5 },
		{ "stg5enm.png", 0x8e0aed53fb7daf9d },
		{ "stg5enm2.anm", 0x339c6eb7d94510a2 },
		{ "stg5enm2.png", 0x23b6e2a1e8cf9332 },
		{ "stg5enm2_a.png", 0x5544d07975ffcf5b },
		{ "stg5enm_a.png", 0xb353fba94aabdef6 },
		{ "stg6bg.anm", 0x160459d2b7f002a0 },
		{ "stg6bg.png", 0x97592865b03d38e8 },
		{ "stg6enm.anm", 0x462772c3a68adcb9 },
		{ "stg6enm.png", 0x2f3090205860a41d },
		{ "stg6enm2.anm", 0x620a16d68f0e4b4d },
		{ "stg6enm2.png", 0x26f6c242d79812af },
		{ "stg6enm2_a.png", 0x7cd2acbe84cf39c8 },
		{ "stg6enm_a.png", 0x3a23127cf181a9c2 },
		{ "stg7bg.anm", 0xaf18feb1e66da2c8 },
		{ "stg7bg.png", 0x5839f0cfca65e068 },
		{ "stg7bg_a.png", 0x7fb6e10ecee7ed14 },
		{ "stg7enm.anm", 0x65c22f9156fdc18e },
		{ "stg7enm.png", 0x3f80bc5e3009cb19 },
		{ "stg7enm2.anm", 0xdf2156914cabc7c9 },
		{ "stg7enm2.png", 0x9f369d8bd13f66c9 },
		{ "stg7enm2_a.png", 0xcd9fd30c0b945838 },
		{ "stg7enm_a.png", 0xbd0176e0c2522c62 },
		{ "ver0102.dat", 0x2afda711c422ed86 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th06";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\紅魔郷ST-TEST.DAT";

	public ArchiveTh06Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\紅魔郷ST.DAT")]
	public void ReadArchiveTh06(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.EoSD, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH06.Archive>(archive);
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
	[InlineData($"{TEST_PATH}\\紅魔郷ST.DAT", true)]
	public async Task ReadArchiveTh06Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.EoSD, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH06.Archive>(archive);
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
	public void WriteArchiveTh06(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.EoSD, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.EoSD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh06Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.EoSD, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.EoSD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
