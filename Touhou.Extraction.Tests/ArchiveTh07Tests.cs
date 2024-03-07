using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh07Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x27006193c7743ac6 },
		{ "capture.anm", 0x8d9977b19ddd910a },
		{ "demorpy0.rpy", 0xecfad623d321ee6 },
		{ "demorpy1.rpy", 0x3f1f88e360b7cc50 },
		{ "demorpy2.rpy", 0xcf0375aa1d885b93 },
		{ "ecldata1.ecl", 0xb461e6e31ae552ca },
		{ "ecldata2.ecl", 0x16596ba5aac8b363 },
		{ "ecldata3.ecl", 0x9361177cdc801cb8 },
		{ "ecldata4.ecl", 0x531f872a930a13c6 },
		{ "ecldata5.ecl", 0x601f382f351cab89 },
		{ "ecldata6.ecl", 0x76e51131a5b932ab },
		{ "ecldata7.ecl", 0xe5d2415ac09b57d8 },
		{ "ecldata8.ecl", 0xd6e17a1473910dc7 },
		{ "eff01.anm", 0xe2d30aa3ead7d779 },
		{ "eff02.anm", 0x33b578efbc0ee871 },
		{ "eff03.anm", 0x5268176684211379 },
		{ "eff04.anm", 0xc93daead25fcd25e },
		{ "eff04b.anm", 0xb22d148ec23f5ba8 },
		{ "eff05.anm", 0x836d10e2aab48aec },
		{ "eff06.anm", 0xc0ac3377fb407105 },
		{ "eff07.anm", 0x6e494ad8be2c843a },
		{ "eff08.anm", 0xf49650691fafea4e },
		{ "end00.end", 0x22495fe3e4c5c87d },
		{ "end00.jpg", 0x9f1499e04bb413a3 },
		{ "end00b.end", 0x77c2ef010ab6a955 },
		{ "end00b.jpg", 0xc4c47c7bb19566b3 },
		{ "end01.end", 0xa14950fad4031ac5 },
		{ "end01.jpg", 0xafac415f1b7a398 },
		{ "end02.jpg", 0x673d77fb30f9340a },
		{ "end02b.jpg", 0x145e9a9c86b4a243 },
		{ "end03.jpg", 0x649ff0927fb8e688 },
		{ "end04.jpg", 0x67b460f77a257b4f },
		{ "end05.jpg", 0xebab0bcad3e13bab },
		{ "end06.jpg", 0x9dfb21d7d2060b32 },
		{ "end07.jpg", 0x1e3f4ead8da3bba1 },
		{ "end08.jpg", 0x9cfbd27d4e72b049 },
		{ "end09.jpg", 0xa26168d169450bef },
		{ "end10.end", 0x9d36b519d7a5f861 },
		{ "end10.jpg", 0x6f1c62d6c11069b9 },
		{ "end10b.end", 0x7281c8a272bc24b7 },
		{ "end11.end", 0xf44195fa89b7109e },
		{ "end11.jpg", 0x9bca9d85eb2ce0d8 },
		{ "end12.jpg", 0xf34648bcc6ef82b9 },
		{ "end13.jpg", 0x61c9ec79b4859d76 },
		{ "end14.jpg", 0x7bee1bda476d3573 },
		{ "end15.jpg", 0x6e201137b11f6afa },
		{ "end16.jpg", 0x4dd78b156f866692 },
		{ "end17.jpg", 0x2890269e93b96c47 },
		{ "end18.jpg", 0xa419f9a182f2e3ed },
		{ "end19.jpg", 0xdff962d886cc7b89 },
		{ "end20.end", 0x904323de17cf3c21 },
		{ "end20.jpg", 0x6a01ae5062ee6a5b },
		{ "end20b.end", 0x281375c4304ef5c1 },
		{ "end21.end", 0x7dd87d1258e034bb },
		{ "end21.jpg", 0xe281e478767734b0 },
		{ "etama.anm", 0xd6a3b40487b1467b },
		{ "face_01_00.anm", 0xaecfcbc601bf7f46 },
		{ "face_02_00.anm", 0x6ad162a9f6143551 },
		{ "face_03_00.anm", 0x741b70e6eaf1d052 },
		{ "face_04_00.anm", 0xae5d1753c39439df },
		{ "face_05_00.anm", 0x6a4f19bea4da741f },
		{ "face_06_00.anm", 0x82b043c0dd0ed035 },
		{ "face_07_00.anm", 0xb8b9696e00a40997 },
		{ "face_08_00.anm", 0xa598eb115c4556da },
		{ "face_mr00.anm", 0x69d9c0826886522c },
		{ "face_rm00.anm", 0x1606a3b5a21a1533 },
		{ "face_sk00.anm", 0x3642e3f01f326942 },
		{ "front.anm", 0x8cde7fe97428e0d2 },
		{ "init.mid", 0x615b08e868387ca9 },
		{ "loading.anm", 0x1de2dbe54304d35f },
		{ "loading2.anm", 0x2e121055d40aa0c1 },
		{ "loading3.anm", 0x41b92e402b735405 },
		{ "msg1.dat", 0xe579e2f8b8af2cd4 },
		{ "msg2.dat", 0x652c827c1ce64819 },
		{ "msg3.dat", 0x8a7505f1af2c1534 },
		{ "msg4.dat", 0xc1adc80a1e1fa9a6 },
		{ "msg5.dat", 0x693035097bead412 },
		{ "msg6.dat", 0x2e0f230dc54d2627 },
		{ "msg7.dat", 0x2c16caacf5172b6 },
		{ "msg8.dat", 0x275a5d5b5868f3ae },
		{ "music.jpg", 0xdfe133727bf0b7b5 },
		{ "music00.anm", 0x56a6fc0966b261c6 },
		{ "musiccmt.txt", 0xd6282a3ff5d018d7 },
		{ "phantasm.jpg", 0xe939da7a201641eb },
		{ "player00.anm", 0x7fa71ea1743c1bc4 },
		{ "player01.anm", 0xed095c7613b3fcfe },
		{ "player02.anm", 0x92c3c3dc7fed9f87 },
		{ "ply00a.sht", 0x550a6d5e115dc494 },
		{ "ply00as.sht", 0x7bbe35f1deb1764e },
		{ "ply00b.sht", 0x5dfcedbcb60d4799 },
		{ "ply00bs.sht", 0x7587a699e762a9f2 },
		{ "ply01a.sht", 0x5e4eae4e9b11bc5 },
		{ "ply01as.sht", 0x1da81ee4ae95a9a },
		{ "ply01b.sht", 0x419cf05cb6e8bab3 },
		{ "ply01bs.sht", 0x86b0a9accdad1176 },
		{ "ply02a.sht", 0x39496640cdb24ba0 },
		{ "ply02as.sht", 0x1b5c473de24f891e },
		{ "ply02b.sht", 0x2886024d07962845 },
		{ "ply02bs.sht", 0xd982c12562ef7108 },
		{ "result.jpg", 0x67d59eed492114a1 },
		{ "result00.anm", 0x85e262d55d042546 },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_border.wav", 0x27471485eab0fd5a },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cat00.wav", 0xe9c3723f9c349cd4 },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xb6d1b84102563556 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_kira00.wav", 0x4f2f039598526c8c },
		{ "se_kira01.wav", 0x2c536ba2b46f2c98 },
		{ "se_kira02.wav", 0x4e131637658887da },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x3848f4c79d5ff998 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_pldead00.wav", 0x3f08a461c8255687 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xdc3657c81ae4f0c3 },
		{ "se_power1.wav", 0xf1c37bfbdefac624 },
		{ "se_powerup.wav", 0x18d8eb75d98e6104 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout.wav", 0xd85cf511311ffe3a },
		{ "select00.jpg", 0xa7d76a924483c903 },
		{ "staff00.end", 0xd2100d0d4014e534 },
		{ "staff00.jpg", 0x2a7b69b94cafa2db },
		{ "staff01.anm", 0xf7d6cca7c02c4d65 },
		{ "stage1.std", 0x8c365a3f59bb0ec3 },
		{ "stage2.std", 0xd88aafb99d6d2788 },
		{ "stage3.std", 0x6af1be41179aa96e },
		{ "stage4.std", 0xc4ac81558f5a673b },
		{ "stage5.std", 0x6b08950d06d9b876 },
		{ "stage6.std", 0x2ae9b3fba57be714 },
		{ "stage7.std", 0x5d0121fc63b1884a },
		{ "stage8.std", 0xfe7fcd8afc3de46d },
		{ "std1txt.anm", 0xc4f3423e968f8908 },
		{ "std2txt.anm", 0x7aace26190fc55bb },
		{ "std3txt.anm", 0x74ae8c05faf19fe9 },
		{ "std4txt.anm", 0x1cd28e44d6598baa },
		{ "std5txt.anm", 0x42d4ee98c7fd3240 },
		{ "std6txt.anm", 0x525d6b6e8c3fbe0b },
		{ "std7txt.anm", 0x474a6df78a94e338 },
		{ "std8txt.anm", 0xe9147892885682ae },
		{ "stg1bg.anm", 0x89b5feef2088d472 },
		{ "stg1enm.anm", 0x22d95e86e8c4f83a },
		{ "stg2bg.anm", 0x34be4279e7abf78f },
		{ "stg2enm.anm", 0x5c1638e6f4c44160 },
		{ "stg3bg.anm", 0xf391e6f4fe7e7e8c },
		{ "stg3enm.anm", 0xe2a62ee8ed8fc5fc },
		{ "stg4bg.anm", 0x65a9fefd8eff4830 },
		{ "stg4bg2.anm", 0xdb11046ea5c39a87 },
		{ "stg4bg3.anm", 0xc37dc8981c62c517 },
		{ "stg4bg4.anm", 0x16fe892a1cd9fda6 },
		{ "stg4bg5.anm", 0x8aae011a9343521f },
		{ "stg4enm.anm", 0xf04a71b0c76efffa },
		{ "stg5bg.anm", 0x9cfadd386c5a09a8 },
		{ "stg5enm.anm", 0x942eae5941a95c76 },
		{ "stg6bg.anm", 0xabd832d50bdf1b84 },
		{ "stg6enm.anm", 0xe18fedfd497e0064 },
		{ "stg7bg.anm", 0xdea22690c23a53dd },
		{ "stg7enm.anm", 0x59beb0e2ff2fba8b },
		{ "stg8bg.anm", 0xdf545ac13d84d751 },
		{ "stg8enm.anm", 0x4715369c681c227c },
		{ "text.anm", 0x52c0b82849751f66 },
		{ "th07_01.mid", 0xa68abe91f183f00c },
		{ "th07_0100b.ver", 0x8315badc37a345ff },
		{ "th07_02.mid", 0xeec3fe91b4305986 },
		{ "th07_03.mid", 0x72e496652052f00c },
		{ "th07_04.mid", 0xec5a66e6f4928497 },
		{ "th07_05.mid", 0xf277884140341efa },
		{ "th07_06.mid", 0xbea51972490b3fab },
		{ "th07_07.mid", 0x750506166e5fffe0 },
		{ "th07_08.mid", 0x35ac6829e55854a1 },
		{ "th07_09.mid", 0x2fdbfa887dd7df23 },
		{ "th07_10.mid", 0xf4490da50928e0b6 },
		{ "th07_11.mid", 0x8fd148f617855778 },
		{ "th07_12.mid", 0x88fa0820e5b6d492 },
		{ "th07_13.mid", 0xc6033ac61f000559 },
		{ "th07_13b.mid", 0x3392b13df8c2394b },
		{ "th07_14.mid", 0x273cf17cd2a90513 },
		{ "th07_15.mid", 0xb9ae8ef9d62511ac },
		{ "th07_16.mid", 0x38a18df74610e6a6 },
		{ "th07_17.mid", 0xa5950767538541f5 },
		{ "th07_18.mid", 0xb9f59c863de31240 },
		{ "th07_19.mid", 0x7bf6b2a08c74d319 },
		{ "th07logo.jpg", 0x619f50a4276f376a },
		{ "thbgm.fmt", 0x413eefff15d3fcdd },
		{ "title00.jpg", 0xafa412b9894634f9 },
		{ "title01.anm", 0x5846b3f558df4ac }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th07";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th07-test.dat";

	public ArchiveTh07Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th07.dat")]
	public void ReadArchiveTh07(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.PCB, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH06.Archive>(archive);
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
	[InlineData($"{TEST_PATH}\\th07.dat", true)]
	public async Task ReadArchiveTh07Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.PCB, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH06.Archive>(archive);
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
	public void WriteArchiveTh07(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.PCB, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.PCB, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh07Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.PCB, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.PCB, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
