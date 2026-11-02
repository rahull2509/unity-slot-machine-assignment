using System;
using System.Collections.Generic;
using SlotMachineGame.Data;

namespace SlotMachineGame.Gameplay
{
    /// <summary>
    /// Calculates payout coin amounts given winning combinations and wager values.
    /// Can use a ScriptableObject PayoutConfiguration or an in-memory dictionary.
    /// </summary>
    public class PayoutCalculator
    {
        private readonly PayoutConfiguration configuration;
        private readonly Dictionary<int, int> fallbackMultipliers;

        public PayoutCalculator(PayoutConfiguration config = null)
        {
            configuration = config;
            fallbackMultipliers = new Dictionary<int, int>
            {
                { 1, 10 }, // 7: 10x
                { 2, 5 },  // Cherries: 5x
                { 3, 4 },  // Bell: 4x
                { 4, 3 }   // BAR: 3x
            };
        }

        public int GetMultiplier(int symbolId)
        {
            if (configuration != null)
            {
                int mult = configuration.GetMultiplier(symbolId);
                if (mult > 0) return mult;
            }

            if (fallbackMultipliers.TryGetValue(symbolId, out int fallback))
            {
                return fallback;
            }

            return 0;
        }

        public int CalculatePayout(int symbolId, int betAmount)
        {
            if (betAmount <= 0) return 0;
            int multiplier = GetMultiplier(symbolId);
            return betAmount * multiplier;
        }
    }
}

