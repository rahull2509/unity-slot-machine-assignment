using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlotMachineGame.UI
{
    /// <summary>
    /// Displays a confirmation dialog for resetting wallet balance to initial state.
    /// Utilizes original popup artwork and sliced Yes/No button sprites.
    /// </summary>
    public class ResetDialogController : MonoBehaviour
    {
        [SerializeField] private GameObject dialogRoot;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Text legacyMessageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        public event Action OnConfirmReset;
        public event Action OnCancelReset;

        private void Awake()
        {
            if (yesButton != null)
                yesButton.onClick.AddListener(HandleYes);
            if (noButton != null)
                noButton.onClick.AddListener(HandleNo);

            HideImmediate();
        }

        public void Show(string promptMessage = "Reset coins balance to 1,000?")
        {
            if (dialogRoot != null)
                dialogRoot.SetActive(true);

            if (messageText != null)
                messageText.text = promptMessage;
            if (legacyMessageText != null)
                legacyMessageText.text = promptMessage;
        }

        private void HandleYes()
        {
            HideImmediate();
            OnConfirmReset?.Invoke();
        }

        private void HandleNo()
        {
            HideImmediate();
            OnCancelReset?.Invoke();
        }

        public void HideImmediate()
        {
            if (dialogRoot != null)
                dialogRoot.SetActive(false);
        }
    }
}
