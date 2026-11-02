using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SlotMachineGame.Core;
using SlotMachineGame.Data;
using SlotMachineGame.Gameplay;
using SlotMachineGame.Reels;
using SlotMachineGame.UI;
using SlotMachineGame.Audio;

namespace SlotMachineGame.Editor
{
    /// <summary>
    /// Automated scene builder that programmatically creates, configures, and links the complete
    /// SlotMachineScene with pixel-perfect asset alignment, Canvas Scaler, UI hierarchy, and audio.
    /// Uses robust uGUI Text with built-in font for 100% reliable rendering on WebGL and Desktop.
    /// </summary>
    public static class SlotGameSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SlotMachineScene.unity";
        private const string ProvidedArtPath = "Assets/Art/ProvidedAssets";
        private const string ScriptableObjectsPath = "Assets/ScriptableObjects";

        [MenuItem("SlotMachine/Build Playable Scene", false, 2)]
        public static void BuildPlayableScene()
        {
            // 1. Ensure assets are imported and sliced first
            SlotGameAssetImporter.ImportAndConfigureAll();

            // 2. Create new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Get reliable built-in font
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 3. Setup Camera
            GameObject cameraGo = new GameObject("Main Camera");
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.05f, 0.12f, 1f); // Deep casino purple
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraGo.transform.position = new Vector3(0, 0, -10f);
            cameraGo.tag = "MainCamera";

            // 4. Setup EventSystem
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            // 5. Setup Master Canvas
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Load art references
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/bg_gradient.png");
            Sprite smCabinet = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-machine4.png");
            Sprite popupSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/popup.png");

            Sprite sym7 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol1.png");
            Sprite symCherry = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol2.png");
            Sprite symBell = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol3.png");
            Sprite symBar = AssetDatabase.LoadAssetAtPath<Sprite>($"{ProvidedArtPath}/slot-symbol4.png");

            Sprite decNormal = LoadSubSprite($"{ProvidedArtPath}/slot_machine_buttons-03.png", "slot_machine_buttons-03_Normal");
            Sprite incNormal = LoadSubSprite($"{ProvidedArtPath}/slot_machine_buttons-04.png", "slot_machine_buttons-04_Normal");
            Sprite btnYesNormal = LoadSubSprite($"{ProvidedArtPath}/Yes_No_Btn.png", "Btn_Yes_Normal");
            Sprite btnNoNormal = LoadSubSprite($"{ProvidedArtPath}/Yes_No_Btn.png", "Btn_No_Normal");

            SymbolDatabase symbolDb = AssetDatabase.LoadAssetAtPath<SymbolDatabase>($"{ScriptableObjectsPath}/SymbolDatabase.asset");
            PayoutConfiguration payoutConfig = AssetDatabase.LoadAssetAtPath<PayoutConfiguration>($"{ScriptableObjectsPath}/PayoutConfiguration.asset");

            // 6. Background Layer
            GameObject bgGo = CreateUIElement("Background", canvasGo.transform);
            RectTransform bgRect = bgGo.GetComponent<RectTransform>();
            StretchFull(bgRect);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.raycastTarget = false;

            // 7. Cabinet Root (Centered, 816 x 624 native proportion scaled up slightly for 1080p)
            GameObject cabinetRoot = CreateUIElement("CabinetRoot", canvasGo.transform);
            RectTransform cabinetRect = cabinetRoot.GetComponent<RectTransform>();
            cabinetRect.anchoredPosition = new Vector2(0, 35f);
            cabinetRect.sizeDelta = new Vector2(816f, 624f);
            cabinetRect.localScale = new Vector3(1.15f, 1.15f, 1f);

