using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotMachineGame.Data;
using SlotMachineGame.Gameplay;
using SlotMachineGame.Reels;
using SlotMachineGame.UI;
using SlotMachineGame.Audio;

namespace SlotMachineGame.Core
{
    /// <summary>
    /// Master coordinator orchestrating slot machine spin execution, reel synchronization,
    /// RNG outcome determination, payout evaluation, and audio/visual feedback.
    /// </summary>
    public class SlotMachineController : MonoBehaviour
    {
        [Header("Subsystems & References")]
        [SerializeField] private ReelController[] reels;
        [SerializeField] private WalletManager walletManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Configuration Assets")]
        [SerializeField] private SymbolDatabase symbolDatabase;
        [SerializeField] private PayoutConfiguration payoutConfiguration;

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Idle;

        private IRandomProvider randomProvider;
        private WinningEvaluator winningEvaluator;
        private PayoutCalculator payoutCalculator;

        private int[] targetOutcomes = new int[3];
        private int stoppedReelCount = 0;
        private bool isInitialized = false;

        public GameState CurrentState => currentState;
        public IReadOnlyList<int> LastTargetOutcomes => targetOutcomes;

        public event Action OnSpinStarted;
        public event Action<WinEvaluationResult, int> OnSpinCompleted;

        private void Awake()
        {
            InitializeDependencies();
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Initialize(new RandomSymbolProvider());
            }
        }

        public void InitializeDependencies()
        {
            if (winningEvaluator == null)
                winningEvaluator = new WinningEvaluator();
            if (payoutCalculator == null)
                payoutCalculator = new PayoutCalculator(payoutConfiguration);
            if (randomProvider == null)
                randomProvider = new RandomSymbolProvider();
        }

