using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh17Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0xf77d0571b89b80e4 },
		{ "ascii_1280.anm", 0x78bebfe67e4a8e40 },
		{ "ascii_960.anm", 0x65b5eb5fb8c794a5 },
		{ "beast.anm", 0x1a642780fea89ed9 },
		{ "bullet.anm", 0x5727ddbbca5c3fb3 },
		{ "default.ecl", 0x11945074b6919238 },
		{ "demo1.rpy", 0xc56bd02cfa19c449 },
		{ "demo2.rpy", 0x40fc0cb424b58a18 },
		{ "demo3.rpy", 0x71a2405251248d12 },
		{ "e01.anm", 0xdfb5cb62bffbacce },
		{ "e01.msg", 0x59656c72256e1cf7 },
		{ "e02.anm", 0xde3fc80e15213359 },
		{ "e02.msg", 0x1ff708cccaec053a },
		{ "e03.anm", 0x4b3711f36176cd48 },
		{ "e03.msg", 0x634bdc8244dc136 },
		{ "e04.anm", 0x457526e86afe66d7 },
		{ "e04.msg", 0x974e7178fb92b192 },
		{ "e05.anm", 0x1759f2128dcbfd61 },
		{ "e05.msg", 0x959baae8a04ca674 },
		{ "e06.anm", 0xad2e67b6c86828ad },
		{ "e06.msg", 0x4367822ed98720f4 },
		{ "e07.anm", 0xcfdfb064a11161c7 },
		{ "e07.msg", 0xc6bdd6e9ced647e3 },
		{ "e08.anm", 0x9ddd1443a07b1890 },
		{ "e08.msg", 0x8be9fee5b9788782 },
		{ "e09.anm", 0xd7b520dac6ddfb04 },
		{ "e09.msg", 0xd3d4a616e3b2101 },
		{ "e10.anm", 0xc084157aac80c58f },
		{ "e10.msg", 0x50bb7178af4e0da2 },
		{ "e11.anm", 0xf4af25d9d866abb6 },
		{ "e11.msg", 0xc96ce404a58bbd15 },
		{ "e12.anm", 0xa484941b41887abc },
		{ "e12.msg", 0xb59ea489f795d4ba },
		{ "effect.anm", 0xafff4336b0f9acf1 },
		{ "enemy.anm", 0x7819065feb1974d1 },
		{ "enemyb.anm", 0x24bfbd5f562afe9c },
		{ "front.anm", 0x907488f7e59e7ca9 },
		{ "help.anm", 0xa13430fa0f97190c },
		{ "help_01.png", 0x437af0464a73d79e },
		{ "help_02.png", 0xd18ce020166dc263 },
		{ "help_03.png", 0x99cb0a14556898a7 },
		{ "help_04.png", 0x8fe7c5d2b263467d },
		{ "help_05.png", 0x9cdb5d949c73ad88 },
		{ "help_06.png", 0xc553224fbb56e2aa },
		{ "help_07.png", 0xf6d2c7ccb86b21ea },
		{ "help_08.png", 0xeca25be8a75452dd },
		{ "help_09.png", 0x30da2ea60009cf5c },
		{ "musiccmt.txt", 0x7cc70235bd854680 },
		{ "pl00.anm", 0x954e1c8b26692965 },
		{ "pl00a.sht", 0x696dc7795cff6579 },
		{ "pl00b.sht", 0xe832488e129b0a21 },
		{ "pl00c.sht", 0x523d17f257348d90 },
		{ "pl01.anm", 0x8816b425c20aaff3 },
		{ "pl01a.sht", 0x601b04867b10743b },
		{ "pl01b.sht", 0xd9a23d07607a3b64 },
		{ "pl01c.sht", 0x3b1deefe3641037a },
		{ "pl02.anm", 0x921a1505f3112bc0 },
		{ "pl02a.sht", 0x7d677eb657bffac2 },
		{ "pl02b.sht", 0x851eb8ac0b3339ea },
		{ "pl02c.sht", 0xfd8438c7ec6e19d8 },
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
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0x93c46ba31ca6db3d },
		{ "st01.ecl", 0x12f27f153cf34811 },
		{ "st01.std", 0xdc679c6b60417348 },
		{ "st01a.msg", 0x7dbca371ecc14b8f },
		{ "st01b.msg", 0xf85123256bcc571a },
		{ "st01bs.ecl", 0xcc7e6e6ec42e4ee4 },
		{ "st01c.msg", 0xb3815e01c572aeb4 },
		{ "st01d.msg", 0x4896bab16dc6a777 },
		{ "st01e.msg", 0xc9da0aa05343e278 },
		{ "st01enm.anm", 0x3cfbc52dd2ca79ba },
		{ "st01f.msg", 0x7de96eba71aea524 },
		{ "st01g.msg", 0xb233dd80c7a3a450 },
		{ "st01h.msg", 0x1be2bf04ca28d8bb },
		{ "st01i.msg", 0x4429ab3707558093 },
		{ "st01logo.anm", 0xb0982dc603700bb7 },
		{ "st01mbs.ecl", 0xd811587aa02df7ac },
		{ "st01wl.anm", 0xc158214a57d7b36e },
		{ "st02.ecl", 0x8bd122df195f1091 },
		{ "st02.std", 0xb852e6bb372868a2 },
		{ "st02a.msg", 0xf8d4d13336b691f5 },
		{ "st02b.msg", 0xce0f2016dcb1da9b },
		{ "st02bs.ecl", 0xabfb2c15a38854d1 },
		{ "st02c.msg", 0xaf2ab1a1158e2a5e },
		{ "st02d.msg", 0x40f96626e8871249 },
		{ "st02e.msg", 0xe08584444872c12f },
		{ "st02enm.anm", 0xfaab56141a229479 },
		{ "st02f.msg", 0x1a9903b0a587c192 },
		{ "st02g.msg", 0xf14d8c4034db16fa },
		{ "st02h.msg", 0x6e042d9f79a05b3a },
		{ "st02i.msg", 0xc30dec83fc568c5e },
		{ "st02logo.anm", 0x93cc4e961f2fb51e },
		{ "st02mbs.ecl", 0x33c26c44f19a483 },
		{ "st02wl.anm", 0xc77333d34179c9fe },
		{ "st03.ecl", 0xe3d492d6687a0518 },
		{ "st03.std", 0x2c1913344771cb21 },
		{ "st03a.msg", 0x1e7742b05db46e63 },
		{ "st03b.msg", 0xee6ca87d2cd7f3d9 },
		{ "st03bs.ecl", 0x5e51e78e2b12124c },
		{ "st03c.msg", 0x28e18901b5850c6e },
		{ "st03d.msg", 0x1ae857d034140fb4 },
		{ "st03e.msg", 0xcb92d9cabd21b66 },
		{ "st03enm.anm", 0x76e9770bed21e62d },
		{ "st03f.msg", 0x3ce8b769d5c51598 },
		{ "st03g.msg", 0x2264320ce8430de4 },
		{ "st03h.msg", 0x8ceb46e11761bbcb },
		{ "st03i.msg", 0xeffd59281008bda4 },
		{ "st03logo.anm", 0xdcb03396f9a18a7d },
		{ "st03mbs.ecl", 0x79dde021ce5d4462 },
		{ "st03wl.anm", 0x20e7a3fadde95f1c },
		{ "st04.ecl", 0x803e620c19cd3fea },
		{ "st04.std", 0x5b93e38c522e99dc },
		{ "st04a.msg", 0x19212a51e58bde01 },
		{ "st04b.msg", 0x4f6281e6d3c14929 },
		{ "st04bs.ecl", 0x57887e3ffe7fede3 },
		{ "st04c.msg", 0xe626dade0d3610e3 },
		{ "st04d.msg", 0x7e33bf86b94e8b72 },
		{ "st04e.msg", 0xed95748d774c033f },
		{ "st04enm.anm", 0xc7c3f0f5df44d885 },
		{ "st04f.msg", 0x69bb8418ee13d36e },
		{ "st04g.msg", 0x1fa6c10c65b94e92 },
		{ "st04h.msg", 0xecdb94d4bf844cb9 },
		{ "st04i.msg", 0x973903f2b3400e55 },
		{ "st04logo.anm", 0x3de4a64de8409cfb },
		{ "st04mbs.ecl", 0x9719eb5adff34a1f },
		{ "st04wl.anm", 0x7197bbfc20d53b7d },
		{ "st05.ecl", 0xf86458ebf6dcdd16 },
		{ "st05.std", 0xb37d85e3977a53de },
		{ "st05a.msg", 0x860ba0aba78e940e },
		{ "st05b.msg", 0xf61b6916f4a9b29f },
		{ "st05bs.ecl", 0x8aed0359a9549f36 },
		{ "st05c.msg", 0xb76b5bf271191cae },
		{ "st05d.msg", 0x71e34627e6d160a7 },
		{ "st05e.msg", 0x2c3bd7d56d2bdf16 },
		{ "st05enm.anm", 0x3769e2b0f14f1dac },
		{ "st05f.msg", 0xfcd46a94396ed163 },
		{ "st05g.msg", 0x44fb7bb3c6444308 },
		{ "st05h.msg", 0x4545ff041e9a9f },
		{ "st05i.msg", 0xfc94be16cf93f68 },
		{ "st05logo.anm", 0x57633e69d0d43c86 },
		{ "st05mbs.ecl", 0x9a4dc485bedae73 },
		{ "st05wl.anm", 0x8d52c6632e3c9dc5 },
		{ "st06.ecl", 0xb53e4d9ac2510ba7 },
		{ "st06.std", 0x58c01bdd6c108b9 },
		{ "st06a.msg", 0xb254ae6a8ce45fa1 },
		{ "st06b.msg", 0xf5ba9265c3109708 },
		{ "st06bs.ecl", 0x46e88df0a389c03f },
		{ "st06c.msg", 0xef687a14362ba22b },
		{ "st06d.msg", 0xbf08c0496439c081 },
		{ "st06e.msg", 0xa43493a393eb191b },
		{ "st06enm.anm", 0xe4e86d6311544339 },
		{ "st06f.msg", 0xf4da8784a61821ef },
		{ "st06g.msg", 0x8041bcbd58c699bd },
		{ "st06h.msg", 0x96f9ef9b63df6e55 },
		{ "st06i.msg", 0xfbb0500ba14dff6e },
		{ "st06logo.anm", 0x77cc83fb5ba81bb8 },
		{ "st06mbs.ecl", 0x2b9441c87115cac },
		{ "st06wl.anm", 0x8241eb7cd2d6d0c1 },
		{ "st07.ecl", 0xc586753b540729bf },
		{ "st07.std", 0x28bf49bb31ab169c },
		{ "st07a.msg", 0x377266b642c0439e },
		{ "st07b.msg", 0x7f91b385364dcf3a },
		{ "st07bs.ecl", 0x9d236a285e3b0b48 },
		{ "st07c.msg", 0x8a2c1c8c6646842 },
		{ "st07d.msg", 0x488804f22e753468 },
		{ "st07e.msg", 0x47a89d70865fdb0f },
		{ "st07enm.anm", 0x92611315d0419fbf },
		{ "st07enm2.anm", 0xe4986faaeba515a0 },
		{ "st07f.msg", 0xff8648babd49e489 },
		{ "st07g.msg", 0xf5d860cface40fd2 },
		{ "st07h.msg", 0xddc4f10a17e7dea0 },
		{ "st07i.msg", 0xc766340a2cb4ee5c },
		{ "st07logo.anm", 0xd9c23eed693c859a },
		{ "st07mbs.ecl", 0x74300f386a1a9100 },
		{ "st07wl.anm", 0x26dca68100a2cf95 },
		{ "staff.anm", 0xa39b1747ca50b595 },
		{ "staff1.msg", 0xbd383e716cf975fa },
		{ "staff2.msg", 0xcc66ad6ea61f6da4 },
		{ "staff3.msg", 0x2606f855770b2e8f },
		{ "staff4.msg", 0x1fac0f6b2a4c0aff },
		{ "text.anm", 0x5f05a8420251630f },
		{ "th17_0100b.ver", 0x5ab6b347136623f0 },
		{ "thbgm.fmt", 0x8a0f979e93dbde09 },
		{ "title.anm", 0xf8bfb3e4378dde01 },
		{ "title_v.anm", 0x882ab30474fa0ab6 },
		{ "trophy.anm", 0x624211e16e65dff0 },
		{ "trophy.txt", 0x330a5fadd6aa2a91 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th17";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th17-test.dat";

	public ArchiveTh17Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th17.dat")]
	public void ReadArchiveTh17(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.WBaWC, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th17.dat", true)]
	public async Task ReadArchiveTh17Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.WBaWC, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh17(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.WBaWC, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.WBaWC, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh17Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.WBaWC, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.WBaWC, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
