using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlotMachineGame.UI
{
    /// <summary>
    /// Controls the bet modification UI, including decrease/increase buttons and active wager readout.
    /// </summary>
    public class BetController : MonoBehaviour
    {
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Text legacyBetText;
        [SerializeField] private TextMeshProUGUI betText;

        public event Action OnIncreaseRequested;
        public event Action OnDecreaseRequested;

        private void Awake()
        {
            if (decreaseButton != null)
                decreaseButton.onClick.AddListener(() => OnDecreaseRequested?.Invoke());
            if (increaseButton != null)
                increaseButton.onClick.AddListener(() => OnIncreaseRequested?.Invoke());
        }

        public void UpdateBetDisplay(int betValue, bool canDecrease, bool canIncrease, bool isSpinning)
        {
            string formatted = betValue.ToString("N0");

            if (legacyBetText != null)
                legacyBetText.text = formatted;
            if (betText != null)
                betText.text = formatted;

            if (decreaseButton != null)
                decreaseButton.interactable = !isSpinning && canDecrease;
            if (increaseButton != null)
                increaseButton.interactable = !isSpinning && canIncrease;
        }

        public void SetInteractable(bool interactable)
        {
            if (decreaseButton != null)
                decreaseButton.interactable = interactable;
            if (increaseButton != null)
                increaseButton.interactable = interactable;
        }
    }
}
