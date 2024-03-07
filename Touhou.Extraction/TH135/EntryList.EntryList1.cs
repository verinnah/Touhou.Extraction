using System.Runtime.InteropServices;
using Touhou.Extraction.Crypto;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH135;

internal abstract partial class EntryList
{
	/// <inheritdoc/>
	private sealed class EntryList1 : EntryList
	{
		/// <inheritdoc/>
		internal EntryList1(List<EntryTh135> entries) : base(entries) { }

		/// <inheritdoc/>
		internal EntryList1(List<EntryTh135> entries, ArchiveReadOptions options, string[]? extensionFilters) : base(entries, options, extensionFilters) { }

		/// <inheritdoc/>
		internal override void Read(RsaReader rsaReader, uint entryCount, FnList fileNames)
		{
			for (uint entryIndex = 0; entryIndex < entryCount; entryIndex++)
			{
				ReadOnlySpan<byte> entryData = rsaReader.Read(sizeof(uint) * 2);

				byte[] hashData = rsaReader.Read(sizeof(uint) * 2);
				// hash[1] seems unused
				uint[] hash = new uint[2];
				Buffer.BlockCopy(hashData, 0, hash, 0, hashData.Length);

				byte[] keyData = rsaReader.Read(sizeof(uint) * 4);
				uint[] key = new uint[4];
				Buffer.BlockCopy(keyData, 0, key, 0, keyData.Length);

				uint fileNameHash = hash[0] ^ key[2];

				string fileName = fileNames.GetFileNameFromHash(fileNameHash, out bool isUnknown);

				if ((Options.HasFlag(ArchiveReadOptions.ExcludeUnknownEntries) && isUnknown) || (ExtensionFilters?.Contains(Path.GetExtension(fileName)) is false))
				{
					continue;
				}

				uint size = SpanHelpers.ReadUInt32(entryData) ^ key[0];
				uint offset = SpanHelpers.ReadUInt32(entryData, sizeof(uint)) ^ key[1];

				for (int c = 0; c < 4; c++)
				{
					key[c] = (uint)-(int)key[c];
				}

				Entries.Add(new EntryTh135((int)size, (int)offset, fileNameHash, key, fileName));
			}
		}

		/// <inheritdoc/>
		internal override async ValueTask ReadAsync(RsaReader rsaReader, uint entryCount, FnList fileNames)
		{
			for (uint entryIndex = 0; entryIndex < entryCount; entryIndex++)
			{
				ReadOnlyMemory<byte> entryData = await rsaReader.ReadAsync(sizeof(uint) * 2).ConfigureAwait(false);

				byte[] hashData = await rsaReader.ReadAsync(sizeof(uint) * 2).ConfigureAwait(false);
				// hash[1] seems unused
				uint[] hash = new uint[2];
				Buffer.BlockCopy(hashData, 0, hash, 0, hashData.Length);

				byte[] keyData = await rsaReader.ReadAsync(sizeof(uint) * 4).ConfigureAwait(false);
				uint[] key = new uint[4];
				Buffer.BlockCopy(keyData, 0, key, 0, keyData.Length);

				uint fileNameHash = hash[0] ^ key[2];

				string fileName = fileNames.GetFileNameFromHash(fileNameHash, out bool isUnknown);

				if ((Options.HasFlag(ArchiveReadOptions.ExcludeUnknownEntries) && isUnknown) || (ExtensionFilters?.Contains(Path.GetExtension(fileName)) is false))
				{
					continue;
				}

				uint size = MemoryHelpers.ReadUInt32(entryData) ^ key[0];
				uint offset = MemoryHelpers.ReadUInt32(entryData, sizeof(uint)) ^ key[1];

				for (int c = 0; c < 4; c++)
				{
					key[c] = (uint)-(int)key[c];
				}

				Entries.Add(new EntryTh135((int)size, (int)offset, fileNameHash, key, fileName));
			}
		}

		/// <inheritdoc/>
		internal override void Write(RsaWriter rsaWriter)
		{
			rsaWriter.WriteUInt32((uint)Entries.Count);

			byte[] buffer = new byte[sizeof(uint) * 2];

			byte[] keyBuffer = new byte[sizeof(uint) * 4];
			Span<uint> key = MemoryMarshal.Cast<byte, uint>(keyBuffer);

			byte[] hashBuffer = new byte[sizeof(uint) * 2];
			Span<uint> hash = MemoryMarshal.Cast<byte, uint>(hashBuffer);
			// Unused
			hash[1] = 0;

			foreach (EntryTh135 entry in Entries)
			{
				entry.Key.CopyTo(key);

				for (int c = 0; c < 4; c++)
				{
					key[c] = (uint)-(int)key[c];
				}

				MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), (uint)entry.Size ^ key[0]);
				MemoryMarshal.Write(buffer.AsSpan(sizeof(uint), sizeof(uint)), (uint)(entry.Offset ^ key[1]));

				hash[0] = entry.FileNameHash ^ key[2];

				rsaWriter.Write(buffer);
				rsaWriter.Write(hashBuffer);
				rsaWriter.Write(keyBuffer);
			}
		}

		/// <inheritdoc/>
		internal override async ValueTask WriteAsync(RsaWriter rsaWriter)
		{
			await rsaWriter.WriteUInt32Async((uint)Entries.Count).ConfigureAwait(false);

			byte[] buffer = new byte[sizeof(uint) * 2];

			byte[] keyBuffer = new byte[sizeof(uint) * 4];
			uint[] key = new uint[4];

			byte[] hashBuffer = new byte[sizeof(uint) * 2];
			uint[] hash = new uint[2];
			// Unused
			hash[1] = 0;

			foreach (EntryTh135 entry in Entries)
			{
				entry.Key.CopyTo(key.AsSpan());

				for (int c = 0; c < 4; c++)
				{
					key[c] = (uint)-(int)key[c];
				}

				MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), (uint)entry.Size ^ key[0]);
				MemoryMarshal.Write(buffer.AsSpan(sizeof(uint), sizeof(uint)), (uint)(entry.Offset ^ key[1]));

				hash[0] = entry.FileNameHash ^ key[2];

				await rsaWriter.WriteAsync(buffer).ConfigureAwait(false);

				MemoryMarshal.AsBytes<uint>(hash).CopyTo(hashBuffer);
				await rsaWriter.WriteAsync(hashBuffer).ConfigureAwait(false);

				MemoryMarshal.AsBytes<uint>(key).CopyTo(keyBuffer);
				await rsaWriter.WriteAsync(keyBuffer).ConfigureAwait(false);
			}
		}
	}
}
