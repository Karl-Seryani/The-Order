using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// A door that requires a specific key in the player's inventory to unlock.
    /// Delegates to DoorController for open/close once unlocked.
    /// Hunter cannot open locked doors.
    /// </summary>
    [RequireComponent(typeof(DoorController))]
    public class LockedDoor : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Lock Settings")]
        [SerializeField] private Items.ItemData _requiredItem;
        [SerializeField] private string _lockedPrompt = "Locked";

        #endregion

        #region Private Fields

        private DoorController _doorController;
        private bool _isUnlocked;

        #endregion

        #region Public API

        /// <summary>True if this door has been unlocked.</summary>
        public bool IsUnlocked => _isUnlocked;

        /// <summary>The item required to unlock this door.</summary>
        public Items.ItemData RequiredItem => _requiredItem;

        /// <summary>
        /// Unlock this door without an item. Used for external unlock triggers.
        /// </summary>
        public void ForceUnlock()
        {
            _isUnlocked = true;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _doorController = GetComponent<DoorController>();
        }

        #endregion

        #region IInteractable

        /// <summary>Checks inventory for key, then delegates to DoorController.</summary>
        public void Interact(GameObject interactor)
        {
            if (!_isUnlocked)
            {
                var inventory = Player.PlayerInventory.Instance;

                if (inventory != null && inventory.HasKey(_requiredItem))
                {
                    _isUnlocked = true;
                    GameEvents.DoorUnlocked(_requiredItem, transform.position);
                    GameEvents.ItemUsed(_requiredItem);
                }
                else
                {
                    GameEvents.LockedDoorAttempt(_requiredItem);
                    return;
                }
            }

            _doorController.Interact(interactor);
        }

        /// <summary>Returns context-aware prompt text.</summary>
        public string GetPromptText()
        {
            if (!_isUnlocked)
            {
                var inventory = Player.PlayerInventory.Instance;

                if (inventory != null && inventory.HasKey(_requiredItem))
                    return $"Unlock with {_requiredItem.DisplayName}";

                if (_requiredItem != null)
                    return $"{_lockedPrompt} — requires {_requiredItem.DisplayName}";

                return _lockedPrompt;
            }

            return _doorController.GetPromptText();
        }

        #endregion
    }
}
