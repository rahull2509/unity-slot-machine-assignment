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
        [SerializeField] private Text legacyWinTitleText;
        [SerializeField] private Text legacyWinAmountText;
        [SerializeField] private TextMeshProUGUI winTitleText;
        [SerializeField] private TextMeshProUGUI winAmountText;
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

            string titleStr = $"BIG WIN!\n{symbolTitle}";
            if (legacyWinTitleText != null)
                legacyWinTitleText.text = titleStr;
            if (winTitleText != null)
                winTitleText.text = titleStr;

            string amountStr = $"+{winAmount:N0} COINS";
            if (legacyWinAmountText != null)
                legacyWinAmountText.text = amountStr;
            if (winAmountText != null)
                winAmountText.text = amountStr;
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
