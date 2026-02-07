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
        public void IsInSightCone_FlashlightDoubledRange_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 hunterForward = Vector3.forward;
            Vector3 playerPos = new Vector3(0, 0, 25);

            // Without flashlight: 15m range → false
            bool withoutFlashlight = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 15f, 110f);
            Assert.IsFalse(withoutFlashlight);

            // With flashlight: 30m range → true
            bool withFlashlight = HunterAI.IsInSightCone(hunterPos, hunterForward, playerPos, 30f, 110f);
            Assert.IsTrue(withFlashlight);
        }

        #endregion

        #region Hearing Range Tests

        [Test]
        public void IsInHearingRange_Running_Within12m_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 10);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 12f);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInHearingRange_Running_Beyond12m_ReturnsFalse()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 15);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 12f);

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
        public void IsInHearingRange_AtExactRange_ReturnsTrue()
        {
            Vector3 hunterPos = Vector3.zero;
            Vector3 soundPos = new Vector3(0, 0, 12);

            bool result = HunterAI.IsInHearingRange(hunterPos, soundPos, 12f);

            Assert.IsTrue(result);
        }

        #endregion
    }
}
