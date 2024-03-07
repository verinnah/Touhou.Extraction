using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh16Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x6ad3cef8e2c4c570 },
		{ "ascii_1280.anm", 0x5809200bff6169fa },
		{ "ascii_960.anm", 0xabcaadd4f35b00e6 },
		{ "bullet.anm", 0x5727ddbbca5c3fb3 },
		{ "default.ecl", 0xc33866f5d3d1d9fc },
		{ "demo1.rpy", 0x6dc3c4cdca7f8d3c },
		{ "demo2.rpy", 0x23963ff59312f3b },
		{ "demo3.rpy", 0x1c80d119c583ab06 },
		{ "demo4.rpy", 0x360061c2b344dbb7 },
		{ "e01.anm", 0xc76eb28ce7c76d3c },
		{ "e01.msg", 0xfa3e9b279dcd9985 },
		{ "e02.anm", 0x51e7147b36e5eb20 },
		{ "e02.msg", 0x4ee55ba9836ebd86 },
		{ "e03.anm", 0x36a074d0e80ac9d },
		{ "e03.msg", 0xf6833f60ab5cd869 },
		{ "e04.anm", 0xdd649d55da9d9fcc },
		{ "e04.msg", 0xe0a777523e4d9f5b },
		{ "e05.anm", 0x8fb2607a445f9f86 },
		{ "e05.msg", 0xb4dc97ccdf2c97f0 },
		{ "e06.anm", 0xa40b13f23022e21 },
		{ "e06.msg", 0xa0e870819f02b157 },
		{ "e07.anm", 0x6785ca1d9258f4e5 },
		{ "e07.msg", 0x74d52af07bd34bea },
		{ "e08.anm", 0x7b7a9846e5e56f2d },
		{ "e08.msg", 0x7a3a49e6ab858e4d },
		{ "effect.anm", 0xc4223d261cb40a33 },
		{ "enemy.anm", 0xe54e019a836377cb },
		{ "front.anm", 0xde23f472e1a94c2d },
		{ "help.anm", 0x271115c01ce72fb9 },
		{ "help_01.png", 0xd863d62183ddf505 },
		{ "help_02.png", 0xccaf1ce15d287b4a },
		{ "help_03.png", 0x96c45c957b732b22 },
		{ "help_04.png", 0xd13dd54ac20078 },
		{ "help_05.png", 0xcc790a5ad458eec3 },
		{ "help_06.png", 0x3c36bea5d47cf456 },
		{ "help_07.png", 0x47bb1465c89079ed },
		{ "help_08.png", 0x58f54fda36e2d999 },
		{ "help_09.png", 0xff0e8ff55e96c60a },
		{ "musiccmt.txt", 0xaf7df825b38bac2a },
		{ "pl00.anm", 0x55294364b96e4e1d },
		{ "pl00.sht", 0xfc4220eaed638bab },
		{ "pl00sub.anm", 0x649dbb859aaa784a },
		{ "pl00sub.sht", 0xbdac632be32fc6da },
		{ "pl01.anm", 0xa253af2404a6ce9e },
		{ "pl01.sht", 0x708f5f9696789623 },
		{ "pl01sub.anm", 0xe513e408f6f91276 },
		{ "pl01sub.sht", 0xa54c52596cccf74f },
		{ "pl02.anm", 0x5eaed7cae0f77ec7 },
		{ "pl02.sht", 0x53c501dc7cc8bfb9 },
		{ "pl02sub.anm", 0x72fb2d436c2881d3 },
		{ "pl02sub.sht", 0x99c606f96b598d71 },
		{ "pl03.anm", 0x12495863f857e0dc },
		{ "pl03.sht", 0xa4b1685b4b69412e },
		{ "pl03sub.anm", 0x886b530c4e41f1e3 },
		{ "pl03sub.sht", 0xc5f513969f2e90f4 },
		{ "pl04sub.anm", 0x38d4598cd4566878 },
		{ "pl04sub.sht", 0xa75497a82fef5c12 },
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
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0xd7bd9f9942f7549c },
		{ "st01.ecl", 0xf3a661b6cfb4b4b8 },
		{ "st01.std", 0xd10c260d27e6489e },
		{ "st01a.msg", 0x4377adc61ff265aa },
		{ "st01b.msg", 0xd17be2fe58f6b3ed },
		{ "st01bs.ecl", 0xc8569e595c8e88eb },
		{ "st01c.msg", 0xccd4225d340f305 },
		{ "st01d.msg", 0x1cd9bd92b2da925a },
		{ "st01enm.anm", 0x89ea2029da64dfe6 },
		{ "st01logo.anm", 0x253cf831b98dc672 },
		{ "st01mbs.ecl", 0x180eb9ce535e207a },
		{ "st01wl.anm", 0xa05448262cb2f7c1 },
		{ "st02.ecl", 0x84b0c32456e1d06a },
		{ "st02.std", 0x46d3a47ba9b532d2 },
		{ "st02a.msg", 0x98845282afdd962d },
		{ "st02b.msg", 0x4518a45f2c55d920 },
		{ "st02bs.ecl", 0xe77ad82f622f1a5f },
		{ "st02c.msg", 0x59e520d59ecc542 },
		{ "st02d.msg", 0x750f25d23ac9a16e },
		{ "st02enm.anm", 0x7745761e8eddf2ab },
		{ "st02logo.anm", 0x491eb032f8c89d01 },
		{ "st02mbs.ecl", 0xc9823fe412278b8 },
		{ "st02wl.anm", 0xb1d9010078689c1d },
		{ "st03.ecl", 0xd7f21f0798ce6296 },
		{ "st03.std", 0x42a8627873d789d8 },
		{ "st03a.msg", 0x5e78fd9669083ea6 },
		{ "st03b.msg", 0x970eb94266f0e3c7 },
		{ "st03bs.ecl", 0x88644ce228869edc },
		{ "st03c.msg", 0xc095e1168d17f219 },
		{ "st03d.msg", 0xe4299e28027e2222 },
		{ "st03enm.anm", 0x5953fb1209a320d1 },
		{ "st03logo.anm", 0x18cc8115f100f0a4 },
		{ "st03mbs.ecl", 0x86a6203cad159ae6 },
		{ "st03wl.anm", 0x1cc8b21a09287b15 },
		{ "st04.ecl", 0xb65d380be7c9b3bf },
		{ "st04.std", 0x9b40793fc812ea02 },
		{ "st04a.msg", 0xd0c965fbad0ade22 },
		{ "st04b.msg", 0x59e5f56aefbf88cc },
		{ "st04bs.ecl", 0xfefe5c89cd90b8a7 },
		{ "st04c.msg", 0xd12ee268e2498c94 },
		{ "st04d.msg", 0x1eaf9e56f9e1828f },
		{ "st04enm.anm", 0x7b7b961faaf11752 },
		{ "st04logo.anm", 0x8da22e8a517ae016 },
		{ "st04mbs.ecl", 0xf5d07aeb9ecbc680 },
		{ "st04wl.anm", 0x38de3594a3154c82 },
		{ "st05.ecl", 0x4c8c8eabf6936ac7 },
		{ "st05.std", 0x84ed5df4aab2c335 },
		{ "st05a.msg", 0xead3b2990a3d7289 },
		{ "st05b.msg", 0x7e82fd44670a7ca3 },
		{ "st05bs.ecl", 0x23a7ad22088de98a },
		{ "st05c.msg", 0x7b7880046c99a08f },
		{ "st05d.msg", 0x1304704ee049e500 },
		{ "st05enm.anm", 0x953247549d20cc5b },
		{ "st05enm2.anm", 0x80ff8e698bfeec7 },
		{ "st05logo.anm", 0xb2c6c942a07d9c3a },
		{ "st05mbs.ecl", 0xb63ba3e732d02ac0 },
		{ "st05wl.anm", 0xbf8c3c2549213d73 },
		{ "st06.ecl", 0x5d652905c37c58cd },
		{ "st06.std", 0xe8545a1b7f31aa6f },
		{ "st06a.msg", 0xdaaa221946c2ff04 },
		{ "st06b.msg", 0xfe17d211d5f43ac9 },
		{ "st06bs.ecl", 0x225d7bd8d48766d1 },
		{ "st06c.msg", 0xa0bfa2b67cc8853f },
		{ "st06d.msg", 0x59c80fc554f7aefb },
		{ "st06enm.anm", 0xfb9a34b3946e96f5 },
		{ "st06logo.anm", 0x27318cc0641ab7b },
		{ "st06wl.anm", 0x7ff3bd8c98649e47 },
		{ "st07.ecl", 0x9a3e500e72e11e74 },
		{ "st07.std", 0xb641479b141470b7 },
		{ "st07a.msg", 0x3f513850f03ea000 },
		{ "st07b.msg", 0x6d18c021e6028d94 },
		{ "st07bs.ecl", 0x773593a8c8fe3ec3 },
		{ "st07c.msg", 0x528b201c50f2988f },
		{ "st07d.msg", 0xf70e5550f06124e9 },
		{ "st07enm.anm", 0x9848e2fa6ea422d6 },
		{ "st07enm2.anm", 0x1698d971c2c66f81 },
		{ "st07enm3.anm", 0x9aa4c1de166908c8 },
		{ "st07logo.anm", 0x7e838aa0eed93ae9 },
		{ "st07mbs.ecl", 0x5a74bb883d3a1f8a },
		{ "st07mbs2.ecl", 0xe973e689b17ed3a8 },
		{ "st07wl.anm", 0x1b29bd3738746ad3 },
		{ "staff.anm", 0x5eb9e5093f448d10 },
		{ "staff1.msg", 0xeb984673e814504d },
		{ "staff2.msg", 0x9e063b156e207f5f },
		{ "staff3.msg", 0x282bd66a4c6e561a },
		{ "staff4.msg", 0xe130e6ebc845e5d },
		{ "text.anm", 0x5f05a8420251630f },
		{ "th16_0100a.ver", 0xe8ea8f481573e110 },
		{ "thbgm.fmt", 0x360b4ff5416fdb40 },
		{ "title.anm", 0xfb4799131762e44 },
		{ "title_v.anm", 0xfab8731605e73a99 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th16";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th16-test.dat";

	public ArchiveTh16Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th16.dat")]
	public void ReadArchiveTh16(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.HSiFS, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th16.dat", true)]
	public async Task ReadArchiveTh16Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.HSiFS, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh16(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.HSiFS, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.HSiFS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh16Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.HSiFS, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.HSiFS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