        public void Initialize(IRandomProvider customRng = null)
        {
            InitializeDependencies();

            if (customRng != null)
                randomProvider = customRng;

            if (reels != null)
            {
                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null)
                    {
                        reels[i].Initialize(i, symbolDatabase, 1);
                        reels[i].OnReelStopped += HandleReelStopped;
                    }
                }
            }

            if (walletManager != null)
            {
                walletManager.OnBalanceChanged += HandleBalanceChanged;
                walletManager.OnBetChanged += HandleBetChanged;
                walletManager.OnInsufficientFunds += HandleInsufficientFunds;
            }

            if (uiManager != null)
            {
                if (uiManager.SpinButton != null)
                    uiManager.SpinButton.OnSpinClicked += () => RequestSpin();

                if (uiManager.BetCtrl != null)
                {
                    uiManager.BetCtrl.OnIncreaseRequested += HandleIncreaseBet;
                    uiManager.BetCtrl.OnDecreaseRequested += HandleDecreaseBet;
                }

                uiManager.OnSoundToggleClicked += HandleSoundToggle;
                uiManager.OnResetButtonClicked += HandleResetClicked;

                if (uiManager.ResetDialog != null)
                {
                    uiManager.ResetDialog.OnConfirmReset += HandleConfirmReset;
                }

                if (walletManager != null)
                {
                    uiManager.SetBalance(walletManager.CurrentBalance);
                    uiManager.SetLastWin(0);
                    UpdateBetDisplay();
                }

                uiManager.SetStatusMessage("PRESS SPIN TO PLAY!");
                uiManager.SetSpinInteractable(true);
            }

            isInitialized = true;
        }

        public void SetRandomProvider(IRandomProvider provider)
        {
            randomProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool RequestSpin()
        {
            // Disallow concurrent spins or spins in invalid states
            if (currentState != GameState.Idle)
            {
                return false;
            }

            if (walletManager == null || symbolDatabase == null || reels == null || reels.Length < 3)
            {
                Debug.LogWarning("[SlotMachineController] Missing required references for spin execution.");
                return false;
            }

            // Validate wallet balance & deduct bet
            if (!walletManager.TryDeductBet())
            {
                currentState = GameState.InsufficientFunds;
                uiManager?.SetStatusMessage("INSUFFICIENT COINS! LOWER BET OR RESET.");
                uiManager?.ResetDialog.Show("Out of coins for this bet! Reset balance to 1,000?");
                return false;
            }

            // Generate 3 independent random outcomes
            int symbolCount = symbolDatabase.Count;
            for (int i = 0; i < 3; i++)
            {
                int randomIdx = randomProvider.NextSymbolIndex(symbolCount);
                targetOutcomes[i] = symbolDatabase.GetSymbolByIndex(randomIdx).SymbolId;
            }

            // Transition state and lock controls
            currentState = GameState.Spinning;
            stoppedReelCount = 0;

            if (uiManager != null)
            {
                uiManager.SetSpinInteractable(false);
                uiManager.BetCtrl?.SetInteractable(false);
                uiManager.SetStatusMessage("GOOD LUCK! REELS SPINNING...");
                uiManager.SetLastWin(0);
            }

            if (audioManager != null)
            {
                audioManager.PlayClick();
                audioManager.PlaySpinLoop();
            }

            OnSpinStarted?.Invoke();

            // Trigger sequential reel animations
            for (int i = 0; i < 3; i++)
            {
                reels[i].SpinTo(targetOutcomes[i]);
            }

            return true;
        }

        private void HandleReelStopped(int reelIndex, int finalSymbolId)
        {
            stoppedReelCount++;

            if (audioManager != null)
            {
                audioManager.PlayReelStop();
            }

            if (stoppedReelCount >= 3)
            {
                StartCoroutine(EvaluateOutcomeRoutine());
            }
        }

        private IEnumerator EvaluateOutcomeRoutine()
        {
            currentState = GameState.Evaluating;

            if (audioManager != null)
            {
                audioManager.StopSpinLoop();
            }

            // Brief pause to allow settle bounce to finish
            yield return new WaitForSeconds(0.25f);

            int s1 = reels[0].CurrentCenterSymbolId;
            int s2 = reels[1].CurrentCenterSymbolId;
            int s3 = reels[2].CurrentCenterSymbolId;

            WinEvaluationResult result = winningEvaluator.EvaluateCenterLine(s1, s2, s3);
            int payoutAmount = 0;

            if (result.IsWin)
            {
                currentState = GameState.Win;
                int bet = walletManager != null ? walletManager.CurrentBet : 10;
                payoutAmount = payoutCalculator.CalculatePayout(result.WinningSymbolId, bet);

                if (walletManager != null)
                {
                    walletManager.CreditWin(payoutAmount);
                }

                SymbolData winSymbol = symbolDatabase.GetSymbolById(result.WinningSymbolId);
                string symbolName = winSymbol != null ? winSymbol.DisplayName : $"Symbol #{result.WinningSymbolId}";

                if (uiManager != null)
                {
                    uiManager.SetStatusMessage($"WINNER! +{payoutAmount:N0} COINS ({symbolName})");
                    uiManager.SetLastWin(payoutAmount);

                    // Show victory popup for significant wins
                    if (payoutAmount >= (bet * 4))
                    {
                        uiManager.WinPopup?.Show(payoutAmount, symbolName);
                    }
                }

                if (audioManager != null)
                {
                    audioManager.PlayWin();
                }
            }
            else
            {
                currentState = GameState.Loss;
                if (uiManager != null)
                {
                    uiManager.SetStatusMessage("TRY AGAIN! PRESS SPIN.");
                    uiManager.SetLastWin(0);
                }
            }

            OnSpinCompleted?.Invoke(result, payoutAmount);

            // Check if player is completely out of funds
            if (walletManager != null && walletManager.CurrentBalance < walletManager.MinBet)
            {
                currentState = GameState.InsufficientFunds;
                if (uiManager != null)
                {
                    uiManager.SetStatusMessage("OUT OF COINS! RESET TO CONTINUE.");
                    uiManager.ResetDialog?.Show("You have run out of coins! Reset balance to 1,000?");
                }
            }
            else
            {
                currentState = GameState.Idle;
                if (uiManager != null)
                {
                    uiManager.SetSpinInteractable(true);
                    UpdateBetDisplay();
                }
            }
        }

        private void HandleBalanceChanged(int newBalance)
        {
            uiManager?.SetBalance(newBalance);
            UpdateBetDisplay();
        }

        private void HandleBetChanged(int newBet)
        {
            UpdateBetDisplay();
            audioManager?.PlayClick();
        }

        private void UpdateBetDisplay()
        {
            if (uiManager != null && uiManager.BetCtrl != null && walletManager != null)
            {
                bool canDecrease = walletManager.CurrentBet > walletManager.MinBet;
                bool canIncrease = walletManager.CurrentBet < walletManager.MaxBet;
                uiManager.BetCtrl.UpdateBetDisplay(walletManager.CurrentBet, canDecrease, canIncrease, currentState == GameState.Spinning);
            }
        }

        private void HandleIncreaseBet()
        {
            if (currentState == GameState.Spinning) return;
            walletManager?.IncreaseBet();
        }

        private void HandleDecreaseBet()
        {
            if (currentState == GameState.Spinning) return;
            walletManager?.DecreaseBet();
        }

        private void HandleSoundToggle()
        {
            if (audioManager != null)
            {
                audioManager.ToggleMute();
                uiManager?.UpdateSoundButtonUI(audioManager.IsMuted);
            }
        }

        private void HandleResetClicked()
        {
            if (currentState == GameState.Spinning) return;
            uiManager?.ResetDialog?.Show("Reset your coins wallet to 1,000?");
        }

        private void HandleConfirmReset()
        {
            walletManager?.ResetWallet();
            currentState = GameState.Idle;
            uiManager?.SetStatusMessage("WALLET RESET TO 1,000 COINS! GOOD LUCK.");
            uiManager?.SetSpinInteractable(true);
            UpdateBetDisplay();
        }

        private void HandleInsufficientFunds()
        {
            uiManager?.SetStatusMessage("INSUFFICIENT BALANCE FOR THIS BET!");
            uiManager?.ResetDialog?.Show("Insufficient coins! Would you like to reset to 1,000 coins?");
        }

        private void OnDestroy()
        {
            if (reels != null)
            {
                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null)
                        reels[i].OnReelStopped -= HandleReelStopped;
                }
            }
        }
    }
}
