using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh075Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "data\\character\\03.dat", 0x5bacfa3eeddd11ca },
		{ "data\\character\\042.dat", 0x293e78b06fc2c269 },
		{ "data\\character\\052.dat", 0xbbf5b4a222217ce },
		{ "data\\character\\062.dat", 0x3d25c5283d9dd721 },
		{ "data\\character\\072.dat", 0x1bbe7dbce7d8dc8a },
		{ "data\\character\\082.dat", 0xf8bedf39fbf41362 },
		{ "data\\character\\10.dat", 0xb116b120ba9338b0 },
		{ "data\\character\\102.dat", 0x2bc6e7ba7a33b3e8 },
		{ "data\\character\\alice\\alice.pat", 0x1d6effa813d9a234 },
		{ "data\\character\\alice\\alice.sce", 0x3c6168c33fd022d3 },
		{ "data\\character\\alice\\cardlist.dat", 0x7099f276b7691ec2 },
		{ "data\\character\\marisa\\marisa.pat", 0x11d45f3075df21a6 },
		{ "data\\character\\marisa\\marisa.sce", 0x4a32fa1ed88f1bc8 },
		{ "data\\character\\meiling\\meiling.pat", 0x84e5dab8673b114b },
		{ "data\\character\\meiling\\meiling.sce", 0x975b85d8af6bbb4 },
		{ "data\\character\\patchouli\\patchouli.pat", 0x4f6a171a9a120c59 },
		{ "data\\character\\patchouli\\patchouli.sce", 0x20526dd3635ee5b0 },
		{ "data\\character\\reimu\\reimu.pat", 0x2ea407308491ee23 },
		{ "data\\character\\reimu\\reimu.sce", 0xba4f7cfdb3af7732 },
		{ "data\\character\\remilia\\remilia.pat", 0x42316d961b3c17e8 },
		{ "data\\character\\remilia\\remilia.sce", 0x15156acdb85cefb2 },
		{ "data\\character\\sakuya\\sakuya.pat", 0x81e2ee6602f61213 },
		{ "data\\character\\sakuya\\sakuya.sce", 0x8ffd58be2c289bb9 },
		{ "data\\character\\suika\\suika.pat", 0xce4a53f2a0b5eb3b },
		{ "data\\character\\suika\\suika.sce", 0xda596b9145b34b47 },
		{ "data\\character\\youmu\\youmu.pat", 0x68e79cdf3662dc35 },
		{ "data\\character\\youmu\\youmu.sce", 0xabab1101c4ad08a4 },
		{ "data\\character\\yukari\\yukari.pat", 0x334151bc214d6f4 },
		{ "data\\character\\yukari\\yukari.sce", 0xee2ed6ec1a92a384 },
		{ "data\\character\\yuyuko\\yuyuko.pat", 0x6f91f7ed94c2716c },
		{ "data\\character\\yuyuko\\yuyuko.sce", 0x9caacb3e1e7d8a84 },
		{ "data\\system\\replay.dat", 0xba170d3f0c9dff49 },
		{ "data\\system\\select.dat", 0x92d274d2b7a64860 },
		{ "data\\system\\selectbg.dat", 0x31d925a165c37809 },
		{ "data\\system\\selectchar.dat", 0x1aca2aa3b32b554e },
		{ "musicroom.dat", 0xc1c06571c9d6a4ad },
		{ "wave\\se10.dat", 0x8688f61a796741fd }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th075";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th075b-test.dat";

	public ArchiveTh075Tests()
	{
		// Set up code page 932 (Shift-JIS)
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		Directory.CreateDirectory(ENTRIES_PATH);
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th075b.dat")]
	public void ReadArchiveTh075(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.IaMP, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH75.Archive>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th075b.dat", true)]
	public async Task ReadArchiveTh075Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.IaMP, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH75.Archive>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
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
					Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

					await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
					await entryStream.WriteAsync(entryData);
				}
			}
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public void WriteArchiveTh075(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.IaMP, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.IaMP, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
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
	public async Task WriteArchiveTh075Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.IaMP, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.IaMP, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
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
