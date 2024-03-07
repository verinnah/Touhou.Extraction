using System.Buffers;
using System.IO.Hashing;
using Touhou.Extraction.Tests.Utils;
using Touhou.Extraction.TH105;

namespace Touhou.Extraction.Tests;

public sealed class CV2Tests : IDisposable
{
	private const string TEST_PATH = "test-data\\th105";
	private const string OUTPUT_PATH = $"{TEST_PATH}\\bulletMa000-test.cv2";

	[Theory]
	[InlineData($"{TEST_PATH}\\bulletMa000.cv2", 0x7b271176cd36b005)]
	public void Extract(string path, ulong hash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using FileStream fileStream = new(path, FileUtils.OpenReadFileStreamOptions);
		using MemoryStream decryptedStream = new((int)fileStream.Length);

		CV2.Extract(fileStream, null, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)decryptedStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\bulletMa000.cv2", 0x7b271176cd36b005, true)]
	public async Task ExtractAsync(string path, ulong hash, bool writeToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using FileStream fileStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions);
		await using MemoryStream decryptedStream = new((int)fileStream.Length);

		await CV2.ExtractAsync(fileStream, null, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)decryptedStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data.Span));

		if (writeToDisk)
		{
			string entryPath = Path.Combine(TEST_PATH, $"{Path.GetFileNameWithoutExtension(path)}.png");

			if (!File.Exists(entryPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

				await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
				await entryStream.WriteAsync(data);
			}
		}
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\attackAa000.cv2", 0x4936a986c1f5a813)]
	public void Extract8bpp(string path, ulong hash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using FileStream fileStream = new(path, FileUtils.OpenReadFileStreamOptions);
		using FileStream paletteStream = new($"{TEST_PATH}\\palette000-full.pal", FileUtils.OpenReadFileStreamOptions);
		using MemoryStream decryptedStream = new((int)fileStream.Length);

		CV2.Extract(fileStream, paletteStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)decryptedStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\attackAa000.cv2", 0x4936a986c1f5a813, true)]
	public async Task Extract8bppAsync(string path, ulong hash, bool writeToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using FileStream fileStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions);
		await using FileStream paletteStream = new($"{TEST_PATH}\\palette000-full.pal", FileUtils.AsyncOpenReadFileStreamOptions);
		await using MemoryStream decryptedStream = new((int)fileStream.Length);

		await CV2.ExtractAsync(fileStream, paletteStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)decryptedStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data.Span));

		if (writeToDisk)
		{
			string entryPath = Path.Combine(TEST_PATH, $"{Path.GetFileNameWithoutExtension(path)}.png");

			if (!File.Exists(entryPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

				await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
				await entryStream.WriteAsync(data);
			}
		}
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\bulletMa000.png", 0x203b1fa2d4e0cc4f, 0x7b271176cd36b005)]
	public void Pack(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using (FileStream inputStream = new(path, FileUtils.OpenReadFileStreamOptions))
		{
			using FileStream outputStream = new(OUTPUT_PATH, FileUtils.OpenWriteFileStreamOptions);

			CV2.Pack(inputStream, outputStream);
		}

		using FileStream fileStream = new(OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Span<byte> encryptedData = buffer.AsSpan(0, (int)fileStream.Length);

		fileStream.ReadExactly(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData));

		ArrayPool<byte>.Shared.Return(buffer);

		using MemoryStream decryptedStream = new((int)fileStream.Length);

		CV2.Extract(fileStream, null, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)decryptedStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\bulletMa000.png", 0x203b1fa2d4e0cc4f, 0x7b271176cd36b005)]
	public async Task PackAsync(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using (FileStream inputStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions))
		{
			await using FileStream outputStream = new(OUTPUT_PATH, FileUtils.AsyncOpenWriteFileStreamOptions);

			await CV2.PackAsync(inputStream, outputStream);
		}

		await using FileStream fileStream = new(OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Memory<byte> encryptedData = buffer.AsMemory(0, (int)fileStream.Length);

		await fileStream.ReadExactlyAsync(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData.Span));

		ArrayPool<byte>.Shared.Return(buffer);

		await using MemoryStream decryptedStream = new((int)fileStream.Length);

		await CV2.ExtractAsync(fileStream, null, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)decryptedStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data.Span));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\attackAa000.png")]
	public void Pack8bpp(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using FileStream inputStream = new(path, FileUtils.OpenReadFileStreamOptions);

		Assert.Throws<NotSupportedException>(() => CV2.Pack(inputStream, Stream.Null));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\attackAa000.png")]
	public async Task Pack8bppAsync(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using FileStream inputStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions);

		await Assert.ThrowsAsync<NotSupportedException>(async () => await CV2.PackAsync(inputStream, Stream.Null));
	}

	public void Dispose() => File.Delete(OUTPUT_PATH);
}
