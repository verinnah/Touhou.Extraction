using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh03Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "0016.PI", 0xbd02e139293edfe8 },
		{ "00EX.PI", 0x1e3a6dee9181e33a },
		{ "00MM.M", 0xb2b133173d730d06 },
		{ "00SL.CD2", 0xf0a7e69cb7fb2446 },
		{ "0116.PI", 0xd9e3ebe24f3e88f8 },
		{ "01EX.PI", 0x6ea841650b3508d6 },
		{ "01MM.M", 0x9dc239e007d07f80 },
		{ "0216.PI", 0xf0c023832b28c88e },
		{ "02EX.PI", 0xfd78c4fabe5706a4 },
		{ "02MM.M", 0xebed8bf66a24c616 },
		{ "02SL.CD2", 0xe56ef6ad19215bb6 },
		{ "0316.PI", 0x84df2d2add2f85b0 },
		{ "03EX.PI", 0x5f9f1a4ede4e7a2a },
		{ "03MM.M", 0x3284429dc5644f84 },
		{ "0416.PI", 0x6f7d8f135d940613 },
		{ "04EX.PI", 0x46b49d0d66357600 },
		{ "04MM.M", 0x8fdf9c43295393c },
		{ "04SL.CD2", 0xb0e1b2478ccf1225 },
		{ "0516.PI", 0xd69f8f86a908a9e2 },
		{ "05EX.PI", 0x6ac208ef89c2b2ce },
		{ "05MM.M", 0x6a2b5e67d4618ee6 },
		{ "0616.PI", 0x299918b89fa7c6e5 },
		{ "06EX.PI", 0x97f9985de5ad5324 },
		{ "06MM.M", 0x20e2693890e58dc9 },
		{ "06SL.CD2", 0x7bed9e8fc5175782 },
		{ "0716.PI", 0xecab5ef15739cad },
		{ "07EX.PI", 0xc400ea96998b7855 },
		{ "07MM.M", 0x5b64629d88b2e513 },
		{ "0816.PI", 0x6fdfa55de972b1b4 },
		{ "08EX.PI", 0x248047beb65ce8fc },
		{ "08MM.M", 0x654398049060f158 },
		{ "08SL.CD2", 0x5f6675a64bf050cd },
		{ "0916.PI", 0xb32636db26a6f77f },
		{ "09EX.PI", 0xccc2ad0d699ba5a7 },
		{ "1016.PI", 0xeffa1408b9388f8c },
		{ "10EX.PI", 0x43b12d9d83699d1c },
		{ "10SL.CD2", 0xc03320fde30d0b4a },
		{ "1116.PI", 0x47898a466d807d4e },
		{ "11EX.PI", 0x4ae67ee3c1467192 },
		{ "1216.PI", 0x68bb1751b32b5448 },
		{ "12EX.PI", 0x597dd878b93ea08c },
		{ "12SL.CD2", 0x93ba2561ec67b669 },
		{ "1316.PI", 0xe1acb58ecdf59da3 },
		{ "13EX.PI", 0xca803fd2adab8d8b },
		{ "1416.PI", 0x4f590801ade64bc5 },
		{ "14EX.PI", 0x159729f6f3531a09 },
		{ "14SL.CD2", 0xd8c90f9a54a23222 },
		{ "1516.PI", 0x16564c254ebf1bfb },
		{ "15EX.PI", 0x463130a0a678537d },
		{ "1616.PI", 0x40c67e18a37adc94 },
		{ "16EX.PI", 0x8c72e042b0aef275 },
		{ "16SL.CD2", 0x8c9604a6da045c42 },
		{ "1716.PI", 0xb3d649268452c57a },
		{ "17EX.PI", 0xe7fb58feb33abd28 },
		{ "99SL.CDG", 0xd6ba367bac763c61 },
		{ "@00DM0.TXT", 0x5a66d46aa940f604 },
		{ "@00DM1.TXT", 0x233682eda1006ee7 },
		{ "@00ED.TXT", 0xb4c8f7c7658cc655 },
		{ "@00TX.TXT", 0x40348d0d77a9af4 },
		{ "@01DM0.TXT", 0xb6e3b62be25f4f25 },
		{ "@01DM1.TXT", 0x5d83ce610c82eb55 },
		{ "@01ED.TXT", 0x2831a1fbecaa9cda },
		{ "@01TX.TXT", 0x8c55d620e6908ddb },
		{ "@02DM0.TXT", 0x7c5c7bca239caf56 },
		{ "@02DM1.TXT", 0x8a2eb4f7a9e64e5c },
		{ "@02ED.TXT", 0xdbd82da216de6f40 },
		{ "@02TX.TXT", 0x275a3a01446fdc67 },
		{ "@03DM0.TXT", 0x79b2aace7c9e6cb6 },
		{ "@03DM1.TXT", 0x7d47054922ee85c },
		{ "@03ED.TXT", 0xb899903127292b0f },
		{ "@03TX.TXT", 0x3e5f80432c2a05b6 },
		{ "@04DM0.TXT", 0x457e2b7b970bb0c6 },
		{ "@04DM1.TXT", 0x881e7984f3a15397 },
		{ "@04ED.TXT", 0xb33a60351cc5a25a },
		{ "@04TX.TXT", 0xe312427e5ba6258d },
		{ "@05DM0.TXT", 0x9b737e5f1f109fb6 },
		{ "@05DM1.TXT", 0x331634372a9935a2 },
		{ "@05ED.TXT", 0xa7f7cbd955f85ed0 },
		{ "@05TX.TXT", 0xbc16eb2b64958194 },
		{ "@06DM0.TXT", 0x78c7b6818d311289 },
		{ "@06DM1.TXT", 0xb2ad1eb542d1615e },
		{ "@06ED.TXT", 0x78f45e7033b5939e },
		{ "@06TX.TXT", 0x43fb0ed60cf16dfb },
		{ "@07DM0.TXT", 0x6e2b1c830758f46d },
		{ "@07DM1.TXT", 0x133a2523ba2821de },
		{ "@07ED.TXT", 0x5bbcda5f069c13f0 },
		{ "@07TX.TXT", 0x456824f55909dde2 },
		{ "@08DM0.TXT", 0x1e859315acb44c0e },
		{ "@08DM1.TXT", 0x8275ab72f9d6b55f },
		{ "@08ED.TXT", 0xfa427c62f5290b32 },
		{ "@08TX.TXT", 0x382b4ac067ca05bd },
		{ "@99ED.TXT", 0x7b1cadfb80ab6f67 },
		{ "CHNAME.BFT", 0x4cfc7c0dce30feb1 },
		{ "CONTI.CD2", 0xbae3f3002b7f9ce7 },
		{ "CONTI.PI", 0x16ea5a8430672b2c },
		{ "DEC.M", 0x2cd7557357ad64b3 },
		{ "DEMO1.M", 0x238bbe78a8d59a30 },
		{ "DEMO2.M", 0x2449c6f0ae1db806 },
		{ "DEMO3.M", 0xfad1abcfb71b020d },
		{ "DEMO4.M", 0xe1d81b5583550857 },
		{ "DEMO5.M", 0xad1ec0f5c2b99846 },
		{ "DM1A.PI", 0x53a88420b4c9f9ba },
		{ "DM1B.PI", 0x4951680e83f0cd2c },
		{ "DM1C.PI", 0x9c4c130bbda84b4a },
		{ "DM1D.PI", 0x85a5c9f3e9f79563 },
		{ "DM1E.PI", 0xf0c0dd52710d9232 },
		{ "DM1F.PI", 0x55e88b84ecb1a787 },
		{ "DM2A.PI", 0x7ccd06b4757a3304 },
		{ "DM3A.PI", 0x44aad8f4e8d4a63d },
		{ "DM3B.PI", 0xfe9e5170c494d3d4 },
		{ "DM3C.PI", 0xa218c6e3f697c1e3 },
		{ "DM3D.PI", 0x878a1c2f4c164a20 },
		{ "DM3E.PI", 0x5b584c8e603e1adc },
		{ "DM3F.PI", 0xc2d04d4238f13208 },
		{ "DM3G.PI", 0xdd0b4e85295d699d },
		{ "DM3H.PI", 0x5a92e927f9c505f8 },
		{ "DM3I.PI", 0x5cdc620798f16520 },
		{ "DM3J.PI", 0x70b61900ea573ae8 },
		{ "DM3K.PI", 0x8706ce4868133f9c },
		{ "DM3L.PI", 0x695d10033ffa8834 },
		{ "DM3M.PI", 0x5d9c815805130 },
		{ "DM3N.PI", 0xbe352d6e70e9bcc3 },
		{ "DM3O.PI", 0xcee6a79c8c130296 },
		{ "DM3P.PI", 0x66ed90ae1b89ea80 },
		{ "DM4A.PI", 0x8d2a51c28c04df8e },
		{ "ED.M", 0xd5a4592154c67e6d },
		{ "EDBK1.RGB", 0x86c1f9a4067f0eae },
		{ "EN2.PI", 0xc2fd725a9cbd6e9e },
		{ "ENEMY00.PI", 0x379c3b2960a490db },
		{ "ENEMY01.PI", 0xf1228c47b5ea19ec },
		{ "ENEMY02.PI", 0x6d45d51921bae01e },
		{ "ENEMY03.PI", 0x15867bcb18e3cad8 },
		{ "ENEMY04.PI", 0x5904c0f7fc466307 },
		{ "LOGO.CD2", 0xde940ffb45d8a8b1 },
		{ "LOGO0.RGB", 0x725308784ae848a8 },
		{ "LOGO1.RGB", 0x3ed67157be0961ec },
		{ "LOGO5.CDG", 0x2eeec0d24cf21c28 },
		{ "MIKOFT.BFT", 0x135965878426e1de },
		{ "MUSIC.TXT", 0xe2d82a4becc41ec8 },
		{ "OP.M", 0xe71c65496eb41f74 },
		{ "OP3.PI", 0x608773096d0c3538 },
		{ "OPWIN.BFT", 0xc09f2bd20a7493e6 },
		{ "OVER.M", 0xb271f3144ccc1fa5 },
		{ "OVER.PI", 0x4603db95c1c4fab9 },
		{ "REGI1.BFT", 0x9fba4858bbb0b341 },
		{ "REGI2.BFT", 0x1d816457e0665190 },
		{ "REGIB.PI", 0x810e5c784a5dd4ce },
		{ "RFT0.CDG", 0x9b1f8f9a04c6657c },
		{ "RFT1.CDG", 0xe72c596bad72332b },
		{ "RFT2.CDG", 0x9261f8e264d38aa7 },
		{ "RFT3.CDG", 0x4cf4f8b2d8c89ba },
		{ "SCORE.M", 0x1db2f116f871548b },
		{ "SELECT.M", 0xee9938395ed8f4e1 },
		{ "SLEX.CD2", 0xddac7b3d11df180d },
		{ "SLEX.CDG", 0xeacf36886f37c5e1 },
		{ "SLWIN.CDG", 0xdf3bb22d9ccd4b5a },
		{ "ST.CD2", 0xde318b7f71546c87 },
		{ "STF1.CDG", 0xcc03f40cc03ed822 },
		{ "STF10.CDG", 0x5714cc0539b84562 },
		{ "STF11.CDG", 0x58d65cda1d1f3e67 },
		{ "STF12.CDG", 0xe10795ce59dc2f7e },
		{ "STF2.CDG", 0x6031b36068eca6fb },
		{ "STF3.CDG", 0xdb6384e8c11d00b4 },
		{ "STF4.CDG", 0xfb385ecdb74336e8 },
		{ "STF5.CDG", 0xa9b332254609ed31 },
		{ "STF6.CDG", 0x24f4c5e5ab752428 },
		{ "STF7.CDG", 0x2f5d33175fd99abc },
		{ "STF8.CDG", 0xf842e7a169e66d7d },
		{ "STF9.CDG", 0x1bc327890d5b5350 },
		{ "STNX0.PI", 0x27973de5f1ac4b8c },
		{ "STNX1.PI", 0x5bd5736b4aaf268a },
		{ "STNX2.PI", 0xfc25f7399ce25672 },
		{ "STNX3.PI", 0xe92ef8e14c059f89 },
		{ "STNX4.PI", 0x3e9f26c225f58745 },
		{ "STNX5.PI", 0xd6101bb2ba5435cd },
		{ "TL01.PI", 0x7113182e6f8a496a },
		{ "TL02.PI", 0x5294400197e1c1e4 },
		{ "TLSL.RGB", 0x4e0c6bcbf3644295 },
		{ "WIN.M", 0x69a8ff5e8028be28 },
		{ "YUME.EFC", 0xbfbab22ac5f4cb57 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th03";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\夢時空1-TEST.DAT";

	public ArchiveTh03Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\夢時空1.DAT")]
	public void ReadArchiveTh03(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.PoDD, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\夢時空1.DAT", true)]
	public async Task ReadArchiveTh03Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.PoDD, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh03(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.PoDD, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.PoDD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh03Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.PoDD, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.PoDD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
