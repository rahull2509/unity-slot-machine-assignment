using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotMachineGame.Core;

namespace SlotMachineGame.UI
{
    /// <summary>
    /// Master UI coordinator responsible for updating HUD elements, wallet displays,
    /// notification banners, and modal dialogs. Supports both uGUI Text and TextMeshPro.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD Readouts (uGUI Text)")]
        [SerializeField] private Text legacyBalanceText;
        [SerializeField] private Text legacyWinAmountText;
        [SerializeField] private Text legacyStatusMessageText;

        [Header("TextMeshPro Readouts (Optional)")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI winAmountText;
        [SerializeField] private TextMeshProUGUI statusMessageText;

        [Header("Interactive Sub-Controllers")]
        [SerializeField] private SpinButtonController spinButtonController;
        [SerializeField] private BetController betController;
        [SerializeField] private WinPopupController winPopupController;
        [SerializeField] private ResetDialogController resetDialogController;

        [Header("Utility Buttons")]
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Text soundToggleLabel;
        [SerializeField] private TextMeshProUGUI soundToggleText;
        [SerializeField] private Button resetGameButton;

        public SpinButtonController SpinButton => spinButtonController;
        public BetController BetCtrl => betController;
        public WinPopupController WinPopup => winPopupController;
        public ResetDialogController ResetDialog => resetDialogController;

        public event Action OnSoundToggleClicked;
        public event Action OnResetButtonClicked;

        private void Awake()
        {
            if (soundToggleButton != null)
                soundToggleButton.onClick.AddListener(() => OnSoundToggleClicked?.Invoke());
            if (resetGameButton != null)
                resetGameButton.onClick.AddListener(() => OnResetButtonClicked?.Invoke());
        }

        public void SetBalance(int balance)
        {
            string str = balance.ToString("N0");
            if (legacyBalanceText != null) legacyBalanceText.text = str;
            if (balanceText != null) balanceText.text = str;
        }

        public void SetLastWin(int amount)
        {
            string str = amount.ToString("N0");
            if (legacyWinAmountText != null) legacyWinAmountText.text = str;
            if (winAmountText != null) winAmountText.text = str;
        }

        public void SetStatusMessage(string message)
        {
            if (legacyStatusMessageText != null) legacyStatusMessageText.text = message;
            if (statusMessageText != null) statusMessageText.text = message;
        }

        public void UpdateSoundButtonUI(bool isMuted)
        {
            string text = isMuted ? "SOUND: OFF" : "SOUND: ON";
            if (soundToggleLabel != null) soundToggleLabel.text = text;
            if (soundToggleText != null) soundToggleText.text = text;
        }

        public void SetSpinInteractable(bool canSpin)
        {
            if (spinButtonController != null)
            {
                spinButtonController.SetInteractable(canSpin);
            }
        }
    }
}
