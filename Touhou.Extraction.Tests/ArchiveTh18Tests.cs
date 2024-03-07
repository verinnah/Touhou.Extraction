using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh18Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "abcard.anm", 0x904fb3d1c2ec8b18 },
		{ "ability.anm", 0x34e7ef79f1bdaa3e },
		{ "ability.txt", 0x618a03f9e74a3d9b },
		{ "abmenu.anm", 0x58aad8568a30cdad },
		{ "ascii.anm", 0x46ea09ec90393a6d },
		{ "ascii1280.anm", 0x6ccca507b92f131d },
		{ "ascii_960.anm", 0xbb80502620eb9cd1 },
		{ "bullet.anm", 0x2046cfaf618bc708 },
		{ "default.ecl", 0x6a9717c907d7493d },
		{ "demo1.rpy", 0xb65497b6e4d9ad77 },
		{ "demo2.rpy", 0xcc88e95f77cc6250 },
		{ "demo3.rpy", 0x9be80d8c7252fe86 },
		{ "demo4.rpy", 0x5e52c4c26b74c729 },
		{ "e01.anm", 0x8fc8c31b6cf627c0 },
		{ "e01.msg", 0x2ff04681847137b },
		{ "e02.anm", 0x1dc2941999ff78ea },
		{ "e02.msg", 0x6ada9a14b925d95d },
		{ "e03.anm", 0x9bdc1c608ffc08bf },
		{ "e03.msg", 0x902c4344421a06a7 },
		{ "e04.anm", 0x7e91f7fef655e485 },
		{ "e04.msg", 0xed2b84e21ef83837 },
		{ "e05.anm", 0x4317d172cc2773f0 },
		{ "e05.msg", 0xfc9bf0560cc8b44e },
		{ "e06.anm", 0x67a8e2b9bcdcc9ba },
		{ "e06.msg", 0x8dc34f11684ce1c6 },
		{ "e07.anm", 0x4a19294633247c3 },
		{ "e07.msg", 0x1f7a6cd75e5cc9d0 },
		{ "e08.anm", 0xc039ad41b1dafb7b },
		{ "e08.msg", 0x4f261b11d75cda66 },
		{ "e09.anm", 0x18b3818818ee341e },
		{ "e09.msg", 0xaa989aa615528a12 },
		{ "e10.anm", 0xc084157aac80c58f },
		{ "e10.msg", 0x12aa73820a704345 },
		{ "e11.anm", 0x80259e33d4f288a2 },
		{ "e11.msg", 0x7066af0601354e26 },
		{ "e12.anm", 0x786313e68484d98a },
		{ "e12.msg", 0x498493dbf4396aa4 },
		{ "effect.anm", 0xbb8fb0c509ddf41c },
		{ "enemy.anm", 0x9f2c58973121678e },
		{ "front.anm", 0x859faccd40d97ce1 },
		{ "help.anm", 0xa4a6ced592bdd1ba },
		{ "help_01.png", 0x8c46131f59f0440a },
		{ "help_02.png", 0x1cfaaedcbead29dc },
		{ "help_03.png", 0x5c8e713f5e0aa340 },
		{ "help_04.png", 0xfd3e7b33aacf1e6e },
		{ "help_05.png", 0x227b5c62933fd6d8 },
		{ "help_06.png", 0xd2996e6ea66a91cd },
		{ "help_07.png", 0x8d1b02e1d202eed8 },
		{ "help_08.png", 0xeca25be8a75452dd },
		{ "help_09.png", 0x30da2ea60009cf5c },
		{ "musiccmt.txt", 0x9042ab586c0c59c5 },
		{ "notice.anm", 0x208f36a8db63db59 },
		{ "notice_01.png", 0x84d97888c5aaa60c },
		{ "notice_02.png", 0x1fcfc056feab98e0 },
		{ "notice_03.png", 0x9541010470058a1 },
		{ "pl00.anm", 0x8bd6f4fedac7f94f },
		{ "pl00.sht", 0x99dba8b23d25ce3b },
		{ "pl01.anm", 0x37c1dec8029af980 },
		{ "pl01.sht", 0x3a83a775c9db8a00 },
		{ "pl02.anm", 0xcb3c4862a4c2f2ff },
		{ "pl02.sht", 0x3e402ede6bb3add8 },
		{ "pl03.anm", 0x919605676a91ba67 },
		{ "pl03.sht", 0x2c61bd8913d9c0c0 },
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
		{ "se_changeitem.wav", 0x6aec872d94ad63df },
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
		{ "se_notice.wav", 0x313ab006c69a541c },
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
		{ "se_release.wav", 0xdce9f51f983fdbbc },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_tan03.wav", 0x73387ed64c1c8f3a },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_trophy.wav", 0x1cf3838471ae12d2 },
		{ "se_warpl.wav", 0x3f46b12f94b42d43 },
		{ "se_warpr.wav", 0x8940e22b4364b080 },
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0x5816686baeaa66dc },
		{ "st01.ecl", 0xefbf9a207cff1feb },
		{ "st01.std", 0xd6bf0f03d633477b },
		{ "st01a.msg", 0xd056404771777e64 },
		{ "st01b.msg", 0x9f37ffb3f71f47c0 },
		{ "st01bs.ecl", 0xa527abba841f38a4 },
		{ "st01c.msg", 0x1aa063b4f9e1ecb6 },
		{ "st01d.msg", 0xf18d0d92ae498f53 },
		{ "st01enm.anm", 0x5070d29ed217fbd4 },
		{ "st01logo.anm", 0xb4e693250ff22b46 },
		{ "st01mbs.ecl", 0xd9c42d4fd35101db },
		{ "st01wl.anm", 0xd14b981c482bf18a },
		{ "st02.ecl", 0x4ef8ee63ac2a656c },
		{ "st02.std", 0xeeda1b40ed20dcc7 },
		{ "st02a.msg", 0xcbf5193f20f3f2d },
		{ "st02b.msg", 0x8e38e195b62baf33 },
		{ "st02bs.ecl", 0x1ea086fbce5076df },
		{ "st02c.msg", 0x66b05a2b6d23ad0d },
		{ "st02d.msg", 0xf8bd893db3756745 },
		{ "st02enm.anm", 0x6ee51963148d6d1c },
		{ "st02logo.anm", 0xd934f3d7f897a1eb },
		{ "st02mbs.ecl", 0x275ad719c3bb3afb },
		{ "st02wl.anm", 0xe3d4049be2dd1106 },
		{ "st03.ecl", 0xa474d563f4b5a3 },
		{ "st03.std", 0x887f816c2aa3063c },
		{ "st03a.msg", 0x89ac871b644869f9 },
		{ "st03b.msg", 0x3af8712edb5dd923 },
		{ "st03bs.ecl", 0xd2805a7e1936a488 },
		{ "st03c.msg", 0x2422a5daa0d4a4ad },
		{ "st03d.msg", 0x3f5efd69672082d2 },
		{ "st03enm.anm", 0x56f2ebd5d92f7705 },
		{ "st03logo.anm", 0x3cb2827e87c036bd },
		{ "st03mbs.ecl", 0x87c885c15be666f7 },
		{ "st03wl.anm", 0xbae3a3a99c0d59c5 },
		{ "st04.ecl", 0x96aa58d597e4ef02 },
		{ "st04.std", 0x32e026701839cb75 },
		{ "st04a.msg", 0x7639267f7ce80365 },
		{ "st04b.msg", 0xcaa40793b2e32d9e },
		{ "st04bs.ecl", 0x9e4c385a90fc727c },
		{ "st04c.msg", 0x58a55dc6794c8c04 },
		{ "st04d.msg", 0x9967b8371bc3f8dc },
		{ "st04enm.anm", 0xb2d14f0aa331a0db },
		{ "st04logo.anm", 0xb74df41910f4c357 },
		{ "st04mbs.ecl", 0x429d14bcfb3289f2 },
		{ "st04wl.anm", 0x46075726fc71c6e5 },
		{ "st05.ecl", 0xcaa6466ea79c0d99 },
		{ "st05.std", 0xf3338b1c91b7a651 },
		{ "st05a.msg", 0xd01de206968553c2 },
		{ "st05b.msg", 0xc674650925248866 },
		{ "st05bs.ecl", 0xa857692eed63cafe },
		{ "st05c.msg", 0xb581ba8a8834ec8e },
		{ "st05d.msg", 0x82a1ce528709062e },
		{ "st05enm.anm", 0x2c797e49176c3e37 },
		{ "st05enm2.anm", 0x88d49df57952b53b },
		{ "st05logo.anm", 0xa8a2181e5b4892e6 },
		{ "st05mbs.ecl", 0xfc593bed1dd755b },
		{ "st05wl.anm", 0x69ecf14a3954c84d },
		{ "st06.ecl", 0x9f2d0a23f191c127 },
		{ "st06.std", 0x295d3f5e0d016eac },
		{ "st06a.msg", 0xd146ed60fa66737 },
		{ "st06b.msg", 0x29f2dfd38794a4b3 },
		{ "st06bs.ecl", 0xe6e14f6de12a7a25 },
		{ "st06c.msg", 0xe27de7da08f433e9 },
		{ "st06d.msg", 0x3dd8876de1fd760e },
		{ "st06enm.anm", 0xe76fd0e7e219f878 },
		{ "st06logo.anm", 0x79fe005789a91fd4 },
		{ "st06mbs.ecl", 0x1d40446a22bf791b },
		{ "st06wl.anm", 0x7565ba17bcf7df0 },
		{ "st07.ecl", 0x8ba12dd348fb9e37 },
		{ "st07.std", 0x49092a8022f5177a },
		{ "st07a.msg", 0xb330bb28cde442c8 },
		{ "st07b.msg", 0xe3469ecda9d6b61f },
		{ "st07bs.ecl", 0x82a2c89f850b039f },
		{ "st07c.msg", 0xd21c75a0fd933d8d },
		{ "st07d.msg", 0xef6574dda8dfbf1b },
		{ "st07enm.anm", 0x93f9c2e0600da611 },
		{ "st07logo.anm", 0xcaba18adf05edc93 },
		{ "st07mbs.ecl", 0x3ecfd2a02f69d418 },
		{ "st07wl.anm", 0x880e84f9fddadde7 },
		{ "staff.anm", 0x975537354c1227b5 },
		{ "staff1.msg", 0xe7d8f34b98868e0a },
		{ "staff2.msg", 0x8a4dc4bd8e748b38 },
		{ "staff3.msg", 0xeab96dd3a305c467 },
		{ "staff4.msg", 0xe3e1671592ecc9c8 },
		{ "text.anm", 0x4129a4f8d276735a },
		{ "th18_0100a.ver", 0x16828d794405107e },
		{ "thbgm.fmt", 0xa9d51791ae160237 },
		{ "title.anm", 0x29bc74e8b218eb5d },
		{ "title_v.anm", 0x225154b11d6b3761 },
		{ "trophy.anm", 0x1e465e9689c82195 },
		{ "trophy.txt", 0x423325f305049079 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th18";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th18-test.dat";

	public ArchiveTh18Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th18.dat")]
	public void ReadArchiveTh18(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.UM, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th18.dat", true)]
	public async Task ReadArchiveTh18Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.UM, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh18(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.UM, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.UM, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh18Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.UM, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.UM, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
