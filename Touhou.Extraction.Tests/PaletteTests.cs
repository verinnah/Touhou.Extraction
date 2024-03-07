using System.Buffers;
using System.IO.Hashing;
using Touhou.Extraction.Tests.Utils;
using Touhou.Extraction.TH105;

namespace Touhou.Extraction.Tests;

public sealed class PaletteTests : IDisposable
{
	private const int FULL_PALETTE_SIZE = 1024;
	private const string TEST_PATH = "test-data\\th105";
	private const string OUTPUT_PATH = $"{TEST_PATH}\\palette000-test.pal";

	[Theory]
	[InlineData($"{TEST_PATH}\\palette000.pal", 0xef6c686e5e215574)]
	public void Extract(string path, ulong hash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using FileStream fileStream = new(path, FileUtils.OpenReadFileStreamOptions);
		using MemoryStream decryptedStream = new(FULL_PALETTE_SIZE);

		Palette.Extract(fileStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, FULL_PALETTE_SIZE);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(FULL_PALETTE_SIZE, decryptedStream.Length);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\palette000.pal", 0xef6c686e5e215574, true)]
	public async Task ExtractAsync(string path, ulong hash, bool writeToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using FileStream fileStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions);
		await using MemoryStream decryptedStream = new(FULL_PALETTE_SIZE);

		await Palette.ExtractAsync(fileStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, FULL_PALETTE_SIZE);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(FULL_PALETTE_SIZE, decryptedStream.Length);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data.Span));

		if (writeToDisk)
		{
			string entryPath = Path.Combine(TEST_PATH, $"{Path.GetFileNameWithoutExtension(path)}-full.pal");

			if (!File.Exists(entryPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

				await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
				await entryStream.WriteAsync(data);
			}
		}
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\palette000-full.pal", 0x4c20e6ed2e1d2718, 0xef6c686e5e215574)]
	public void Pack(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using (FileStream inputStream = new(path, FileUtils.OpenReadFileStreamOptions))
		{
			using FileStream outputStream = new(OUTPUT_PATH, FileUtils.OpenWriteFileStreamOptions);

			Palette.Pack(inputStream, outputStream);
		}

		using FileStream fileStream = new(OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Span<byte> encryptedData = buffer.AsSpan(0, (int)fileStream.Length);

		fileStream.ReadExactly(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData));

		ArrayPool<byte>.Shared.Return(buffer);

		using MemoryStream decryptedStream = new(FULL_PALETTE_SIZE);

		Palette.Extract(fileStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, FULL_PALETTE_SIZE);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(FULL_PALETTE_SIZE, decryptedStream.Length);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\palette000-full.pal", 0x4c20e6ed2e1d2718, 0xef6c686e5e215574)]
	public async Task PackAsync(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using (FileStream inputStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions))
		{
			await using FileStream outputStream = new(OUTPUT_PATH, FileUtils.AsyncOpenWriteFileStreamOptions);

			await Palette.PackAsync(inputStream, outputStream);
		}

		await using FileStream fileStream = new(OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Memory<byte> encryptedData = buffer.AsMemory(0, (int)fileStream.Length);

		await fileStream.ReadExactlyAsync(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData.Span));

		ArrayPool<byte>.Shared.Return(buffer);

		await using MemoryStream decryptedStream = new(FULL_PALETTE_SIZE);

		await Palette.ExtractAsync(fileStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, FULL_PALETTE_SIZE);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(FULL_PALETTE_SIZE, decryptedStream.Length);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data.Span));
	}

	public void Dispose() => File.Delete(OUTPUT_PATH);
}
