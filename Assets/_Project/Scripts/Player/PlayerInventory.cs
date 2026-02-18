using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Legacy inventory singleton. Kept for backward compatibility.
    /// Item carrying is now handled by HeldItemController.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Singleton

        /// <summary>Current inventory instance.</summary>
        public static PlayerInventory Instance { get; private set; }

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
