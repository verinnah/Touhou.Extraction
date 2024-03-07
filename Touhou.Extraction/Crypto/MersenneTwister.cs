namespace Touhou.Extraction.Crypto;

/// <summary>
/// Pseudorandom number generator using the Mersenne Twister algorithm. This class cannot be inherited.
/// </summary>
internal sealed class MersenneTwister
{
	private int _mti;
	private readonly uint[] _mt = new uint[N];

	private static readonly uint[] s_mag01 = [0x0u, 0x9908B0DFu];

	private const int N = 624;
	private const int M = 397;
	private const uint UPPER_MASK = 0x80000000u;
	private const uint LOWER_MASK = 0x7FFFFFFFu;

	/// <summary>
	/// Initializes the PRNG with a default seed of 5489.
	/// </summary>
	internal MersenneTwister() => Initialize(5489);

	/// <summary>
	/// Initializes the PRNG with the specified seed.
	/// </summary>
	/// <param name="seed">The seed to initialize the PRNG.</param>
	internal MersenneTwister(uint seed) => Initialize(seed);

	private void Initialize(uint seed)
	{
		_mt[0] = seed;
		for (_mti = 1; _mti < N; _mti++)
		{
			_mt[_mti] = (uint)((1812433253u * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30))) + _mti);
		}
	}

	/// <summary>
	/// Generates a pseudorandom unsigned integer.
	/// </summary>
	/// <returns>The next pseudorandom unsigned integer.</returns>
	internal uint NextUInt32()
	{
		uint y;

		if (_mti >= N)
		{
			int kk = 0;

			for (; kk < N - M; kk++)
			{
				y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
				_mt[kk] = _mt[kk + M] ^ (y >> 1) ^ s_mag01[y & 0x1u];
			}

			for (; kk < N - 1; kk++)
			{
				y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
				_mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ s_mag01[y & 0x1u];
			}

			y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
			_mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ s_mag01[y & 0x1u];

			_mti = 0;
		}

		y = _mt[_mti++];

		y ^= y >> 11;
		y ^= (y << 7) & 0x9D2C5680u;
		y ^= (y << 15) & 0xEFC60000u;
		y ^= y >> 18;

		return y;
	}
}
