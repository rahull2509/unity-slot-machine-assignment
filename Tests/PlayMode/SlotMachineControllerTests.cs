using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SlotMachineGame.Core;
using SlotMachineGame.Data;
using SlotMachineGame.Gameplay;
using SlotMachineGame.Reels;
using SlotMachineGame.UI;

namespace SlotMachineGame.Tests
{
    [TestFixture]
    public class SlotMachineControllerTests
    {
        private GameObject rootGo;
        private SlotMachineController controller;
        private WalletManager wallet;
        private SymbolDatabase database;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            rootGo = new GameObject("TestSlotMachineRoot");
            wallet = rootGo.AddComponent<WalletManager>();
            wallet.Initialize(1000, 10);

            controller = rootGo.AddComponent<SlotMachineController>();

            // Create in-memory SymbolDatabase
            database = ScriptableObject.CreateInstance<SymbolDatabase>();
            var s1 = new SymbolData(1, "7", null, 10);
            var s2 = new SymbolData(2, "Cherries", null, 5);
            var s3 = new SymbolData(3, "Bell", null, 4);
            var s4 = new SymbolData(4, "BAR", null, 3);
            database.SetSymbols(new[] { s1, s2, s3, s4 });

            // Create mock reels
            var reels = new ReelController[3];
            for (int i = 0; i < 3; i++)
            {
                var reelGo = new GameObject($"Reel_{i}");
                reelGo.transform.SetParent(rootGo.transform);
                var rc = reelGo.AddComponent<ReelController>();
                // Add dummy symbol views
                for (int v = 0; v < 3; v++)
                {
                    var viewGo = new GameObject($"View_{v}");
                    viewGo.transform.SetParent(reelGo.transform);
                    viewGo.AddComponent<RectTransform>();
                    viewGo.AddComponent<ReelSymbolView>();
                }
                rc.Initialize(i, database, 1);
                reels[i] = rc;
            }

            // Set private serialized fields via reflection or helper
            SetPrivateField(controller, "walletManager", wallet);
            SetPrivateField(controller, "symbolDatabase", database);
            SetPrivateField(controller, "reels", reels);

            controller.Initialize(new RandomSymbolProvider(42));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (rootGo != null)
            {
                Object.Destroy(rootGo);
            }
            if (database != null)
            {
                Object.Destroy(database);
            }
            yield return null;
        }

        [Test]
        public void InitialState_IsIdle()
        {
            Assert.AreEqual(GameState.Idle, controller.CurrentState);
        }

        [Test]
        public void RequestSpin_WithSufficientBalance_StartsSpinAndDeductsBet()
        {
            int startingBalance = wallet.CurrentBalance;
            int bet = wallet.CurrentBet;

            bool accepted = controller.RequestSpin();

            Assert.IsTrue(accepted);
            Assert.AreEqual(GameState.Spinning, controller.CurrentState);
            Assert.AreEqual(startingBalance - bet, wallet.CurrentBalance);
        }

        [Test]
        public void RequestSpin_WhileAlreadySpinning_IsRejected()
        {
            bool first = controller.RequestSpin();
            Assert.IsTrue(first);

            // Attempt secondary spin while actively spinning
            bool second = controller.RequestSpin();
            Assert.IsFalse(second, "Second spin request while spinning must be rejected.");
        }

        [Test]
        public void RequestSpin_WithInsufficientBalance_IsRejected()
        {
            wallet.Initialize(5, 10); // Balance 5, Bet 10
            bool accepted = controller.RequestSpin();

            Assert.IsFalse(accepted);
            Assert.AreEqual(GameState.InsufficientFunds, controller.CurrentState);
            Assert.AreEqual(5, wallet.CurrentBalance);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
