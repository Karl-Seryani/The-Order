using System.Collections.Generic;
using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Stores pocket items (keys) that don't occupy the player's hand.
    /// Keys are picked up silently and used automatically at locked doors.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Singleton

        /// <summary>Current inventory instance.</summary>
        public static PlayerInventory Instance { get; private set; }

        #endregion

        #region Private Fields

        /// <summary>Static so key data survives scene reload (respawn). Cleared on new game.</summary>
        private static readonly HashSet<Items.ItemData> _keys = new();

        #endregion

        #region Public API

        /// <summary>Add a key to the inventory.</summary>
        public void AddKey(Items.ItemData key)
        {
            if (key == null) return;
            _keys.Add(key);
        }

        /// <summary>Check if the player has a specific key.</summary>
        public bool HasKey(Items.ItemData key)
        {
            return key != null && _keys.Contains(key);
        }

        /// <summary>Clear all keys. Used when resetting a run from DeathScreenUI.</summary>
        public static void ClearKeys()
        {
            _keys.Clear();
        }

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

            // Clear keys on fresh game start (Day 1 of a new run)
            // Keys persist between deaths within a run (Day 2, Day 3)
            if (RunStateManager.Instance != null)
            {
                if (RunStateManager.Instance.CurrentDay <= 1)
                    _keys.Clear();
            }
            else if (GameManager.Instance == null || !GameManager.Instance.SkipWakeUpSequence)
            {
                _keys.Clear();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion
    }
}
