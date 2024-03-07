using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh11Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x52cdfd9885937a9b },
		{ "bullet.anm", 0x571bd9ea7a66a39a },
		{ "default.ecl", 0x225ae544347fa81e },
		{ "demo0.rpy", 0x2427a9f960d416fc },
		{ "demo1.rpy", 0xb2e2c4ebf047b13d },
		{ "demo2.rpy", 0xc2ed357a0a6c3dc5 },
		{ "demo3.rpy", 0x1fc5506cda36943f },
		{ "e00.anm", 0x959e1018a97b9a58 },
		{ "e00.msg", 0xf4728feef3fa6275 },
		{ "e01.anm", 0x69cb675632560ec6 },
		{ "e01.msg", 0x76f66ec00a6de562 },
		{ "e02.anm", 0xf4675d0d6a40a3c9 },
		{ "e02.msg", 0xcc5b028e238078b2 },
		{ "e03.anm", 0x964cbd0f07642b94 },
		{ "e03.msg", 0x81b15d3299b20f02 },
		{ "e04.anm", 0xe37092b916cef48f },
		{ "e04.msg", 0x92b671e8769a33e },
		{ "e05.anm", 0x338cb2e0e9d19091 },
		{ "e05.msg", 0x8ae3848b7da496fe },
		{ "e06.anm", 0x368c7c21d1966eb1 },
		{ "e06.msg", 0x10d28286c76f20e4 },
		{ "e07.anm", 0xa5c622c453652450 },
		{ "e07.msg", 0xed76615eaa6bec0 },
		{ "e08.anm", 0xbcb41c440afd6b0d },
		{ "e08.msg", 0x904bdb73b6cd9fb8 },
		{ "e09.anm", 0xe2f01ceb233d8941 },
		{ "e09.msg", 0xd7a71a25ccb6082d },
		{ "e10.anm", 0x8be73c4841397159 },
		{ "e10.msg", 0x6d2b51dc7ef23f13 },
		{ "e11.anm", 0x73022d2051aa5bf6 },
		{ "e11.msg", 0xbeac25998074e8b6 },
		{ "enemy.anm", 0xa4bb0a585fe967a9 },
		{ "front.anm", 0x4cbad8f94c07defa },
		{ "musiccmt.txt", 0x490e8ae11d3bf169 },
		{ "pl00.anm", 0x4917dc3c7f7153f5 },
		{ "pl00a.sht", 0x2da8b83ec59d7b86 },
		{ "pl00b.sht", 0x789ac482d5cc603f },
		{ "pl00c.sht", 0x70966438fbb6c59c },
		{ "pl01.anm", 0x5857cb7f0e04d7ea },
		{ "pl01a.sht", 0xbc4053f963bc8889 },
		{ "pl01b.sht", 0x9f6cd7fe15986981 },
		{ "pl01c.sht", 0x2dde1d37adb39ad2 },
		{ "se_alert.wav", 0xdb7d5e019573b8e8 },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_cat01.wav", 0xdafdbe921b8a187a },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_enep02.wav", 0xccecfaf28e083794 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_hint00.wav", 0x8d3972cd463a626f },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_msl.wav", 0xa051455a80988408 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_option.wav", 0xf77e827eef8f3242 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
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
		{ "se_warpl.wav", 0x3f46b12f94b42d43 },
		{ "se_warpr.wav", 0x8940e22b4364b080 },
		{ "se_water.wav", 0xfe563d471b4f7fb2 },
		{ "sig.anm", 0xe56556f954193cbc },
		{ "st01_00a.msg", 0xf51320138728998f },
		{ "st01_00b.msg", 0xd7f5208a1c519912 },
		{ "st01_00c.msg", 0x46afb0564e0881e0 },
		{ "st01_01a.msg", 0xed392b4556add2ad },
		{ "st01_01b.msg", 0x89958884c221843c },
		{ "st01_01c.msg", 0x5845ebad550cc405 },
		{ "st01logo.anm", 0xf0dc64c45f8a7572 },
		{ "st02_00a.msg", 0xc26cd9ce1a3c052f },
		{ "st02_00b.msg", 0xe0231fe91e2b2a2 },
		{ "st02_00c.msg", 0xe07f5dddc78623df },
		{ "st02_01a.msg", 0xccfa111f7638bb92 },
		{ "st02_01b.msg", 0x11538367b3be8477 },
		{ "st02_01c.msg", 0x12045191b0202f16 },
		{ "st02logo.anm", 0x618ac5a3ef6b4c7 },
		{ "st03_00a.msg", 0xa41effd26f0f899e },
		{ "st03_00b.msg", 0xf5d35fcd8098ea03 },
		{ "st03_00c.msg", 0xc443fd36a2c1b190 },
		{ "st03_01a.msg", 0x99bb1bad9d59a41d },
		{ "st03_01b.msg", 0x11bc8da6d8411ded },
		{ "st03_01c.msg", 0x27f34e5d025e973f },
		{ "st03logo.anm", 0xaa66abc8e0ddc567 },
		{ "st04_00a.msg", 0xbc7fd98c30206249 },
		{ "st04_00b.msg", 0xa55baa02e2963013 },
		{ "st04_00c.msg", 0x2668fc343f4ba6d2 },
		{ "st04_01a.msg", 0xb9441ab1cb8a449e },
		{ "st04_01b.msg", 0xd2fc7334437678e4 },
		{ "st04_01c.msg", 0xe2c9cc7ca7d3dcea },
		{ "st04logo.anm", 0x663d822531cb56ec },
		{ "st05_00a.msg", 0xb11be5ac0a80894d },
		{ "st05_00b.msg", 0x8908b7ef5981e499 },
		{ "st05_00c.msg", 0x866ea2028a1b1819 },
		{ "st05_01a.msg", 0x7012bcb96a826545 },
		{ "st05_01b.msg", 0x83e5eb86bea1e72 },
		{ "st05_01c.msg", 0xf8455d008f2b094e },
		{ "st05logo.anm", 0x66fef27aeef4c2bd },
		{ "st06_00a.msg", 0x9a1e4ab7b83bf2d },
		{ "st06_00b.msg", 0x4601b8d9d305c097 },
		{ "st06_00c.msg", 0xa052e580c345c5d7 },
		{ "st06_01a.msg", 0x9e3cebc7c68f6ff8 },
		{ "st06_01b.msg", 0x97ed33d08ab21a74 },
		{ "st06_01c.msg", 0xc048b47e97ae1f41 },
		{ "st06logo.anm", 0xff4b3635e84a0b02 },
		{ "st07_00a.msg", 0xf194276a00276800 },
		{ "st07_00b.msg", 0x29a83fb30600ccd4 },
		{ "st07_00c.msg", 0x7711a9881ea550f7 },
		{ "st07_01a.msg", 0x261fe7b2196fa1bf },
		{ "st07_01b.msg", 0x175ff424cbecd37 },
		{ "st07_01c.msg", 0xb7ff9bae10ed38fa },
		{ "st07logo.anm", 0x446abd941a54a0c3 },
		{ "staff.anm", 0x4ac4ec09d89d5c33 },
		{ "staff.msg", 0xb5d357c43ca919db },
		{ "stage01.anm", 0xea7698344aed392e },
		{ "stage01.ecl", 0xa39559220035d105 },
		{ "stage01.std", 0xba91f6e662ee1b41 },
		{ "stage02.anm", 0x5496f882053d3037 },
		{ "stage02.ecl", 0x1a93c00da60a2720 },
		{ "stage02.std", 0x23d58ebeca43d451 },
		{ "stage03.anm", 0x37fdea333002a8fe },
		{ "stage03.ecl", 0x9e4016ad98941de7 },
		{ "stage03.std", 0xbfc4d16519c9f86b },
		{ "stage04.anm", 0x1031fdf474d1d59e },
		{ "stage04.ecl", 0x4a380770c3c6f825 },
		{ "stage04.std", 0x33ecdd3a2e7d84da },
		{ "stage05.anm", 0x6aecbd67bd78d811 },
		{ "stage05.ecl", 0x93b4dfdda0d9ed8a },
		{ "stage05.std", 0x34b86b988dd771ae },
		{ "stage05boss.ecl", 0xf049c83c35a31f52 },
		{ "stage05mboss.ecl", 0xdbd337d4ca0e53fd },
		{ "stage06.anm", 0xf512d85032cfef29 },
		{ "stage06.ecl", 0x43b82bc5d588925b },
		{ "stage06.std", 0xa08a3d1d4f34b6b5 },
		{ "stage06boss.ecl", 0xe87e4a90a0acc496 },
		{ "stage06mboss.ecl", 0x136144b383e829ce },
		{ "stage07.anm", 0xf294d5c417559206 },
		{ "stage07.ecl", 0x22f13a28b03c2efd },
		{ "stage07.std", 0x64cacbf5f5995e5 },
		{ "stage07boss.ecl", 0xa8a714657b20a1a8 },
		{ "stage07mboss.ecl", 0x2ae7512a174b898c },
		{ "stage4c00a.ecl", 0x9e984d34ab89ee85 },
		{ "stage4c00b.ecl", 0x55a1b3b312e8d24d },
		{ "stage4c00c.ecl", 0xd0eb396dc05b1399 },
		{ "stage4c01a.ecl", 0x18345dddb73a5c24 },
		{ "stage4c01b.ecl", 0xb90e4df0f2a99a7 },
		{ "stage4c01c.ecl", 0xbf2230e68ebdb9de },
		{ "stgenm01.anm", 0xaea8ad5cf22151fd },
		{ "stgenm02.anm", 0xad93d4f005cb9638 },
		{ "stgenm03.anm", 0x12295a277c0186a },
		{ "stgenm04.anm", 0xfd9c71470f6b8e43 },
		{ "stgenm05.anm", 0xa8e091ce727de28b },
		{ "stgenm06.anm", 0xf23ec51dbec3fad8 },
		{ "stgenm07.anm", 0x5f33d9fc537da02d },
		{ "text.anm", 0x3865f73177340ca6 },
		{ "th11_0100a.ver", 0x61f5441c675d1e03 },
		{ "thbgm.fmt", 0x32a44ee27bed9022 },
		{ "title.anm", 0x723f16f70e90a572 },
		{ "title_v.anm", 0xe7a22d9450b956a7 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th11";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th11-test.dat";

	public ArchiveTh11Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th11.dat")]
	public void ReadArchiveTh11(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.SA, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th11.dat", true)]
	public async Task ReadArchiveTh11Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.SA, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh11(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.SA, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.SA, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh11Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.SA, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.SA, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
