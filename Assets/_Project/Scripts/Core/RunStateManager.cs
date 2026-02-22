using System.Collections.Generic;
using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Tracks the current run state across deaths within a 3-day run.
    /// Persists which objects have been used/unlocked/installed so progress
    /// carries over between deaths. Resets on new game from main menu.
    /// </summary>
    public class RunStateManager : MonoBehaviour
    {
        #region Singleton

        public static RunStateManager Instance { get; private set; }

        #endregion

        #region Constants

        public const int MAX_DAYS = 3;

        #endregion

        #region Private Fields

        private int _currentDay = 1;
        private readonly HashSet<string> _usedToolReceivers = new();
        private readonly HashSet<string> _unscrewedScrews = new();
        private readonly HashSet<string> _unlockedDoors = new();
        private readonly Dictionary<string, CarPartState> _carParts = new();
        private readonly Dictionary<string, Vector3> _itemDropPositions = new();
        private readonly HashSet<string> _consumedKeys = new();

        // Hunter persistence
        private Vector3? _hunterPosition;
        private int _hunterWaypointIndex;

        #endregion

        #region Public API

        /// <summary>Current day in the run (1-based).</summary>
        public int CurrentDay => _currentDay;

        /// <summary>True when the player has exhausted all lives.</summary>
        public bool IsGameOver => _currentDay > MAX_DAYS;

        /// <summary>Advance to the next day after a death.</summary>
        public void AdvanceDay()
        {
            _currentDay++;
        }

        /// <summary>Reset all run state for a fresh game.</summary>
        public void ResetRun()
        {
            _currentDay = 1;
            _usedToolReceivers.Clear();
            _unscrewedScrews.Clear();
            _unlockedDoors.Clear();
            _carParts.Clear();
            _itemDropPositions.Clear();
            _consumedKeys.Clear();
            _hunterPosition = null;
        }

        // --- ToolReceiver ---
        public void MarkToolReceiverUsed(string id) => _usedToolReceivers.Add(id);
        public bool IsToolReceiverUsed(string id) => _usedToolReceivers.Contains(id);

        // --- Screws ---
        public void MarkScrewUnscrewed(string id) => _unscrewedScrews.Add(id);
        public bool IsScrewUnscrewed(string id) => _unscrewedScrews.Contains(id);

        // --- Doors ---
        public void MarkDoorUnlocked(string id) => _unlockedDoors.Add(id);
        public bool IsDoorUnlocked(string id) => _unlockedDoors.Contains(id);

        // --- Car Parts ---
        public void SetCarPartState(string id, CarPartState state) => _carParts[id] = state;
        public CarPartState GetCarPartState(string id)
        {
            return _carParts.TryGetValue(id, out var state) ? state : CarPartState.None;
        }

        // --- Item Positions ---
        public void SaveItemDropPosition(string id, Vector3 position) => _itemDropPositions[id] = position;
        public bool TryGetItemDropPosition(string id, out Vector3 position) => _itemDropPositions.TryGetValue(id, out position);
        public void ClearItemDropPosition(string id) => _itemDropPositions.Remove(id);

        // --- Consumed Keys ---
        public void MarkKeyConsumed(string id) => _consumedKeys.Add(id);
        public bool IsKeyConsumed(string id) => _consumedKeys.Contains(id);

        // --- Hunter State ---
        public void SaveHunterState(Vector3 position, int waypointIndex)
        {
            _hunterPosition = position;
            _hunterWaypointIndex = waypointIndex;
        }
        public Vector3? GetHunterPosition() => _hunterPosition;
        public int GetHunterWaypointIndex() => _hunterWaypointIndex;
        public void ClearHunterState() => _hunterPosition = null;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion
    }
}
