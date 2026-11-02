namespace SlotMachineGame.Core
{
    /// <summary>
    /// Represents the high-level operational state of the slot machine.
    /// </summary>
    public enum GameState
    {
        Idle,
        Spinning,
        Evaluating,
        Win,
        Loss,
        InsufficientFunds
    }
}
