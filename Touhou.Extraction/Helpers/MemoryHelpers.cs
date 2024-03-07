using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Touhou.Extraction.Helpers;

/// <summary>
/// Provides static methods for common read operations on <see cref="ReadOnlyMemory{T}"/>. This class cannot be inherited.
/// </summary>
internal static class MemoryHelpers
{
	/// <summary>
	/// Reads a signed 16-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static short ReadInt16(ReadOnlyMemory<byte> value) => MemoryMarshal.Read<short>(value.Span[..sizeof(short)]);

	/// <summary>
	/// Reads a signed 16-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static short ReadInt16(ReadOnlyMemory<byte> value, int offset) => MemoryMarshal.Read<short>(value.Span.Slice(offset, sizeof(short)));

	/// <summary>
	/// Reads an unsigned 16-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ushort ReadUInt16(ReadOnlyMemory<byte> value) => MemoryMarshal.Read<ushort>(value.Span[..sizeof(ushort)]);

	/// <summary>
	/// Reads an unsigned 16-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ushort ReadUInt16(ReadOnlyMemory<byte> value, int offset) => MemoryMarshal.Read<ushort>(value.Span.Slice(offset, sizeof(ushort)));

	/// <summary>
	/// Reads an signed 32-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int ReadInt32(ReadOnlyMemory<byte> value) => MemoryMarshal.Read<int>(value.Span[..sizeof(int)]);

	/// <summary>
	/// Reads an signed 32-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int ReadInt32(ReadOnlyMemory<byte> value, int offset) => MemoryMarshal.Read<int>(value.Span.Slice(offset, sizeof(int)));

	/// <summary>
	/// Reads an unsigned 32-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadUInt32(ReadOnlyMemory<byte> value) => MemoryMarshal.Read<uint>(value.Span[..sizeof(uint)]);

	/// <summary>
	/// Reads an unsigned 32-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadUInt32(ReadOnlyMemory<byte> value, int offset) => MemoryMarshal.Read<uint>(value.Span.Slice(offset, sizeof(uint)));

	/// <summary>
	/// Reads an signed 64-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static long ReadInt64(ReadOnlyMemory<byte> value) => MemoryMarshal.Read<long>(value.Span[..sizeof(long)]);

	/// <summary>
	/// Reads an signed 64-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static long ReadInt64(ReadOnlyMemory<byte> value, int offset) => MemoryMarshal.Read<long>(value.Span.Slice(offset, sizeof(long)));

	/// <summary>
	/// Reads an unsigned 64-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ReadUInt64(ReadOnlyMemory<byte> value) => MemoryMarshal.Read<ulong>(value.Span[..sizeof(ulong)]);

	/// <summary>
	/// Reads an unsigned 64-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ReadUInt64(ReadOnlyMemory<byte> value, int offset) => MemoryMarshal.Read<ulong>(value.Span.Slice(offset, sizeof(ulong)));

	/// <summary>
	/// Reads a UTF-8 string from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ReadString(ReadOnlyMemory<byte> value)
	{
		int indexOfNull = value.Span.IndexOf((byte)0x0);
		return Encoding.UTF8.GetString(indexOfNull == -1 ? value.Span : value.Span[..indexOfNull]);
	}

	/// <summary>
	/// Reads a UTF-8 string at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The memory to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ReadString(ReadOnlyMemory<byte> value, int offset)
	{
		ReadOnlySpan<byte> slice = value.Span[offset..];
		int indexOfNull = slice.IndexOf((byte)0x0);
		return Encoding.UTF8.GetString(indexOfNull == -1 ? slice : slice[..indexOfNull]);
	}
}
