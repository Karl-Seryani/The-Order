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

        /// <summary>Fired when the player begins sprinting.</summary>
        public static event Action OnPlayerSprinted;
        public static void PlayerSprinted() => OnPlayerSprinted?.Invoke();

        /// <summary>Fired when the flashlight is toggled. True = on.</summary>
        public static event Action<bool> OnFlashlightToggled;
        public static void FlashlightToggled(bool isOn) => OnFlashlightToggled?.Invoke(isOn);

        #endregion

        #region Interaction

        /// <summary>Fired when the player interacts with any IInteractable.</summary>
        public static event Action<IInteractable> OnInteraction;
        public static void Interaction(IInteractable target) => OnInteraction?.Invoke(target);

        #endregion

        #region Hunter

        /// <summary>Fired when Mike's FSM transitions to a new state.</summary>
        public static event Action<HunterState> OnHunterStateChanged;
        public static void HunterStateChanged(HunterState newState) => OnHunterStateChanged?.Invoke(newState);

        /// <summary>Fired when Mike detects the player (detection meter full).</summary>
        public static event Action OnPlayerDetected;
        public static void PlayerDetected() => OnPlayerDetected?.Invoke();

        /// <summary>Fired when Mike loses track of the player.</summary>
        public static event Action OnPlayerLost;
        public static void PlayerLost() => OnPlayerLost?.Invoke();

        #endregion

        #region Sanity

        /// <summary>Fired when sanity value changes. Args: current, max.</summary>
        public static event Action<float, float> OnSanityChanged;
        public static void SanityChanged(float current, float max) => OnSanityChanged?.Invoke(current, max);

        /// <summary>Fired when sanity hits zero — triggers teleport and disorientation.</summary>
        public static event Action OnSanityBreak;
        public static void SanityBreak() => OnSanityBreak?.Invoke();

        #endregion

        #region Clues

        /// <summary>Fired when a clue is viewed (first interaction). Passes the clue data for display.</summary>
        public static event Action<ClueData> OnClueViewed;
        public static void ClueViewed(ClueData clue) => OnClueViewed?.Invoke(clue);

        /// <summary>Fired when a clue is collected. Passes the clue data.</summary>
        public static event Action<ClueData> OnClueCollected;
        public static void ClueCollected(ClueData clue) => OnClueCollected?.Invoke(clue);

        #endregion

        #region Environment

        /// <summary>Fired when a door is opened. Passes door world position for sound propagation.</summary>
        public static event Action<Vector3> OnDoorOpened;
        public static void DoorOpened(Vector3 position) => OnDoorOpened?.Invoke(position);

        /// <summary>Fired when a door is closed. Passes door world position for sound propagation.</summary>
        public static event Action<Vector3> OnDoorClosed;
        public static void DoorClosed(Vector3 position) => OnDoorClosed?.Invoke(position);

        /// <summary>Fired when the player enters a hiding spot.</summary>
        public static event Action OnPlayerHid;
        public static void PlayerHid() => OnPlayerHid?.Invoke();

        /// <summary>Fired when the player exits a hiding spot.</summary>
        public static event Action OnPlayerExitedHiding;
        public static void PlayerExitedHiding() => OnPlayerExitedHiding?.Invoke();

        /// <summary>Fired when the player fails the breath-hold QTE while hiding.</summary>
        public static event Action OnBreathHoldFailed;
        public static void BreathHoldFailed() => OnBreathHoldFailed?.Invoke();

        #endregion

        #region UI

        /// <summary>Fired when the current objective text changes.</summary>
        public static event Action<string> OnObjectiveChanged;
        public static void ObjectiveChanged(string objectiveText) => OnObjectiveChanged?.Invoke(objectiveText);

        #endregion

        #region Endings

        /// <summary>Fired when an ending is triggered. Passes ending data.</summary>
        public static event Action<EndingData> OnEndingTriggered;
        public static void EndingTriggered(EndingData ending) => OnEndingTriggered?.Invoke(ending);

        #endregion
    }
}
