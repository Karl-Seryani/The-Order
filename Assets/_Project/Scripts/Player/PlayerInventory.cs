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

        private readonly HashSet<Items.ItemData> _keys = new();

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

        /// <summary>Remove a key after use (optional — keys persist by default).</summary>
        public void RemoveKey(Items.ItemData key)
        {
            _keys.Remove(key);
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
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion
    }
}