            // 7a. Cavity Backplate (Behind reels: dark recessed backing plate so symbols stand out vibrantly)
            GameObject backPlateGo = CreateUIElement("BackingPlate", cabinetRoot.transform);
            RectTransform backRect = backPlateGo.GetComponent<RectTransform>();
            backRect.anchoredPosition = new Vector2(0, -32f);
            backRect.sizeDelta = new Vector2(370f, 210f);
            Image backImg = backPlateGo.AddComponent<Image>();
            backImg.color = new Color(0.10f, 0.08f, 0.20f, 1f); // Deep casino purple backing
            backImg.raycastTarget = false;

            // 7b. Reel Viewport Aperture (Masked area: 360 x 205, pos: 0, -32)
            GameObject reelAperture = CreateUIElement("ReelAperture", cabinetRoot.transform);
            RectTransform apRect = reelAperture.GetComponent<RectTransform>();
            apRect.anchoredPosition = new Vector2(0, -32f);
            apRect.sizeDelta = new Vector2(360f, 205f);
            Image apImg = reelAperture.AddComponent<Image>();
            apImg.color = new Color(0.12f, 0.09f, 0.22f, 0.95f);
            reelAperture.AddComponent<RectMask2D>();

            // 7c. Three Reels
            ReelController[] reels = new ReelController[3];
            float[] reelXPositions = new float[] { -118f, 0f, 118f };
            int[] initialSymbols = new int[] { 1, 2, 3 }; // Lucky 7, Cherries, Bell initially showing

            for (int r = 0; r < 3; r++)
            {
                GameObject reelGo = CreateUIElement($"Reel_{r}", reelAperture.transform);
                RectTransform rRect = reelGo.GetComponent<RectTransform>();
                rRect.anchoredPosition = new Vector2(reelXPositions[r], 0f);
                rRect.sizeDelta = new Vector2(110f, 205f);

                ReelController controller = reelGo.AddComponent<ReelController>();
                ReelAnimator anim = reelGo.AddComponent<ReelAnimator>();

                // Create 5 recycled symbol views
                var symbolViews = new List<ReelSymbolView>();
                for (int v = 0; v < 5; v++)
                {
                    GameObject viewGo = CreateUIElement($"View_{v}", reelGo.transform);
                    RectTransform vRect = viewGo.GetComponent<RectTransform>();
                    vRect.sizeDelta = new Vector2(96f, 96f);
                    vRect.anchoredPosition = new Vector2(0f, (v - 2) * 100f);

                    Image iconImg = viewGo.AddComponent<Image>();
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;

                    // Assign initial sprite from database
                    int symIdx = (initialSymbols[r] + (v - 2) + 4) % 4;
                    SymbolData initData = symbolDb != null ? symbolDb.GetSymbolByIndex(symIdx) : null;
                    if (initData != null && initData.Sprite != null)
                    {
                        iconImg.sprite = initData.Sprite;
                    }

                    ReelSymbolView symView = viewGo.AddComponent<ReelSymbolView>();
                    SetPrivate(symView, "iconImage", iconImg);
                    SetPrivate(symView, "rectTransform", vRect);
                    symbolViews.Add(symView);
                }

                SetPrivate(controller, "reelIndex", r);
                SetPrivate(controller, "symbolDatabase", symbolDb);
                SetPrivate(controller, "animator", anim);
                SetPrivate(controller, "symbolViews", symbolViews);
                SetPrivate(controller, "baseSpinDuration", 1.2f + (r * 0.5f));

                // Initialize view positions immediately
                controller.Initialize(r, symbolDb, initialSymbols[r]);
                reels[r] = controller;
            }

            // 7d. Front Cabinet Shell (Transparent cutouts reveal the spinning reels!)
            GameObject cabFrontGo = CreateUIElement("CabinetFront", cabinetRoot.transform);
            RectTransform cabFrontRect = cabFrontGo.GetComponent<RectTransform>();
            cabFrontRect.anchoredPosition = Vector2.zero;
            cabFrontRect.sizeDelta = new Vector2(816f, 624f);
            Image cabFrontImg = cabFrontGo.AddComponent<Image>();
            cabFrontImg.sprite = smCabinet;
            cabFrontImg.preserveAspect = true;
            cabFrontImg.raycastTarget = false;

