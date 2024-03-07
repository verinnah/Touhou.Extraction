using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh10Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x5e154fa2b09b18de },
		{ "bullet.anm", 0x2faba07ef9c4090 },
		{ "capture.anm", 0x6b52d34a6595d44f },
		{ "default.ecl", 0xfea9452761e928d4 },
		{ "demo0.rpy", 0x6f9684738256ecd1 },
		{ "demo1.rpy", 0x475b097b8d3e27a4 },
		{ "demo2.rpy", 0x6adbd02b48847008 },
		{ "demo3.rpy", 0x7c44c39e3016735a },
		{ "e00.anm", 0x8ac070c7e7e6839c },
		{ "e00.msg", 0xaf9425ffa8fe5218 },
		{ "e01.anm", 0xdc2ac41ee653b24b },
		{ "e01.msg", 0xaa33591d9a12aea3 },
		{ "e02.anm", 0xbf452d95487a4f50 },
		{ "e02.msg", 0x32b2bc9a82756b4c },
		{ "e03.anm", 0xa974a893fdc402b3 },
		{ "e03.msg", 0x3b1ac0778ed4f8c5 },
		{ "e04.anm", 0xe5ef9470f4f7c594 },
		{ "e04.msg", 0x509b52d99b930aa1 },
		{ "e05.anm", 0xe54600e26994416 },
		{ "e05.msg", 0x2e66bf1732266027 },
		{ "e06.anm", 0x30a035d3b4b19574 },
		{ "e06.msg", 0xc726a71c8e0f4f1e },
		{ "e07.anm", 0x3b9b3e8962e789c1 },
		{ "e07.msg", 0x203991b875cfe818 },
		{ "e08.anm", 0x79e8ff79b3924757 },
		{ "e08.msg", 0x1d6f437964fe4f90 },
		{ "e09.anm", 0xb70a18ca6ca3b67a },
		{ "e09.msg", 0xbca94e1ff7a2d1ac },
		{ "e10.anm", 0x827d7a04f2761961 },
		{ "e10.msg", 0xe1304c18364f614c },
		{ "e11.anm", 0xf392d40cf13a5667 },
		{ "e11.msg", 0xbb5aeefc22c1124c },
		{ "enemy.anm", 0xd31fe2b5b9525752 },
		{ "front.anm", 0x665e06555fecf6d6 },
		{ "musiccmt.txt", 0xc1fe85b6bf5b8675 },
		{ "nowloading.anm", 0xfb61b6ba9915e0b3 },
		{ "pl00.anm", 0x6ba94ad7606b3416 },
		{ "pl00a.sht", 0x128eb3a22317abcb },
		{ "pl00b.sht", 0x8794c8a4ff14bb2a },
		{ "pl00c.sht", 0xe62324af8c24cb14 },
		{ "pl01.anm", 0x216fc715c1e8a83b },
		{ "pl01a.sht", 0x5195d017883c9589 },
		{ "pl01b.sht", 0xe74af863bea7e9e4 },
		{ "pl01c.sht", 0xd9c6c8328a3cb4e4 },
		{ "se_bonus3.wav", 0x79f95f4d940461b7 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
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
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_option.wav", 0xadabeccdcebc65c3 },
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
		{ "se_water.wav", 0xfe563d471b4f7fb2 },
		{ "sig.anm", 0x52f7cc39cf72d021 },
		{ "st01_00.msg", 0xd61c1b7089d902f8 },
		{ "st01_01.msg", 0xbbc4fc23e15febef },
		{ "st01logo.anm", 0x6d6a20bb371075a6 },
		{ "st02_00.msg", 0xe53ae34d647766df },
		{ "st02_01.msg", 0xb3e57fe8c5712ae },
		{ "st02logo.anm", 0xe1cd000e01c84d73 },
		{ "st03_00.msg", 0x85d441c1e5006833 },
		{ "st03_01.msg", 0xe92714ca95b95f42 },
		{ "st03logo.anm", 0xd1063b0217d8ba27 },
		{ "st04_00.msg", 0x5b0780d70967160 },
		{ "st04_01.msg", 0x285d22ab0cd48e61 },
		{ "st04logo.anm", 0xf62be3c5018dd4be },
		{ "st05_00.msg", 0x55ec34764dd4b16a },
		{ "st05_01.msg", 0x7cb6dc936134efc },
		{ "st05logo.anm", 0xb0f8a79810f7e27a },
		{ "st06_00.msg", 0x7e2874b655f0d192 },
		{ "st06_01.msg", 0x9fc2649d32436b58 },
		{ "st06logo.anm", 0xa7c68994f12c2c6c },
		{ "st07_00.msg", 0x7698f30fff8b6149 },
		{ "st07_01.msg", 0xb4ea206d3aa917b0 },
		{ "st07logo.anm", 0x19a2861ccf262a08 },
		{ "staff.anm", 0x97e726681bdc2af4 },
		{ "staff.msg", 0x1d5244b4ad0e02f1 },
		{ "stage01.anm", 0x581de1e483e3dca2 },
		{ "stage01.ecl", 0xaa1aaff13acebdc6 },
		{ "stage01.std", 0xacbe118d8b98ad1d },
		{ "stage02.anm", 0x8104ff1e69c5ba11 },
		{ "stage02.ecl", 0xb61dbe8634ffa62b },
		{ "stage02.std", 0xec03a03abd1c3f96 },
		{ "stage03.anm", 0x2c00a551a4512f1f },
		{ "stage03.ecl", 0x46595cbf0a3f76d6 },
		{ "stage03.std", 0xbf4ee0d86bbdbaf2 },
		{ "stage04.anm", 0x5c45417ae685c687 },
		{ "stage04.ecl", 0x708c7fe3ca8f68bd },
		{ "stage04.std", 0x7d5e158b4cfef007 },
		{ "stage05.anm", 0x64d5bbe60dbe2951 },
		{ "stage05.ecl", 0x2e2c7d3807bae5f1 },
		{ "stage05.std", 0x7e826fd9f01e7d57 },
		{ "stage06.anm", 0x14820f7863c1074 },
		{ "stage06.ecl", 0xdb8b10b71605ea1e },
		{ "stage06.std", 0xd335cbad1307bf22 },
		{ "stage07.anm", 0xd364fa3973c051db },
		{ "stage07.ecl", 0x2f3390b0e61934fe },
		{ "stage07.std", 0xcf0b8a1d299db051 },
		{ "stgenm01.anm", 0x4288cbdabeeeb47a },
		{ "stgenm02.anm", 0x8974ce1b931c724 },
		{ "stgenm03.anm", 0x65202d5199f45e71 },
		{ "stgenm04.anm", 0xc6ded19478e8aae5 },
		{ "stgenm05.anm", 0x472b9d54719e0ab1 },
		{ "stgenm06.anm", 0x772cdf8d170c3443 },
		{ "stgenm07.anm", 0x105b7dbd13b3be71 },
		{ "text.anm", 0x7fcadc766c76ba8c },
		{ "th10_0100a.ver", 0x61f5441c675d1e03 },
		{ "thbgm.fmt", 0xa3a9fd35865b25a3 },
		{ "title.anm", 0xa5da08b0f927cbb8 },
		{ "title_v.anm", 0xc6caf8e7df5a367d }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th10";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th10-test.dat";

	public ArchiveTh10Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th10.dat")]
	public void ReadArchiveTh10(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.MoF, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th10.dat", true)]
	public async Task ReadArchiveTh10Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.MoF, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh10(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.MoF, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.MoF, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh10Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.MoF, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.MoF, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
