using System.Collections.Generic;
using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Tracks collected keys. Singleton accessible via PlayerInventory.Instance.
    /// Listens to GameEvents.OnKeyCollected to auto-add keys.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Singleton

        /// <summary>Current inventory instance.</summary>
        public static PlayerInventory Instance { get; private set; }

        #endregion

        #region Private Fields

        private readonly HashSet<string> _collectedKeyIds = new HashSet<string>();

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

        private void OnEnable()
        {
            GameEvents.OnKeyCollected += HandleKeyCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnKeyCollected -= HandleKeyCollected;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region Public API

        /// <summary>Check if the player has a specific key.</summary>
        public bool HasKey(Doors.KeyData key)
        {
            if (key == null) return false;
            return _collectedKeyIds.Contains(key.Id);
        }

        /// <summary>Total number of keys collected.</summary>
        public int KeyCount => _collectedKeyIds.Count;

        #endregion

        #region Event Handlers

        private void HandleKeyCollected(Doors.KeyData key)
        {
            if (key == null) return;
            _collectedKeyIds.Add(key.Id);
        }

        #endregion
    }
}
