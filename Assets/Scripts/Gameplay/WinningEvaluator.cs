using System;

namespace SlotMachineGame.Gameplay
{
    /// <summary>
    /// Encapsulates the result of evaluating the payline outcome across the reels.
    /// </summary>
    public readonly struct WinEvaluationResult
    {
        public bool IsWin { get; }
        public int WinningSymbolId { get; }
        public int MatchCount { get; }
        public string Description { get; }

        public WinEvaluationResult(bool isWin, int winningSymbolId, int matchCount, string description)
        {
            IsWin = isWin;
            WinningSymbolId = winningSymbolId;
            MatchCount = matchCount;
            Description = description;
        }

        public static WinEvaluationResult Loss(string reason = "No matching combination.") =>
            new WinEvaluationResult(false, -1, 0, reason);

        public static WinEvaluationResult Win(int symbolId, int count, string description) =>
            new WinEvaluationResult(true, symbolId, count, description);
    }

    /// <summary>
    /// Pure evaluator for slot machine payline rules.
    /// Evaluates whether the reels satisfy the 3-matching-symbol winning condition.
    /// </summary>
    public class WinningEvaluator
    {
        /// <summary>
        /// Evaluates the center payline outcome given the 3 symbol IDs displayed on reels 1, 2, and 3.
        /// </summary>
        public WinEvaluationResult EvaluateCenterLine(int reel1SymbolId, int reel2SymbolId, int reel3SymbolId)
        {
            // All three reels must match for the winning condition
            if (reel1SymbolId == reel2SymbolId && reel2SymbolId == reel3SymbolId)
            {
                return WinEvaluationResult.Win(
                    reel1SymbolId,
                    3,
                    $"Jackpot! 3 Matching Symbols (ID: {reel1SymbolId})"
                );
            }

            return WinEvaluationResult.Loss("Symbols did not match.");
        }

        /// <summary>
        /// Convenience overload evaluating an array of reel outcomes.
        /// </summary>
        public WinEvaluationResult Evaluate(int[] centerSymbols)
        {
            if (centerSymbols == null || centerSymbols.Length < 3)
                throw new ArgumentException("Evaluation requires at least 3 reel outcomes.", nameof(centerSymbols));

            return EvaluateCenterLine(centerSymbols[0], centerSymbols[1], centerSymbols[2]);
        }
    }
}

