using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachineGame.Data
{
    [Serializable]
    public struct SymbolPayoutEntry
    {
        public int symbolId;
        public int multiplier;

        public SymbolPayoutEntry(int symbolId, int multiplier)
        {
            this.symbolId = symbolId;
            this.multiplier = multiplier;
        }
    }

    /// <summary>
    /// ScriptableObject defining payout multipliers for each winning symbol combination.
    /// </summary>
    [CreateAssetMenu(fileName = "PayoutConfiguration", menuName = "SlotMachine/Payout Configuration")]
    public class PayoutConfiguration : ScriptableObject
    {
        [SerializeField]
        private List<SymbolPayoutEntry> payoutEntries = new List<SymbolPayoutEntry>
        {
            new SymbolPayoutEntry(1, 10), // Lucky 7: 10x
            new SymbolPayoutEntry(2, 5),  // Cherries: 5x
            new SymbolPayoutEntry(3, 4),  // Bell: 4x
            new SymbolPayoutEntry(4, 3)   // BAR: 3x
        };

        public IReadOnlyList<SymbolPayoutEntry> PayoutEntries => payoutEntries;

        public int GetMultiplier(int symbolId)
        {
            if (payoutEntries == null) return 0;
            for (int i = 0; i < payoutEntries.Count; i++)
            {
                if (payoutEntries[i].symbolId == symbolId)
                    return payoutEntries[i].multiplier;
            }
            return 0;
        }

        public void SetEntries(IEnumerable<SymbolPayoutEntry> entries)
        {
            payoutEntries = new List<SymbolPayoutEntry>(entries);
        }
    }
}

