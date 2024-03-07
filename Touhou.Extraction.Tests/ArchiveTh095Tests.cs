using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh095Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x5232068c8ad6fda0 },
		{ "bullet.anm", 0x6ac04350a3515d44 },
		{ "capture.anm", 0xf9055e2ecddde0c8 },
		{ "demo0.rpy", 0x1be3a066d050f94d },
		{ "demo1.rpy", 0xd5def2772d901945 },
		{ "demo2.rpy", 0x4ff2709da079d5b1 },
		{ "ecl10_a.ecl", 0x2df6f70a4ecadde0 },
		{ "ecl10_b.ecl", 0xa4faf37bb83d0d },
		{ "ecl10_c.ecl", 0x23028fa9d7d68ce },
		{ "ecl10_d.ecl", 0xc5d8299a3c4ff693 },
		{ "ecl11_a.ecl", 0xf92f29b23e2aa0ad },
		{ "ecl11_b.ecl", 0x2462252f1a715d45 },
		{ "ecl11_c.ecl", 0xa0159f6ee94ff8e1 },
		{ "ecl11_d.ecl", 0x804c238eb78260a2 },
		{ "ecl12_a.ecl", 0xb2e55da5e65f9142 },
		{ "ecl12_b.ecl", 0x84174e6e4a00d271 },
		{ "ecl12_c.ecl", 0x2acaeb7468859bbd },
		{ "ecl12_d.ecl", 0x2740e2c96e1f8791 },
		{ "ecl13_a.ecl", 0xae35911ef11bb7df },
		{ "ecl13_b.ecl", 0xe02e84b744f08479 },
		{ "ecl13_c.ecl", 0xc85c6c5db7650f49 },
		{ "ecl13_d.ecl", 0xd7bcda961c5b0a99 },
		{ "ecl14_a.ecl", 0x298f920f476dbe08 },
		{ "ecl14_b.ecl", 0xf3fa152f9aea1ad0 },
		{ "ecl14_c.ecl", 0xfcd7ac90eb2b064f },
		{ "ecl14_d.ecl", 0xbbdb279100c1f019 },
		{ "ecl15_a.ecl", 0xdc036acbd5743d4d },
		{ "ecl15_b.ecl", 0x8984d37a68ee2482 },
		{ "ecl15_c.ecl", 0x4b80784c6cfe91cc },
		{ "ecl15_d.ecl", 0x4f57b7d786ac5b72 },
		{ "ecl16_a.ecl", 0xd7b486a6e07a9ee9 },
		{ "ecl16_b.ecl", 0xe3e0b62d9b80f7f4 },
		{ "ecl16_c.ecl", 0xf1442647a26b1c6b },
		{ "ecl16_d.ecl", 0x4ec726f1e4289767 },
		{ "ecl17_a.ecl", 0x55a7052721b1cf4 },
		{ "ecl17_b.ecl", 0x5f802e3b78cbb03e },
		{ "ecl17_c.ecl", 0x8fb70f1fd7f95a33 },
		{ "ecl17_d.ecl", 0x43963dd70c5b5a06 },
		{ "ecl18_a.ecl", 0x8163b3d8caf806dd },
		{ "ecl18_b.ecl", 0x29f32bdf0fd5516c },
		{ "ecl18_c.ecl", 0x962b99e9522170d },
		{ "ecl18_d.ecl", 0x2c39b01ccb3602c0 },
		{ "ecl19_a.ecl", 0x83c8ba2018942ef0 },
		{ "ecl19_b.ecl", 0xe9bf09e168088084 },
		{ "ecl19_c.ecl", 0x76ce5e5aaee69d03 },
		{ "ecl19_d.ecl", 0xa140deb693b92845 },
		{ "ecl1_a.ecl", 0xb71906b4b7bb27c5 },
		{ "ecl1_b.ecl", 0xa76abec62fd038a9 },
		{ "ecl1_c.ecl", 0x58fb5055bff4a132 },
		{ "ecl20_a.ecl", 0x1cd50c81f8a4c262 },
		{ "ecl20_b.ecl", 0x787960399a8fa2a0 },
		{ "ecl20_c.ecl", 0xe6cde2b88850ee39 },
		{ "ecl20_d.ecl", 0x1d27a8428bb58f7 },
		{ "ecl21_a.ecl", 0xc0f6a06848aee0e },
		{ "ecl21_b.ecl", 0xe67c02f4955787db },
		{ "ecl21_c.ecl", 0xf79915327097da1 },
		{ "ecl21_d.ecl", 0x85dd34826dbb6a65 },
		{ "ecl22_a.ecl", 0xa7d8a8c9c09f2c7e },
		{ "ecl22_b.ecl", 0xc8002793763fdb00 },
		{ "ecl23_a.ecl", 0xdcfca18d30ae9da3 },
		{ "ecl23_b.ecl", 0xf08a35c6b2bcae0 },
		{ "ecl24_a.ecl", 0x4276ef81cf9342f2 },
		{ "ecl24_b.ecl", 0xd7bbe40eae701b0a },
		{ "ecl25_a.ecl", 0x447fecf388f12362 },
		{ "ecl25_b.ecl", 0x2e3954d6debd6727 },
		{ "ecl2_a.ecl", 0x447513c612e19860 },
		{ "ecl2_b.ecl", 0x99d89db22a9229fd },
		{ "ecl2_c.ecl", 0x21a84846f878467c },
		{ "ecl3_a.ecl", 0xb209fc2439ab2e34 },
		{ "ecl3_b.ecl", 0x8396fb512dc805fe },
		{ "ecl3_c.ecl", 0x285bf035ed4d4e7b },
		{ "ecl4_a.ecl", 0x13e2cd4b308e5551 },
		{ "ecl4_b.ecl", 0x862790c804fd60f7 },
		{ "ecl4_c.ecl", 0xc84053a1c7f91c8a },
		{ "ecl5_a.ecl", 0x79bf31b1f16f3cd7 },
		{ "ecl5_b.ecl", 0xfe9c5694b4d45fb8 },
		{ "ecl5_c.ecl", 0xcb5ae7a92977d482 },
		{ "ecl5_d.ecl", 0x449314bd0d3ffb51 },
		{ "ecl6_a.ecl", 0x208eb554bdc235d },
		{ "ecl6_b.ecl", 0x74a480e3c1895f70 },
		{ "ecl6_c.ecl", 0x3a254ce2dba88d87 },
		{ "ecl6_d.ecl", 0x6c92aca854a8b7e3 },
		{ "ecl7_a.ecl", 0xa4e5a30c81fc5c75 },
		{ "ecl7_b.ecl", 0x1e4a1f39626353b1 },
		{ "ecl7_c.ecl", 0xf5ab66eff7962919 },
		{ "ecl7_d.ecl", 0xd342d22b6fc4b8be },
		{ "ecl8_a.ecl", 0xbfd4d81998219b20 },
		{ "ecl8_b.ecl", 0x39315113df7aaa19 },
		{ "ecl9_a.ecl", 0xa8b93318c73005bf },
		{ "ecl9_b.ecl", 0xefc85f68f2b8db9e },
		{ "ecl9_c.ecl", 0xd703ffdd0254f870 },
		{ "enm1.anm", 0xd7a6d0d3fc6773cd },
		{ "enm10.anm", 0x45994be178ec08d6 },
		{ "enm11.anm", 0x4405738508909d14 },
		{ "enm12.anm", 0xbfa47738157a5453 },
		{ "enm13.anm", 0xf97dcbe0b12291c7 },
		{ "enm14.anm", 0x86a8f342af498b5c },
		{ "enm15.anm", 0xbbaaf766e38cea7 },
		{ "enm16.anm", 0x8d86b6f46666e6e0 },
		{ "enm17.anm", 0x71965c5e1c038336 },
		{ "enm18.anm", 0x1beddebb999024f0 },
		{ "enm19.anm", 0xdf661d6d29cd7dc2 },
		{ "enm2.anm", 0xf1bf7e7dc240c477 },
		{ "enm20.anm", 0x332bed048a7fe287 },
		{ "enm21.anm", 0xe3d700241256735b },
		{ "enm22.anm", 0x9f67672d8f8e0f1d },
		{ "enm23.anm", 0xd4fa8c5840d2f731 },
		{ "enm24.anm", 0x454ee7e1fe1d3b59 },
		{ "enm25.anm", 0x7ead3f1280d0ccdf },
		{ "enm3.anm", 0xe933cb35bc06e6c2 },
		{ "enm4.anm", 0x82051d825a363eb8 },
		{ "enm5.anm", 0xa22f23169189bb0a },
		{ "enm6.anm", 0x6e1a539ee6235e8b },
		{ "enm7.anm", 0xf182797364d6c8e8 },
		{ "enm8.anm", 0xef8daf9d74891741 },
		{ "enm9.anm", 0xf066c7f11da02e55 },
		{ "fc00.anm", 0x8ea2badd6a826a6a },
		{ "fc00b.anm", 0x272218e4049682e1 },
		{ "fc01.anm", 0xc033aa6c8743f882 },
		{ "fc01b.anm", 0xe3f67d420fee724a },
		{ "fc02.anm", 0x4df7f6e40b7fedc5 },
		{ "fc02b.anm", 0x8b839e1e4bdd5dc7 },
		{ "fc03.anm", 0x82f90b7c4e92f637 },
		{ "fc03b.anm", 0x5438aee1a7fffd86 },
		{ "fc04.anm", 0xc94fe97962baa304 },
		{ "fc04b.anm", 0x2a566501292eebf7 },
		{ "fc05.anm", 0xeb74aa4b9a3ecb66 },
		{ "fc05b.anm", 0x4258a53786b4a061 },
		{ "fc06.anm", 0x3008c9a2ded0ff6f },
		{ "fc06b.anm", 0x1de10b135ef1fefb },
		{ "fc07.anm", 0xf7552931da28ceb5 },
		{ "fc07b.anm", 0x825fab052a41c5e9 },
		{ "fc08.anm", 0x4b75e9476f9db15f },
		{ "fc08b.anm", 0xeaaca30018013016 },
		{ "front.anm", 0xb3b697b6c63dc602 },
		{ "help.txt", 0xfada7d88847a37cb },
		{ "help_00.anm", 0xc98820b05f390a16 },
		{ "help_01.anm", 0x4d2cad894117c52b },
		{ "help_02.anm", 0x3f62e109baa3dbc4 },
		{ "help_03.anm", 0x511f148b637ac98a },
		{ "help_04.anm", 0x18fcbdd1f900a0c6 },
		{ "help_05.anm", 0xc444e9f5968956f6 },
		{ "help_06.anm", 0xe5d671ed62d5471d },
		{ "help_07.anm", 0x126d1e9a16bfda19 },
		{ "help_08.anm", 0x2ba563761cfc21a4 },
		{ "mission.msg", 0x84e3ca2104009d68 },
		{ "mission000.anm", 0xbf2dbd30b6252cd8 },
		{ "mission001.anm", 0xca1245482546e70c },
		{ "mission002.anm", 0xf22dc58d0c033524 },
		{ "mission003.anm", 0x50e4fa0c6c80eed3 },
		{ "mission004.anm", 0x172f7642fd4d7fd6 },
		{ "mission005.anm", 0xd95901d4084037bd },
		{ "mission006.anm", 0x8468c298c97bc1c3 },
		{ "mission007.anm", 0xbd7f9225c083f83b },
		{ "mission008.anm", 0x4c9fb9352dcfd426 },
		{ "mission009.anm", 0x29eef4bafaaca28e },
		{ "mission010.anm", 0x79d71d8b49f1b47e },
		{ "mission011.anm", 0xa9718cd0f8ec5224 },
		{ "mission012.anm", 0x5077a7900f5f66eb },
		{ "mission013.anm", 0xd88d46bcd858e7c5 },
		{ "mission014.anm", 0xc5a4e33836ab6360 },
		{ "mission015.anm", 0x54d9cce260583974 },
		{ "mission016.anm", 0x27ed201cd344bb26 },
		{ "mission017.anm", 0xeddeb3fde5bb8ae3 },
		{ "mission018.anm", 0x9fd40758f0f86d9c },
		{ "mission019.anm", 0x27baded48cfc214d },
		{ "mission020.anm", 0xe1b4005bf9994ea8 },
		{ "mission021.anm", 0xfbecd830502ed253 },
		{ "mission022.anm", 0x8c15171d7f9e9d0a },
		{ "mission023.anm", 0x88257fbefd6de896 },
		{ "mission024.anm", 0x7412edb527baf0b2 },
		{ "mission025.anm", 0x43035df94da887be },
		{ "mission101.anm", 0x21d141b4c94e5902 },
		{ "mission102.anm", 0xd402266482f50727 },
		{ "mission103.anm", 0x518e3eba71ee7330 },
		{ "mission104.anm", 0x9f2d0e6484afc41c },
		{ "mission105.anm", 0xb562d7a3a6aa7575 },
		{ "mission106.anm", 0xa998ea14c05fe77a },
		{ "mission107.anm", 0x16ff9ed08d2499e6 },
		{ "mission108.anm", 0x9b1761934650134 },
		{ "mission109.anm", 0xbd3a75d1a95b6707 },
		{ "mission110.anm", 0x3e8755584ec3c064 },
		{ "mission_02.anm", 0xb8e76dabe4472017 },
		{ "mission_03.anm", 0x656355253a974381 },
		{ "mission_04.anm", 0x9cba33b5a71a57dd },
		{ "mission_05.anm", 0xa4941a846b0c46a },
		{ "mission_06.anm", 0x1ecbbdfdf2cada17 },
		{ "mission_07.anm", 0xe5d9176450a8039f },
		{ "mission_08.anm", 0x7a823ac146476650 },
		{ "mission_09.anm", 0xd7b821b4e79365fc },
		{ "mission_10.anm", 0xf6d27cecac462dc1 },
		{ "mission_11.anm", 0xea3b24495d11d99b },
		{ "mission_12.anm", 0x5cd1efbeea10430f },
		{ "musiccmt.txt", 0xedf41840bcdd3efe },
		{ "nowloading.anm", 0x30631b89c81c27a5 },
		{ "pause.anm", 0x27ca190eb7675d20 },
		{ "photo.anm", 0xa6f32748cbb8435b },
		{ "player.anm", 0x69fd291b183742fd },
		{ "player.sht", 0xd3ff5fdbe9957e5 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_focus.wav", 0xf8014b382298e0f0 },
		{ "se_focusin.wav", 0xb12cc628c631ab01 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_nice.wav", 0xa6ab57f4d6404778 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_option.wav", 0xadabeccdcebc65c3 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xa8311a42586a4915 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0x18d8eb75d98e6104 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_shutter.wav", 0x18275555311e4ea0 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "text.anm", 0x840852d1660c2207 },
		{ "th08logo.jpg", 0x9ddb5471955881a2 },
		{ "th095_0102a.ver", 0xb80541ae241b20d5 },
		{ "thbgm.fmt", 0x7be00338fb5d67ed },
		{ "title.anm", 0x63979a505c2c0f49 },
		{ "title_v.anm", 0x6c84c6638316b4e7 },
		{ "world01.anm", 0xe5f63ba1354f27c2 },
		{ "world01.std", 0xb1e950e7c2a8ab08 },
		{ "world02.anm", 0xa94d1872858a93e },
		{ "world02.std", 0x3b239d94f8575404 },
		{ "world03.anm", 0x473a0a17fffa367b },
		{ "world03.std", 0xc7e752d5b40da2f1 },
		{ "world04.anm", 0x31aa3e887ed3ba08 },
		{ "world04.std", 0xcdeee951c5f8a444 },
		{ "world05.anm", 0x240b4a5487469e62 },
		{ "world05.std", 0x32f4c049d315f9d5 },
		{ "world06.anm", 0x966719f7103f97a9 },
		{ "world06.std", 0xef1d0828dcfaa856 },
		{ "world07.anm", 0x9358aace0d086cc1 },
		{ "world07.std", 0xc7976f06e62f1f65 },
		{ "world08.anm", 0xd95bc31ac42717 },
		{ "world08.std", 0xa06d82d977206d06 },
		{ "world09.anm", 0xc243536a6e8704fa },
		{ "world09.std", 0x3f7746af0aa3e906 },
		{ "world10.anm", 0xae699e77b63b5a64 },
		{ "world10.std", 0xda054fef07e83462 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th095";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th095-test.dat";

	public ArchiveTh095Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th095.dat")]
	public void ReadArchiveTh095(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.StB, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th095.dat", true)]
	public async Task ReadArchiveTh095Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.StB, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh095(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.StB, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.StB, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh095Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.StB, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.StB, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
