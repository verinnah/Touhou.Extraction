using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh14Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0xb0e6f820cd478d51 },
		{ "ascii_1280.anm", 0x5ada876d0b2f619b },
		{ "ascii_960.anm", 0x378788db80777a6a },
		{ "bullet.anm", 0x5efb41551fe4cfd8 },
		{ "default.ecl", 0xfbca82895208e4f0 },
		{ "demo0.rpy", 0xe5e4324d7308b194 },
		{ "demo1.rpy", 0x820005a8f43fce1d },
		{ "demo2.rpy", 0xad0df6c4890dd61a },
		{ "e01.anm", 0x6fe4ee4f34391279 },
		{ "e01.msg", 0x32c55ec699ef286d },
		{ "e02.anm", 0x22c771f025130d10 },
		{ "e02.msg", 0xff5fdc1f1b7e8acc },
		{ "e03.anm", 0xf422efef256dce98 },
		{ "e03.msg", 0xc2833dfcb343c929 },
		{ "e04.anm", 0x40389a182a16680c },
		{ "e04.msg", 0x74333f28116c1967 },
		{ "e05.anm", 0x84ddcdc781725a84 },
		{ "e05.msg", 0xdac264029455642e },
		{ "e06.anm", 0xa1b885708a6c2a2f },
		{ "e06.msg", 0x2beede2969c8a2be },
		{ "e07.anm", 0x6bd163b823799fbb },
		{ "e07.msg", 0xfaecb86cdc82f86c },
		{ "e08.anm", 0xbd6dd4eefa304323 },
		{ "e08.msg", 0xafc5b0c078ffcc2b },
		{ "e09.anm", 0x3476a8b786380e3a },
		{ "e09.msg", 0x59e90ddf974c23e0 },
		{ "e10.anm", 0x63a9ac19e9dc9153 },
		{ "e10.msg", 0x35a4507e50b7cdb4 },
		{ "e11.anm", 0x7f0f48b486133a5e },
		{ "e11.msg", 0x6018062c103e1356 },
		{ "e12.anm", 0xde974b41a877cd9b },
		{ "e12.msg", 0x9145413f86b4c239 },
		{ "effect.anm", 0x84597fc892e3a57d },
		{ "enemy.anm", 0x58b826159d7acfd },
		{ "front.anm", 0x858e4faa0a8649d7 },
		{ "help.anm", 0x63b9fa7e5d91b45c },
		{ "help_01.png", 0x67e24485c35c4dad },
		{ "help_02.png", 0x4b61929ff9654d42 },
		{ "help_03.png", 0x64c091c511dfec77 },
		{ "help_04.png", 0xe53bd48f883769eb },
		{ "help_05.png", 0x7f96cb1e4c3c4bdc },
		{ "help_06.png", 0xa134782c5ef39ca6 },
		{ "help_07.png", 0x239f08eecbff08f },
		{ "musiccmt.txt", 0x80b5b9b8a26a968e },
		{ "pl00.anm", 0x7d83268e34be6fd },
		{ "pl00a.sht", 0x2dc168a4ca5b0fec },
		{ "pl00b.anm", 0xa2b8c54ca6ebbe53 },
		{ "pl00b.sht", 0x29dd0c30bd20f14 },
		{ "pl01.anm", 0xa885c619ea1e74ce },
		{ "pl01a.sht", 0x7c3b66a79074318b },
		{ "pl01b.anm", 0xb5e8cb8fe6c11206 },
		{ "pl01b.sht", 0xa8f22523402b6409 },
		{ "pl02.anm", 0xd1da7b3d2d457224 },
		{ "pl02a.sht", 0x67d84e6f0c47e326 },
		{ "pl02b.anm", 0xca753a8d1b612036 },
		{ "pl02b.sht", 0xceebf285a2b27f79 },
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
		{ "sig.anm", 0x6bdf52ca61939212 },
		{ "st01.ecl", 0x4a55d23f1c3b2761 },
		{ "st01.std", 0x42eb261abad3772e },
		{ "st01a.msg", 0xde571f1065f3274f },
		{ "st01b.msg", 0xf2d602a8ec36a91 },
		{ "st01bs.ecl", 0xd034289a327f09f2 },
		{ "st01c.msg", 0x44a7747b21d8996b },
		{ "st01d.msg", 0x99027a5ced4924c4 },
		{ "st01e.msg", 0xdf2fcedd2a6886f5 },
		{ "st01enm.anm", 0x3bedd2c99f816ae1 },
		{ "st01f.msg", 0x12d89b06aea7ef55 },
		{ "st01logo.anm", 0xa439ef2c3d2f9554 },
		{ "st01mbs.ecl", 0x4b3bd677bb28fc4c },
		{ "st01menm.anm", 0x22ef2cb94916edc },
		{ "st01wl.anm", 0xa22deca19434e63e },
		{ "st02.ecl", 0x50e81517b9347041 },
		{ "st02.std", 0xc4232ee2036d77e2 },
		{ "st02a.msg", 0xcad220ba3d0430b3 },
		{ "st02b.msg", 0x58ec1ff682204a23 },
		{ "st02bs.ecl", 0x5a7951f6b324e9b4 },
		{ "st02c.msg", 0xc10384315af10082 },
		{ "st02d.msg", 0x726e5638c05ba6bb },
		{ "st02e.msg", 0xcd628e9aa4ab5b85 },
		{ "st02enm.anm", 0x418002dcbb4dae99 },
		{ "st02f.msg", 0xca1658d23ce7d21e },
		{ "st02logo.anm", 0xedf3fc0d3d343114 },
		{ "st02mbs.ecl", 0x8a6cddf2d2d0f31d },
		{ "st02wl.anm", 0x7bc979581c70031f },
		{ "st03.ecl", 0xd494b19411825a5d },
		{ "st03.std", 0xe1b6773e70a44ba0 },
		{ "st03a.msg", 0xe8358f4416af9d36 },
		{ "st03b.msg", 0x92ba856d0c66822f },
		{ "st03bs.ecl", 0x8996f4bd37e477e0 },
		{ "st03c.msg", 0x7840258d2a277311 },
		{ "st03d.msg", 0x970a4ff08e193993 },
		{ "st03e.msg", 0x725d2398b99376c7 },
		{ "st03enm.anm", 0x32b1758f08929011 },
		{ "st03f.msg", 0xd29d7b576cb7a70b },
		{ "st03logo.anm", 0xa2a43d02902fbef4 },
		{ "st03mbs.ecl", 0xa0e88273cbc950eb },
		{ "st03wl.anm", 0xc60aa9ef4e976279 },
		{ "st04.ecl", 0x5ccc1a4c5a57be97 },
		{ "st04.std", 0xdf617191f3b1690d },
		{ "st04a.msg", 0x2a3d0786f8cf517f },
		{ "st04b.msg", 0xb1e879a67e2a8a8e },
		{ "st04bs.ecl", 0x728333f157f6deb3 },
		{ "st04bs2.ecl", 0xc7f66c4844917f17 },
		{ "st04c.msg", 0x5775101fc8be21c0 },
		{ "st04d.msg", 0xab546ad4e6c697 },
		{ "st04e.msg", 0xdfb9a9b1537488b8 },
		{ "st04enm.anm", 0xf25c1ad289cf4282 },
		{ "st04enm2.anm", 0x78039e529a5a96ba },
		{ "st04f.msg", 0xe1b0657bf3f44d0b },
		{ "st04logo.anm", 0x70640da6d00dd519 },
		{ "st04mbs.ecl", 0x8bd1f23b5b281b82 },
		{ "st04mbs2.ecl", 0xa3f8f7708ea4b01f },
		{ "st04wl.anm", 0x8f8987b18fb22648 },
		{ "st05.ecl", 0x29be9f613163da62 },
		{ "st05.std", 0xf5c8944fae290405 },
		{ "st05a.msg", 0x5a605b2aaf845b30 },
		{ "st05b.msg", 0xa1479517e9ca1ed0 },
		{ "st05bs.ecl", 0x7c4842eec600de94 },
		{ "st05c.msg", 0x71df44f456846d97 },
		{ "st05d.msg", 0x913939a9f4b56d8f },
		{ "st05e.msg", 0xa1b790ef95010a9e },
		{ "st05enm.anm", 0x81a5ed5312b381a8 },
		{ "st05f.msg", 0xd7fd1e2dc4f7b91c },
		{ "st05logo.anm", 0x46c13b0a76f5f7a0 },
		{ "st05mbs.ecl", 0x7df24aba028c8aa6 },
		{ "st05wl.anm", 0x1e35249f22e096d },
		{ "st06.ecl", 0x454c152672373f13 },
		{ "st06.std", 0x827f9e87205700e9 },
		{ "st06a.msg", 0xdef97efc9c8947f0 },
		{ "st06b.msg", 0x2f28ddbfd86d867d },
		{ "st06bs.ecl", 0xbd6ac23eef30203d },
		{ "st06c.msg", 0x72b724a7228e0deb },
		{ "st06d.msg", 0x8b4f5ede5d6aa52b },
		{ "st06e.msg", 0x59f52830775b45a2 },
		{ "st06enm.anm", 0xe04faa07ae20b66 },
		{ "st06f.msg", 0xb1c05b38e1848fdd },
		{ "st06logo.anm", 0xf1a30e57ef415fe2 },
		{ "st06mbs.ecl", 0xd48a39134913935c },
		{ "st06menm.anm", 0xc00fb1fe9a9338ce },
		{ "st06wl.anm", 0x1257bfa26aa80bf3 },
		{ "st07.ecl", 0x610a2a14b462739f },
		{ "st07.std", 0xd689fe4726fb5a2f },
		{ "st07a.msg", 0xd17e76a6e1f3d0b5 },
		{ "st07b.msg", 0x5c3dbf1fa7818fdc },
		{ "st07bs.ecl", 0xa6f1e2fd86ac6dcf },
		{ "st07c.msg", 0x81d31c815908b4ef },
		{ "st07d.msg", 0xa7348bb23608b997 },
		{ "st07e.msg", 0xed44e3449755dba2 },
		{ "st07enm.anm", 0x3097f17042c8e4ba },
		{ "st07enm2.anm", 0x4e4068d9acf9251c },
		{ "st07enm3.anm", 0xcaa6a4bc9a09ab32 },
		{ "st07f.msg", 0xe8eff7e9fa40b272 },
		{ "st07logo.anm", 0xbecb2ae6af9e936c },
		{ "st07mbs.ecl", 0x92a0c18cf434915 },
		{ "st07wl.anm", 0x27127fe47f5f3a2f },
		{ "staff.anm", 0xdd91475da8b670fc },
		{ "staff.msg", 0x7ef411ea14651b4f },
		{ "text.anm", 0x71c8ee8e60743754 },
		{ "th14_0100b.ver", 0xc0609152fa142e63 },
		{ "thbgm.fmt", 0xf7f6702680bdc808 },
		{ "title.anm", 0x8970c70a80854ae7 },
		{ "title_v.anm", 0x4c94082ee4a6d317 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th14";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th14-test.dat";

	public ArchiveTh14Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th14.dat")]
	public void ReadArchiveTh14(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.DDC, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th14.dat", true)]
	public async Task ReadArchiveTh14Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.DDC, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh14(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.DDC, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.DDC, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh14Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.DDC, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.DDC, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
