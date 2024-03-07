using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh185Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "abcard.anm", 0xa239e0158828c98c },
		{ "ability.anm", 0x338179daa252148c },
		{ "ability.txt", 0xa70c4017ec473454 },
		{ "abmenu.anm", 0xd6674d0395401c83 },
		{ "ascii.anm", 0x99d7a7d962d7062b },
		{ "ascii1280.anm", 0x566d61bcc3096dc1 },
		{ "ascii_960.anm", 0xa7f9f30155d73f70 },
		{ "boss01.anm", 0xf704f4874adb249f },
		{ "boss01.ecl", 0xaed45dbca60e5780 },
		{ "boss01t.anm", 0x432afe28d54c61d7 },
		{ "boss01t.ecl", 0x21274aaa5280a02a },
		{ "boss02.anm", 0xa421308987ed6e6f },
		{ "boss02.ecl", 0x53ef8e297641c358 },
		{ "boss03.anm", 0x418ac7155f79541b },
		{ "boss03.ecl", 0xd4791b45e488fc05 },
		{ "boss04.anm", 0xe3d23b0b7a6cc11c },
		{ "boss04.ecl", 0xffccc7fc039426c },
		{ "boss05.anm", 0xe180f6a7cff484a7 },
		{ "boss05.ecl", 0x1a4949d50960dfee },
		{ "boss06.anm", 0x4cfce6923fccd36c },
		{ "boss06.ecl", 0xe335b1ebed042940 },
		{ "boss07.anm", 0xb22087cf53edadc },
		{ "boss07.ecl", 0x4cda1e15a2a84b62 },
		{ "boss08.anm", 0x158d53a59610522d },
		{ "boss08.ecl", 0x48bb91b8987b7ef3 },
		{ "boss09.anm", 0xb89174cfa68ae9e6 },
		{ "boss09.ecl", 0xc6987412a1732fcc },
		{ "boss10.anm", 0xac1299acfb9bd063 },
		{ "boss10.ecl", 0x356358ad9f576e1 },
		{ "boss11.anm", 0x3a275c73dc288265 },
		{ "boss11.ecl", 0xf2548c31fe87658f },
		{ "boss12.anm", 0xc543e260a6aba3c2 },
		{ "boss12.ecl", 0xba044ba2d2e50ad5 },
		{ "boss13.anm", 0x669be6944e1a90b3 },
		{ "boss13.ecl", 0x53fbe11bee0a259d },
		{ "boss14.anm", 0xb03eeae6cec32165 },
		{ "boss14.ecl", 0x1b2002b362907e50 },
		{ "boss15.anm", 0x69b46aaad6ea7e44 },
		{ "boss15.ecl", 0x4d422df96ce912cf },
		{ "boss16.anm", 0x7854b25e31ef50b7 },
		{ "boss16.ecl", 0x806da175de050d8 },
		{ "boss17.anm", 0x68c7e54b40bd1907 },
		{ "boss17.ecl", 0x2416b441536e274d },
		{ "boss18.anm", 0x5d458baafed5ce68 },
		{ "boss18.ecl", 0x27e9df6d0a7f0991 },
		{ "boss19.anm", 0x6b214382afd3b00e },
		{ "boss19.ecl", 0x94cfefcc8146616f },
		{ "boss20.anm", 0x83f883a6f7e56303 },
		{ "boss20.ecl", 0x4d1d7b540ac18a4e },
		{ "boss21.anm", 0x9fd21749c1baebbd },
		{ "boss21.ecl", 0xbbace7bfc563390 },
		{ "boss22.anm", 0x266bd8ed21acc021 },
		{ "boss22.ecl", 0xc7682dfc75943025 },
		{ "boss23.anm", 0x8676bf04f9fdbe36 },
		{ "boss23.ecl", 0xca98967cb77c5d23 },
		{ "boss24.anm", 0x7d5f0f801e1e5c05 },
		{ "boss24.ecl", 0xe3fd28b495b5685a },
		{ "boss25.anm", 0x9019fb5e5674e8fa },
		{ "boss25.ecl", 0xabc74a71c4a36ae5 },
		{ "boss26.anm", 0x6c2ed2372869d88 },
		{ "boss26.ecl", 0x8a0f14eb117eee6d },
		{ "boss27.anm", 0x9102fcb402575c74 },
		{ "boss27.ecl", 0x7c5394ef4359cf98 },
		{ "bullet.anm", 0x1168a84c103de9ef },
		{ "cardpos.txt", 0x782032e7f501b20c },
		{ "default.ecl", 0x40f20e66536d5247 },
		{ "effect.anm", 0xbb8fb0c509ddf41c },
		{ "enemy.anm", 0x9f2c58973121678e },
		{ "front.anm", 0xff911beeacd72b4f },
		{ "help.anm", 0x97fe174c899faf1f },
		{ "help_01.png", 0xd4c4211c36b61b47 },
		{ "help_02.png", 0xad2642f7367bb54 },
		{ "help_03.png", 0x17f2bdce3297a6ce },
		{ "help_04.png", 0x97789c79999368db },
		{ "help_05.png", 0xf1143b1df1a5291c },
		{ "help_06.png", 0x23c621a785e9a598 },
		{ "help_07.png", 0x6fdd192eb67ab3e1 },
		{ "help_08.png", 0xd4f875cd534233d0 },
		{ "help_09.png", 0xbce4ac239ce2a3ff },
		{ "musiccmt.txt", 0x906e53d4b3b5eccc },
		{ "notice.anm", 0x208f36a8db63db59 },
		{ "notice_01.png", 0xb7eea3099fef1265 },
		{ "notice_02.png", 0xc6b12e88d53471a3 },
		{ "notice_03.png", 0xdf1cb878de52e718 },
		{ "notice_04.png", 0xe5f9d3f59971a0ae },
		{ "notice_05.png", 0x48856f4132e0bbb3 },
		{ "notice_06.png", 0x8d4ca74359959d4e },
		{ "notice_07.png", 0x214088228148d44b },
		{ "notice_08.png", 0x77e078031eaeb77 },
		{ "notice_09.png", 0x1dac2b1ae02bb03f },
		{ "notice_10.png", 0xd2aecb913644ce1c },
		{ "notice_11.png", 0xf19ac82998bc7015 },
		{ "notice_12.png", 0x3632739e239b22d0 },
		{ "notice_13.png", 0x91172f8797d2e367 },
		{ "notice_14.png", 0xc180cd92e0f25bb0 },
		{ "pl01.anm", 0xc62b821a9f046cf9 },
		{ "pl01.sht", 0x8aed77f6c992d7f6 },
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
		{ "sig.anm", 0x6418158bcdf42555 },
		{ "text.anm", 0xc4ed733bf51122db },
		{ "th185_0100a.ver", 0xd5673baf96f6ee1e },
		{ "thbgm.fmt", 0xd4ee0f43738c5fb4 },
		{ "title.anm", 0xeaab1075c60947c0 },
		{ "title_v.anm", 0x866c128afdab41c8 },
		{ "trophy.anm", 0x8a1f88a0c5cbf973 },
		{ "trophy.txt", 0x8109169529b403cc },
		{ "turtrial.msg", 0x73b7647feafa64e4 },
		{ "wave01.ecl", 0x3ef9946af0be1bd5 },
		{ "wave01t.ecl", 0xf17e4fe572adf4ff },
		{ "wave02.ecl", 0x8ce409496c01394f },
		{ "wave02t.ecl", 0x81fa68a5c48e119f },
		{ "wave03.ecl", 0x4781e075361951ff },
		{ "wave03t.ecl", 0xcb5dc984a5764ce1 },
		{ "wave04.ecl", 0xaad882e6286a01ae },
		{ "wave05.ecl", 0x1cfeb4dc705a1677 },
		{ "wave06.ecl", 0x7ef88091085f18cf },
		{ "wave07.ecl", 0x479b2cdd62e084aa },
		{ "wave08.ecl", 0xacfaae8f35ac4127 },
		{ "wave09.ecl", 0xd76acec9b93316a8 },
		{ "wave10.ecl", 0xa0c86f7e7eff2abe },
		{ "wave11.ecl", 0x5edb65c034a5d784 },
		{ "wave12.ecl", 0x6d53c7dd885a53b3 },
		{ "wave13.ecl", 0x1613115391bf776f },
		{ "wave14.ecl", 0xec29c8a26f4ce4a3 },
		{ "wave15.ecl", 0x673845f9bcb7b9c6 },
		{ "wave16.ecl", 0x5879fb1d3c9028d1 },
		{ "wave17.ecl", 0xb052654e4e1131b9 },
		{ "wave18.ecl", 0xa21c97cd07f5f0c2 },
		{ "wave19.ecl", 0xb2db99fe6494b59a },
		{ "wave20.ecl", 0x82cb94c5855556b5 },
		{ "wave21.ecl", 0x1303c19b5e463244 },
		{ "wave22.ecl", 0x9ea3675a12cc4799 },
		{ "wave23.ecl", 0x46e826d90702b2e1 },
		{ "wave24.ecl", 0xf4393f260c2857a },
		{ "wave25.ecl", 0x5ff3d865eaed56d5 },
		{ "wave26.ecl", 0xfa7e683468887109 },
		{ "wave27.ecl", 0xaba45e46c549f067 },
		{ "wave28.ecl", 0x517e78c9096ab5de },
		{ "wave29.ecl", 0xc1d63e6d7e11be6c },
		{ "wave30.ecl", 0x3b335cc1db4c21bf },
		{ "wave31.ecl", 0xcb6038a787766701 },
		{ "wave32.ecl", 0x87482b1b4afcc134 },
		{ "wave33.ecl", 0x48bf58a2c97ba8bc },
		{ "wave34.ecl", 0xecaf1059c3c669bd },
		{ "wave35.ecl", 0x57d456352a00a8d3 },
		{ "wave36.ecl", 0xd08d23c790f6add0 },
		{ "wave37.ecl", 0x70237590e42b82fa },
		{ "wave38.ecl", 0x1cb6fe375eb83b6c },
		{ "wave39.ecl", 0x18d79734c456a0 },
		{ "wave40.ecl", 0x6a67981ee57cb5a },
		{ "wave41.ecl", 0x6613d72bc2a070a4 },
		{ "wave42.ecl", 0x4c701a6054f50ed },
		{ "wave43.ecl", 0x7508e0a9da63efc2 },
		{ "wave44.ecl", 0xffe149fdc178c3a2 },
		{ "wave45.ecl", 0xdf9601839d116d86 },
		{ "wave46.ecl", 0x427fe8b66d2d0c8e },
		{ "wave47.ecl", 0x615fbfcef479ecf9 },
		{ "wave48.ecl", 0x43281b6deb98dc93 },
		{ "wave49.ecl", 0x503e568faa037de8 },
		{ "wave50.ecl", 0xf701e49e8b302e68 },
		{ "wave51.ecl", 0x6a769f81b8ec9d18 },
		{ "wave52.ecl", 0x32e2fe56caf35660 },
		{ "wave53.ecl", 0xd4169b15cf9e13c4 },
		{ "wave54.ecl", 0xedddc20dba5b7bb9 },
		{ "wave55.ecl", 0x644ae165e992c5b8 },
		{ "wave56.ecl", 0xc9657080c0196966 },
		{ "wave57.ecl", 0x8f837d7b3109a9f6 },
		{ "wave58.ecl", 0x7abbe4a422deb60 },
		{ "wave59.ecl", 0x1ae69d55ac7e1c54 },
		{ "wave60.ecl", 0x37f11f9ce9514338 },
		{ "wave61.ecl", 0x1f831fb6a8be5d8c },
		{ "wave62.ecl", 0x937abdcd56dc407a },
		{ "wave63.ecl", 0x4d25735ea6c7059b },
		{ "wave64.ecl", 0x867ce1dd7dabf4f1 },
		{ "wave65.ecl", 0x33b3a79e6c3730ad },
		{ "wave66.ecl", 0x3bfe9f4f376c406b },
		{ "wave67.ecl", 0x9657f4d4cac6293e },
		{ "wave68.ecl", 0x7cbc64edca063a26 },
		{ "wave69.ecl", 0xc6fa824b0e4c3777 },
		{ "wave70.ecl", 0x8a16d0627d765c9 },
		{ "wave71.ecl", 0xce380e167c65c9c },
		{ "wave72.ecl", 0x7b55fd9460777f0e },
		{ "wave73.ecl", 0x50d757036a8b679 },
		{ "world.txt", 0xe2c98524782fba7f },
		{ "world01.anm", 0x25cef3d62b611247 },
		{ "world01.ecl", 0x2bbfa2c2a35ebe80 },
		{ "world01.std", 0x23adb40fcbc63b56 },
		{ "world01t.anm", 0xc7082dbcf0ee63da },
		{ "world01t.ecl", 0x3c4a828400ac51ee },
		{ "world01t.std", 0xe9203576c8f5ab0e },
		{ "world02.anm", 0x3e6dfea4f6f7cd7c },
		{ "world02.ecl", 0x43555b2f7483ce98 },
		{ "world02.std", 0xf6902ca21d560e42 },
		{ "world03.anm", 0xe05d95e116692176 },
		{ "world03.ecl", 0x5825750c0b60690f },
		{ "world03.std", 0x6ca5670243ba23ad },
		{ "world04.anm", 0x823b34da843817e },
		{ "world04.ecl", 0x29178a70939be45e },
		{ "world04.msg", 0x16d5707dd2f7edfe },
		{ "world04.std", 0x71a124ca6e2a6bb9 },
		{ "world05.anm", 0x20fadf1f125b1c53 },
		{ "world05.ecl", 0x8799ef4f033ca61b },
		{ "world05.std", 0xeb558fe407f32c1a },
		{ "world06.anm", 0xc2f0f1892efc239f },
		{ "world06.ecl", 0x60530d1a184d0a59 },
		{ "world06.msg", 0x59193b6a75e609b2 },
		{ "world06.std", 0xcb5e126f1caf1a57 },
		{ "world07.anm", 0xecd73a07e6acfc4d },
		{ "world07.ecl", 0x12167e40e21e51b4 },
		{ "world07.msg", 0x8098752230bf5227 },
		{ "world07.std", 0x3d125f8084e6df36 },
		{ "world08.anm", 0x4a82ee8a11093fb5 },
		{ "world08.ecl", 0x487e906fb3b0261b },
		{ "world08.std", 0x60160193b25b411b }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th185";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th185-test.dat";

	public ArchiveTh185Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th185.dat")]
	public void ReadArchiveTh185(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.HBM, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th185.dat", true)]
	public async Task ReadArchiveTh185Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.HBM, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh185(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.HBM, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.HBM, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh185Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.HBM, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.HBM, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
