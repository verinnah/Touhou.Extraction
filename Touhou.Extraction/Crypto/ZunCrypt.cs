namespace Touhou.Extraction.Crypto;

internal static class ZunCrypt
{
	internal static void Encrypt(Span<byte> data, byte key, byte step, uint block, uint limit)
	{
		uint size = (uint)data.Length;
		Span<byte> tmp = new byte[block];
		uint increment = (block >> 1) + (block & 1);

		if (size < block >> 2)
		{
			size = 0;
		}
		else
		{
			size -= (uint)((((size % block < block >> 2) ? 1 : 0) * size % block) + (size % 2));
		}

		if (limit % block != 0)
		{
			limit += block - (limit % block);
		}

		int dataPtr = 0;
		int endPtr = (int)(size < limit ? size : limit);
		while (dataPtr < endPtr)
		{
			int inPtr;
			int outPtr = 0;

			if (endPtr - dataPtr < block)
			{
				block = (uint)(endPtr - dataPtr);
				increment = (block >> 1) + (block & 1);
			}

			for (inPtr = (int)(dataPtr + block - 1); inPtr > dataPtr; outPtr++, key += step)
			{
				tmp[outPtr] = (byte)(data[inPtr--] ^ key);
				tmp[(int)(outPtr + increment)] = (byte)(data[inPtr--] ^ (key + (step * increment)));
			}

			if ((block & 1) != 0)
			{
				tmp[outPtr] = (byte)(data[inPtr] ^ key);
				key += step;
			}

			key += (byte)(step * increment);

			tmp[..(int)block].CopyTo(data.Slice(dataPtr, (int)block));
			dataPtr += (int)block;
		}
	}

	internal static void Decrypt(Span<byte> data, byte key, byte step, uint block, uint limit)
	{
		uint size = (uint)data.Length;
		Span<byte> tmp = new byte[block];
		uint increment = (block >> 1) + (block & 1);

		if (size < block >> 2)
		{
			size = 0;
		}
		else
		{
			size -= (uint)((((size % block < block >> 2) ? 1 : 0) * size % block) + (size % 2));
		}

		if (limit % block != 0)
		{
			limit += block - (limit % block);
		}

		int dataPtr = 0;
		int endPtr = (int)(size < limit ? size : limit);
		while (dataPtr < endPtr)
		{
			int outPtr;
			int inPtr = dataPtr;

			if ((endPtr - dataPtr) < block)
			{
				block = (uint)(endPtr - dataPtr);
				increment = (block >> 1) + (block & 1);
			}

			int tmpPtr = 0;
			for (outPtr = (int)(tmpPtr + block - 1); outPtr > tmpPtr; inPtr++, key += step)
			{
				tmp[outPtr--] = (byte)(data[inPtr] ^ key);
				tmp[outPtr--] = (byte)(data[(int)(inPtr + increment)] ^ (key + (step * increment)));
			}

			if ((block & 1) != 0)
			{
				tmp[outPtr] = (byte)(data[inPtr] ^ key);
				key += step;
			}

			key += (byte)(step * increment);

			tmp[..(int)block].CopyTo(data.Slice(dataPtr, (int)block));
			dataPtr += (int)block;
		}
	}
}
