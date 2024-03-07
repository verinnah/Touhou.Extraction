using System.Buffers;
using System.IO.Hashing;
using Touhou.Extraction.Tests.Utils;
using Touhou.Extraction.TH105;

namespace Touhou.Extraction.Tests;

public sealed class CV3Tests : IDisposable
{
	private const string TEST_PATH = "test-data\\th105";
	private const string OUTPUT_PATH = $"{TEST_PATH}\\049-test.cv3";

	[Theory]
	[InlineData($"{TEST_PATH}\\049.cv3", 0x293d0901d4e78bf5)]
	public void Decrypt(string path, ulong hash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using FileStream fileStream = new(path, FileUtils.OpenReadFileStreamOptions);
		using MemoryStream decryptedStream = new((int)fileStream.Length + 22);

		CV3.Extract(fileStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)fileStream.Length + 22);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length + 22, decryptedStream.Length);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\049.cv3", 0x293d0901d4e78bf5, true)]
	public async Task DecryptAsync(string path, ulong hash, bool writeToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using FileStream fileStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions);
		await using MemoryStream decryptedStream = new((int)fileStream.Length + 22);

		await CV3.ExtractAsync(fileStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)fileStream.Length + 22);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual((int)fileStream.Length + 22, decryptedStream.Length);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data.Span));

		if (writeToDisk)
		{
			string entryPath = Path.Combine(TEST_PATH, $"{Path.GetFileNameWithoutExtension(path)}.wav");

			if (!File.Exists(entryPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

				await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
				await entryStream.WriteAsync(data);
			}
		}
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\049.wav", 0x8c5d1cdf5699fba7, 0x293d0901d4e78bf5)]
	public void Encrypt(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using (FileStream inputStream = new(path, FileUtils.OpenReadFileStreamOptions))
		{
			using FileStream outputStream = new(OUTPUT_PATH, FileUtils.OpenWriteFileStreamOptions);

			CV3.Pack(inputStream, outputStream);
		}

		using FileStream fileStream = new(OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Span<byte> encryptedData = buffer.AsSpan(0, (int)fileStream.Length);

		fileStream.ReadExactly(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData));

		ArrayPool<byte>.Shared.Return(buffer);

		using MemoryStream decryptedStream = new((int)fileStream.Length + 22);

		CV3.Extract(fileStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)fileStream.Length + 22);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length + 22, decryptedStream.Length);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\049.wav", 0x8c5d1cdf5699fba7, 0x293d0901d4e78bf5)]
	public async Task EncryptAsync(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using (FileStream inputStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions))
		{
			await using FileStream outputStream = new(OUTPUT_PATH, FileUtils.AsyncOpenWriteFileStreamOptions);

			await CV3.PackAsync(inputStream, outputStream);
		}

		await using FileStream fileStream = new(OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Memory<byte> encryptedData = buffer.AsMemory(0, (int)fileStream.Length);

		await fileStream.ReadExactlyAsync(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData.Span));

		ArrayPool<byte>.Shared.Return(buffer);

		await using MemoryStream decryptedStream = new((int)fileStream.Length + 22);

		await CV3.ExtractAsync(fileStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)fileStream.Length + 22);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length + 22, decryptedStream.Length);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data.Span));
	}

	public void Dispose() => File.Delete(OUTPUT_PATH);
}
