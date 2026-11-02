using NUnit.Framework;
using UnityEngine;
using SlotMachineGame.Gameplay;

namespace SlotMachineGame.Tests
{
    [TestFixture]
    public class WalletManagerTests
    {
        private GameObject hostGameObject;
        private WalletManager wallet;

        [SetUp]
        public void SetUp()
        {
            hostGameObject = new GameObject("TestWalletHost");
            wallet = hostGameObject.AddComponent<WalletManager>();
            wallet.Initialize(1000, 10);
        }

        [TearDown]
        public void TearDown()
        {
            if (hostGameObject != null)
            {
                Object.DestroyImmediate(hostGameObject);
            }
        }

        [Test]
        public void InitialState_HasCorrectDefaults()
        {
            Assert.AreEqual(1000, wallet.CurrentBalance);
            Assert.AreEqual(10, wallet.CurrentBet);
            Assert.AreEqual(0, wallet.LastPayout);
        }

        [Test]
        public void TryDeductBet_SufficientBalance_ReducesBalance()
        {
            bool success = wallet.TryDeductBet();
            Assert.IsTrue(success);
            Assert.AreEqual(990, wallet.CurrentBalance);
        }

        [Test]
        public void TryDeductBet_InsufficientBalance_ReturnsFalseAndPreservesBalance()
        {
            wallet.Initialize(5, 10);
            bool success = wallet.TryDeductBet();
            Assert.IsFalse(success);
            Assert.AreEqual(5, wallet.CurrentBalance);
        }

        [Test]
        public void CreditWin_IncreasesBalanceAndRecordsLastPayout()
        {
            wallet.CreditWin(150);
            Assert.AreEqual(1150, wallet.CurrentBalance);
            Assert.AreEqual(150, wallet.LastPayout);
        }

        [Test]
        public void IncreaseBet_StepsThroughTiers()
        {
            Assert.AreEqual(10, wallet.CurrentBet);
            wallet.IncreaseBet();
            Assert.AreEqual(20, wallet.CurrentBet);
            wallet.IncreaseBet();
            Assert.AreEqual(50, wallet.CurrentBet);
        }

        [Test]
        public void DecreaseBet_StepsDownThroughTiers()
        {
            wallet.IncreaseBet(); // 20
            wallet.IncreaseBet(); // 50
            Assert.AreEqual(50, wallet.CurrentBet);

            wallet.DecreaseBet();
            Assert.AreEqual(20, wallet.CurrentBet);
            wallet.DecreaseBet();
            Assert.AreEqual(10, wallet.CurrentBet);
        }

        [Test]
        public void DecreaseBet_AtMinimum_DoesNotGoBelowMin()
        {
            Assert.AreEqual(10, wallet.CurrentBet);
            wallet.DecreaseBet();
            Assert.AreEqual(10, wallet.CurrentBet);
        }

        [Test]
        public void IncreaseBet_AtMaximum_DoesNotExceedMax()
        {
            for (int i = 0; i < 10; i++)
            {
                wallet.IncreaseBet();
            }
            Assert.AreEqual(500, wallet.CurrentBet);
        }

        [Test]
        public void ResetWallet_RestoresConfiguredDefaults()
        {
            wallet.TryDeductBet();
            wallet.CreditWin(300);
            wallet.IncreaseBet();

            wallet.ResetWallet();

            Assert.AreEqual(1000, wallet.CurrentBalance);
            Assert.AreEqual(10, wallet.CurrentBet);
            Assert.AreEqual(0, wallet.LastPayout);
        }

        [Test]
        public void CreditWin_NegativeAmount_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() => wallet.CreditWin(-50));
        }

        [Test]
        public void CreditWin_PotentialOverflow_CapsAtIntMaxValueWithoutWrapping()
        {
            wallet.Initialize(int.MaxValue - 100, 10);
            wallet.CreditWin(500);

            Assert.AreEqual(int.MaxValue, wallet.CurrentBalance);
            Assert.GreaterOrEqual(wallet.CurrentBalance, 0);
        }

        [Test]
        public void TryDeductBet_NeverAllowsNegativeBalance()
        {
            wallet.Initialize(0, 10);
            bool deducted = wallet.TryDeductBet();

            Assert.IsFalse(deducted);
            Assert.AreEqual(0, wallet.CurrentBalance);
        }

        [Test]
        public void ResetWallet_WhenInitialBalanceWasInvalid_RestoresSafeDefault()
        {
            wallet.Initialize(-100, 10);
            wallet.ResetWallet();

            Assert.AreEqual(1000, wallet.CurrentBalance);
        }
    }
}

