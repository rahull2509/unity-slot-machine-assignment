using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotMachineGame.Data;

namespace SlotMachineGame.Reels
{
    /// <summary>
    /// Executes smooth continuous vertical scrolling, realistic deceleration, and center payline snapping
    /// with settle bounce physics for a single slot reel.
    /// </summary>
    public class ReelAnimator : MonoBehaviour
    {
        [Header("Motion Configuration")]
        [SerializeField] private float maxSpeed = 2200f; // pixels per second
        [SerializeField] private float accelerationTime = 0.25f;
        [SerializeField] private float decelerationTime = 0.45f;
        [SerializeField] private float bounceOvershoot = 18f;
        [SerializeField] private float bounceDuration = 0.18f;
        [SerializeField] private float symbolSpacing = 100f;

        private Coroutine spinCoroutine;
        private bool isSpinning;

        public bool IsSpinning => isSpinning;

        public void StartSpin(
            List<ReelSymbolView> views,
            SymbolDatabase symbolDatabase,
            int targetSymbolId,
            float steadySpinDuration,
            Action<int> onCompleted)
        {
            if (isSpinning && spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
            }

            spinCoroutine = StartCoroutine(SpinRoutine(views, symbolDatabase, targetSymbolId, steadySpinDuration, onCompleted));
        }

        public void StopImmediate()
        {
            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
                spinCoroutine = null;
            }
            isSpinning = false;
        }

        private IEnumerator SpinRoutine(
            List<ReelSymbolView> views,
            SymbolDatabase symbolDatabase,
            int targetSymbolId,
            float steadySpinDuration,
            Action<int> onCompleted)
        {
            isSpinning = true;

            int viewCount = views.Count;
            float topY = (viewCount / 2) * symbolSpacing;
            float bottomThreshold = -((viewCount / 2) * symbolSpacing) - (symbolSpacing * 0.5f);
            float wrapToY = topY + symbolSpacing;

            float currentSpeed = 0f;
            float elapsed = 0f;

            // Phase 1: Acceleration
            while (elapsed < accelerationTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / accelerationTime);
                currentSpeed = Mathf.Lerp(0f, maxSpeed, Mathf.SmoothStep(0f, 1f, t));
                StepConveyor(views, symbolDatabase, currentSpeed * Time.deltaTime, bottomThreshold, wrapToY);
                yield return null;
            }
            currentSpeed = maxSpeed;

            // Phase 2: Steady state high-speed spin
            float spinTimer = 0f;
            while (spinTimer < steadySpinDuration)
            {
                spinTimer += Time.deltaTime;
                StepConveyor(views, symbolDatabase, currentSpeed * Time.deltaTime, bottomThreshold, wrapToY);
                yield return null;
            }

            // Phase 3: Deceleration & Target Acquisition
            // Choose view 0 as the designated center landing view and assign targetSymbolId
            ReelSymbolView centerLandingView = views[0];
            SymbolData targetData = symbolDatabase.GetSymbolById(targetSymbolId);

            // Spin until centerLandingView is about to wrap or position above the viewport
            // Then glide it into exact position y = 0
            float decelElapsed = 0f;
            while (decelElapsed < decelerationTime)
            {
                decelElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(decelElapsed / decelerationTime);
                currentSpeed = Mathf.Lerp(maxSpeed, 400f, t);
                StepConveyor(views, symbolDatabase, currentSpeed * Time.deltaTime, bottomThreshold, wrapToY);
                yield return null;
            }

            // Align all views so that the target view smoothly approaches y = 0
            centerLandingView.SetSymbol(targetData);

            // Populate adjacent views with valid symbols from database so there are no empty views
            int targetIndexInDb = 0;
            for (int i = 0; i < symbolDatabase.Count; i++)
            {
                if (symbolDatabase.GetSymbolByIndex(i).SymbolId == targetSymbolId)
                {
                    targetIndexInDb = i;
                    break;
                }
            }

            // Glide remaining distance to 0
            float approachSpeed = 600f;
            float currentCenterY = centerLandingView.GetYPosition();

            // Bring the center view to just above zero if it's below
            if (currentCenterY < 0f)
            {
                currentCenterY += symbolSpacing * viewCount;
                centerLandingView.SetPosition(currentCenterY);
            }

            while (centerLandingView.GetYPosition() > 0f)
            {
                float step = approachSpeed * Time.deltaTime;
                if (centerLandingView.GetYPosition() - step <= 0f)
                {
                    // Snapped to zero
                    centerLandingView.SetPosition(0f);
                    break;
                }
                StepConveyor(views, symbolDatabase, step, bottomThreshold, wrapToY);
                yield return null;
            }

            // Perfectly align all other views relative to center view
            SnapViewsToGrid(views, centerLandingView, symbolDatabase, targetIndexInDb);

            // Phase 4: Settle bounce / overshoot
            float bounceElapsed = 0f;
            while (bounceElapsed < bounceDuration)
            {
                bounceElapsed += Time.deltaTime;
                float t = bounceElapsed / bounceDuration;
                // Bounce formula: overshoot down, then damp back to 0
                float offset = -bounceOvershoot * Mathf.Sin(t * Mathf.PI) * (1f - t);

                for (int i = 0; i < views.Count; i++)
                {
                    float baseY = (i - (viewCount / 2)) * symbolSpacing;
                    views[i].SetPosition(baseY + offset);
                }
                yield return null;
            }

            // Final exact snap
            for (int i = 0; i < views.Count; i++)
            {
                float baseY = (i - (viewCount / 2)) * symbolSpacing;
                views[i].SetPosition(baseY);
            }

            isSpinning = false;
            spinCoroutine = null;
            onCompleted?.Invoke(targetSymbolId);
        }

        private void StepConveyor(
            List<ReelSymbolView> views,
            SymbolDatabase symbolDatabase,
            float deltaY,
            float bottomThreshold,
            float wrapToY)
        {
            for (int i = 0; i < views.Count; i++)
            {
                var view = views[i];
                float newY = view.GetYPosition() - deltaY;
                if (newY < bottomThreshold)
                {
                    newY += (views.Count * symbolSpacing);
                    // Assign random symbol while spinning
                    int randIdx = UnityEngine.Random.Range(0, symbolDatabase.Count);
                    view.SetSymbol(symbolDatabase.GetSymbolByIndex(randIdx));
                }
                view.SetPosition(newY);
            }
        }

        private void SnapViewsToGrid(
            List<ReelSymbolView> views,
            ReelSymbolView centerView,
            SymbolDatabase symbolDatabase,
            int centerSymbolDbIndex)
        {
            int centerIdxInViews = views.IndexOf(centerView);
            int half = views.Count / 2;

            for (int i = 0; i < views.Count; i++)
            {
                int relativeOffset = i - centerIdxInViews;
                float targetY = relativeOffset * symbolSpacing;
                views[i].SetPosition(targetY);

                if (views[i] != centerView)
                {
                    int dbIdx = (centerSymbolDbIndex + relativeOffset) % symbolDatabase.Count;
                    if (dbIdx < 0) dbIdx += symbolDatabase.Count;
                    views[i].SetSymbol(symbolDatabase.GetSymbolByIndex(dbIdx));
                }
            }
        }
    }
}
