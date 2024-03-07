using System.Runtime.InteropServices;
using Touhou.Extraction.Crypto;
using Touhou.Extraction.Helpers;

namespace Touhou.Extraction.TH135;

internal abstract partial class EntryList
{
	/// <inheritdoc/>
	private sealed class EntryList0 : EntryList
	{
		/// <inheritdoc/>
		internal EntryList0(List<EntryTh135> entries) : base(entries) { }

		/// <inheritdoc/>
		internal EntryList0(List<EntryTh135> entries, ArchiveReadOptions options, string[]? extensionFilters) : base(entries, options, extensionFilters) { }

		/// <inheritdoc/>
		internal override void Read(RsaReader rsaReader, uint entryCount, FnList fileNames)
		{
			for (uint entryIndex = 0; entryIndex < entryCount; entryIndex++)
			{
				ReadOnlySpan<byte> entryData = rsaReader.Read(sizeof(uint) * 2);

				uint hash = rsaReader.ReadUInt32();
				string fileName = fileNames.GetFileNameFromHash(hash, out bool isUnknown);

				byte[] keyData = rsaReader.Read(sizeof(uint) * 4);

				if ((Options.HasFlag(ArchiveReadOptions.ExcludeUnknownEntries) && isUnknown) || (ExtensionFilters?.Contains(Path.GetExtension(fileName)) is false))
				{
					continue;
				}

				uint[] key = new uint[4];
				Buffer.BlockCopy(keyData, 0, key, 0, keyData.Length);

				Entries.Add(new EntryTh135(size: SpanHelpers.ReadInt32(entryData), offset: SpanHelpers.ReadInt32(entryData, sizeof(uint)), hash, key, fileName));
			}
		}

		/// <inheritdoc/>
		internal override async ValueTask ReadAsync(RsaReader rsaReader, uint entryCount, FnList fileNames)
		{
			for (uint entryIndex = 0; entryIndex < entryCount; entryIndex++)
			{
				ReadOnlyMemory<byte> entryData = await rsaReader.ReadAsync(sizeof(uint) * 2).ConfigureAwait(false);

				uint hash = await rsaReader.ReadUInt32Async().ConfigureAwait(false);
				string fileName = fileNames.GetFileNameFromHash(hash, out bool isUnknown);

				byte[] keyData = await rsaReader.ReadAsync(sizeof(uint) * 4).ConfigureAwait(false);

				if ((Options.HasFlag(ArchiveReadOptions.ExcludeUnknownEntries) && isUnknown) || (ExtensionFilters?.Contains(Path.GetExtension(fileName)) is false))
				{
					continue;
				}

				uint[] key = new uint[4];
				Buffer.BlockCopy(keyData, 0, key, 0, keyData.Length);

				Entries.Add(new EntryTh135(size: MemoryHelpers.ReadInt32(entryData), offset: MemoryHelpers.ReadInt32(entryData, sizeof(uint)), hash, key, fileName));
			}
		}

		/// <inheritdoc/>
		internal override void Write(RsaWriter rsaWriter)
		{
			rsaWriter.WriteUInt32((uint)Entries.Count);

			byte[] buffer = new byte[sizeof(uint) * 4];

			foreach (EntryTh135 entry in Entries)
			{
				MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), (uint)entry.Size);
				MemoryMarshal.Write(buffer.AsSpan(sizeof(uint), sizeof(uint)), entry.Offset);

				rsaWriter.Write(buffer, sizeof(uint) * 2);

				rsaWriter.WriteUInt32(entry.FileNameHash);

				MemoryMarshal.AsBytes<uint>(entry.Key).CopyTo(buffer.AsSpan(0, sizeof(uint) * 4));
				rsaWriter.Write(buffer);
			}
		}

		/// <inheritdoc/>
		internal override async ValueTask WriteAsync(RsaWriter rsaWriter)
		{
			await rsaWriter.WriteUInt32Async((uint)Entries.Count).ConfigureAwait(false);

			byte[] buffer = new byte[sizeof(uint) * 4];

			foreach (EntryTh135 entry in Entries)
			{
				MemoryMarshal.Write(buffer.AsSpan(0, sizeof(uint)), (uint)entry.Size);
				MemoryMarshal.Write(buffer.AsSpan(sizeof(uint), sizeof(uint)), entry.Offset);

				await rsaWriter.WriteAsync(buffer, sizeof(uint) * 2).ConfigureAwait(false);

				await rsaWriter.WriteUInt32Async(entry.FileNameHash).ConfigureAwait(false);

				MemoryMarshal.AsBytes<uint>(entry.Key).CopyTo(buffer);
				await rsaWriter.WriteAsync(buffer).ConfigureAwait(false);
			}
		}
	}
}
