using NUnit.Framework;
using SlotMachineGame.Gameplay;

namespace SlotMachineGame.Tests
{
    [TestFixture]
    public class WinningEvaluatorTests
    {
        private WinningEvaluator evaluator;

        [SetUp]
        public void SetUp()
        {
            evaluator = new WinningEvaluator();
        }

        [Test]
        public void Evaluate_ThreeLucky7_ReturnsWin()
        {
            var result = evaluator.EvaluateCenterLine(1, 1, 1);
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(1, result.WinningSymbolId);
            Assert.AreEqual(3, result.MatchCount);
        }

        [Test]
        public void Evaluate_ThreeCherries_ReturnsWin()
        {
            var result = evaluator.EvaluateCenterLine(2, 2, 2);
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(2, result.WinningSymbolId);
            Assert.AreEqual(3, result.MatchCount);
        }

        [Test]
        public void Evaluate_ThreeBells_ReturnsWin()
        {
            var result = evaluator.EvaluateCenterLine(3, 3, 3);
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(3, result.WinningSymbolId);
            Assert.AreEqual(3, result.MatchCount);
        }

        [Test]
        public void Evaluate_ThreeBARs_ReturnsWin()
        {
            var result = evaluator.EvaluateCenterLine(4, 4, 4);
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(4, result.WinningSymbolId);
            Assert.AreEqual(3, result.MatchCount);
        }

        [Test]
        public void Evaluate_DifferentSymbols_ReturnsLoss()
        {
            var result = evaluator.EvaluateCenterLine(1, 2, 3);
            Assert.IsFalse(result.IsWin);
            Assert.AreEqual(-1, result.WinningSymbolId);
        }

        [Test]
        public void Evaluate_TwoMatchingSymbols_ReturnsLoss()
        {
            var result1 = evaluator.EvaluateCenterLine(1, 1, 2);
            var result2 = evaluator.EvaluateCenterLine(2, 3, 3);
            var result3 = evaluator.EvaluateCenterLine(4, 1, 4);

            Assert.IsFalse(result1.IsWin);
            Assert.IsFalse(result2.IsWin);
            Assert.IsFalse(result3.IsWin);
        }
    }
}
