using NUnit.Framework;
using SlotMachineGame.Gameplay;

namespace SlotMachineGame.Tests
{
    [TestFixture]
    public class RandomSymbolProviderTests
    {
        [Test]
        public void NextSymbolIndex_AlwaysWithinValidRange()
        {
            var rng = new RandomSymbolProvider();
            int symbolCount = 4;

            for (int i = 0; i < 1000; i++)
            {
                int outcome = rng.NextSymbolIndex(symbolCount);
                Assert.GreaterOrEqual(outcome, 0);
                Assert.Less(outcome, symbolCount);
            }
        }

        [Test]
        public void NextSymbolIndex_GeneratesAllSymbolsOverDistribution()
        {
            var rng = new RandomSymbolProvider();
            int symbolCount = 4;
            int[] counts = new int[symbolCount];

            for (int i = 0; i < 2000; i++)
            {
                int outcome = rng.NextSymbolIndex(symbolCount);
                counts[outcome]++;
            }

            // In 2000 trials across 4 symbols, each should appear at least 250 times
            for (int s = 0; s < symbolCount; s++)
            {
                Assert.Greater(counts[s], 250, $"Symbol {s} appeared too infrequently: {counts[s]} times");
            }
        }

        [Test]
        public void DeterministicSeed_GeneratesIdenticalSequence()
        {
            var rng1 = new RandomSymbolProvider(12345);
            var rng2 = new RandomSymbolProvider(12345);

            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(rng1.NextSymbolIndex(4), rng2.NextSymbolIndex(4));
            }
        }

        [Test]
        public void DifferentSeeds_GenerateDifferentSequences()
        {
            var rng1 = new RandomSymbolProvider(1111);
            var rng2 = new RandomSymbolProvider(9999);

            bool foundDifference = false;
            for (int i = 0; i < 50; i++)
            {
                if (rng1.NextSymbolIndex(4) != rng2.NextSymbolIndex(4))
                {
                    foundDifference = true;
                    break;
                }
            }

            Assert.IsTrue(foundDifference, "Different seeds should generate distinct sequences.");
        }
    }
}
