using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlotMachineGame.UI
{
    /// <summary>
    /// Displays a celebration modal dialog upon hitting winning combinations using original popup artwork.
    /// </summary>
    public class WinPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TextMeshProUGUI winTitleText;
        [SerializeField] private TextMeshProUGUI winAmountText;
        [SerializeField] private Text legacyWinAmountText;
        [SerializeField] private Button collectButton;

        public event Action OnPopupClosed;

        private void Awake()
        {
            if (collectButton != null)
                collectButton.onClick.AddListener(ClosePopup);

            HideImmediate();
        }

        public void Show(int winAmount, string symbolTitle)
        {
            if (popupRoot != null)
                popupRoot.SetActive(true);

            if (winTitleText != null)
                winTitleText.text = $"BIG WIN!\n{symbolTitle}";

            string amountStr = $"+{winAmount:N0} COINS";
            if (winAmountText != null)
                winAmountText.text = amountStr;
            if (legacyWinAmountText != null)
                legacyWinAmountText.text = amountStr;
        }

        public void ClosePopup()
        {
            if (popupRoot != null)
                popupRoot.SetActive(false);

            OnPopupClosed?.Invoke();
        }

        public void HideImmediate()
        {
            if (popupRoot != null)
                popupRoot.SetActive(false);
        }
    }
}
