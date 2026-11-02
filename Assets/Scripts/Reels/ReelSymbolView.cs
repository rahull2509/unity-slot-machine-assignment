using UnityEngine;
using UnityEngine.UI;
using SlotMachineGame.Data;

namespace SlotMachineGame.Reels
{
    /// <summary>
    /// Visual component presenting a single symbol sprite within a reel strip.
    /// Recycled continuously during spinning animations to eliminate runtime GC allocations.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ReelSymbolView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private RectTransform rectTransform;

        public int CurrentSymbolId { get; private set; } = -1;
        public RectTransform RectTransform => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
        }

        public void SetSymbol(SymbolData symbolData)
        {
            if (symbolData == null) return;
            CurrentSymbolId = symbolData.SymbolId;
            if (iconImage != null)
            {
                iconImage.sprite = symbolData.Sprite;
                iconImage.enabled = symbolData.Sprite != null;
            }
        }

        public void SetPosition(float yPosition)
        {
            Vector2 pos = RectTransform.anchoredPosition;
            pos.y = yPosition;
            RectTransform.anchoredPosition = pos;
        }

        public float GetYPosition()
        {
            return RectTransform.anchoredPosition.y;
        }
    }
}

