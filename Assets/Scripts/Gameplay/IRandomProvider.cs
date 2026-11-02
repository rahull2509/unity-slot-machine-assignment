namespace SlotMachineGame.Gameplay
{
    /// <summary>
    /// Abstraction for random number generation to allow deterministic testing and decouple gameplay logic.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// Returns an integer outcome in range [0, symbolCount - 1].
        /// </summary>
        int NextSymbolIndex(int symbolCount);

        /// <summary>
        /// Returns an integer outcome in range [minInclusive, maxExclusive - 1].
        /// </summary>
        int NextRange(int minInclusive, int maxExclusive);
    }
}
