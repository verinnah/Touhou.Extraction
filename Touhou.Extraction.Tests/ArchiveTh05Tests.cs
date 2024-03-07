using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh05Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "BB0.BB", 0x788e3147d1b87225 },
		{ "BB0.CDG", 0xe463a3d3d2971cf2 },
		{ "BB1.BB", 0xdac0b0c237b37f7e },
		{ "BB1.CDG", 0x8d7557a0ce1c0b39 },
		{ "BB2.BB", 0xcb684f698302e84f },
		{ "BB2.CDG", 0x72a337eb23be4651 },
		{ "BB3.BB", 0x788e3147d1b87225 },
		{ "BB3.CDG", 0x481ba01d131c5b39 },
		{ "BOMB0.BFT", 0xba9b04c8b1d471e3 },
		{ "BOMB1.BFT", 0x261275bfb4ac047b },
		{ "BOMB3.BFT", 0xd4248f8018a3e5e6 },
		{ "BSS0.CD2", 0xd1c1d65e770a357a },
		{ "BSS1.CD2", 0xa6003e53d1b701b7 },
		{ "BSS2.CD2", 0x81256332b68637cd },
		{ "BSS3.CD2", 0x12d103af02088685 },
		{ "BSS4.CD2", 0xba142902ac0ea967 },
		{ "BSS5.CD2", 0xba142902ac0ea967 },
		{ "BSS6.CD2", 0xf21c678459d4b7e2 },
		{ "DEMO0.REC", 0x175c1931bbd2c6a5 },
		{ "DEMO1.REC", 0xc7a0e9347018f967 },
		{ "DEMO2.REC", 0xc1b98b2a1699dd86 },
		{ "DEMO3.REC", 0x994b3cb53ac8296f },
		{ "DEMO4.REC", 0xa448ea674abbd006 },
		{ "DEMO5.REC", 0xdebe2b03690917a },
		{ "EYE.CDG", 0x389939949b770bce },
		{ "EYE.RGB", 0x74715b19b805f6f5 },
		{ "KAO0.CD2", 0xd0e47d07523ad1a1 },
		{ "KAO1.CD2", 0x1fa9e00e6d4906e8 },
		{ "KAO2.CD2", 0x265e733c2f7ce8fd },
		{ "KAO3.CD2", 0xb4e2da7b3a3b3006 },
		{ "LS00.BB", 0xaddcbabe485d3621 },
		{ "MARI.BFT", 0x83738e75fc71329d },
		{ "MARI16.BFT", 0xe2de1b00fa789cf5 },
		{ "MIKO.EFC", 0x7cb7ea9e0ef96b5e },
		{ "MIKO.EFS", 0x194fc82145e4529b },
		{ "MIKO16.BFT", 0x8261978fafdfcbd5 },
		{ "MIKO32.BFT", 0x53491e983b06c148 },
		{ "MIKOD.BFT", 0xfd152cd36d49f9f4 },
		{ "MIMA.BFT", 0x8a40eb5e83479495 },
		{ "MIMA16.BFT", 0xd59208e2456e95a5 },
		{ "REIMU.BFT", 0xaad8cc56af5ab4d8 },
		{ "REIMU16.BFT", 0xfe2838275a9ec8c2 },
		{ "ST00.BB", 0x7370aaed984adf9c },
		{ "ST00.BB1", 0x3185eca5c00b38c3 },
		{ "ST00.BFT", 0xbda2cf51b6c3ca94 },
		{ "ST00.BMT", 0x45dc8876a694ae5f },
		{ "ST00.M", 0x2e9e8729bd807c95 },
		{ "ST00.M2", 0x958c0aa5937c17bc },
		{ "ST00.MAP", 0x2706a892c5f88206 },
		{ "ST00.MPN", 0xde6c2507d7ba6e34 },
		{ "ST00.STD", 0xc0cc79deb78ef52d },
		{ "ST00B.M", 0xf2c63f8e340a3d49 },
		{ "ST00B.M2", 0x9f6be02d65e20471 },
		{ "ST00BK.CDG", 0x8a82443f3f5863bb },
		{ "ST01.BB", 0x4d508edba216e1cc },
		{ "ST01.BB1", 0x29f99965f3f1cd0e },
		{ "ST01.BFT", 0x3a4d0f623cbbf03e },
		{ "ST01.BMT", 0xe733aeb534e0ebdf },
		{ "ST01.M", 0xaa8553061d246622 },
		{ "ST01.M2", 0x7adfeae434ec213b },
		{ "ST01.MAP", 0xe2d61d2d10bc4430 },
		{ "ST01.MPN", 0xdffda6a6b254153a },
		{ "ST01.STD", 0x57c5e216efbb342f },
		{ "ST01B.M", 0xd6b832560e5682a9 },
		{ "ST01B.M2", 0xfe363199bc127d97 },
		{ "ST01BK.CDG", 0x22cbf7e02bce13f7 },
		{ "ST02.BB", 0x69e160d4da398124 },
		{ "ST02.BB1", 0xd50d36f8e895fde },
		{ "ST02.BB2", 0x63a36413ce425ff5 },
		{ "ST02.BB3", 0x66b3bfc805ffd646 },
		{ "ST02.BFT", 0x283b56245da3e7a5 },
		{ "ST02.BMT", 0xe4bd21fccad85e80 },
		{ "ST02.M", 0xf8af337926d7c4ef },
		{ "ST02.M2", 0x37913da433ee2054 },
		{ "ST02.MAP", 0x25b65c7f8a45956b },
		{ "ST02.MPN", 0x936b1c0f15b3ec09 },
		{ "ST02.STD", 0x4f48b24fbdee5727 },
		{ "ST02B.M", 0xf891ef81b4e29831 },
		{ "ST02B.M2", 0xaee7af42ddcbab5c },
		{ "ST02BK.CDG", 0x901c9bbff339af82 },
		{ "ST03.BB", 0xa51c54d70804af1e },
		{ "ST03.BB1", 0x4949bafc2e9ad11e },
		{ "ST03.BB2", 0x7381b28390833044 },
		{ "ST03.BB3", 0x6bff98bcccb8662 },
		{ "ST03.BFT", 0x2d77e184e012b9a3 },
		{ "ST03.BMT", 0x262b18fd1005a8c6 },
		{ "ST03.M", 0x2f154ffbb158979d },
		{ "ST03.M2", 0xbc8c5bfc3197d912 },
		{ "ST03.MAP", 0x6baf57f0b161964d },
		{ "ST03.MPN", 0x7bbcd398a7513b62 },
		{ "ST03.STD", 0x4c339f6788fa260d },
		{ "ST03B.M", 0xf34a42fae28e354 },
		{ "ST03B.M2", 0x4fe4bc8182bcb9b },
		{ "ST03BK.CDG", 0xb27cf9219f00b460 },
		{ "ST03C.M", 0xa8181e1a0f4c147b },
		{ "ST03C.M2", 0x66c27d41e3b8db16 },
		{ "ST03D.M", 0x6c386f85f38869e2 },
		{ "ST03D.M2", 0x7e532f2dd326ea7f },
		{ "ST04.BB", 0x341557c1398e6950 },
		{ "ST04.BB1", 0x35483b6cff355e42 },
		{ "ST04.BB2", 0x3b1870629f436350 },
		{ "ST04.BFT", 0xa829c3f255861aa1 },
		{ "ST04.BMT", 0x97359ecb3ffa9f4a },
		{ "ST04.M", 0x726299fd714855dd },
		{ "ST04.M2", 0x456e28afa5e0ec99 },
		{ "ST04.MAP", 0x4040b938b735b161 },
		{ "ST04.MPN", 0x7f92c0db80abc74c },
		{ "ST04.STD", 0x77cf6e58c6df5d7 },
		{ "ST04B.M", 0x13dac1ccd8956179 },
		{ "ST04B.M2", 0x3b9ea652f87daf86 },
		{ "ST04BK.CDG", 0xc331d08fe6a5c174 },
		{ "ST05.BB", 0xbf25b89e05f61e8b },
		{ "ST05.BB1", 0x9aff3b03bc75d337 },
		{ "ST05.BB2", 0x1449782e162c8113 },
		{ "ST05.BB3", 0x272e504f8785fa5 },
		{ "ST05.BB4", 0x632dbada89223e2d },
		{ "ST05.M", 0x6f835a85a340c03a },
		{ "ST05.M2", 0x3c51030675b00b61 },
		{ "ST05.MAP", 0x4040b938b735b161 },
		{ "ST05.STD", 0xe49c4057e76951e },
		{ "ST05B.M", 0xa808f47bba219c74 },
		{ "ST05B.M2", 0xaac70e3766889448 },
		{ "ST05BK.CDG", 0xaafc149c77f02447 },
		{ "ST05BK2.CDG", 0xd90264e3589c5c24 },
		{ "ST06.BB1", 0x324340c821e7c649 },
		{ "ST06.BB2", 0x7d1aa9b8f5c490f9 },
		{ "ST06.BFT", 0xbd7afbfbdddc82ac },
		{ "ST06.BMT", 0x298092805771a827 },
		{ "ST06.M", 0x338909e9649cc82d },
		{ "ST06.M2", 0x6d6d589e48fd37cd },
		{ "ST06.MAP", 0x1016de073dd9bf9b },
		{ "ST06.MPN", 0x6fc371066ded682a },
		{ "ST06.STD", 0x92d21f3372fef730 },
		{ "ST06B.M", 0xd6a4a7b391d4b6a7 },
		{ "ST06B.M2", 0xade3b5f72964a04c },
		{ "ST06_16.BFT", 0x2d91e0996cac09d5 },
		{ "TXT1.BB", 0xd7712c20c68d065b },
		{ "TXT2.BB", 0xd7f7f2f77c36013f },
		{ "YUKA.BFT", 0xef04d1e55074cd7 },
		{ "YUKA16.BFT", 0xe2c6e3740639813c },
		{ "_DM00.TX2", 0x83d18bfea5be1416 },
		{ "_DM01.TX2", 0x5020e75ed775c6c6 },
		{ "_DM02.TX2", 0x3617cafe31cc8921 },
		{ "_DM03.TX2", 0x82388faaf8fb3883 },
		{ "_DM04.TX2", 0x5e777b41a0a81083 },
		{ "_DM05.TX2", 0x991ab521f1dee746 },
		{ "_DM06.TX2", 0xb64a2bf38b645420 },
		{ "_DM08.TX2", 0x90fb3e40193ac2a1 },
		{ "_DM09.TX2", 0xcfbfd5c465ab71d }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th05";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\怪綺談2-TEST.DAT";

	public ArchiveTh05Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\怪綺談2.DAT")]
	public void ReadArchiveTh05(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.MS, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH01.Archive>(archive);
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
			Assert.True(entryData.Length >= entry.Size);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\怪綺談2.DAT", true)]
	public async Task ReadArchiveTh05Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.MS, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH01.Archive>(archive);
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
			Assert.True(entryData.Length >= entry.Size);
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
	public void WriteArchiveTh05(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.MS, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.MS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh05Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.MS, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.MS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
