namespace Touhou.Extraction.Compression;

/// <summary>
/// Provides static methods for (de)compression of data as implemented by the RLE algorithm in Touhou 1-5. This class cannot be inherited.
/// </summary>
internal static class ThRLE
{
	/// <summary>
	/// Compresses <paramref name="inputData"/> using RLE (Run-Length Encoding) into the specified <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputData">The data to compress.</param>
	/// <param name="outputStream">The stream into which the compressed data will be written.</param>
	/// <returns>The size of the compressed data.</returns>
	internal static int Compress(ReadOnlySpan<byte> inputData, Stream outputStream)
	{
		ArgumentNullException.ThrowIfNull(outputStream);

		uint rl = 0;
		byte previous = 0;
		int bytesRead = 0;
		int bytesWritten = 0;

		while (bytesRead < inputData.Length)
		{
			byte current = inputData[bytesRead];

			if (bytesRead == 0)
			{
				previous = (byte)~current;
			}

			bytesRead++;

			if (rl != 0)
			{
				if (current != previous || rl == 0x100)
				{
					outputStream.Write(new byte[2] { (byte)(rl - 1), current });

					bytesWritten += 2;
					rl = 0;
				}
			}
			else
			{
				outputStream.WriteByte(current);

				bytesWritten++;
			}

			if (current == previous)
			{
				rl++;
			}

			previous = current;
		}

		if (rl != 0)
		{
			byte length = (byte)(rl - 1);
			outputStream.WriteByte(length);

			bytesWritten++;
		}

		return bytesWritten;
	}

	/// <inheritdoc cref="Compress(ReadOnlySpan{byte}, Stream)"/>
	internal static async ValueTask<int> CompressAsync(ReadOnlyMemory<byte> inputData, Stream outputStream)
	{
		ArgumentNullException.ThrowIfNull(outputStream);

		uint rl = 0;
		byte previous = 0;
		int bytesRead = 0;
		int bytesWritten = 0;

		while (bytesRead < inputData.Length)
		{
			byte current = inputData.Span[bytesRead];

			if (bytesRead == 0)
			{
				previous = (byte)~current;
			}

			bytesRead++;

			if (rl != 0)
			{
				if (current != previous || rl == 0x100)
				{
					await outputStream.WriteAsync(new byte[2] { (byte)(rl - 1), current }).ConfigureAwait(false);

					bytesWritten += 2;
					rl = 0;
				}
			}
			else
			{
				outputStream.WriteByte(current);

				bytesWritten++;
			}

			if (current == previous)
			{
				rl++;
			}

			previous = current;
		}

		if (rl != 0)
		{
			byte length = (byte)(rl - 1);
			outputStream.WriteByte(length);

			bytesWritten++;
		}

		return bytesWritten;
	}

	/// <summary>
	/// Decompresses <paramref name="inputData"/> compressed by RLE (Run-Length Encoding) into the specified <paramref name="outputStream"/>.
	/// </summary>
	/// <param name="inputData">The data to decompress.</param>
	/// <param name="outputStream">The stream into which the decompressed data will be written.</param>
	/// <returns>The size of the decompressed data.</returns>
	internal static int Decompress(ReadOnlySpan<byte> inputData, Stream outputStream)
	{
		ArgumentNullException.ThrowIfNull(outputStream);

		int bytesRead = 0;
		int bytesWritten = 0;

		if (inputData.Length < 3)
		{
			for (uint c = 0; c < inputData.Length; c++)
			{
				outputStream.WriteByte(inputData[bytesRead++]);
				bytesWritten++;
			}
		}
		else
		{
			byte previous = inputData[bytesRead++];

			outputStream.WriteByte(previous);
			bytesWritten++;

			byte currrent = inputData[bytesRead++];

			outputStream.WriteByte(currrent);
			bytesWritten++;

			while (bytesRead < inputData.Length)
			{
				if (previous == currrent)
				{
					byte count = inputData[bytesRead++];

					for (int c = 0; c < count; c++, bytesWritten++)
					{
						outputStream.WriteByte(currrent);
					}

					if (bytesRead == inputData.Length)
					{
						break;
					}
				}

				previous = currrent;
				currrent = inputData[bytesRead++];

				outputStream.WriteByte(currrent);
				bytesWritten++;
			}
		}

		return bytesWritten;
	}
}
