using NUnit.Framework;
using UnityEngine;
using TheOrder.Doors;

namespace TheOrder.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for DoorController state and prompt logic.
    /// Animation tests require PlayMode (coroutines).
    /// </summary>
    [TestFixture]
    public class DoorControllerTests
    {
        [Test]
        public void IsOpen_Initially_ReturnsFalse()
        {
            var go = new GameObject("DoorTest");
            var door = go.AddComponent<DoorController>();

            Assert.IsFalse(door.IsOpen);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GetPromptText_WhenClosed_ReturnsOpenDoor()
        {
            var go = new GameObject("DoorTest");
            var door = go.AddComponent<DoorController>();

            Assert.AreEqual("Open Door", door.GetPromptText());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void IsAnimating_Initially_ReturnsFalse()
        {
            var go = new GameObject("DoorTest");
            var door = go.AddComponent<DoorController>();

            Assert.IsFalse(door.IsAnimating);

            Object.DestroyImmediate(go);
        }
    }
}
