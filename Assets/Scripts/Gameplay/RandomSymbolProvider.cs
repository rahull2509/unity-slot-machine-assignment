using System;
using System.Security.Cryptography;

namespace SlotMachineGame.Gameplay
{
    /// <summary>
    /// Thread-safe and cryptographically seeded random outcome provider for slot machine spins.
    /// Provides optional fixed-seed construction for automated unit and integration tests.
    /// </summary>
    public class RandomSymbolProvider : IRandomProvider
    {
        private readonly System.Random rng;
        private readonly bool isDeterministic;

        /// <summary>
        /// Production constructor using strong entropy for independent uniform outcome generation.
        /// </summary>
        public RandomSymbolProvider()
        {
            byte[] seedBytes = new byte[4];
            using (var rngCrypto = RandomNumberGenerator.Create())
            {
                rngCrypto.GetBytes(seedBytes);
            }
            int seed = BitConverter.ToInt32(seedBytes, 0);
            rng = new System.Random(seed);
            isDeterministic = false;
        }

        /// <summary>
        /// Testing constructor allowing reproducible pseudorandom sequence.
        /// </summary>
        public RandomSymbolProvider(int fixedSeed)
        {
            rng = new System.Random(fixedSeed);
            isDeterministic = true;
        }

        public bool IsDeterministic => isDeterministic;

        public int NextSymbolIndex(int symbolCount)
        {
            if (symbolCount <= 0)
                throw new ArgumentException("Symbol count must be greater than zero.", nameof(symbolCount));

            lock (rng)
            {
                return rng.Next(0, symbolCount);
            }
        }

        public int NextRange(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new ArgumentException("minInclusive must be strictly less than maxExclusive.");

            lock (rng)
            {
                return rng.Next(minInclusive, maxExclusive);
            }
        }
    }
}
