using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachineGame.UI
{
    /// <summary>
    /// Manages the state, visual appearance, and interactability of the main SPIN button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SpinButtonController : MonoBehaviour
    {
        [SerializeField] private Button spinButton;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite disabledSprite;

        public event Action OnSpinClicked;

        private void Awake()
        {
            if (spinButton == null)
                spinButton = GetComponent<Button>();
            if (buttonImage == null)
                buttonImage = GetComponent<Image>();

            if (spinButton != null)
            {
                spinButton.onClick.AddListener(HandleClicked);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (spinButton != null)
            {
                spinButton.interactable = interactable;
            }

            if (buttonImage != null)
            {
                if (interactable && normalSprite != null)
                    buttonImage.sprite = normalSprite;
                else if (!interactable && disabledSprite != null)
                    buttonImage.sprite = disabledSprite;
            }
        }

        public void ConfigureSprites(Sprite normal, Sprite disabled)
        {
            normalSprite = normal;
            disabledSprite = disabled;
        }

        private void HandleClicked()
        {
            OnSpinClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(HandleClicked);
            }
        }
    }
}
