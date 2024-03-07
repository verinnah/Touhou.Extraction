using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh12Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x597ddd8dbee247ba },
		{ "bullet.anm", 0xcdb1f17b0fb89d4 },
		{ "default.ecl", 0xdc594e3d65a3559d },
		{ "demo0.rpy", 0xdfff4ccc7ee8bd7c },
		{ "demo1.rpy", 0xf0bfbcc8fa672ae3 },
		{ "demo2.rpy", 0x59843bf86b1f0f1b },
		{ "demo3.rpy", 0x59843bf86b1f0f1b },
		{ "e00.anm", 0x34e84fa0696a929f },
		{ "e00.msg", 0xdab37a38035304aa },
		{ "e01.anm", 0xe36328c78c92b911 },
		{ "e01.msg", 0x14df49b4f35f78bf },
		{ "e02.anm", 0x28b76a27a1b8076f },
		{ "e02.msg", 0xb5c1fee4e5cf096d },
		{ "e03.anm", 0xc79542944e9cc835 },
		{ "e03.msg", 0x48ac3670b4dd37d6 },
		{ "e04.anm", 0x121f74bc38cea128 },
		{ "e04.msg", 0xb621b793fad3c0de },
		{ "e05.anm", 0x4586134b7b90f99e },
		{ "e05.msg", 0x5221929c98eb8082 },
		{ "e06.anm", 0x5ec7fcd220141fa },
		{ "e06.msg", 0x32d6d6e547addb12 },
		{ "e07.anm", 0xe127bb4034b632c1 },
		{ "e07.msg", 0x6e25e23aa4d4ed9b },
		{ "e08.anm", 0xf26f68d6f54a2c7b },
		{ "e08.msg", 0x16592b6a9dffdc7e },
		{ "e09.anm", 0x166c2469c418a18 },
		{ "e09.msg", 0x7182a3decd4f0f86 },
		{ "e10.anm", 0xcf5472f9efe92265 },
		{ "e10.msg", 0xe104ec1bb858c09c },
		{ "e11.anm", 0x7455ff97dc639fbc },
		{ "e11.msg", 0xdd31cd5313e8b9c4 },
		{ "enemy.anm", 0xa32f107eb3777f20 },
		{ "front.anm", 0x12c4847e52bfc14f },
		{ "musiccmt.txt", 0x5f14e9cb30982c55 },
		{ "pl00.anm", 0xfa5d68ab3dfe5f1 },
		{ "pl00a.sht", 0xb794db9d4f996cc },
		{ "pl00b.sht", 0x46e9cb5ee52ec4cb },
		{ "pl01.anm", 0x670d67bc341bdf03 },
		{ "pl01a.sht", 0x8da387e598327afe },
		{ "pl01b.sht", 0x9f401dd916e21d5a },
		{ "pl02.anm", 0x1584427d63dfeb33 },
		{ "pl02a.sht", 0x79901357a4cba5ac },
		{ "pl02b.sht", 0x740e740b4489d5b3 },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_changeitem.wav", 0x6aec872d94ad63df },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_don00.wav", 0x63c0b2aeacd1e2a8 },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_enep02.wav", 0xccecfaf28e083794 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_item01.wav", 0x483a913fdaf532aa },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_lazer02.wav", 0x4a056aa97b4c53d3 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_nodamage.wav", 0xc186b0bb70f73fb8 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_option.wav", 0xf77e827eef8f3242 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_piyo.wav", 0x78729a640997df96 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xa8311a42586a4915 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0x18d8eb75d98e6104 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_ufo.wav", 0xae45302d06837a38 },
		{ "se_ufoalert.wav", 0x61a1c8423f756073 },
		{ "sig.anm", 0x9cc8b405f0e37626 },
		{ "st01_00a.msg", 0x4a2b3bcc0f831415 },
		{ "st01_00b.msg", 0xe7d970922a6675b0 },
		{ "st01_01a.msg", 0x82522db3c1b87b8 },
		{ "st01_01b.msg", 0x89dcccfdcd0fefd5 },
		{ "st01_02a.msg", 0x4cbf07dc2bd23e68 },
		{ "st01_02b.msg", 0x70dfc0dd38dca8a8 },
		{ "st01logo.anm", 0xdf49af52651c8a2a },
		{ "st02_00a.msg", 0xf3f969bfaddd1453 },
		{ "st02_00b.msg", 0x7ffbc533829a0e8b },
		{ "st02_01a.msg", 0xd277a5657a6fa477 },
		{ "st02_01b.msg", 0x888b374739ecc43e },
		{ "st02_02a.msg", 0x420c7ebabda7dde7 },
		{ "st02_02b.msg", 0xc1227c57549b8178 },
		{ "st02logo.anm", 0x2bf0fab360083b66 },
		{ "st03_00a.msg", 0x1cfbd17904d18 },
		{ "st03_00b.msg", 0x8d4c9f6d297def76 },
		{ "st03_01a.msg", 0x2a43f8c081b3223f },
		{ "st03_01b.msg", 0x2a4666b786154368 },
		{ "st03_02a.msg", 0x94b13ded58d5ef97 },
		{ "st03_02b.msg", 0x4f41f9e34a371e45 },
		{ "st03logo.anm", 0xe75414c0950594c7 },
		{ "st04_00a.msg", 0x10c23532dfa5be43 },
		{ "st04_00b.msg", 0xb19a1d0b208fb512 },
		{ "st04_01a.msg", 0x4b36a35eadc86fd2 },
		{ "st04_01b.msg", 0x9b901d1bc9c035b7 },
		{ "st04_02a.msg", 0x50bad77de8ba28ae },
		{ "st04_02b.msg", 0x905149cbacc32130 },
		{ "st04logo.anm", 0x96d4f4fe4d2c609c },
		{ "st05_00a.msg", 0x5a0d2d182ebf90d4 },
		{ "st05_00b.msg", 0x3fc4d81dc5630648 },
		{ "st05_01a.msg", 0x507039d4aa965f0b },
		{ "st05_01b.msg", 0xaa34003526a6106d },
		{ "st05_02a.msg", 0xee6f6690220deb9f },
		{ "st05_02b.msg", 0x2859dc969cf57090 },
		{ "st05logo.anm", 0xf813b982d544cc6d },
		{ "st06_00a.msg", 0xbc0ba1da58b747fa },
		{ "st06_00b.msg", 0x85df214f6f28ce3d },
		{ "st06_01a.msg", 0xdaefa67e468d361e },
		{ "st06_01b.msg", 0x485dcba25ea6b30d },
		{ "st06_02a.msg", 0xe48ce642b352849f },
		{ "st06_02b.msg", 0x3547a2fdff13b925 },
		{ "st06logo.anm", 0x9afe19b4c092f3de },
		{ "st07_00a.msg", 0x5f9ddc12a53fd4a0 },
		{ "st07_00b.msg", 0xf4a163976b587f8c },
		{ "st07_01a.msg", 0xac79c41acb349422 },
		{ "st07_01b.msg", 0xcfc730fa79dab27e },
		{ "st07_02a.msg", 0xe867c76d3113778c },
		{ "st07_02b.msg", 0x95a541343858270e },
		{ "st07logo.anm", 0x4572c7fefbef3c72 },
		{ "staff.anm", 0x1d8998cb97379213 },
		{ "staff.msg", 0xcd2cef75148e22e },
		{ "stage01.anm", 0xd23f3942c5372ef2 },
		{ "stage01.ecl", 0xb5357d57a417e746 },
		{ "stage01.std", 0x4a32752f01c7ae46 },
		{ "stage02.anm", 0xe1a8a42164922361 },
		{ "stage02.ecl", 0x396c6d94604da23a },
		{ "stage02.std", 0x8a7a05a871c592e },
		{ "stage03.anm", 0xf081db462c313e7c },
		{ "stage03.ecl", 0x649a00f1ae7de3b7 },
		{ "stage03.std", 0x485d8d508faaa4fb },
		{ "stage04.anm", 0xff9b269878a09de7 },
		{ "stage04.ecl", 0x9191eb7bbfd14bbd },
		{ "stage04.std", 0xde1d6f92e8dee7d5 },
		{ "stage05.anm", 0xa9e7bc13850829f3 },
		{ "stage05.ecl", 0x5694300606b07413 },
		{ "stage05.std", 0x6cc8ed27ec594abe },
		{ "stage06.anm", 0x6ddbd93adbd829e3 },
		{ "stage06.ecl", 0x21abe70d17947c2a },
		{ "stage06.std", 0xd9e579ae500b28c },
		{ "stage07.anm", 0x44664eec7fe743ce },
		{ "stage07.ecl", 0x962f5d09b5b11ab7 },
		{ "stage07.std", 0x500b986466fcec43 },
		{ "stage07boss.ecl", 0x2dd7ec3acd96a5c6 },
		{ "stgenm01.anm", 0xcf1ed1e3e860fac5 },
		{ "stgenm02.anm", 0x6f4f0bdbccb9ec18 },
		{ "stgenm03.anm", 0xed290681b8e1013e },
		{ "stgenm04.anm", 0xf02b2b618c8bf101 },
		{ "stgenm05.anm", 0x152d7f0e0bed9437 },
		{ "stgenm05m.anm", 0x20d87236ab41fcdc },
		{ "stgenm06.anm", 0xc8157536ec65df04 },
		{ "stgenm07.anm", 0xc9cea1f9a2635aa },
		{ "stgenm07m.anm", 0x795c0a49dbb4be10 },
		{ "text.anm", 0x350fd56a57b5e196 },
		{ "th12_0100b.ver", 0xac2a0a35b999e99d },
		{ "thbgm.fmt", 0xf3006188032b5c3f },
		{ "title.anm", 0xab1a01bb10112014 },
		{ "title_v.anm", 0x29051fcd4d8e8b5a }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th12";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th12-test.dat";

	public ArchiveTh12Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th12.dat")]
	public void ReadArchiveTh12(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.UFO, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th12.dat", true)]
	public async Task ReadArchiveTh12Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.UFO, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh12(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.UFO, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.UFO, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh12Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.UFO, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.UFO, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
