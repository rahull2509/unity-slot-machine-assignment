using NUnit.Framework;
using SlotMachineGame.Gameplay;

namespace SlotMachineGame.Tests
{
    [TestFixture]
    public class PayoutCalculatorTests
    {
        private PayoutCalculator calculator;

        [SetUp]
        public void SetUp()
        {
            calculator = new PayoutCalculator();
        }

        [Test]
        public void GetMultiplier_Lucky7_Returns10()
        {
            Assert.AreEqual(10, calculator.GetMultiplier(1));
        }

        [Test]
        public void GetMultiplier_Cherries_Returns5()
        {
            Assert.AreEqual(5, calculator.GetMultiplier(2));
        }

        [Test]
        public void GetMultiplier_Bell_Returns4()
        {
            Assert.AreEqual(4, calculator.GetMultiplier(3));
        }

        [Test]
        public void GetMultiplier_BAR_Returns3()
        {
            Assert.AreEqual(3, calculator.GetMultiplier(4));
        }

        [Test]
        public void CalculatePayout_WithBet10_CalculatesExpectedPayouts()
        {
            int bet = 10;
            Assert.AreEqual(100, calculator.CalculatePayout(1, bet)); // 10 * 10
            Assert.AreEqual(50, calculator.CalculatePayout(2, bet));  // 10 * 5
            Assert.AreEqual(40, calculator.CalculatePayout(3, bet));  // 10 * 4
            Assert.AreEqual(30, calculator.CalculatePayout(4, bet));  // 10 * 3
        }

        [Test]
        public void CalculatePayout_WithBet50_CalculatesExpectedPayouts()
        {
            int bet = 50;
            Assert.AreEqual(500, calculator.CalculatePayout(1, bet));
            Assert.AreEqual(250, calculator.CalculatePayout(2, bet));
            Assert.AreEqual(200, calculator.CalculatePayout(3, bet));
            Assert.AreEqual(150, calculator.CalculatePayout(4, bet));
        }

        [Test]
        public void CalculatePayout_ZeroOrNegativeBet_ReturnsZero()
        {
            Assert.AreEqual(0, calculator.CalculatePayout(1, 0));
            Assert.AreEqual(0, calculator.CalculatePayout(1, -20));
        }

        [Test]
        public void CalculatePayout_UnknownSymbol_ReturnsZero()
        {
            Assert.AreEqual(0, calculator.CalculatePayout(999, 10));
        }
    }
}
