using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachineGame.Data
{
    /// <summary>
    /// ScriptableObject database containing the configured symbols available on the slot reels.
    /// </summary>
    [CreateAssetMenu(fileName = "SymbolDatabase", menuName = "SlotMachine/Symbol Database")]
    public class SymbolDatabase : ScriptableObject
    {
        [SerializeField] private List<SymbolData> symbols = new List<SymbolData>();

        public IReadOnlyList<SymbolData> Symbols => symbols;
        public int Count => symbols != null ? symbols.Count : 0;

        public SymbolData GetSymbolById(int id)
        {
            if (symbols == null) return null;
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i].SymbolId == id)
                    return symbols[i];
            }
            return null;
        }

        public SymbolData GetSymbolByIndex(int index)
        {
            if (symbols == null || index < 0 || index >= symbols.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} out of bounds for database with count {Count}");
            return symbols[index];
        }

        public void SetSymbols(IEnumerable<SymbolData> newSymbols)
        {
            symbols = new List<SymbolData>(newSymbols);
        }
    }
}

