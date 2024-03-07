using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh04Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "CAR.CD2", 0x593c288f5059439e },
		{ "CONG00.PI", 0x8aeed715eafd86bf },
		{ "CONG01.PI", 0xa61ca599583d94b },
		{ "CONG02.PI", 0x6e1f8fb17cfbad3a },
		{ "CONG03.PI", 0x3d4b9589a94c77fc },
		{ "CONG04.PI", 0xdb416144bc1ab26d },
		{ "CONG10.PI", 0xe0e1aca6e5e01830 },
		{ "CONG11.PI", 0x9afbaa798b6bf1dd },
		{ "CONG12.PI", 0xe1e202b0f04f06b8 },
		{ "CONG13.PI", 0x2c484e5d0e51e7cc },
		{ "CONG14.PI", 0xe8d3a46362eca445 },
		{ "ED00.PI", 0xf68c27754dab334e },
		{ "ED01.PI", 0xc3976c92dff8ff99 },
		{ "ED02.PI", 0x1fe16baa6ac0119e },
		{ "ED03.PI", 0xd7dffcd5f83c5ae8 },
		{ "ED04.PI", 0xeecfd09af26d03f2 },
		{ "ED05.PI", 0x2c9e97fb115a63e7 },
		{ "ED06.PI", 0x5927300086e0013f },
		{ "ED07.PI", 0x77bbf1eeb9ecdfa5 },
		{ "ED08.PI", 0x8dbddd7a32243166 },
		{ "ED09.PI", 0x942c067ba1b51446 },
		{ "ED10.PI", 0xbeb8628313e082f3 },
		{ "ED11.PI", 0xa2009e17abc2295e },
		{ "ED12.PI", 0xe51639ab1409b7f9 },
		{ "ED13.PI", 0xeda7dd7177084623 },
		{ "ED14.PI", 0x8a1b83c03478547c },
		{ "ED15.PI", 0xeb495b316621d95f },
		{ "ED16.PI", 0x66ba6d2ce3eb3244 },
		{ "END1.M26", 0x706ada3984af6ed1 },
		{ "END1.M86", 0x64f8d18d77ddf983 },
		{ "END2.M26", 0xacde109142896840 },
		{ "END2.M86", 0xa92755edf5c0c34d },
		{ "GAMEFT.BFT", 0xbfb355bdeab8fc74 },
		{ "HI01.PI", 0x4bcaf5d4fd4be872 },
		{ "HI_M.BFT", 0xb02a2464494bd0ac },
		{ "LOGO.M26", 0x4e63cc3de75d01c7 },
		{ "LOGO.M86", 0x4e63cc3de75d01c7 },
		{ "MIKO.EFC", 0xeed35bf86f7580e },
		{ "MIKO.EFS", 0x194fc82145e4529b },
		{ "MS.PI", 0xb9ebd5009ebcb533 },
		{ "MSWIN.BFT", 0xeb0467b821f99e4c },
		{ "MUSIC.PI", 0x1c41de6ab37d577f },
		{ "NAME.M26", 0x33809d105a124acb },
		{ "NAME.M86", 0xfe02ab3db6eff220 },
		{ "OP.M26", 0x389661f977339831 },
		{ "OP.M86", 0x3a873869a2341544 },
		{ "OP0B.PI", 0x6910e6f0f9a6b918 },
		{ "OP1.PI", 0x52d7111ba666c707 },
		{ "OP1B.PI", 0x78c4c225b7052c61 },
		{ "OP2B.PI", 0x88da88db38216b33 },
		{ "OP3B.PI", 0xa2d3e57c3cac6426 },
		{ "OP4B.PI", 0x1b4d185c26950158 },
		{ "OP5B.PI", 0xe1fbf30de24d8f4 },
		{ "OPS.RGB", 0x21eb4a9dd59a68c5 },
		{ "SCNUM.BFT", 0xbc16effbf47a74b0 },
		{ "SCNUM2.BFT", 0x22964a76f7a39ad1 },
		{ "SFF1.CDG", 0x969680cc5bb6d4d1 },
		{ "SFF1.PI", 0x923e4f9d636693ac },
		{ "SFF1B.CDG", 0xf6db07925551ea1c },
		{ "SFF2.CDG", 0x4ff7e7fedd245fdb },
		{ "SFF2.PI", 0x56f9b350be00e3b },
		{ "SFF2B.CDG", 0x20bb305fe7355d3d },
		{ "SFF3.CDG", 0x8ca1b48812d16b79 },
		{ "SFF3B.CDG", 0x9396776e76596637 },
		{ "SFF4.CDG", 0x5446e47accff4d15 },
		{ "SFF4B.CDG", 0x2644063d0d68aacf },
		{ "SFF5.CDG", 0xf1cee694ba253875 },
		{ "SFF5B.CDG", 0xa2b65ff5c2f93eb3 },
		{ "SFF6.CDG", 0xa76122099b1eae86 },
		{ "SFF6B.CDG", 0x963a970ce9abcaaf },
		{ "SFF7.CDG", 0xb6615e3c1c47687d },
		{ "SFF7B.CDG", 0x37a3eb78dd41aaf3 },
		{ "SFF8.CDG", 0x4eef934eb54149de },
		{ "SFF8B.CDG", 0x3f713a19d860f57a },
		{ "SFF9.CDG", 0xf872bef195e4e80a },
		{ "SFF9B.CDG", 0x401f022d4006e1e8 },
		{ "SFT1.CD2", 0x9f693b21dad5d443 },
		{ "SFT2.CD2", 0x98303be60705771c },
		{ "SL.CD2", 0x9668d99d59b1b880 },
		{ "SLB1.PI", 0x8ca43b319b350d21 },
		{ "ST00.M26", 0x73e5d676461423b8 },
		{ "ST00.M86", 0xe22000133ae39e51 },
		{ "ST00B.M26", 0x6780b5dd3aa2a561 },
		{ "ST00B.M86", 0x4a78b26d9caf993c },
		{ "ST01.M26", 0xe8bca91635778e42 },
		{ "ST01.M86", 0xf65cf292f5d51160 },
		{ "ST01B.M26", 0x123ad8210e7aa48b },
		{ "ST01B.M86", 0x19e1c31362dfa4bd },
		{ "ST02.M26", 0x722369d69d9ac72 },
		{ "ST02.M86", 0x2d7ad1db518d3616 },
		{ "ST02B.M26", 0xc9b1127c9aa06199 },
		{ "ST02B.M86", 0x6f92b901f31f158e },
		{ "ST03.M26", 0xaec01c5a569996e2 },
		{ "ST03.M86", 0x670fd45b9db50976 },
		{ "ST03B.M26", 0xdc048dd6bbd58b10 },
		{ "ST03B.M86", 0x341cd561e0036f85 },
		{ "ST03C.M26", 0xe1ca710e8dd2346a },
		{ "ST03C.M86", 0x62c0c907b1195fc },
		{ "ST04.M26", 0x5c8da4984e6e59f4 },
		{ "ST04.M86", 0x8d9f866151094 },
		{ "ST04B.M26", 0xb72d742ea89bc693 },
		{ "ST04B.M86", 0x562d867c2902d382 },
		{ "ST05.M26", 0x1df030b9d233d6d3 },
		{ "ST05.M86", 0x1063a962e2f3af0d },
		{ "ST05B.M26", 0xe9d42f838c89fa1b },
		{ "ST05B.M86", 0x49cf3bd9987de6e5 },
		{ "ST06.M26", 0x8dcfff99a74db808 },
		{ "ST06.M86", 0xd946a85234d4813d },
		{ "ST06B.M26", 0xf36ab6ae327bf7e9 },
		{ "ST06B.M86", 0x954dddb164e028c8 },
		{ "ST06C.M26", 0xdd2057621e569894 },
		{ "ST06C.M86", 0x3706323f640f6105 },
		{ "ST10.M26", 0x2abeef1c492adb8e },
		{ "ST10.M86", 0xa0399ee873b85a26 },
		{ "STAFF.M26", 0xedfd374cb6c41933 },
		{ "STAFF.M86", 0x32c38c4a5d7f3f16 },
		{ "UDE.PI", 0x4db592929f3fa0c6 },
		{ "ZUN00.PI", 0x328ae2041266aa91 },
		{ "ZUN01.BFT", 0x585ecf6a221436f6 },
		{ "ZUN02.BFT", 0x2f88b44a3970cd54 },
		{ "ZUN03.BFT", 0xe32dedbeaf6ea772 },
		{ "ZUN04.BFT", 0x7ee9a6cf7a74413a },
		{ "_ED000.TXT", 0x518c10cc7af4509e },
		{ "_ED001.TXT", 0x3e4a793933cdd461 },
		{ "_ED010.TXT", 0x2dd5f38678fef4a8 },
		{ "_ED011.TXT", 0x7a51216886563a83 },
		{ "_ED100.TXT", 0xdbd3c9d1631065ed },
		{ "_ED101.TXT", 0x4ec2ca8a5b7362e5 },
		{ "_ED110.TXT", 0x159160a604dab455 },
		{ "_ED111.TXT", 0x48f02d64ae5c48f0 },
		{ "_MUSIC.TXT", 0xec05ae9f0096a25d },
		{ "_UDE.TXT", 0x8461270dc76639c7 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th04";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th04-test.dat";

	public ArchiveTh04Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\幻想郷ED.DAT")]
	public void ReadArchiveTh04(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.LLS, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\幻想郷ED.DAT", true)]
	public async Task ReadArchiveTh04Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.LLS, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh04(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.LLS, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.LLS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh04Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.LLS, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.LLS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
