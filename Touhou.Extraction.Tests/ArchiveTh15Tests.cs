using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh15Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x8f838e3ae7926960 },
		{ "ascii_1280.anm", 0xf90479e7b50b3714 },
		{ "ascii_960.anm", 0x57ec0f3d06896030 },
		{ "bullet.anm", 0x232683d1a13fd8fd },
		{ "default.ecl", 0x4e70fae63f261d10 },
		{ "demo0.rpy", 0x6a4f5852491caaa },
		{ "demo1.rpy", 0xb285812347935cc2 },
		{ "demo2.rpy", 0xab6dd9438799f90d },
		{ "e01.anm", 0x624842282a6a1ca9 },
		{ "e01.msg", 0x3181a92352b86357 },
		{ "e02.anm", 0x50f6e838f1380351 },
		{ "e02.msg", 0xd549ac54b3a52885 },
		{ "e03.anm", 0xa162b6dc88da48fa },
		{ "e03.msg", 0x7e3daf949f5ab227 },
		{ "e04.anm", 0xade8a76fab292df5 },
		{ "e04.msg", 0xdc076bef0984fd98 },
		{ "e05.anm", 0x229333d4d1cf3257 },
		{ "e05.msg", 0x240c73b529081b11 },
		{ "e06.anm", 0x65752417ccdbebfa },
		{ "e06.msg", 0xac742686e5579907 },
		{ "e07.anm", 0xbead72a929fccfe3 },
		{ "e07.msg", 0x955bfe956ab9d87b },
		{ "e08.anm", 0xcfd9f6cc9c5cb719 },
		{ "e08.msg", 0x5af7e82efb6cb5a9 },
		{ "effect.anm", 0x94ac8c95a24506d4 },
		{ "enemy.anm", 0xe54e019a836377cb },
		{ "front.anm", 0x18a0c520fafaf056 },
		{ "help.anm", 0x48345cc961e770d0 },
		{ "help_01.png", 0x68a548ca10c6dd8a },
		{ "help_02.png", 0x89db7c4951bf69c3 },
		{ "help_03.png", 0xfc39ba078c74ff04 },
		{ "help_04.png", 0x532a9347a63110de },
		{ "help_05.png", 0xb44a463d1249a522 },
		{ "help_06.png", 0x5d5956ea0f897933 },
		{ "help_07.png", 0x57854e37930ca262 },
		{ "help_08.png", 0x4f54f7ff99c985e9 },
		{ "help_09.png", 0x7e91be7ffdbf4dcd },
		{ "musiccmt.txt", 0xad94c38db5b92dc2 },
		{ "pl00.anm", 0xb4273680c02f7225 },
		{ "pl00.sht", 0x461e724e903a1c3c },
		{ "pl01.anm", 0x7ba144f4c25e6836 },
		{ "pl01.sht", 0xb4513fcaf305411d },
		{ "pl02.anm", 0x8ed5b9a24dcbd544 },
		{ "pl02.sht", 0x6441da12656dd9a8 },
		{ "pl03.anm", 0xfc1c0dbd76c2ef99 },
		{ "pl03.sht", 0xb50f4184ade1fbfe },
		{ "se_big.wav", 0xfb4ba3e45749198c },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_bonus4.wav", 0xab65a052ae231a27 },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_ch02.wav", 0x4514aa1d8424c7e2 },
		{ "se_ch03.wav", 0x336f51977bc49eca },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_don00.wav", 0x63c0b2aeacd1e2a8 },
		{ "se_enep00.wav", 0x4070801026567dbc },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_enep02.wav", 0xccecfaf28e083794 },
		{ "se_etbreak.wav", 0x431e75acdafd24d6 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_extend2.wav", 0x8c587999087b4057 },
		{ "se_fault.wav", 0xc39c65e2e184195a },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_heal.wav", 0xa7e6ed8a265161c5 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_item01.wav", 0x483a913fdaf532aa },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_lazer02.wav", 0x4a056aa97b4c53d3 },
		{ "se_lgods1.wav", 0x826111b4bd43576a },
		{ "se_lgods2.wav", 0x7f5ab5a28d4bf7ec },
		{ "se_lgods3.wav", 0x71e9cd949e97a969 },
		{ "se_lgods4.wav", 0xad856a2857b1f82c },
		{ "se_lgodsget.wav", 0xa5d3140d23ae97a },
		{ "se_msl.wav", 0xa051455a80988408 },
		{ "se_msl2.wav", 0xf7ad9edb01f32546 },
		{ "se_msl3.wav", 0xd049a9c6c2ed2ac5 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_nodamage.wav", 0xc186b0bb70f73fb8 },
		{ "se_noise.wav", 0x883a93ad91100247 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_pin00.wav", 0xba49fda931e39a6 },
		{ "se_pin01.wav", 0x6c9fa1b12d5f1b45 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_pldead01.wav", 0xb21e8f6cf05e07c },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xa8311a42586a4915 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0xb66702f933cb7b14 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_tan03.wav", 0x73387ed64c1c8f3a },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0x87aaae9c90225a91 },
		{ "st01.ecl", 0x790ebde7221de732 },
		{ "st01.std", 0xe3e7bd8bd98d7bf8 },
		{ "st01a.msg", 0x141be05b8b4ca088 },
		{ "st01b.msg", 0xa4e5ef6c370faedc },
		{ "st01bs.ecl", 0xbff476f4a752d056 },
		{ "st01c.msg", 0x450a570b5d17d678 },
		{ "st01d.msg", 0x7c2e11155b3642a5 },
		{ "st01enm.anm", 0x87d10886e3d696f3 },
		{ "st01logo.anm", 0xdcc822f8889effcd },
		{ "st01mbs.ecl", 0x5a0a832331cb050e },
		{ "st01mbs2.ecl", 0xf8adec98e48a9b3a },
		{ "st01wl.anm", 0x35a496376f152cac },
		{ "st02.ecl", 0xe6a44b69662aba50 },
		{ "st02.std", 0xdba80fe3c9af4603 },
		{ "st02a.msg", 0xa0531e5ffdbb395e },
		{ "st02b.msg", 0x9a7f8ce8d64614fc },
		{ "st02bs.ecl", 0xad86b4f2655d6934 },
		{ "st02c.msg", 0xbbb3a6c2c671cc8a },
		{ "st02d.msg", 0xa5497dea4959f4d9 },
		{ "st02enm.anm", 0x866ade1eb582372d },
		{ "st02logo.anm", 0x64d4a166ecb69d1a },
		{ "st02mbs.ecl", 0xabccf02fce94a91 },
		{ "st02wl.anm", 0xee52729772671208 },
		{ "st03.ecl", 0xb8270ca698c55b9b },
		{ "st03.std", 0x3797239d877b463b },
		{ "st03a.msg", 0xe77059b3d4964559 },
		{ "st03b.msg", 0xe727a045e77ade2 },
		{ "st03bs.ecl", 0xf0935b111003eef6 },
		{ "st03c.msg", 0x10d133594389413a },
		{ "st03d.msg", 0x5dce4ec50ae83592 },
		{ "st03enm.anm", 0x26a7d28a0c4a6342 },
		{ "st03logo.anm", 0x383161f200e49e5c },
		{ "st03mbs.ecl", 0xa3dd53526f6e0dc7 },
		{ "st03wl.anm", 0xdc8f0fbe7cfb1e90 },
		{ "st04.ecl", 0xeb68c2ec0ec00250 },
		{ "st04.std", 0xdeccb1b9e5183f5a },
		{ "st04a.msg", 0x57e99702f70cd54c },
		{ "st04b.msg", 0x7d50126c25725dbc },
		{ "st04bs.ecl", 0x215de3a9f7a3e602 },
		{ "st04c.msg", 0xeaecce35247c2aa1 },
		{ "st04d.msg", 0xf8b006c681dcdab7 },
		{ "st04enm.anm", 0xf73fb1da1d78471e },
		{ "st04logo.anm", 0x8d3dd6d438ffb428 },
		{ "st04mbs.ecl", 0x1243da9719cee16e },
		{ "st04wl.anm", 0x89a411c288c1109b },
		{ "st05.ecl", 0x5f1277ce3163e303 },
		{ "st05.std", 0x8032e8dbcaaa8703 },
		{ "st05a.msg", 0x93ccb072b1430df3 },
		{ "st05b.msg", 0x4f67d3b3938556e6 },
		{ "st05bs.ecl", 0x8dc938b30bb9f0ef },
		{ "st05c.msg", 0xcd92d54966fb06b3 },
		{ "st05d.msg", 0x8970c136efb779d8 },
		{ "st05enm.anm", 0xcfe97dfc4ece156c },
		{ "st05logo.anm", 0x4d5c4196469756a5 },
		{ "st05mbs.ecl", 0xc4d69f0f008b9415 },
		{ "st05wl.anm", 0x2063bff7229bf7eb },
		{ "st06.ecl", 0xda2857040705e1d8 },
		{ "st06.std", 0xa0ae66dbd42da4b7 },
		{ "st06a.msg", 0xed495a5b175e9559 },
		{ "st06b.msg", 0x2b9afe419b2fec1a },
		{ "st06bs.ecl", 0x4bb1c5cb592c1855 },
		{ "st06c.msg", 0x929584e2143fa7a7 },
		{ "st06d.msg", 0xed5031d5ef766d7a },
		{ "st06enm.anm", 0xea40d17e17d5c997 },
		{ "st06logo.anm", 0x8d2a412f30dcf8cd },
		{ "st06wl.anm", 0x2e0a5ef77e6ff774 },
		{ "st07.ecl", 0x3dc3e6ed996ae405 },
		{ "st07.std", 0x9e26fd17f4100c23 },
		{ "st07a.msg", 0xa520cfaabf162442 },
		{ "st07b.msg", 0x3812eb9cb8e0a714 },
		{ "st07bs.ecl", 0xc71dd8b84c47b776 },
		{ "st07bs2.ecl", 0xcddb24d8d441fed4 },
		{ "st07c.msg", 0x778708288ef08f58 },
		{ "st07d.msg", 0xca104fee20e1d788 },
		{ "st07enm.anm", 0x371b401bd7314de5 },
		{ "st07enm2.anm", 0xdabd27838dbffa8 },
		{ "st07enm3.anm", 0x281c85ba9ff3efbc },
		{ "st07logo.anm", 0x1b2dfae6e0a4b8ed },
		{ "st07mbs.ecl", 0xffeade216bd56243 },
		{ "st07wl.anm", 0x583a47da034666c },
		{ "staff.anm", 0xef1322ffbdd4bf85 },
		{ "staff1.msg", 0x7b777e836631493c },
		{ "staff2.msg", 0x64140ea0fc15159c },
		{ "staff3.msg", 0xea64834bab9b41a4 },
		{ "staff4.msg", 0xc4e3adf83b41b547 },
		{ "staff5.msg", 0xe1038a95e4cb72eb },
		{ "staff6.msg", 0xe9ee6eb504643d2b },
		{ "text.anm", 0x6a4c1609ebdd8f0b },
		{ "th15_0100b.ver", 0x819599f88192eb15 },
		{ "thbgm.fmt", 0x1fc6cc468e88f78d },
		{ "title.anm", 0x5aa295194db3153b },
		{ "title_v.anm", 0xd41486e67f38e05b }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th15";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th15-test.dat";

	public ArchiveTh15Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th15.dat")]
	public void ReadArchiveTh15(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.LoLK, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH95.THA1>(archive);
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
	[InlineData($"{TEST_PATH}\\th15.dat", true)]
	public async Task ReadArchiveTh15Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.LoLK, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH95.THA1>(archive);
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
	public void WriteArchiveTh15(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.LoLK, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.LoLK, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh15Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.LoLK, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.LoLK, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
