using System;
using UnityEngine;

namespace SlotMachineGame.Gameplay
{
    /// <summary>
    /// Manages the virtual coin economy, active bet tier, validation, and balance modifications.
    /// Guarantees that wallet balance can never fall below zero.
    /// </summary>
    public class WalletManager : MonoBehaviour
    {
        [Header("Starting Balance & Limits")]
        [SerializeField] private int initialBalance = 1000;
        [SerializeField] private int defaultBet = 10;
        [SerializeField] private int minBet = 10;
        [SerializeField] private int maxBet = 500;

        [Header("Available Bet Tiers")]
        [SerializeField] private int[] betTiers = new int[] { 10, 20, 50, 100, 200, 500 };

        [Header("Runtime State")]
        [SerializeField] private int currentBalance = 1000;
        [SerializeField] private int currentBetIndex = 0;
        [SerializeField] private int lastPayout = 0;

        public int CurrentBalance => currentBalance;
        public int CurrentBet => betTiers != null && betTiers.Length > 0 ? betTiers[currentBetIndex] : defaultBet;
        public int LastPayout => lastPayout;
        public int MinBet => minBet;
        public int MaxBet => maxBet;

        public event Action<int> OnBalanceChanged;
        public event Action<int> OnBetChanged;
        public event Action<int> OnPayoutAwarded;
        public event Action OnInsufficientFunds;

        private void Awake()
        {
            Initialize(initialBalance, defaultBet);
        }

        public void Initialize(int startingBalance, int startingBet)
        {
            initialBalance = startingBalance >= 0 ? startingBalance : 1000;
            currentBalance = initialBalance;
            lastPayout = 0;

            if (betTiers == null || betTiers.Length == 0)
            {
                betTiers = new int[] { 10, 20, 50, 100, 200, 500 };
            }

            currentBetIndex = 0;
            for (int i = 0; i < betTiers.Length; i++)
            {
                if (betTiers[i] == startingBet)
                {
                    currentBetIndex = i;
                    break;
                }
            }

            OnBalanceChanged?.Invoke(currentBalance);
            OnBetChanged?.Invoke(CurrentBet);
        }

        public bool CanAffordCurrentBet()
        {
            return currentBalance >= CurrentBet && CurrentBet > 0;
        }

        public bool TryDeductBet()
        {
            int bet = CurrentBet;
            if (bet <= 0 || currentBalance < bet)
            {
                OnInsufficientFunds?.Invoke();
                return false;
            }

            currentBalance = Mathf.Max(0, currentBalance - bet);
            OnBalanceChanged?.Invoke(currentBalance);
            return true;
        }

        public void CreditWin(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Win credit cannot be negative.", nameof(amount));

            lastPayout = amount;
            long newBalance = (long)currentBalance + amount;
            if (newBalance > int.MaxValue)
            {
                currentBalance = int.MaxValue;
            }
            else
            {
                currentBalance = (int)newBalance;
            }

            OnPayoutAwarded?.Invoke(amount);
            OnBalanceChanged?.Invoke(currentBalance);
        }

        public void IncreaseBet()
        {
            if (betTiers == null || betTiers.Length == 0) return;
            if (currentBetIndex < betTiers.Length - 1)
            {
                currentBetIndex++;
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        public void DecreaseBet()
        {
            if (betTiers == null || betTiers.Length == 0) return;
            if (currentBetIndex > 0)
            {
                currentBetIndex--;
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        public void SetBetDirect(int betValue)
        {
            if (betTiers == null) return;
            for (int i = 0; i < betTiers.Length; i++)
            {
                if (betTiers[i] == betValue)
                {
                    currentBetIndex = i;
                    OnBetChanged?.Invoke(CurrentBet);
                    return;
                }
            }
        }

        public void ResetWallet()
        {
            currentBalance = initialBalance > 0 ? initialBalance : 1000;
            currentBetIndex = 0;
            lastPayout = 0;
            OnBalanceChanged?.Invoke(currentBalance);
            OnBetChanged?.Invoke(CurrentBet);
            OnPayoutAwarded?.Invoke(0);
        }
    }
}

