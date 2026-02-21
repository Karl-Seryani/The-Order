using System;
using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Central static event bus. All system communication flows through here.
    /// No direct references between systems — subscribe/publish only.
    /// </summary>
    public static class GameEvents
    {
        #region Game State

        /// <summary>Fired when the game state changes (menu, playing, paused, etc).</summary>
        public static event Action<GameState> OnGameStateChanged;
        public static void GameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);

        #endregion

        #region Player

        /// <summary>Fired every frame the player moves. Includes position and speed magnitude.</summary>
        public static event Action<Vector3, float> OnPlayerMoved;
        public static void PlayerMoved(Vector3 position, float speed) => OnPlayerMoved?.Invoke(position, speed);

        /// <summary>Fired when the flashlight is toggled. True = on.</summary>
        public static event Action<bool> OnFlashlightToggled;
        public static void FlashlightToggled(bool isOn) => OnFlashlightToggled?.Invoke(isOn);

        /// <summary>Fired every frame with the player's facing direction. Used for flashlight cone detection.</summary>
        public static event Action<Vector3> OnPlayerFacingChanged;
        public static void PlayerFacingChanged(Vector3 forward) => OnPlayerFacingChanged?.Invoke(forward);

        #endregion

        #region Wake-Up

        /// <summary>Fired when the wake-up blink sequence starts. Systems should disable input/HUD/Hunter.</summary>
        public static event Action OnWakeUpStarted;
        public static void WakeUpStarted() => OnWakeUpStarted?.Invoke();

        /// <summary>Fired when the wake-up blink sequence completes. Systems should re-enable.</summary>
        public static event Action OnWakeUpCompleted;
        public static void WakeUpCompleted() => OnWakeUpCompleted?.Invoke();

        #endregion

        #region Hunter

        /// <summary>Fired when the Hunter's FSM transitions to a new state.</summary>
        public static event Action<HunterState> OnHunterStateChanged;
        public static void HunterStateChanged(HunterState newState) => OnHunterStateChanged?.Invoke(newState);

        /// <summary>Fired when the Hunter detects the player (enters chase).</summary>
        public static event Action OnPlayerDetected;
        public static void PlayerDetected() => OnPlayerDetected?.Invoke();

        /// <summary>Fired when the Hunter loses track of the player.</summary>
        public static event Action OnPlayerLost;
        public static void PlayerLost() => OnPlayerLost?.Invoke();

        /// <summary>Fired when the Hunter catches the player. Triggers death screen.</summary>
        public static event Action OnPlayerCaught;
        public static void PlayerCaught() => OnPlayerCaught?.Invoke();

        #endregion

        #region Clues

        /// <summary>Fired when a clue is viewed (first interaction). Passes the clue data for display.</summary>
        public static event Action<ClueData> OnClueViewed;
        public static void ClueViewed(ClueData clue) => OnClueViewed?.Invoke(clue);

        /// <summary>Fired when a clue is collected. Passes the clue data.</summary>
        public static event Action<ClueData> OnClueCollected;
        public static void ClueCollected(ClueData clue) => OnClueCollected?.Invoke(clue);

        #endregion

        #region Doors

        /// <summary>Fired when a locked door is unlocked. Passes item used and door position.</summary>
        public static event Action<Items.ItemData, Vector3> OnDoorUnlocked;
        public static void DoorUnlocked(Items.ItemData item, Vector3 position) => OnDoorUnlocked?.Invoke(item, position);

        /// <summary>Fired when the player tries a locked door without the correct item. For UI feedback.</summary>
        public static event Action<Items.ItemData> OnLockedDoorAttempt;
        public static void LockedDoorAttempt(Items.ItemData requiredItem) => OnLockedDoorAttempt?.Invoke(requiredItem);

        #endregion

        #region Interaction Noise

        /// <summary>Fired during interactions (doors, furniture). Loudness 0-1 scales with interaction speed.</summary>
        public static event Action<Vector3, float> OnInteractableNoise;
        public static void InteractableNoise(Vector3 position, float loudness) => OnInteractableNoise?.Invoke(position, loudness);

        #endregion

        #region Environment

        /// <summary>Fired when a door is opened. Passes door world position for sound propagation.</summary>
        public static event Action<Vector3> OnDoorOpened;
        public static void DoorOpened(Vector3 position) => OnDoorOpened?.Invoke(position);

        /// <summary>Fired when a door is closed. Passes door world position for sound propagation.</summary>
        public static event Action<Vector3> OnDoorClosed;
        public static void DoorClosed(Vector3 position) => OnDoorClosed?.Invoke(position);

        #endregion

        #region UI

        /// <summary>Fired when the current objective text changes.</summary>
        public static event Action<string> OnObjectiveChanged;
        public static void ObjectiveChanged(string objectiveText) => OnObjectiveChanged?.Invoke(objectiveText);

        #endregion

        #region Items

        /// <summary>Fired when the player picks up an item into their hand.</summary>
        public static event Action<Items.ItemData> OnItemPickedUp;
        public static void ItemPickedUp(Items.ItemData item) => OnItemPickedUp?.Invoke(item);

        /// <summary>Fired when the player drops a held item. Passes item data and world position.</summary>
        public static event Action<Items.ItemData, Vector3> OnItemDropped;
        public static void ItemDropped(Items.ItemData item, Vector3 position) => OnItemDropped?.Invoke(item, position);

        /// <summary>Fired when a held item is used on a receiver (ToolReceiver or LockedDoor).</summary>
        public static event Action<Items.ItemData> OnItemUsed;
        public static void ItemUsed(Items.ItemData item) => OnItemUsed?.Invoke(item);

        #endregion

        #region Endings

        /// <summary>Fired when an ending is triggered. Passes ending data.</summary>
        public static event Action<EndingData> OnEndingTriggered;
        public static void EndingTriggered(EndingData ending) => OnEndingTriggered?.Invoke(ending);

        #endregion
    }
}
