using System.Buffers;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh175Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "app.conf", 0x4044fc1a563d3ea7 },
		{ "lib/resource/default_cursor.png", 0xe654207844328cfb },
		{ "lib/script/boot.nut", 0x9cda35571ef69025 },
		{ "lib/script/component/asset.nut", 0x9539fceee91d1c26 },
		{ "lib/script/component/const.pnut", 0xab48fe42b5df1bee },
		{ "lib/script/component/gameobject.nut", 0xcc9fbf9913b4e8bb },
		{ "lib/script/component/resource_pool.nut", 0x1536cf16cc19ff2 },
		{ "lib/script/component/scene.nut", 0xfcb47da8faa4ff84 },
		{ "lib/script/component/tilemap.nut", 0x134f76f94a751ab1 },
		{ "lib/script/component/window_manager.nut", 0xa3490423d4b19da5 },
		{ "lib/script/component/window_manager/class_control_button.nut", 0xfb178b8def1ad5c },
		{ "lib/script/component/window_manager/class_control_combobox.nut", 0x5d0524d560433d3a },
		{ "lib/script/component/window_manager/class_control_edit.nut", 0x72fb1da6b515ffe0 },
		{ "lib/script/component/window_manager/class_control_list.nut", 0x3ab1b65f2a1f82a5 },
		{ "lib/script/component/window_manager/class_control_static.nut", 0x438b87faf3cf3d1a },
		{ "lib/script/component/window_manager/class_dialog_openfile.nut", 0x9bc42b9e20fdda47 },
		{ "lib/script/component/window_manager/class_window.nut", 0x209c994c6af994fd },
		{ "lib/script/component/window_manager/message.pnut", 0xcebdee0d061e9844 },
		{ "lib/script/component/window_manager/style.pnut", 0xbd63694475220b07 },
		{ "lib/script/component/world.nut", 0x33e22841719c004e },
		{ "lib/script/lib/action_pattern.nut", 0x7d572e18558f40a1 },
		{ "lib/script/lib/action_stack.nut", 0xc84da897e5eb9095 },
		{ "lib/shader/clear.frag.fx", 0x3f26e0e6b6cb1bc3 },
		{ "lib/shader/clear.frag.spv", 0x5b80e563458129db },
		{ "lib/shader/default.frag.fx", 0x9116ddb53732c5c4 },
		{ "lib/shader/default.frag.spv", 0x59c25ce097d56a24 },
		{ "lib/shader/font.frag.fx", 0x423fa3866ae4c8a4 },
		{ "lib/shader/font.frag.spv", 0x6677cafef100feab },
		{ "lib/shader/test.frag.fx", 0xc5289175340bf86a },
		{ "lib/shader/test.frag.spv", 0x20446a3c54efc04c },
		{ "main.pl", 0xa4295c4a6f8f3b34 },
		{ "modules/audio_xa2.avs", 0xa4aa3d7749ecf6bf },
		{ "modules/composition.avs", 0xde1ece34c293d6a1 },
		{ "modules/dynamicshape.avs", 0x631bb7bf6dfa6067 },
		{ "modules/dynamictexture.avs", 0xccf09fc11e0ec53a },
		{ "modules/fileparser_sq.avs", 0x85fb80b6d7c25ab },
		{ "modules/font_ft.avs", 0x6fdcda31ee1f60a0 },
		{ "modules/gameobject.avs", 0xccd699d126d3ecce },
		{ "modules/graphics_dx11.avs", 0x71de87716d076c2b },
		{ "modules/input.avs", 0x768340fabddc7ac8 },
		{ "modules/keybuffer.avs", 0xd4fbdb35b7442f2 },
		{ "modules/lang_cgs.avs", 0x2fbe6c1c6bc7c233 },
		{ "modules/lang_squirrel3.1.avs", 0x5dac9a495b25a640 },
		{ "modules/liquid.avs", 0xc6267b15db7d1b0c },
		{ "modules/plang.avs", 0xa19d3a7453245ab5 },
		{ "modules/random.avs", 0x2a75d16b9fa36a23 },
		{ "modules/regex.avs", 0x48a9541b55747337 },
		{ "modules/steam_api.avs", 0x44513e352afea90a },
		{ "modules/system.avs", 0x7e0e6546eec465a4 },
		{ "modules/tilemap.avs", 0xb0daabedadc34b7e },
		{ "modules/transcoder_icu.avs", 0xe7abec3057061e3a },
		{ "modules/vspace.avs", 0x884586d44334240 },
		{ "payloader.exe", 0x814ee14b8b802e8 },
		{ "unk/c0d764ba", 0x4c180056ae0a014f },
		{ "unk/e26797dd", 0x45851054562db554 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th175";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\game-test.exe";

	public ArchiveTh175Tests()
	{
		// Set up code page 932 (Shift-JIS)
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		Directory.CreateDirectory(ENTRIES_PATH);
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\game.exe")]
	public void ReadArchiveTh175(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.GI, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH175.CGX>(archive);
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
	[InlineData($"{TEST_PATH}\\game.exe", true)]
	public async Task ReadArchiveTh175Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.GI, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH175.CGX>(archive);
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
	public void WriteArchiveTh175(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.GI, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.GI, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

		using (FileStream archiveStream = new(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions))
		{
			Span<byte> magic = stackalloc byte[2];
			archiveStream.ReadExactly(magic);

			Assert.Equal("MZ"u8, magic);
		}

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
	public async Task WriteArchiveTh175Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.GI, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.GI, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

		await using (FileStream archiveStream = new(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions))
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(2);
			await archiveStream.ReadExactlyAsync(buffer.AsMemory(0, 2));

			Assert.Equal("MZ"u8, buffer.AsSpan(0, 2));

			ArrayPool<byte>.Shared.Return(buffer);
		}

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
