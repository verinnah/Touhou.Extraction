using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Touhou.Extraction.Helpers;

/// <summary>
/// Provides static methods for common read operations on <see cref="ReadOnlySpan{T}"/>. This class cannot be inherited.
/// </summary>
internal static class SpanHelpers
{
	/// <summary>
	/// Reads a signed 16-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static short ReadInt16(ReadOnlySpan<byte> value) => MemoryMarshal.Read<short>(value[..sizeof(short)]);

	/// <summary>
	/// Reads a signed 16-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static short ReadInt16(ReadOnlySpan<byte> value, int offset) => MemoryMarshal.Read<short>(value.Slice(offset, sizeof(short)));

	/// <summary>
	/// Reads an unsigned 16-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ushort ReadUInt16(ReadOnlySpan<byte> value) => MemoryMarshal.Read<ushort>(value[..sizeof(ushort)]);

	/// <summary>
	/// Reads an unsigned 16-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ushort ReadUInt16(ReadOnlySpan<byte> value, int offset) => MemoryMarshal.Read<ushort>(value.Slice(offset, sizeof(ushort)));

	/// <summary>
	/// Reads an signed 32-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int ReadInt32(ReadOnlySpan<byte> value) => MemoryMarshal.Read<int>(value[..sizeof(int)]);

	/// <summary>
	/// Reads an signed 32-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int ReadInt32(ReadOnlySpan<byte> value, int offset) => MemoryMarshal.Read<int>(value.Slice(offset, sizeof(int)));

	/// <summary>
	/// Reads an unsigned 32-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadUInt32(ReadOnlySpan<byte> value) => MemoryMarshal.Read<uint>(value[..sizeof(uint)]);

	/// <summary>
	/// Reads an unsigned 32-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadUInt32(ReadOnlySpan<byte> value, int offset) => MemoryMarshal.Read<uint>(value.Slice(offset, sizeof(uint)));

	/// <summary>
	/// Reads an signed 64-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static long ReadInt64(ReadOnlySpan<byte> value) => MemoryMarshal.Read<long>(value[..sizeof(long)]);

	/// <summary>
	/// Reads an signed 64-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static long ReadInt64(ReadOnlySpan<byte> value, int offset) => MemoryMarshal.Read<long>(value.Slice(offset, sizeof(long)));

	/// <summary>
	/// Reads an unsigned 64-bit integer from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ReadUInt64(ReadOnlySpan<byte> value) => MemoryMarshal.Read<ulong>(value[..sizeof(ulong)]);

	/// <summary>
	/// Reads an unsigned 64-bit integer at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ReadUInt64(ReadOnlySpan<byte> value, int offset) => MemoryMarshal.Read<ulong>(value.Slice(offset, sizeof(ulong)));

	/// <summary>
	/// Reads a UTF-8 string from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ReadString(ReadOnlySpan<byte> value)
	{
		int indexOfNull = value.IndexOf((byte)0x0);
		return Encoding.UTF8.GetString(indexOfNull == -1 ? value : value[..indexOfNull]);
	}

	/// <summary>
	/// Reads a UTF-8 string at <paramref name="offset"/> from <paramref name="value"/>.
	/// </summary>
	/// <param name="value">The span to read from.</param>
	/// <param name="offset">The offset at which to start reading.</param>
	/// <returns>The read value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ReadString(ReadOnlySpan<byte> value, int offset)
	{
		ReadOnlySpan<byte> slice = value[offset..];
		int indexOfNull = slice.IndexOf((byte)0x0);
		return Encoding.UTF8.GetString(indexOfNull == -1 ? slice : slice[..indexOfNull]);
	}
}