            // 7e. Interactive Pull Lever on Right Side of Cabinet
            GameObject leverBtnGo = CreateUIElement("LeverHandleButton", cabinetRoot.transform);
            RectTransform leverRect = leverBtnGo.GetComponent<RectTransform>();
            leverRect.anchoredPosition = new Vector2(340f, -40f);
            leverRect.sizeDelta = new Vector2(80f, 240f);
            Image leverHitImage = leverBtnGo.AddComponent<Image>();
            leverHitImage.color = new Color(0, 0, 0, 0); // Transparent raycast target
            leverHitImage.raycastTarget = true;
            Button leverButton = leverBtnGo.AddComponent<Button>();

            // 8. HUD & Controls (Canvas space)
            // 8a. Title & Status Banner
            GameObject headerGo = CreateUIElement("HeaderPanel", canvasGo.transform);
            RectTransform headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0, -25f);
            headerRect.sizeDelta = new Vector2(900f, 95f);

            Text titleText = CreateUIText("Title", headerGo.transform, defaultFont, "SLOT MACHINE DELUXE", 34, FontStyle.Bold, new Color(1f, 0.88f, 0.2f));
            titleText.rectTransform.anchoredPosition = new Vector2(0, 0);

            Text statusText = CreateUIText("StatusBanner", headerGo.transform, defaultFont, "PRESS SPIN TO PLAY!", 22, FontStyle.Bold, Color.white);
            statusText.rectTransform.anchoredPosition = new Vector2(0, -38f);

            // 8b. Bottom Control Console
            GameObject consoleGo = CreateUIElement("ControlConsole", canvasGo.transform);
            RectTransform consoleRect = consoleGo.GetComponent<RectTransform>();
            consoleRect.anchorMin = new Vector2(0.5f, 0f);
            consoleRect.anchorMax = new Vector2(0.5f, 0f);
            consoleRect.pivot = new Vector2(0.5f, 0f);
            consoleRect.anchoredPosition = new Vector2(0, 35f);
            consoleRect.sizeDelta = new Vector2(1000f, 130f);

            // Spin Button (Prominent Center Console)
            GameObject spinBtnGo = CreateUIElement("SpinButton", consoleGo.transform);
            RectTransform spinBtnRect = spinBtnGo.GetComponent<RectTransform>();
            spinBtnRect.anchoredPosition = new Vector2(0, 45f);
            spinBtnRect.sizeDelta = new Vector2(140f, 140f);

            // Outer golden glow plate
            Image spinPlate = spinBtnGo.AddComponent<Image>();
            spinPlate.color = new Color(0.12f, 0.72f, 0.80f, 1f); // Vibrant Teal/Cyan
            Button spinButton = spinBtnGo.AddComponent<Button>();
            ColorBlock cb = spinButton.colors;
            cb.normalColor = new Color(0.15f, 0.75f, 0.85f, 1f);
            cb.highlightedColor = new Color(0.35f, 0.90f, 1f, 1f);
            cb.pressedColor = new Color(0.10f, 0.55f, 0.65f, 1f);
            cb.disabledColor = new Color(0.35f, 0.35f, 0.40f, 0.8f);
            spinButton.colors = cb;

            Text spinLabel = CreateUIText("SpinLabel", spinBtnGo.transform, defaultFont, "SPIN", 32, FontStyle.Bold, Color.white);
            spinLabel.rectTransform.anchoredPosition = Vector2.zero;

            SpinButtonController spinBtnCtrl = spinBtnGo.AddComponent<SpinButtonController>();
            SetPrivate(spinBtnCtrl, "spinButton", spinButton);
            SetPrivate(spinBtnCtrl, "buttonImage", spinPlate);

            // Connect lever button to also trigger spin
            leverButton.onClick.AddListener(() => spinButton.onClick.Invoke());

            // Bet Controls (Left of Spin Button: pos x = -260)
            GameObject betGroupGo = CreateUIElement("BetGroup", consoleGo.transform);
            RectTransform betGroupRect = betGroupGo.GetComponent<RectTransform>();
            betGroupRect.anchoredPosition = new Vector2(-260f, 45f);
            betGroupRect.sizeDelta = new Vector2(240f, 80f);

            // Bet Decrease Button (<)
            GameObject decBtnGo = CreateUIElement("DecBetBtn", betGroupGo.transform);
            RectTransform decRect = decBtnGo.GetComponent<RectTransform>();
            decRect.anchoredPosition = new Vector2(-75f, 0);
            decRect.sizeDelta = new Vector2(60f, 60f);
            Image decImg = decBtnGo.AddComponent<Image>();
            decImg.sprite = decNormal != null ? decNormal : symCherry;
            decImg.preserveAspect = true;
            Button decButton = decBtnGo.AddComponent<Button>();

            // Bet Readout Box
            GameObject betBoxGo = CreateUIElement("BetBox", betGroupGo.transform);
            RectTransform betBoxRect = betBoxGo.GetComponent<RectTransform>();
            betBoxRect.anchoredPosition = new Vector2(0, 0);
            betBoxRect.sizeDelta = new Vector2(90f, 65f);
            Image betBoxBg = betBoxGo.AddComponent<Image>();
            betBoxBg.color = new Color(0.15f, 0.12f, 0.30f, 0.9f);

            var betLabel = CreateUIText("BetLabel", betBoxGo.transform, defaultFont, "BET", 14, FontStyle.Bold, new Color(0.7f, 0.85f, 1f));
            betLabel.rectTransform.anchoredPosition = new Vector2(0, 16f);
            var betValText = CreateUIText("BetValue", betBoxGo.transform, defaultFont, "10", 24, FontStyle.Bold, Color.yellow);
            betValText.rectTransform.anchoredPosition = new Vector2(0, -10f);

            // Bet Increase Button (>)
            GameObject incBtnGo = CreateUIElement("IncBetBtn", betGroupGo.transform);
            RectTransform incRect = incBtnGo.GetComponent<RectTransform>();
            incRect.anchoredPosition = new Vector2(75f, 0);
            incRect.sizeDelta = new Vector2(60f, 60f);
            Image incImg = incBtnGo.AddComponent<Image>();
            incImg.sprite = incNormal != null ? incNormal : symBar;
            incImg.preserveAspect = true;
            Button incButton = incBtnGo.AddComponent<Button>();

            BetController betCtrl = betGroupGo.AddComponent<BetController>();
            SetPrivate(betCtrl, "decreaseButton", decButton);
            SetPrivate(betCtrl, "increaseButton", incButton);
            SetPrivate(betCtrl, "legacyBetText", betValText);

            // Balance HUD Display (Far Left console: pos x = -480)
            GameObject balanceBoxGo = CreateUIElement("BalanceBox", consoleGo.transform);
            RectTransform balRect = balanceBoxGo.GetComponent<RectTransform>();
            balRect.anchoredPosition = new Vector2(-485f, 45f);
            balRect.sizeDelta = new Vector2(180f, 80f);
            Image balBg = balanceBoxGo.AddComponent<Image>();
            balBg.color = new Color(0.15f, 0.12f, 0.30f, 0.9f);

            var balLabel = CreateUIText("BalanceLabel", balanceBoxGo.transform, defaultFont, "BALANCE", 15, FontStyle.Bold, new Color(0.7f, 0.85f, 1f));
            balLabel.rectTransform.anchoredPosition = new Vector2(0, 16f);
            var balValText = CreateUIText("BalanceValue", balanceBoxGo.transform, defaultFont, "1,000", 26, FontStyle.Bold, Color.white);
            balValText.rectTransform.anchoredPosition = new Vector2(0, -12f);

            // Last Win HUD Display (Right of Spin Button: pos x = +260)
            GameObject winBoxGo = CreateUIElement("WinBox", consoleGo.transform);
            RectTransform winBoxRect = winBoxGo.GetComponent<RectTransform>();
            winBoxRect.anchoredPosition = new Vector2(260f, 45f);
            winBoxRect.sizeDelta = new Vector2(180f, 80f);
            Image winBg = winBoxGo.AddComponent<Image>();
            winBg.color = new Color(0.15f, 0.12f, 0.30f, 0.9f);

            var winLabel = CreateUIText("WinLabel", winBoxGo.transform, defaultFont, "LAST WIN", 15, FontStyle.Bold, new Color(0.7f, 0.85f, 1f));
            winLabel.rectTransform.anchoredPosition = new Vector2(0, 16f);
            var winValText = CreateUIText("WinValue", winBoxGo.transform, defaultFont, "0", 26, FontStyle.Bold, new Color(0.3f, 1f, 0.4f));
            winValText.rectTransform.anchoredPosition = new Vector2(0, -12f);

            // Utility Buttons (Sound Toggle & Reset) pos x = +470
            GameObject utilGroupGo = CreateUIElement("UtilityGroup", consoleGo.transform);
            RectTransform utilRect = utilGroupGo.GetComponent<RectTransform>();
            utilRect.anchoredPosition = new Vector2(470f, 45f);
            utilRect.sizeDelta = new Vector2(160f, 80f);

            GameObject soundBtnGo = CreateUIElement("SoundButton", utilGroupGo.transform);
            RectTransform soundRect = soundBtnGo.GetComponent<RectTransform>();
            soundRect.anchoredPosition = new Vector2(0, 18f);
            soundRect.sizeDelta = new Vector2(140f, 32f);
            Image soundImg = soundBtnGo.AddComponent<Image>();
            soundImg.color = new Color(0.2f, 0.15f, 0.38f, 0.95f);
            Button soundBtn = soundBtnGo.AddComponent<Button>();
            var soundText = CreateUIText("SoundText", soundBtnGo.transform, defaultFont, "SOUND: ON", 14, FontStyle.Bold, Color.white);
            soundText.rectTransform.anchoredPosition = Vector2.zero;

            GameObject resetBtnGo = CreateUIElement("ResetButton", utilGroupGo.transform);
            RectTransform rstRect = resetBtnGo.GetComponent<RectTransform>();
            rstRect.anchoredPosition = new Vector2(0, -20f);
            rstRect.sizeDelta = new Vector2(140f, 32f);
            Image rstImg = resetBtnGo.AddComponent<Image>();
            rstImg.color = new Color(0.38f, 0.15f, 0.22f, 0.95f);
            Button rstBtn = resetBtnGo.AddComponent<Button>();
            var rstText = CreateUIText("ResetText", resetBtnGo.transform, defaultFont, "RESET GAME", 14, FontStyle.Bold, Color.white);
            rstText.rectTransform.anchoredPosition = Vector2.zero;

            // 9. Modals & Dialogs
            // 9a. Win Celebration Popup
            GameObject winPopupRoot = CreateUIElement("WinPopupRoot", canvasGo.transform);
            RectTransform winPopRect = winPopupRoot.GetComponent<RectTransform>();
            StretchFull(winPopRect);
            Image winDimmer = winPopupRoot.AddComponent<Image>();
            winDimmer.color = new Color(0f, 0f, 0f, 0.7f);

            GameObject winDialogBox = CreateUIElement("DialogBox", winPopupRoot.transform);
            RectTransform winBoxTr = winDialogBox.GetComponent<RectTransform>();
            winBoxTr.anchoredPosition = Vector2.zero;
            winBoxTr.sizeDelta = new Vector2(800f, 440f);
            Image winBoxImg = winDialogBox.AddComponent<Image>();
            winBoxImg.sprite = popupSprite;
            winBoxImg.preserveAspect = true;

            var winTitle = CreateUIText("WinTitle", winDialogBox.transform, defaultFont, "BIG WIN!\nLUCKY 7", 32, FontStyle.Bold, Color.yellow);
            winTitle.rectTransform.anchoredPosition = new Vector2(0, 60f);

            var winCoins = CreateUIText("WinCoins", winDialogBox.transform, defaultFont, "+1,000 COINS", 38, FontStyle.Bold, new Color(0.3f, 1f, 0.4f));
            winCoins.rectTransform.anchoredPosition = new Vector2(0, -20f);

            GameObject collectBtnGo = CreateUIElement("CollectButton", winDialogBox.transform);
            RectTransform collRect = collectBtnGo.GetComponent<RectTransform>();
            collRect.anchoredPosition = new Vector2(0, -110f);
            collRect.sizeDelta = new Vector2(220f, 60f);
            Image collImg = collectBtnGo.AddComponent<Image>();
            collImg.sprite = btnYesNormal;
            collImg.preserveAspect = true;
            Button collectBtn = collectBtnGo.AddComponent<Button>();
            var collText = CreateUIText("CollectText", collectBtnGo.transform, defaultFont, "COLLECT", 20, FontStyle.Bold, Color.white);
            collText.rectTransform.anchoredPosition = Vector2.zero;

            WinPopupController winPopupCtrl = winPopupRoot.AddComponent<WinPopupController>();
            SetPrivate(winPopupCtrl, "popupRoot", winPopupRoot);
            SetPrivate(winPopupCtrl, "legacyWinTitleText", winTitle);
            SetPrivate(winPopupCtrl, "legacyWinAmountText", winCoins);
            SetPrivate(winPopupCtrl, "collectButton", collectBtn);

            // 9b. Reset Confirmation Dialog
            GameObject resetDialogRoot = CreateUIElement("ResetDialogRoot", canvasGo.transform);
            RectTransform resetDiagRect = resetDialogRoot.GetComponent<RectTransform>();
            StretchFull(resetDiagRect);
            Image resetDimmer = resetDialogRoot.AddComponent<Image>();
            resetDimmer.color = new Color(0f, 0f, 0f, 0.7f);

            GameObject resetBox = CreateUIElement("ResetBox", resetDialogRoot.transform);
            RectTransform rboxTr = resetBox.GetComponent<RectTransform>();
            rboxTr.anchoredPosition = Vector2.zero;
            rboxTr.sizeDelta = new Vector2(750f, 420f);
            Image rboxImg = resetBox.AddComponent<Image>();
            rboxImg.sprite = popupSprite;
            rboxImg.preserveAspect = true;

            var rMsg = CreateUIText("PromptMessage", resetBox.transform, defaultFont, "Reset wallet balance to 1,000 coins?", 24, FontStyle.Bold, Color.white);
            rMsg.rectTransform.anchoredPosition = new Vector2(0, 30f);

            GameObject yesBtnGo = CreateUIElement("YesBtn", resetBox.transform);
            RectTransform yesRect = yesBtnGo.GetComponent<RectTransform>();
            yesRect.anchoredPosition = new Vector2(-110f, -80f);
            yesRect.sizeDelta = new Vector2(180f, 65f);
            Image yesImg = yesBtnGo.AddComponent<Image>();
            yesImg.sprite = btnYesNormal;
            yesImg.preserveAspect = true;
            Button yesBtn = yesBtnGo.AddComponent<Button>();
            var yesText = CreateUIText("YesText", yesBtnGo.transform, defaultFont, "YES", 22, FontStyle.Bold, Color.white);
            yesText.rectTransform.anchoredPosition = Vector2.zero;

            GameObject noBtnGo = CreateUIElement("NoBtn", resetBox.transform);
            RectTransform noRect = noBtnGo.GetComponent<RectTransform>();
            noRect.anchoredPosition = new Vector2(110f, -80f);
            noRect.sizeDelta = new Vector2(180f, 65f);
            Image noImg = noBtnGo.AddComponent<Image>();
            noImg.sprite = btnNoNormal;
            noImg.preserveAspect = true;
            Button noBtn = noBtnGo.AddComponent<Button>();
            var noText = CreateUIText("NoText", noBtnGo.transform, defaultFont, "NO", 22, FontStyle.Bold, Color.white);
            noText.rectTransform.anchoredPosition = Vector2.zero;

            ResetDialogController resetCtrl = resetDialogRoot.AddComponent<ResetDialogController>();
            SetPrivate(resetCtrl, "dialogRoot", resetDialogRoot);
            SetPrivate(resetCtrl, "legacyMessageText", rMsg);
            SetPrivate(resetCtrl, "yesButton", yesBtn);
            SetPrivate(resetCtrl, "noButton", noBtn);

            // Initially hide modals
            winPopupRoot.SetActive(false);
            resetDialogRoot.SetActive(false);

            // 10. UIManager Wiring
            UIManager uiManager = canvasGo.AddComponent<UIManager>();
            SetPrivate(uiManager, "legacyBalanceText", balValText);
            SetPrivate(uiManager, "legacyWinAmountText", winValText);
            SetPrivate(uiManager, "legacyStatusMessageText", statusText);
            SetPrivate(uiManager, "spinButtonController", spinBtnCtrl);
            SetPrivate(uiManager, "betController", betCtrl);
            SetPrivate(uiManager, "winPopupController", winPopupCtrl);
            SetPrivate(uiManager, "resetDialogController", resetCtrl);
            SetPrivate(uiManager, "soundToggleButton", soundBtn);
            SetPrivate(uiManager, "soundToggleLabel", soundText);
            SetPrivate(uiManager, "resetGameButton", rstBtn);

            // 11. Core Managers Root GameObject
            GameObject coreGo = new GameObject("GameManager");
            GameManager gameMgr = coreGo.AddComponent<GameManager>();
            WalletManager walletMgr = coreGo.AddComponent<WalletManager>();
            AudioManager audioMgr = coreGo.AddComponent<AudioManager>();
            SlotMachineController slotController = coreGo.AddComponent<SlotMachineController>();

            SetPrivate(gameMgr, "slotMachineController", slotController);
            SetPrivate(slotController, "reels", reels);
            SetPrivate(slotController, "walletManager", walletMgr);
            SetPrivate(slotController, "uiManager", uiManager);
            SetPrivate(slotController, "audioManager", audioMgr);
            SetPrivate(slotController, "symbolDatabase", symbolDb);
            SetPrivate(slotController, "payoutConfiguration", payoutConfig);

            // 12. Save Scene
            EditorSceneManager.SaveScene(newScene, ScenePath);

            // 13. Add Scene to Build Settings
            var sceneList = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = sceneList.ToArray();

            Debug.Log($"[SlotGameSceneBuilder] Successfully generated and configured '{ScenePath}' with rock-solid uGUI typography and crisp visible reels!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateUIText(string name, Transform parent, Font font, string text, int fontSize, FontStyle style, Color color)
        {
            GameObject textGo = CreateUIElement(name, parent);
            Text t = textGo.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            // Add drop shadow and crisp outline for arcade readability
            Outline outline = textGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.04f, 0.02f, 0.10f, 0.95f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Shadow shadow = textGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return t;
        }

        private static Sprite LoadSubSprite(string assetPath, string spriteName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var a in assets)
            {
                if (a is Sprite sp && sp.name == spriteName)
                    return sp;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[SlotGameSceneBuilder] Could not find field '{fieldName}' on {target.GetType().Name}");
            }
        }
    }
}
