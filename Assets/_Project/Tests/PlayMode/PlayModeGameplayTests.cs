using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TheOrder;
using TheOrder.Clues;
using TheOrder.Player;
using TheOrder.UI;

namespace TheOrder.Tests.PlayMode
{
    /// <summary>
    /// Minimal input handler stub to satisfy PlayerInteraction without InputSystem setup.
    /// </summary>
    public class TestInputHandler : PlayerInputHandler
    {
        private new void Awake() { }
        private new void Update() { }
    }

    /// <summary>
    /// PlayMode tests for core gameplay flows that can regress in runtime.
    /// </summary>
    public class PlayModeGameplayTests
    {
        #region Helpers

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().Name, fieldName);
            field.SetValue(target, value);
        }

        private static void ClearGameManagerInstance()
        {
            var instanceProp = typeof(GameManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var instance = instanceProp?.GetValue(null) as GameManager;
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
            }

            var backingField = typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
            backingField?.SetValue(null, null);
        }

        private static GameManager CreateGameManager()
        {
            var go = new GameObject("GameManager_Test");
            return go.AddComponent<GameManager>();
        }

        #endregion

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearGameManagerInstance();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ClearGameManagerInstance();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameManager_TogglePause_UpdatesStateAndTimeScale()
        {
            var gm = CreateGameManager();
            gm.SetState(GameState.Playing);

            gm.TogglePause();
            Assert.AreEqual(GameState.Paused, gm.CurrentState);
            Assert.AreEqual(0f, Time.timeScale, 0.0001f);

            gm.TogglePause();
            Assert.AreEqual(GameState.Playing, gm.CurrentState);
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CluePickup_EmptyId_DoesNotDestroy()
        {
            var go = new GameObject("CluePickup_Test");
            var pickup = go.AddComponent<CluePickup>();

            var clue = ScriptableObject.CreateInstance<ClueData>();
            SetPrivateField(clue, "_id", string.Empty);
            SetPrivateField(clue, "_title", "Test Clue");
            SetPrivateField(clue, "_category", ClueCategory.Truth);
            SetPrivateField(pickup, "_clueData", clue);

            pickup.Interact(go); // read
            pickup.Interact(go); // attempt collect

            yield return null;

            Assert.IsFalse(pickup == null, "CluePickup should not be destroyed when ID is empty.");
        }

        [UnityTest]
        public IEnumerator CluePickup_ValidId_DestroysOnCollect()
        {
            var go = new GameObject("CluePickup_Test_Valid");
            var pickup = go.AddComponent<CluePickup>();

            var clue = ScriptableObject.CreateInstance<ClueData>();
            SetPrivateField(clue, "_id", "clue_01");
            SetPrivateField(clue, "_title", "Test Clue");
            SetPrivateField(clue, "_category", ClueCategory.Truth);
            SetPrivateField(pickup, "_clueData", clue);

            pickup.Interact(go); // read
            pickup.Interact(go); // collect

            yield return null;

            Assert.IsTrue(pickup == null, "CluePickup should be destroyed when collected with a valid ID.");
        }

        [UnityTest]
        public IEnumerator PlayerInteraction_NoCamera_DoesNotThrow()
        {
            var go = new GameObject("PlayerInteraction_Test");
            go.AddComponent<TestInputHandler>();
            var interaction = go.AddComponent<PlayerInteraction>();

            // Allow one frame for Update to run
            yield return null;

            Assert.IsFalse(interaction.HasTarget);
        }

        [UnityTest]
        public IEnumerator HUDManager_HidesPromptWhenNotPlaying()
        {
            var gm = CreateGameManager();
            gm.SetState(GameState.Paused);

            var hudGo = new GameObject("HUDManager_Test");
            var hud = hudGo.AddComponent<HUDManager>();

            var promptPanel = new GameObject("InteractionPromptPanel_Test");
            promptPanel.SetActive(true);
            SetPrivateField(hud, "_interactionPromptPanel", promptPanel);

            yield return null;

            Assert.IsFalse(promptPanel.activeSelf, "Prompt panel should be hidden when not in Playing state.");
        }
    }
}
