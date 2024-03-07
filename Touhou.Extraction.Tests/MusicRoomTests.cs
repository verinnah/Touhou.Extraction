using System.Buffers;
using System.IO.Hashing;
using Touhou.Extraction.Tests.Utils;
using Touhou.Extraction.TH75;

namespace Touhou.Extraction.Tests;

public sealed class MusicRoomTests : IDisposable
{
	private const string TEST_PATH = "test-data\\th075";
	private const string OUTPUT_PATH = $"{TEST_PATH}\\musicroom-test.dat";

	[Theory]
	[InlineData($"{TEST_PATH}\\musicroom.dat", 0x4d1c8eea9a3d4c93)]
	public void Decrypt(string path, ulong hash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using FileStream fileStream = new(path, FileUtils.OpenReadFileStreamOptions);
		using MemoryStream decryptedStream = new((int)fileStream.Length);

		MusicRoom.Decrypt(fileStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)fileStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length, decryptedStream.Length);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\musicroom.dat", 0x4d1c8eea9a3d4c93, true)]
	public async Task DecryptAsync(string path, ulong hash, bool writeToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using FileStream fileStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions);
		await using MemoryStream decryptedStream = new((int)fileStream.Length);

		await MusicRoom.DecryptAsync(fileStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)fileStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length, decryptedStream.Length);
		Assert.StrictEqual(hash, XxHash3.HashToUInt64(data.Span));

		if (writeToDisk)
		{
			string entryPath = Path.Combine(TEST_PATH, $"{Path.GetFileNameWithoutExtension(path)}.txt");

			if (!File.Exists(entryPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

				await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
				await entryStream.WriteAsync(data);
			}
		}
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\musicroom.txt", 0x5ac571fcd890cfa0, 0x4d1c8eea9a3d4c93)]
	public void Encrypt(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using (FileStream inputStream = new(path, FileUtils.OpenReadFileStreamOptions))
		{
			using FileStream outputStream = new(OUTPUT_PATH, FileUtils.OpenWriteFileStreamOptions);

			MusicRoom.Encrypt(inputStream, outputStream);
		}

		using FileStream fileStream = new(OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Span<byte> encryptedData = buffer.AsSpan(0, (int)fileStream.Length);

		fileStream.ReadExactly(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData));

		ArrayPool<byte>.Shared.Return(buffer);

		using MemoryStream decryptedStream = new((int)fileStream.Length);

		MusicRoom.Decrypt(fileStream, decryptedStream);

		ReadOnlySpan<byte> data = decryptedStream.GetBuffer().AsSpan(0, (int)fileStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length, decryptedStream.Length);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\musicroom.txt", 0x5ac571fcd890cfa0, 0x4d1c8eea9a3d4c93)]
	public async Task EncryptAsync(string path, ulong encryptedHash, ulong decryptedHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using (FileStream inputStream = new(path, FileUtils.AsyncOpenReadFileStreamOptions))
		{
			await using FileStream outputStream = new(OUTPUT_PATH, FileUtils.AsyncOpenWriteFileStreamOptions);

			await MusicRoom.EncryptAsync(inputStream, outputStream);
		}

		await using FileStream fileStream = new(OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions);

		byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		Memory<byte> encryptedData = buffer.AsMemory(0, (int)fileStream.Length);

		await fileStream.ReadExactlyAsync(encryptedData);

		Assert.StrictEqual(encryptedHash, XxHash3.HashToUInt64(encryptedData.Span));

		ArrayPool<byte>.Shared.Return(buffer);

		await using MemoryStream decryptedStream = new((int)fileStream.Length);

		await MusicRoom.DecryptAsync(fileStream, decryptedStream);

		ReadOnlyMemory<byte> data = decryptedStream.GetBuffer().AsMemory(0, (int)fileStream.Length);

		Assert.False(data.IsEmpty);
		Assert.StrictEqual(fileStream.Length, decryptedStream.Length);
		Assert.StrictEqual(decryptedHash, XxHash3.HashToUInt64(data.Span));
	}

	public void Dispose() => File.Delete(OUTPUT_PATH);
}
