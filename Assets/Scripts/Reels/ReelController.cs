using System;
using System.Collections.Generic;
using UnityEngine;
using SlotMachineGame.Data;

namespace SlotMachineGame.Reels
{
    /// <summary>
    /// Coordinates the views, conveyor pool, and animation state for a single slot reel window.
    /// </summary>
    public class ReelController : MonoBehaviour
    {
        [Header("Reel Configuration")]
        [SerializeField] private int reelIndex;
        [SerializeField] private SymbolDatabase symbolDatabase;
        [SerializeField] private ReelAnimator animator;
        [SerializeField] private List<ReelSymbolView> symbolViews = new List<ReelSymbolView>();

        [Header("Stopping Cadence")]
        [SerializeField] private float baseSpinDuration = 1.2f; // Added to reelIndex * 0.5f

        public int ReelIndex => reelIndex;
        public int CurrentCenterSymbolId { get; private set; } = 1;
        public bool IsSpinning => animator != null && animator.IsSpinning;

        public event Action<int, int> OnReelStopped;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<ReelAnimator>();
            if (symbolViews == null || symbolViews.Count == 0)
                symbolViews = new List<ReelSymbolView>(GetComponentsInChildren<ReelSymbolView>());
        }

        public void Initialize(int index, SymbolDatabase db, int initialSymbolId)
        {
            reelIndex = index;
            symbolDatabase = db;
            CurrentCenterSymbolId = initialSymbolId;

            if (symbolViews == null || symbolViews.Count == 0)
                symbolViews = new List<ReelSymbolView>(GetComponentsInChildren<ReelSymbolView>());

            if (animator == null)
                animator = GetComponent<ReelAnimator>();

            SetInitialDisplay(initialSymbolId);
        }

        public void SetInitialDisplay(int centerSymbolId)
        {
            CurrentCenterSymbolId = centerSymbolId;
            if (symbolDatabase == null || symbolViews == null || symbolViews.Count == 0) return;

            int targetIndex = 0;
            for (int i = 0; i < symbolDatabase.Count; i++)
            {
                if (symbolDatabase.GetSymbolByIndex(i).SymbolId == centerSymbolId)
                {
                    targetIndex = i;
                    break;
                }
            }

            int half = symbolViews.Count / 2;
            for (int i = 0; i < symbolViews.Count; i++)
            {
                int offset = i - half;
                symbolViews[i].SetPosition(offset * 100f);

                int dbIdx = (targetIndex + offset) % symbolDatabase.Count;
                if (dbIdx < 0) dbIdx += symbolDatabase.Count;
                symbolViews[i].SetSymbol(symbolDatabase.GetSymbolByIndex(dbIdx));
            }
        }

        public void SpinTo(int targetSymbolId, float customDuration = -1f)
        {
            CurrentCenterSymbolId = targetSymbolId;
            float duration = customDuration >= 0f ? customDuration : (baseSpinDuration + (reelIndex * 0.5f));

            if (animator != null)
            {
                animator.StartSpin(symbolViews, symbolDatabase, targetSymbolId, duration, HandleSpinCompleted);
            }
            else
            {
                HandleSpinCompleted(targetSymbolId);
            }
        }

        private void HandleSpinCompleted(int finalSymbolId)
        {
            CurrentCenterSymbolId = finalSymbolId;
            OnReelStopped?.Invoke(reelIndex, finalSymbolId);
        }

        public void StopImmediate()
        {
            if (animator != null)
            {
                animator.StopImmediate();
            }
            SetInitialDisplay(CurrentCenterSymbolId);
        }
    }
}

