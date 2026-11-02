using System;
using UnityEngine;

namespace SlotMachineGame.Data
{
    /// <summary>
    /// Represents the immutable metadata and presentation asset for a single reel symbol.
    /// </summary>
    [Serializable]
    public class SymbolData
    {
        [SerializeField] private int symbolId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private int payoutMultiplier;

        public int SymbolId => symbolId;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public int PayoutMultiplier => payoutMultiplier;

        public SymbolData() { }

        public SymbolData(int symbolId, string displayName, Sprite sprite, int payoutMultiplier)
        {
            this.symbolId = symbolId;
            this.displayName = displayName;
            this.sprite = sprite;
            this.payoutMultiplier = payoutMultiplier;
        }

        public override string ToString() => $"Symbol #{symbolId} ({displayName}) [x{payoutMultiplier}]";
    }
}
