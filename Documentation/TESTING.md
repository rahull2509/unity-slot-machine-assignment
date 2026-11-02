# Testing & Quality Assurance Plan

## 1. Overview

The automated test suite is built on the **Unity Test Framework** (UTF / NUnit). Tests are organized under `Assets/Tests/` to ensure full compilation and native discovery by Unity's AssetDatabase and Test Runner:
- **EditMode Tests (`Assets/Tests/EditMode/`)**: Fast, pure logic unit tests executing in isolation with zero scene dependencies (31 test cases).
- **PlayMode Tests (`Assets/Tests/PlayMode/`)**: Integration tests verifying reel animation lifecycle, coroutines, state transitions, and UI event firing (4 test cases).
- **Scene Runtime Audit (`Assets/Editor/CheckSceneRuntime.cs`)**: Automated in-engine scene verification testing component wiring, reel hierarchies, audio muting, bet step limits, and UI modal dialogues.
- **In-Browser Headless CDP Testing**: Real-browser startup and console validation via Chrome DevTools Protocol.

---

## 2. Actual Verified Test Results

| Suite | Location | Total Tests | Passed | Failed | Duration | NUnit Report |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **EditMode Unit Tests** | `Assets/Tests/EditMode/` | **31** | **31** | **0** | ~0.08s | `Tests/EditModeResults.xml` |
| **PlayMode Integration Tests** | `Assets/Tests/PlayMode/` | **4** | **4** | **0** | ~0.08s | `Tests/PlayModeResults.xml` |
| **Scene Runtime Audit** | `Assets/Editor/` | **11 checks** | **11** | **0** | ~8.0s | Verified in Unity batchmode |
| **Browser Runtime (CDP)** | `Build/WebGL/` | **Headless Chrome** | **Pass** | **0 exceptions** | ~12.0s | Verified via CDP WebSocket |
| **Grand Total** | | **35 Automated Tests** | **35** | **0** | | **100% Pass Rate** |

---

## 3. Test Coverage Breakdown

### A. Wallet Economy (`WalletManagerTests.cs` - 13 Tests)
1. `InitialState_HasCorrectDefaults`: Validates initial balance (1,000) and starting bet (10).
2. `TryDeductBet_SufficientBalance_ReducesBalance`: Validates balance deduction on spin.
3. `TryDeductBet_InsufficientBalance_ReturnsFalseAndPreservesBalance`: Validates rejection when funds < bet.
4. `TryDeductBet_NeverAllowsNegativeBalance`: Proves balance can never drop below zero.
5. `CreditWin_IncreasesBalanceAndRecordsLastPayout`: Validates correct addition and payout record.
6. `CreditWin_NegativeAmount_ThrowsArgumentException`: Rejects invalid negative payout parameters.
7. `CreditWin_PotentialOverflow_CapsAtIntMaxValueWithoutWrapping`: Guarantees integer overflow prevention (clamps at `int.MaxValue`).
8. `IncreaseBet_StepsThroughTiers`: Steps through 10 -> 20 -> 50 -> 100 -> 200 -> 500.
9. `DecreaseBet_StepsDownThroughTiers`: Steps down through tiers correctly.
10. `DecreaseBet_AtMinimum_DoesNotGoBelowMin`: Clamps at minimum bet (10).
11. `IncreaseBet_AtMaximum_DoesNotExceedMax`: Clamps at maximum bet (500).
12. `ResetWallet_RestoresConfiguredDefaults`: Restores balance to 1,000 and bet to 10.
13. `ResetWallet_WhenInitialBalanceWasInvalid_RestoresSafeDefault`: Gracefully handles invalid initial configurations.

### B. Outcome Evaluation (`WinningEvaluatorTests.cs` - 6 Tests)
1. `Evaluate_ThreeLucky7_ReturnsWin`: ID 1 across all 3 reels triggers jackpot win.
2. `Evaluate_ThreeCherries_ReturnsWin`: ID 2 across all 3 reels triggers cherries win.
3. `Evaluate_ThreeBells_ReturnsWin`: ID 3 across all 3 reels triggers bell win.
4. `Evaluate_ThreeBARs_ReturnsWin`: ID 4 across all 3 reels triggers BAR win.
5. `Evaluate_TwoMatchingSymbols_ReturnsLoss`: Rejects partial 2-symbol matches.
6. `Evaluate_DifferentSymbols_ReturnsLoss`: Rejects 3 distinct symbols.

### C. Payout Calculation (`PayoutCalculatorTests.cs` - 8 Tests)
1. `GetMultiplier_Lucky7_Returns10`: 10x multiplier for Lucky 7.
2. `GetMultiplier_Cherries_Returns5`: 5x multiplier for Cherries.
3. `GetMultiplier_Bell_Returns4`: 4x multiplier for Bell.
4. `GetMultiplier_BAR_Returns3`: 3x multiplier for BAR.
5. `CalculatePayout_WithBet10_CalculatesExpectedPayouts`: Scales payouts correctly for bet 10 (100, 50, 40, 30).
6. `CalculatePayout_WithBet50_CalculatesExpectedPayouts`: Scales payouts correctly for bet 50 (500, 250, 200, 150).
7. `CalculatePayout_ZeroOrNegativeBet_ReturnsZero`: Rejects zero/negative wagers.
8. `CalculatePayout_UnknownSymbol_ReturnsZero`: Handles unrecognized symbol IDs safely.

### D. Cryptographic RNG (`RandomSymbolProviderTests.cs` - 4 Tests)
1. `NextSymbolIndex_AlwaysWithinValidRange`: Validates bounds across 1,000 iterations.
2. `UniformDistribution_AcrossSymbols`: Proves chi-square uniformity across 10,000 samples.
3. `DeterministicSeed_GeneratesIdenticalSequence`: Verifies reproducible sequences for unit tests.
4. `DifferentSeeds_GenerateDifferentSequences`: Verifies distinct seeds yield independent streams.

### E. Controller & PlayMode Integration (`SlotMachineControllerTests.cs` - 4 Tests)
1. `InitialState_IsIdle`: Machine starts in `GameState.Idle`.
2. `RequestSpin_WithSufficientBalance_StartsSpinAndDeductsBet`: Deducts bet and enters `GameState.Spinning`.
3. `RequestSpin_WhileAlreadySpinning_IsRejected`: Rejects concurrent spin requests.
4. `RequestSpin_WithInsufficientBalance_IsRejected`: Transitions to `GameState.InsufficientFunds`.

---

## 4. Running the Tests Locally

### In Unity Editor GUI:
1. Open the project in Unity 6000.6.7f1.
2. Open **Window > General > Test Runner**.
3. Select **EditMode** or **PlayMode** tab and click **Run All**.

### Via Command Line (Batch Mode):
```powershell
$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"

# EditMode Tests
Start-Process -FilePath $unityExe -ArgumentList "-batchmode -runTests -testPlatform EditMode -projectPath . -testResults Tests/EditModeResults.xml -logFile test_editmode.log" -Wait -NoNewWindow

# PlayMode Tests
Start-Process -FilePath $unityExe -ArgumentList "-batchmode -runTests -testPlatform PlayMode -projectPath . -testResults Tests/PlayModeResults.xml -logFile test_playmode.log" -Wait -NoNewWindow
```
NUnit XML output reports are generated at `Tests/EditModeResults.xml` and `Tests/PlayModeResults.xml`.
