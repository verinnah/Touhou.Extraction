using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh13Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0xde0763a59844e00 },
		{ "astral.anm", 0x6e862dbd1a168657 },
		{ "bullet.anm", 0x971a7fe449192028 },
		{ "default.ecl", 0x937297f40757d384 },
		{ "demo0.rpy", 0x999f2ddfe4aa6cf3 },
		{ "demo1.rpy", 0x20136dd8dc478f0c },
		{ "demo2.rpy", 0xacde185597d45257 },
		{ "demo3.rpy", 0xc186e3463a562e32 },
		{ "e01.anm", 0xda9ce129f877bc2a },
		{ "e01.msg", 0x76c9ce3197370919 },
		{ "e02.anm", 0xd7f18f4fb2e222a },
		{ "e02.msg", 0xe5fb09c6b01dfe1a },
		{ "e03.anm", 0xcafb4887c69b928 },
		{ "e03.msg", 0xb4c7349ed0704c5a },
		{ "e04.anm", 0x2f7715ddb0120661 },
		{ "e04.msg", 0xb51740c0704be21e },
		{ "e05.anm", 0x26148f0a708fe277 },
		{ "e05.msg", 0x4689beb0ce41b63 },
		{ "e06.anm", 0x76d5d6ba05cfcd2e },
		{ "e06.msg", 0x15f7661e9be46bda },
		{ "e07.anm", 0x2e240985728d442d },
		{ "e07.msg", 0x1f69a50084a3d4b3 },
		{ "e08.anm", 0x829e2ee239bb4769 },
		{ "e08.msg", 0x8868938abc4c7003 },
		{ "e09.anm", 0x60ed7d1d26a2cfad },
		{ "e09.msg", 0xd1d6f7ceb7df51f4 },
		{ "e10.anm", 0xc20cd6e09761fb62 },
		{ "e10.msg", 0x497791b323ea5018 },
		{ "e11.anm", 0x931cf2a1bff7759d },
		{ "e11.msg", 0x5c4f10bb68e48091 },
		{ "e12.anm", 0x3b99443cb616f0ff },
		{ "e12.msg", 0x5fc7cb1018cda865 },
		{ "effect.anm", 0x70fc6bb321196952 },
		{ "enemy.anm", 0x7f931cfba6f91e0a },
		{ "front.anm", 0xc482ef508d6f5d72 },
		{ "help.anm", 0x712aa960dbe7056a },
		{ "help_01.png", 0x56c62919c5ae4368 },
		{ "help_02.png", 0x4160e9c9f5a9c03e },
		{ "help_03.png", 0x44232d9240e114f4 },
		{ "help_04.png", 0xa1dc9486a0062a74 },
		{ "help_05.png", 0xfcc7c0f7d1350d00 },
		{ "help_06.png", 0xad94a9b6f9a9b598 },
		{ "help_07.png", 0xa6b52cb6000d98b },
		{ "help_08.png", 0xe4b193842fb90f1e },
		{ "musiccmt.txt", 0x9c8b08975e9f5a4a },
		{ "pl00.anm", 0x41f17a970d9f3072 },
		{ "pl00a.sht", 0xc90cd8651bdc40e },
		{ "pl01.anm", 0xf236ac4578257aff },
		{ "pl01a.sht", 0x2d0d52d5e0e24e0 },
		{ "pl02.anm", 0x3a947734960d21c5 },
		{ "pl02a.sht", 0x23b42a6c59fbf500 },
		{ "pl03.anm", 0xe445e4fb1673f97c },
		{ "pl03a.sht", 0x23b28ad52288be16 },
		{ "se_astralup.wav", 0xeee902d3b061285f },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
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
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_extend2.wav", 0x8c587999087b4057 },
		{ "se_fault.wav", 0xc39c65e2e184195a },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_heal.wav", 0xa7e6ed8a265161c5 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
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
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "sig.anm", 0xf31ff450daf963b6 },
		{ "st01.ecl", 0x117a717a3af0c8ae },
		{ "st01.std", 0x847d3fb5f395193d },
		{ "st01a.msg", 0x2c9ee8dc453f96a0 },
		{ "st01b.msg", 0x527ea6bd191620a2 },
		{ "st01bs.ecl", 0x8e709b69d69c8a71 },
		{ "st01c.msg", 0xd15c9b05012398e6 },
		{ "st01d.msg", 0x2354b797f9229393 },
		{ "st01enm.anm", 0xd610fbe1959caeb9 },
		{ "st01logo.anm", 0x2c6ef1da5584d667 },
		{ "st01wl.anm", 0xeab0bc1c682a96aa },
		{ "st02.ecl", 0x85be88fe0aba9c3f },
		{ "st02.std", 0x43159e4e00f8f8fe },
		{ "st02a.msg", 0xa8f46c92a8e6664a },
		{ "st02b.msg", 0xe1307ead0d47fcc6 },
		{ "st02bs.ecl", 0xafbe425186dbabbc },
		{ "st02c.msg", 0x3292d8e5207cb5bd },
		{ "st02d.msg", 0x7c870182a2234d74 },
		{ "st02enm.anm", 0x42861b87d16813b2 },
		{ "st02logo.anm", 0x3ca967d51eaa3d1c },
		{ "st02mbs.ecl", 0x17eb56e73c773233 },
		{ "st02wl.anm", 0x32e4382d53ba7d37 },
		{ "st03.ecl", 0x3638d4dff98d87fc },
		{ "st03.std", 0xd1577abdc5eb7b36 },
		{ "st03a.msg", 0x6c8fa5b9893a136e },
		{ "st03b.msg", 0xd4f1cfa0da66617a },
		{ "st03bs.ecl", 0xefb1b40f32f61684 },
		{ "st03c.msg", 0x30bc76e2b939208 },
		{ "st03d.msg", 0x20ac8ae8eece4068 },
		{ "st03enm.anm", 0xdaa8f4a9d921b829 },
		{ "st03logo.anm", 0xf42b5f8db0400e7c },
		{ "st03mbs.ecl", 0xd24b7e844c354529 },
		{ "st03menm.anm", 0x2752913b767a7505 },
		{ "st03wl.anm", 0x84644473024a4844 },
		{ "st04.ecl", 0x2bc4be4cc6df4452 },
		{ "st04.std", 0xa0e917c3d270f09a },
		{ "st04a.msg", 0xa4716bbce46dfc9c },
		{ "st04b.msg", 0x53f7e67304df4d94 },
		{ "st04bs.ecl", 0xe357bc3f632e1ec6 },
		{ "st04c.msg", 0x8c29ec1f61853f3b },
		{ "st04d.msg", 0x9e84913a929df5ab },
		{ "st04enm.anm", 0x83b6eac93577799d },
		{ "st04logo.anm", 0x10d52c23cb0fdad9 },
		{ "st04mbs.ecl", 0xa928c739d7ef498e },
		{ "st04menm.anm", 0x6afe26f39e4bb4dc },
		{ "st04wl.anm", 0x4f86a63864004082 },
		{ "st05.ecl", 0x3bd0f09fd02dc78c },
		{ "st05.std", 0x1209784b15c15950 },
		{ "st05a.msg", 0xe3a4684e7668a26f },
		{ "st05b.msg", 0x9353cfd06932e710 },
		{ "st05bs.ecl", 0x9f77e8004f0ee08f },
		{ "st05c.msg", 0xcaf95f75871e656a },
		{ "st05d.msg", 0x94c95cd40ffd379b },
		{ "st05enm.anm", 0xaedb7cfbaea974ea },
		{ "st05logo.anm", 0x3c46ee4d43a69494 },
		{ "st05mbs.ecl", 0x20ffd4cc94661b0a },
		{ "st05menm.anm", 0x1b41602e1be23752 },
		{ "st05wl.anm", 0x83c528e48db9137a },
		{ "st06.ecl", 0xbcc07ff4116afc06 },
		{ "st06.std", 0xa8c9965349d40c22 },
		{ "st06a.msg", 0x55e6d73cdfe6a483 },
		{ "st06b.msg", 0x6de16d8045512bac },
		{ "st06bs.ecl", 0xce1a6e39ad7793d4 },
		{ "st06c.msg", 0x7e2b274cf42e1ef5 },
		{ "st06d.msg", 0x371f48956c76302e },
		{ "st06enm.anm", 0x47ece14cccf4bafe },
		{ "st06enm2.anm", 0x74aa6a54e347e426 },
		{ "st06enm3.anm", 0xbea5537aca63db32 },
		{ "st06logo.anm", 0xdf7e61cfffc9ede5 },
		{ "st06mbs.ecl", 0x9b67492df77f8e5a },
		{ "st06wl.anm", 0xd67cf865e9a3be17 },
		{ "st07.ecl", 0xa386964f8f584ee6 },
		{ "st07.std", 0x20b50668fd985b7f },
		{ "st07a.msg", 0x6391cbcb104bbef1 },
		{ "st07b.msg", 0x2fdc245f9d2d7339 },
		{ "st07bs.ecl", 0x394852370033accd },
		{ "st07c.msg", 0x79d152e251eadd },
		{ "st07d.msg", 0x1eb53d8b701f06f5 },
		{ "st07enm.anm", 0x22a3982e09f294af },
		{ "st07logo.anm", 0xe04500a6b2705803 },
		{ "st07mbs.ecl", 0xb681c4c16d8d600f },
		{ "st07menm.anm", 0xb6bcf6923e8b21c6 },
		{ "st07wl.anm", 0x9a9e796be0b848e4 },
		{ "staff.anm", 0xe87094cd7d10140c },
		{ "staff.msg", 0x2dab88c09ea6d7eb },
		{ "text.anm", 0xcec6169c1cccfc39 },
		{ "th13_0100c.ver", 0x7d99f010cdc4ce8d },
		{ "thbgm.fmt", 0x994b1bdd929550ce },
		{ "title.anm", 0x430db99f62dbbd1b },
		{ "title_v.anm", 0x5017e51370b27364 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th13";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th13-test.dat";

	public ArchiveTh13Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th13.dat")]
	public void ReadArchiveTh13(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.TD, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th13.dat", true)]
	public async Task ReadArchiveTh13Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.TD, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh13(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.TD, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.TD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh13Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.TD, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.TD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
