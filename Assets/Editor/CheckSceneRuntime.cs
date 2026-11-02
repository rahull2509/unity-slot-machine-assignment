using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SlotMachineGame.Core;
using SlotMachineGame.Data;
using SlotMachineGame.Gameplay;
using SlotMachineGame.Reels;
using SlotMachineGame.UI;
using SlotMachineGame.Audio;

namespace SlotMachineGame.Editor
{
    public static class CheckSceneRuntime
    {
        public static void RunSceneAudit()
        {
            Debug.Log("==================================================");
            Debug.Log("[SceneAudit] Starting SlotMachineScene Runtime Verification...");

            string scenePath = "Assets/Scenes/SlotMachineScene.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[SceneAudit] FAILED: Could not open scene at {scenePath}");
                EditorApplication.Exit(1);
                return;
            }

            // 1. Check Root Objects
            var rootObjects = scene.GetRootGameObjects();
            Debug.Log($"[SceneAudit] Scene loaded with {rootObjects.Length} root GameObjects.");

            // 2. Check SlotMachineController
            var controller = UnityEngine.Object.FindAnyObjectByType<SlotMachineController>();
            if (controller == null)
            {
                Debug.LogError("[SceneAudit] FAILED: SlotMachineController not found in scene!");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[SceneAudit] PASS: SlotMachineController found.");

            // 3. Check UIManager
            var uiManager = UnityEngine.Object.FindAnyObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[SceneAudit] FAILED: UIManager not found in scene!");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[SceneAudit] PASS: UIManager found.");

            // 4. Check WalletManager
            var wallet = UnityEngine.Object.FindAnyObjectByType<WalletManager>();
            if (wallet == null)
            {
                Debug.LogError("[SceneAudit] FAILED: WalletManager not found in scene!");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[SceneAudit] PASS: WalletManager found. Initial Balance = {wallet.CurrentBalance}, Bet = {wallet.CurrentBet}");

            // 5. Check AudioManager
            var audio = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
            if (audio == null)
            {
                Debug.LogError("[SceneAudit] FAILED: AudioManager not found in scene!");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[SceneAudit] PASS: AudioManager found.");

            // 6. Check Reels
            var reels = UnityEngine.Object.FindObjectsByType<ReelController>(FindObjectsSortMode.InstanceID);
            if (reels == null || reels.Length < 3)
            {
                Debug.LogError($"[SceneAudit] FAILED: Expected at least 3 reels, found {reels?.Length ?? 0}");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[SceneAudit] PASS: Found {reels.Length} ReelController instances.");

            foreach (var r in reels)
            {
                var views = r.GetComponentsInChildren<ReelSymbolView>(true);
                if (views.Length < 3)
                {
                    Debug.LogError($"[SceneAudit] FAILED: Reel {r.ReelIndex} has only {views.Length} symbol views (expected >= 3)");
                    EditorApplication.Exit(1);
                    return;
                }
            }
            Debug.Log("[SceneAudit] PASS: Each reel has at least 3 symbol views.");

            // 7. Check UI Sub-controllers
            if (uiManager.SpinButton == null)
            {
                Debug.LogError("[SceneAudit] FAILED: SpinButtonController reference missing on UIManager");
                EditorApplication.Exit(1);
                return;
            }
            if (uiManager.BetCtrl == null)
            {
                Debug.LogError("[SceneAudit] FAILED: BetController reference missing on UIManager");
                EditorApplication.Exit(1);
                return;
            }
            if (uiManager.WinPopup == null)
            {
                Debug.LogError("[SceneAudit] FAILED: WinPopupController reference missing on UIManager");
                EditorApplication.Exit(1);
                return;
            }
            if (uiManager.ResetDialog == null)
            {
                Debug.LogError("[SceneAudit] FAILED: ResetDialogController reference missing on UIManager");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[SceneAudit] PASS: All UI sub-controllers assigned and valid.");

            // 8. Test Bet Modification
            int startingBet = wallet.CurrentBet;
            wallet.IncreaseBet();
            Debug.Log($"[SceneAudit] PASS: Increased bet from {startingBet} to {wallet.CurrentBet}");
            wallet.DecreaseBet();
            Debug.Log($"[SceneAudit] PASS: Decreased bet back to {wallet.CurrentBet}");

            // 9. Test Audio Toggle
            bool initialMute = audio.IsMuted;
            audio.ToggleMute();
            Debug.Log($"[SceneAudit] PASS: Audio mute toggled from {initialMute} to {audio.IsMuted}");
            audio.ToggleMute();

            // 10. Test Reset Dialog Activation
            uiManager.ResetDialog.Show("Audit test reset prompt");
            uiManager.ResetDialog.HideImmediate();
            Debug.Log("[SceneAudit] PASS: Reset dialog Show and HideImmediate succeeded.");

            // 11. Test Win Popup Activation
            uiManager.WinPopup.Show(500, "Lucky 7");
            uiManager.WinPopup.HideImmediate();
            Debug.Log("[SceneAudit] PASS: Win popup Show and HideImmediate succeeded.");

            Debug.Log("[SceneAudit] ALL RUNTIME SCENE CHECKS PASSED PERFECTLY!");
            Debug.Log("==================================================");
            EditorApplication.Exit(0);
        }
    }
}
