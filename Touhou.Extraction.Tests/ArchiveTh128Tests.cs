using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh128Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x1c2d92c6f5085be0 },
		{ "boss00.anm", 0xe16f33ae9f0ca4f3  },
		{ "boss01.anm", 0x2da20e6edad68070 },
		{ "boss02.anm", 0x7b4214bf1b696a77 },
		{ "boss03.anm", 0xf7f8ec7db4d0af0 },
		{ "boss04.anm", 0x34297e89f6c9cc3f },
		{ "bullet.anm", 0xae8aa1b1572402de },
		{ "card.anm", 0x8eb22824d31c1fec },
		{ "default.ecl", 0x711245c74072a394 },
		{ "demo0.rpy", 0x68eff0bd8d4cc75e },
		{ "demo1.rpy", 0xee7c2b0cf1f66490 },
		{ "demo2.rpy", 0x2e42956f28421f8f },
		{ "e00.anm", 0xf452f8043bd7cfae },
		{ "e00.msg", 0xa7b4bdcba7a7df8e },
		{ "e01.anm", 0x72bf0654c6e789e5 },
		{ "e01.msg", 0x1771ad049e107974 },
		{ "e02.anm", 0x57380742ebddc2aa },
		{ "e02.msg", 0xfbee363eb0dc2174 },
		{ "e03.anm", 0xf8bc7d68e483dac7 },
		{ "e03.msg", 0x157e37cd144c6ddd },
		{ "e04.anm", 0x3baf584c8ee13bd3 },
		{ "e04.msg", 0x3f88606866ecb42 },
		{ "e05.anm", 0x9ba72b2c10f10598 },
		{ "e05.msg", 0xcb3446c21552d4d },
		{ "ebase.anm", 0xa065f799d73d1908 },
		{ "enemy.anm", 0x3fa61a08fde90575 },
		{ "enemy_bf.anm", 0xa676107ca3f77e3b },
		{ "enemy_ll.anm", 0x9a006140e2bb7309 },
		{ "enemy_ll2.anm", 0x64950f9dcddf060a },
		{ "front.anm", 0x60985e5a08ceb5dc },
		{ "help.anm", 0xcb225ee8df5492f0 },
		{ "help_00.png", 0x57b2676fd8d29570 },
		{ "help_01.png", 0xaf99f563d2a63f39 },
		{ "help_02.png", 0x7ccd7a84cb17537a },
		{ "help_03.png", 0x72d44c1e5668b5ec },
		{ "help_04.png", 0xc6c5dadb16399fad },
		{ "help_05.png", 0x868e42976dd4b96d },
		{ "help_06.png", 0x1087ae78a7b08e81 },
		{ "help_07.png", 0x5babf65c0e1d8f50 },
		{ "help_08.png", 0x969eec0bb01bdcc7 },
		{ "help_09.png", 0x70a03a3f368f3cdb },
		{ "ice.anm", 0xde497eee23ad6d91 },
		{ "logo_a1_1.anm", 0xe9bd29936a27bf94 },
		{ "logo_a1_2.anm", 0xfd5f46e5739424c7 },
		{ "logo_a1_3.anm", 0x2ecdc5f131a37036 },
		{ "logo_a2_2.anm", 0x6788ffb78364e25f },
		{ "logo_a2_3.anm", 0x456086ed68d484c },
		{ "logo_b1_1.anm", 0x8862dc3980128373 },
		{ "logo_b1_2.anm", 0xb6193df15ffa56d9 },
		{ "logo_b1_3.anm", 0x3f3c70561e843aee },
		{ "logo_b2_2.anm", 0x71d439680d8f246b },
		{ "logo_b2_3.anm", 0xb35da47ffe16f005 },
		{ "logo_c1_1.anm", 0x4451e225e1bac19e },
		{ "logo_c1_2.anm", 0xf4eeb6fb2e5cde24 },
		{ "logo_c1_3.anm", 0xb778339a4f3beb74 },
		{ "logo_c2_2.anm", 0xc0bcbf7bbf04180e },
		{ "logo_c2_3.anm", 0xd2384fc5dc359899 },
		{ "logo_ex.anm", 0x1e2125833004579 },
		{ "musiccmt.txt", 0x6e4903eafc23d8f5 },
		{ "pl00.anm", 0xb6ef16bf7580af9 },
		{ "pl00a.sht", 0x51dd3c159e2867c8 },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_cat01.wav", 0xdafdbe921b8a187a },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_ch02.wav", 0x4514aa1d8424c7e2 },
		{ "se_ch03.wav", 0x336f51977bc49eca },
		{ "se_changeitem.wav", 0x6aec872d94ad63df },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_don00.wav", 0x63c0b2aeacd1e2a8 },
		{ "se_down.wav", 0x8d312c1253752bfd },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_enep02.wav", 0xccecfaf28e083794 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_extend2.wav", 0x8c587999087b4057 },
		{ "se_focus.wav", 0xf8014b382298e0f0 },
		{ "se_focusfix.wav", 0x52d7d93ec56cc756 },
		{ "se_focusfix2.wav", 0xf384b5ef0ce1e66 },
		{ "se_focusin.wav", 0xb12cc628c631ab01 },
		{ "se_focusrot.wav", 0x2fee41333e4f931a },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_ice.wav", 0x49bb0a94600096ea },
		{ "se_ice2.wav", 0x8c932c0b48317624 },
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
		{ "se_nice.wav", 0xa6ab57f4d6404778 },
		{ "se_nodamage.wav", 0xc186b0bb70f73fb8 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_option.wav", 0xf77e827eef8f3242 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_pin00.wav", 0xba49fda931e39a6 },
		{ "se_pin01.wav", 0x6c9fa1b12d5f1b45 },
		{ "se_piyo.wav", 0x78729a640997df96 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xa8311a42586a4915 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0xb66702f933cb7b14 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_shutter.wav", 0x18275555311e4ea0 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_ufo.wav", 0xae45302d06837a38 },
		{ "se_ufoalert.wav", 0x61a1c8423f756073 },
		{ "sig.anm", 0x3ba83faa40488260 },
		{ "st_a1.msg", 0x617b0185417b5809 },
		{ "st_a1_1.ecl", 0x3e27ac4a26736a3c },
		{ "st_a1_1b.ecl", 0xdb88eb1ccbd9cefe },
		{ "st_a1_2.ecl", 0x9f31662960c8d963 },
		{ "st_a1_2.msg", 0xfa297c2b53a28538 },
		{ "st_a1_2b.ecl", 0x3fcca5819b7dc62a },
		{ "st_a1_3.ecl", 0xa4d44dbc42c5efa2 },
		{ "st_a1_3.msg", 0xa51609ab3b44e45f },
		{ "st_a1_3b.ecl", 0xf289168ea79a08cc },
		{ "st_a2_2.ecl", 0x325c1512da0d73b2 },
		{ "st_a2_2.msg", 0x288f4906a0b5e6e9 },
		{ "st_a2_2b.ecl", 0x70a3bd5cc58c38c0 },
		{ "st_a2_3.ecl", 0xb6161d9265ab69aa },
		{ "st_a2_3.msg", 0xc6149ac41d9d6615 },
		{ "st_a2_3b.ecl", 0x6e23805821c9440f },
		{ "st_b1.msg", 0x299f56b29ed65245 },
		{ "st_b1_1.ecl", 0xd32823e29f37545b },
		{ "st_b1_1b.ecl", 0xf308c563f3c293b6 },
		{ "st_b1_2.ecl", 0x60fc2b9609e398bd },
		{ "st_b1_2.msg", 0x11153cb1938b0a38 },
		{ "st_b1_2b.ecl", 0x3f3a6305250f0681 },
		{ "st_b1_3.ecl", 0x915e1a6bebce9f2c },
		{ "st_b1_3.msg", 0x450292a5c9eea808 },
		{ "st_b1_3b.ecl", 0x8c2be386370169cb },
		{ "st_b2_2.ecl", 0xf8fd29a9a1a8a91 },
		{ "st_b2_2.msg", 0x4c377a5705c9a320 },
		{ "st_b2_2b.ecl", 0x21f430f3ae5c3e9a },
		{ "st_b2_3.ecl", 0xf82021d8871f94e3 },
		{ "st_b2_3.msg", 0x1c6a75f778e57b31 },
		{ "st_b2_3b.ecl", 0x57b4eabec4816cf4 },
		{ "st_c1.msg", 0x6d764ac3a775bebc },
		{ "st_c1_1.ecl", 0x81c1cf2aa3418ad8 },
		{ "st_c1_1b.ecl", 0xb3e630ccd0aa08b4 },
		{ "st_c1_2.ecl", 0x72d6546ad87367a1 },
		{ "st_c1_2.msg", 0x344a59c657b38d3d },
		{ "st_c1_2b.ecl", 0xab0c3aee6f94a68e },
		{ "st_c1_3.ecl", 0x7d1ac36220fc62a4 },
		{ "st_c1_3.msg", 0x32301b7bf2b08370 },
		{ "st_c1_3b.ecl", 0x62a63c57ba984ddf },
		{ "st_c2_2.ecl", 0x352c88f62db43aae },
		{ "st_c2_2.msg", 0x4a5edc320375efa7 },
		{ "st_c2_2b.ecl", 0xce5392154f6c9f02 },
		{ "st_c2_3.ecl", 0xbb5d2093204239c7 },
		{ "st_c2_3.msg", 0xe31a7c56ce172199 },
		{ "st_c2_3b.ecl", 0xe25403508cf5e7f5 },
		{ "st_ex.ecl", 0xe6347eba777d0e27 },
		{ "st_ex.msg", 0x260319f4b5fe8bc3 },
		{ "st_exb.ecl", 0x4c95094f7b71d33d },
		{ "stage_a1.anm", 0xe0650dc4051010bb },
		{ "stage_a1.std", 0x91a931e29f7f11d4 },
		{ "stage_a2.anm", 0x6006fb513879d352 },
		{ "stage_a2.std", 0xed443d4d34123153 },
		{ "stage_a3.anm", 0xbd5a3e26ff2cce99 },
		{ "stage_a3.std", 0x41113864d36f5916 },
		{ "stage_b1.anm", 0x388d112b052889ea },
		{ "stage_b1.std", 0x4d4a5171bf541182 },
		{ "stage_b2.anm", 0xa911e230e091c4cb },
		{ "stage_b2.std", 0xd3be085c4a697e69 },
		{ "stage_b3.anm", 0x4e31315bf3d754f1 },
		{ "stage_b3.std", 0x64bf607e790a7eb },
		{ "stage_c1.anm", 0x1b4a47004ac7d1f4 },
		{ "stage_c1.std", 0xfa5c08beacf8dbf5 },
		{ "stage_c2.anm", 0x5e2763563e74ee33 },
		{ "stage_c2.std", 0xb242a4b42352e5b9 },
		{ "stage_c3.anm", 0xefe4a3ee27215d28 },
		{ "stage_c3.std", 0xf865842601c97933 },
		{ "stage_ex.anm", 0xabf9c1c60d4a0361 },
		{ "stage_ex.std", 0x33ff2bd79ebe8361 },
		{ "text.anm", 0x7b62e8589fd7fef },
		{ "th128_0100a.ver", 0xc180177ba4e9bc8 },
		{ "thbgm.fmt", 0xd2e1e1a3c5ce95e8 },
		{ "title.anm", 0x70ace13e7b52cda6 },
		{ "title_v.anm", 0xaf8ea0595ecd5ce6 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th128";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th128-test.dat";

	public ArchiveTh128Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th128.dat")]
	public void ReadArchiveTh128(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.GFW, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th128.dat", true)]
	public async Task ReadArchiveTh128Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.GFW, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh128(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.GFW, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.GFW, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh128Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.GFW, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.GFW, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
