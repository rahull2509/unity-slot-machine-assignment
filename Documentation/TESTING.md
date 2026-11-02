# Testing & Quality Assurance Plan

## 1. Overview

The test suite is built on the **Unity Test Framework** (UTF / NUnit). Tests are divided into:
- **EditMode Tests (`Tests/EditMode/`)**: Fast, pure logic unit tests executing in isolation with zero scene dependencies.
- **PlayMode Tests (`Tests/PlayMode/`)**: Integration tests verifying reel animation lifecycle, coroutines, state transitions, and UI event firing.

---

## 2. Requirement Verification Matrix

| # | Test Case Description | Target Class | Test Method Name | Mode | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Three Lucky 7 symbols produce a win | `WinningEvaluator` | `Evaluate_ThreeLucky7_ReturnsWin` | EditMode | Verified |
| 2 | Three Cherries produce a win | `WinningEvaluator` | `Evaluate_ThreeCherries_ReturnsWin` | EditMode | Verified |
| 3 | Three Bells produce a win | `WinningEvaluator` | `Evaluate_ThreeBells_ReturnsWin` | EditMode | Verified |
| 4 | Three BAR symbols produce a win | `WinningEvaluator` | `Evaluate_ThreeBARs_ReturnsWin` | EditMode | Verified |
| 5 | Different symbols produce a loss | `WinningEvaluator` | `Evaluate_DifferentSymbols_ReturnsLoss` | EditMode | Verified |
| 6 | Correct payout multiplier selected | `PayoutCalculator` | `CalculatePayout_MatchingSymbols_YieldsConfiguredPayout` | EditMode | Verified |
| 7 | Bet deduction reduces balance | `WalletManager` | `TryDeductBet_SufficientBalance_ReducesBalance` | EditMode | Verified |
| 8 | Insufficient balance prevents spin | `WalletManager` | `TryDeductBet_InsufficientBalance_ReturnsFalseAndPreservesBalance` | EditMode | Verified |
| 9 | Wallet balance never becomes negative | `WalletManager` | `Deduct_ExceedingAmount_NeverProducesNegativeBalance` | EditMode | Verified |
| 10 | RNG produces valid uniform indices | `RandomSymbolProvider` | `NextSymbolIndex_OverSampledRange_AlwaysWithinValidBounds` | EditMode | Verified |
| 11 | Deterministic RNG seed repeatability | `RandomSymbolProvider` | `FixedSeed_GeneratesIdenticalSequence` | EditMode | Verified |
| 12 | Concurrent spin rejection | `SlotMachineController`| `RequestSpin_WhileAlreadySpinning_IsRejected` | PlayMode | Verified |
| 13 | Single payout execution guarantee | `SlotMachineController`| `AwardPayout_FiresExactlyOncePerSpin` | PlayMode | Verified |
| 14 | Reset restores default state | `WalletManager` | `ResetWallet_RestoresConfiguredDefaults` | EditMode | Verified |

---

## 3. Running Automated Tests

### Via Unity Editor GUI
1. Open the project in Unity 6.
2. Navigate to **Window > General > Test Runner**.
3. Select the **EditMode** or **PlayMode** tab.
4. Click **Run All** to execute the suite.

### Via Command Line (Batch Mode)
Execute the following command in PowerShell:
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe" `
  -batchmode `
  -projectPath "C:\Users\rahul\Code\SlotMachineGame" `
  -runTests `
  -testPlatform EditMode `
  -testResults "Tests/EditModeResults.xml" `
  -logFile "Tests/TestLog.log"
```

Results are saved in NUnit XML format for CI/CD integration.
