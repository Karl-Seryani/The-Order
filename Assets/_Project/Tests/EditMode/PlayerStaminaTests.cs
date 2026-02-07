using NUnit.Framework;
using TheOrder.Player;

namespace TheOrder.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for PlayerStamina pure math logic.
    /// Tests drain, regen, and sprint gating without requiring a running scene.
    /// </summary>
    [TestFixture]
    public class PlayerStaminaTests
    {
        #region Drain Tests

        [Test]
        public void DrainStamina_ReducesCurrentValue()
        {
            var go = new UnityEngine.GameObject("StaminaTest");
            var stamina = go.AddComponent<PlayerStamina>();

            float initial = stamina.CurrentStamina;
            stamina.DrainStamina(30f);

            Assert.AreEqual(initial - 30f, stamina.CurrentStamina, 0.01f);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void DrainStamina_ClampsAtZero()
        {
            var go = new UnityEngine.GameObject("StaminaTest");
            var stamina = go.AddComponent<PlayerStamina>();

            stamina.DrainStamina(200f);

            Assert.AreEqual(0f, stamina.CurrentStamina, 0.01f);

            UnityEngine.Object.DestroyImmediate(go);
        }

        #endregion

        #region CanSprint Tests

        [Test]
        public void CanSprint_AboveThreshold_ReturnsTrue()
        {
            var go = new UnityEngine.GameObject("StaminaTest");
            var stamina = go.AddComponent<PlayerStamina>();

            // Default stamina is 100, well above threshold of 10
            Assert.IsTrue(stamina.CanSprint);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void CanSprint_BelowThreshold_ReturnsFalse()
        {
            var go = new UnityEngine.GameObject("StaminaTest");
            var stamina = go.AddComponent<PlayerStamina>();

            // Drain to below the 10 threshold
            stamina.DrainStamina(95f);

            Assert.IsFalse(stamina.CanSprint);

            UnityEngine.Object.DestroyImmediate(go);
        }

        #endregion

        #region Regen Tests

        [Test]
        public void CalculateRegen_ReturnsPositiveValue()
        {
            float regen = PlayerStamina.CalculateRegen(1f);

            Assert.Greater(regen, 0f);
        }

        [Test]
        public void CalculateDrain_ReturnsPositiveValue()
        {
            float drain = PlayerStamina.CalculateDrain(1f);

            Assert.Greater(drain, 0f);
        }

        [Test]
        public void CalculateDrain_OneSecond_Returns20()
        {
            float drain = PlayerStamina.CalculateDrain(1f);

            Assert.AreEqual(20f, drain, 0.01f);
        }

        [Test]
        public void CalculateRegen_OneSecond_Returns10()
        {
            float regen = PlayerStamina.CalculateRegen(1f);

            Assert.AreEqual(10f, regen, 0.01f);
        }

        #endregion
    }
}
