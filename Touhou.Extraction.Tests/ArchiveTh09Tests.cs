using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh09Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x3b8176505d9edc16 },
		{ "capture.anm", 0xbe53d400fc1b732b },
		{ "demorpy0.rpy", 0x4ba871eb73921d4b },
		{ "demorpy1.rpy", 0x727ba1b7ff68c9b0 },
		{ "demorpy2.rpy", 0x437b3ea189b01831 },
		{ "end00.end", 0x3d6cb5f70e51ef94 },
		{ "end00a.jpg", 0xa4a8726c671c3427 },
		{ "end00b.jpg", 0x6a5fafd4d435e595 },
		{ "end00c.jpg", 0xfd47e28923b750ea },
		{ "end00d.jpg", 0x8bd953e0f9a8a76 },
		{ "end01.end", 0x790aad9761934cca },
		{ "end01a.jpg", 0x456d0468b916961d },
		{ "end01b.jpg", 0x66ac6e253b2be12d },
		{ "end01c.jpg", 0xc0cedc8486007ab0 },
		{ "end02.end", 0x5c20a9515548da65 },
		{ "end02a.jpg", 0x270f43e5f4a6fc29 },
		{ "end02b.jpg", 0x4fbae3694d7cd4aa },
		{ "end02c.jpg", 0x1682d2abfd426482 },
		{ "end03.end", 0x641ebf8df2828373 },
		{ "end03a.jpg", 0x45b0f50fd280795 },
		{ "end03b.jpg", 0x189a4832b9d6a986 },
		{ "end03c.jpg", 0xf6076aa5082109ab },
		{ "end04.end", 0x62b4a6c29f4c8ca5 },
		{ "end04a.jpg", 0x7f9bb23f835cc4ce },
		{ "end04b.jpg", 0xdb671b8709e81b5b },
		{ "end04c.jpg", 0xc3a5fe424cfae9f3 },
		{ "end05.end", 0x94110edcabb979a9 },
		{ "end05a.jpg", 0xce0d98be3163d774 },
		{ "end05b.jpg", 0x5048ec9c55a097a9 },
		{ "end05c.jpg", 0xb1903fd86b2c78bb },
		{ "end06.end", 0x91309c10c07eabb3 },
		{ "end06a.jpg", 0x4012c1441a27884d },
		{ "end06b.jpg", 0x1bf6a1691db4f283 },
		{ "end06c.jpg", 0x9b2a22fe9574cf9f },
		{ "end07.end", 0x2f13e200b365eedc },
		{ "end07a.jpg", 0xa4a8726c671c3427 },
		{ "end07b.jpg", 0x5b5916b62795fc11 },
		{ "end07c.jpg", 0x4c4b870e8b2e91d6 },
		{ "end08.end", 0x3b16e3a42ef11c2 },
		{ "end08a.jpg", 0x85529d21caae6f7a },
		{ "end08b.jpg", 0x1b6b3d9629d3201b },
		{ "end08c.jpg", 0xa9b28f69c09d13dc },
		{ "end09.end", 0x9621856a267e5637 },
		{ "end09a.jpg", 0xdb11a6e163aae67f },
		{ "end09b.jpg", 0x1b4ffbf09dc11253 },
		{ "end09c.jpg", 0xd6d9b6e31dcc9475 },
		{ "end10.end", 0xf3c6aec8520cccb4 },
		{ "end10a.jpg", 0xd4dcb95c812622e4 },
		{ "end10b.jpg", 0x31a13ad6ca57ab62 },
		{ "end10c.jpg", 0x4cbfe5e56a3bb2c8 },
		{ "end11.end", 0x72c2985a58665448 },
		{ "end11a.jpg", 0xcf46c02edb5c82e7 },
		{ "end11b.jpg", 0x4d7dd81bdc4ba9e5 },
		{ "end11c.jpg", 0x5765e3c88f0442d5 },
		{ "end12.end", 0x48ed427c9a4c0dca },
		{ "end12a.jpg", 0xc1d7f9af75977262 },
		{ "end12b.jpg", 0x68c88a32db38f8ff },
		{ "end12c.jpg", 0x288fb7436e148bd9 },
		{ "end13.end", 0x624a1232b1d6a285 },
		{ "end13a.jpg", 0xaa1910c5da01fec6 },
		{ "end13b.jpg", 0x1b555f0a292b5916 },
		{ "end13c.jpg", 0x2c15ff80a9a87054 },
		{ "endstaff.end", 0xaa612d57d4d131ac },
		{ "enemy.anm", 0x68b8937a96d73c60 },
		{ "enemy.ecl", 0x216a28b6a1fd391c },
		{ "enemy1.anm", 0x511503faccf06258 },
		{ "enemy13.anm", 0x88c3e0236e3c6381 },
		{ "etama.anm", 0xd29b8339a09c5bb2 },
		{ "front.anm", 0xd54e2b929d6692dd },
		{ "init.mid", 0x615b08e868387ca9 },
		{ "music00.anm", 0xc545b0087238bf18 },
		{ "music00.png", 0x4df9fcf569ac3d4a },
		{ "musiccmt.txt", 0x3cfb7e00b04e5a12 },
		{ "nowloading.anm", 0x98cfde71704d918e },
		{ "pl00.anm", 0x97a5c310a3973586 },
		{ "pl00.ecl", 0x42ee43eeb39a2099 },
		{ "pl00.msg", 0x9fed29f3396d63f6 },
		{ "pl00.sht", 0xe8a425ca9e74a774 },
		{ "pl00_match.msg", 0xea052de975a64336 },
		{ "pl00b.anm", 0xcd363d1f6a62f1f8 },
		{ "pl01.anm", 0x5d87fd37ca460471 },
		{ "pl01.ecl", 0x9f245e6b3b5567e4 },
		{ "pl01.msg", 0xa11619535a714121 },
		{ "pl01.sht", 0xc7de4e9c1c22c46d },
		{ "pl01_match.msg", 0xbe5518c6fda82685 },
		{ "pl01b.anm", 0x8dcf0c71a0e4d82c },
		{ "pl02.anm", 0x31d2e12789738a82 },
		{ "pl02.ecl", 0x43d6f37b9664b7bb },
		{ "pl02.msg", 0x5bcebde9430321c5 },
		{ "pl02.sht", 0xc668c6ac5251d2c },
		{ "pl02_match.msg", 0x6c7fc8a1844a7a0e },
		{ "pl02b.anm", 0x748c45d2fc798ff4 },
		{ "pl03.anm", 0x9f4095de570a518c },
		{ "pl03.ecl", 0x693baaab6144bbf0 },
		{ "pl03.msg", 0xe483e0766bd2eb0e },
		{ "pl03.sht", 0xe8a1a23b76956672 },
		{ "pl03_match.msg", 0x33bb2d679e9d475e },
		{ "pl03b.anm", 0x92a606b494af947a },
		{ "pl04.anm", 0x168d6bdb9bec6e7e },
		{ "pl04.ecl", 0xb9b421832a898977 },
		{ "pl04.msg", 0x6bb5209cb0aa1772 },
		{ "pl04.sht", 0x29e4690f75dfeeb9 },
		{ "pl04_match.msg", 0x88c90c0f580852e2 },
		{ "pl04b.anm", 0x3ac2c5e2b6470efd },
		{ "pl05.anm", 0xfaccad3bebdb5893 },
		{ "pl05.ecl", 0xe545134a9ec354cc },
		{ "pl05.msg", 0x23c3b1b5972f404 },
		{ "pl05.sht", 0x6491b983f577c060 },
		{ "pl05_match.msg", 0x535632d7f8f6f111 },
		{ "pl05b.anm", 0xef60cd976da9839 },
		{ "pl06.anm", 0x7f464501202843e5 },
		{ "pl06.ecl", 0xe0f66a0c98bf5f6a },
		{ "pl06.msg", 0xf606fc334a0b6001 },
		{ "pl06.sht", 0xb238e4053b67d0ae },
		{ "pl06_fc_s.anm", 0x4f3554b457a0744e },
		{ "pl06_match.msg", 0x23d0b28f584efaa3 },
		{ "pl06b.anm", 0x8322e12749d24b14 },
		{ "pl07.anm", 0x8ee60a43f045c1cd },
		{ "pl07.ecl", 0xc8320cc1a7c45829 },
		{ "pl07.msg", 0x2ac780bcf059072b },
		{ "pl07.sht", 0x34c4af770dce2380 },
		{ "pl07_match.msg", 0x89b0ba804a1ee783 },
		{ "pl07b.anm", 0xa430cc0a7185ee38 },
		{ "pl08.anm", 0x31e583ca641712ab },
		{ "pl08.ecl", 0x39a65e72daf17708 },
		{ "pl08.msg", 0xff3e27d34d4bb55a },
		{ "pl08.sht", 0xe0842116d3d256f0 },
		{ "pl08_match.msg", 0xa363ce390f6044f0 },
		{ "pl08b.anm", 0xd8639b98c906ccac },
		{ "pl09.anm", 0x6a7f2c7d5e7ef7f },
		{ "pl09.ecl", 0xd90fa676492cc022 },
		{ "pl09.msg", 0xb194537065a0d86a },
		{ "pl09.sht", 0x6fc65935c90e7353 },
		{ "pl09_match.msg", 0x7143e388a602eea5 },
		{ "pl09b.anm", 0xbe338e1d013c21e5 },
		{ "pl10.anm", 0x5e66aaa0e0a25d4f },
		{ "pl10.ecl", 0x9d36d0cba3b91a2b },
		{ "pl10.msg", 0x30c427d7017d271e },
		{ "pl10.sht", 0xa14ba5904e893220 },
		{ "pl10_match.msg", 0x326cf7730de8a528 },
		{ "pl10b.anm", 0x4740f48160bbea5e },
		{ "pl11.anm", 0x77cbc7a5c50d6486 },
		{ "pl11.ecl", 0xeb9c31fa3cf97eb9 },
		{ "pl11.msg", 0x7f1b28dbd1867a6a },
		{ "pl11.sht", 0x3f8bd7fb89053d75 },
		{ "pl11_match.msg", 0x938257bad56b09c9 },
		{ "pl11b.anm", 0x7eb34259482ce8b },
		{ "pl12.anm", 0xdfea90957773e2c2 },
		{ "pl12.ecl", 0xc50070ec4b2d21ac },
		{ "pl12.msg", 0x1747bbac368bf6b2 },
		{ "pl12.sht", 0x4f1514b0c495f20d },
		{ "pl12_match.msg", 0xf8ea811b3ce9ace8 },
		{ "pl12b.anm", 0x7270e8b4d9cb2f1a },
		{ "pl13.anm", 0xcf239d7059e6ada },
		{ "pl13.ecl", 0xf25eed0bfce2ed05 },
		{ "pl13.msg", 0xf88540d6b03b228a },
		{ "pl13.sht", 0x27ad62416f8e1074 },
		{ "pl13_match.msg", 0xd2bc774ddd731693 },
		{ "pl13b.anm", 0xb9e4d9149ada210b },
		{ "pl14.anm", 0x79b7f01212cfab29 },
		{ "pl14.ecl", 0x4bceea919d8d19e9 },
		{ "pl14.sht", 0x1f91b13945463606 },
		{ "pl14_match.msg", 0xa65829d87e409fd7 },
		{ "pl14b.anm", 0xa708b0e018a27ed },
		{ "pl15.anm", 0xa1454c99ce22962e },
		{ "pl15.ecl", 0x130301afbd5f1447 },
		{ "pl15.sht", 0xea1b221a0c9f77e9 },
		{ "pl15_match.msg", 0xe8c3c9eda167d4f0 },
		{ "pl15b.anm", 0x82f2392cbda55e92 },
		{ "replay00.png", 0x4e0d3143f9395051 },
		{ "result00.anm", 0xbe08f31b6d3a676a },
		{ "result00.png", 0x123155c687190fb2 },
		{ "resulttext.anm", 0xb978ec31b038a886 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_charge00.wav", 0x964481dd85455817 },
		{ "se_charge01b.wav", 0x51f21e810161f13c },
		{ "se_chargeup.wav", 0x6ae141735d722073 },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_eterase.wav", 0xff8a43cbe44263d9 },
		{ "se_exattack.wav", 0xe8ec2ce65cf5a152 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_gosp.wav", 0x15f65bc53014f5e4 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_life1.wav", 0x87924e5bd88e72f4 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_playerdead.wav", 0xe143037dd6c5f8a8 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xdc3657c81ae4f0c3 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0x18d8eb75d98e6104 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_timestop0.wav", 0xb97722107369bac8 },
		{ "se_warning.wav", 0x7ed2c6c099b4af6b },
		{ "select00.png", 0xf40530a8870f6cf0 },
		{ "staff00.jpg", 0x6c4a2a9d4d6d8b58 },
		{ "staff01.anm", 0x99c3042f86b67786 },
		{ "text.anm", 0xdca0910067a37092 },
		{ "th09.mup", 0x4d647b991a1bc59a },
		{ "th09_00.mid", 0x4fc35092afa7986e },
		{ "th09_00b.mid", 0x848f8beb4827ada2 },
		{ "th09_00c.mid", 0x49a9dbcc906034e2 },
		{ "th09_01.mid", 0xaab8e383228292ba },
		{ "th09_0150a.ver", 0xf6d584ac54fd84e2 },
		{ "th09_02.mid", 0xd7752ee84d285744 },
		{ "th09_05.mid", 0xb2ed353f294c1e8e },
		{ "th09_07.mid", 0x1c9f80cd742263de },
		{ "th09logo.jpg", 0x2624d765ec270e9e },
		{ "thbgm.fmt", 0x13b489957af54e51 },
		{ "title00.png", 0x91d3a2768028d747 },
		{ "title01.anm", 0x4a15427f51fb0d33 },
		{ "world00.anm", 0x4f4dfb74b85bdbd8 },
		{ "world00.std", 0xe0932235da44b405 },
		{ "world01.anm", 0x3505f3056824e203 },
		{ "world01.std", 0xbeb1c0cda535a3e6 },
		{ "world03.anm", 0xc98acafdb0e4b266 },
		{ "world03.std", 0xcc356cd6348dc521 },
		{ "world04.anm", 0xb0f949163eada822 },
		{ "world04.std", 0xbfeef9c8cf48380 },
		{ "world05.anm", 0x9a86e1b1dffa74a3 },
		{ "world05.std", 0xb9d3bac98e5e7011 },
		{ "world06.anm", 0xe0a3478f51b007ff },
		{ "world06.std", 0xa2a602a21c66fcf5 },
		{ "world07.anm", 0x94371a5e9d10118f },
		{ "world07.std", 0x7606bd88ba80d4cd },
		{ "world09.anm", 0x1e2f5519dfe2e8d1 },
		{ "world09.std", 0x2e1dbfc22fc6fb1c },
		{ "world09m.std", 0x81243fe9dad52201 },
		{ "world10.anm", 0x167f745bf1c6648 },
		{ "world10.std", 0x6cd2a28be01b7bc7 },
		{ "world11.anm", 0x755c28762f5b15cc },
		{ "world11.std", 0xe3d3e88548f086ca },
		{ "world11m.std", 0x48c71e8adf051efc },
		{ "world12.anm", 0x865c815ed0ac285d },
		{ "world12.std", 0x2046b3352c71ec7c },
		{ "world13.anm", 0xd8762efc179d5630 },
		{ "world13.std", 0x4eaa07979d457f10 },
		{ "world13m.std", 0x14b8e4fb819d154d }
	}.ToFrozenDictionary();

	private const string TEST_PATH = @"test-data\th09";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th09-test.dat";

	public ArchiveTh09Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th09.dat")]
	public void ReadArchiveTh09(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.PoFV, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH08.PBGZ>(archive);
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
	[InlineData($"{TEST_PATH}\\th09.dat", true)]
	public async Task ReadArchiveTh09Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.PoFV, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH08.PBGZ>(archive);
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
	public void WriteArchiveTh09(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.PoFV, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.PoFV, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh09Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.PoFV, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.PoFV, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
