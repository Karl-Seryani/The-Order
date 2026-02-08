using NUnit.Framework;
using UnityEngine;
using TheOrder.Hunter;

namespace TheOrder.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for Hunter detection math — sight cone and hearing range.
    /// Tests pure static methods that don't require a running scene.
    /// </summary>
    public class HunterDetectionTests
    {
        #region Sight Cone Tests

        [Test]
        public void IsInSightCone_DirectlyAhead_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 10);

            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInSightCone_Behind_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, -5);

            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsInSightCone_AtEdgeOfAngle_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            // Place player at 60 degrees — outside 110-degree cone (55 per side)
            float angle = 60f * Mathf.Deg2Rad;
            Vector3 playerPos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 10f;

            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsInSightCone_WithinAngle_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            // Place player at 40 degrees — inside 110-degree cone (55 per side)
            float angle = 40f * Mathf.Deg2Rad;
            Vector3 playerPos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 10f;

            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInSightCone_BeyondRange_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 20);

            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsInSightCone_AtExactRange_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 15);

            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInSightCone_FlashlightMultipliedRange_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 12);

            // Without flashlight: 3m base range → false at 12m
            bool withoutFlashlight = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 3f, 110f);
            Assert.IsFalse(withoutFlashlight);

            // With flashlight: 3m * 8x = 24m range → true at 12m
            bool withFlashlight = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 24f, 110f);
            Assert.IsTrue(withFlashlight);
        }

        [Test]
        public void IsInSightCone_DarkVision_Within3m_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 2.5f);

            // In darkness (no flashlight): 3m base range, player at 2.5m → true
            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 3f, 110f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInSightCone_DarkVision_Beyond3m_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 5f);

            // In darkness (no flashlight): 3m base range, player at 5m → false
            bool result = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 3f, 110f);

            Assert.IsFalse(result);
        }

        #endregion

        #region Hearing Range Tests

        [Test]
        public void IsInHearingRange_Sprinting_Within8m_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 6);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 8f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInHearingRange_Sprinting_Beyond8m_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 10);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 8f);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsInHearingRange_Walking_Within2m_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 1.5f);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 2f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInHearingRange_Walking_Beyond2m_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 5);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 2f);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsInHearingRange_DoorOpen_Within15m_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 12);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 15f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInHearingRange_DoorOpen_Beyond15m_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 20);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 15f);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsInHearingRange_Sprinting_AtExactRange_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 8);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 8f);

            Assert.IsTrue(result);
        }

        #endregion

        #region Flashlight Detection Tests

        [Test]
        public void Flashlight_PlayerAimingAtHunter_ReturnsTrue()
        {
            // Player aiming flashlight directly at Hunter
            Vector3 hunterPos = new Vector3(0, 0, 10);
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.forward;

            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsTrue(result);
        }

        [Test]
        public void Flashlight_PlayerBehindHunter_AimingAtBack_ReturnsTrue()
        {
            // Player behind Hunter, shining light at Hunter's back — still detected!
            // Hunter faces +Z, player is behind at -Z shining forward
            Vector3 hunterPos = new Vector3(0, 0, 10);
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.forward;

            // Hunter facing doesn't matter — flashlight hits them from behind
            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsTrue(result);
        }

        [Test]
        public void Flashlight_PlayerNotAimingAtHunter_ReturnsFalse()
        {
            // Player aiming flashlight away from Hunter
            Vector3 hunterPos = new Vector3(0, 0, 10);
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.back; // Aiming away from Hunter

            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsFalse(result);
        }

        [Test]
        public void Flashlight_HunterOutsideConeAngle_ReturnsFalse()
        {
            // Hunter is to the side, outside the 60° flashlight cone
            Vector3 hunterPos = new Vector3(10, 0, 1); // Almost perpendicular
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.forward;

            // Angle to Hunter ≈ 84° — outside 30° half-cone
            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsFalse(result);
        }

        [Test]
        public void Flashlight_BeyondRange_ReturnsFalse()
        {
            // Hunter too far away even with flashlight range
            Vector3 hunterPos = new Vector3(0, 0, 30);
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.forward;

            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsFalse(result);
        }

        [Test]
        public void Flashlight_AtEdgeOfCone_ReturnsTrue()
        {
            // Hunter at edge of flashlight cone (29° from center, cone is 30° half)
            float angle = 29f * Mathf.Deg2Rad;
            Vector3 hunterPos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 10f;
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.forward;

            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsTrue(result);
        }

        [Test]
        public void Flashlight_JustOutsideCone_ReturnsFalse()
        {
            // Hunter just outside flashlight cone (31° from center, cone is 30° half)
            float angle = 31f * Mathf.Deg2Rad;
            Vector3 hunterPos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 10f;
            Vector3 playerPos = Vector3.zero;
            Vector3 playerForward = Vector3.forward;

            bool result = HunterAI.IsFlashlightHittingTarget(
                hunterPos, playerPos, playerForward, 60f, 24f);

            Assert.IsFalse(result);
        }

        #endregion
    }
}
