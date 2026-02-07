using NUnit.Framework;

namespace TheOrder.Tests.EditMode
{
    /// <summary>
    /// Smoke test to verify the test runner is working correctly.
    /// </summary>
    public class SmokeTest
    {
        [Test]
        public void TestRunner_Works()
        {
            Assert.Pass("Test runner is operational.");
        }

        [Test]
        public void Enums_GameState_HasExpectedValues()
        {
            Assert.AreEqual(5, System.Enum.GetValues(typeof(GameState)).Length);
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.MainMenu));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Prologue));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Playing));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Paused));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Ending));
        }

        [Test]
        public void Enums_EndingType_HasNineEndings()
        {
            Assert.AreEqual(9, System.Enum.GetValues(typeof(EndingType)).Length);
        }

        [Test]
        public void Enums_ClueCategory_HasTwoCategories()
        {
            Assert.AreEqual(2, System.Enum.GetValues(typeof(ClueCategory)).Length);
        }

        [Test]
        public void Enums_HunterState_HasFourStates()
        {
            Assert.AreEqual(4, System.Enum.GetValues(typeof(HunterState)).Length);
        }
    }
}
