using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// A door that requires a specific key to unlock.
    /// Delegates to DoorController for open/close animation once unlocked.
    /// Hunter cannot open locked doors.
    /// </summary>
    [RequireComponent(typeof(DoorController))]
    public class LockedDoor : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Lock Settings")]
        [SerializeField] private KeyData _requiredKey;
        [SerializeField] private string _lockedPrompt = "Locked";

        #endregion

        #region Private Fields

        private DoorController _doorController;
        private bool _isUnlocked;

        #endregion

        #region Public API

        /// <summary>True if this door has been unlocked.</summary>
        public bool IsUnlocked => _isUnlocked;

        /// <summary>The key required to unlock this door.</summary>
        public KeyData RequiredKey => _requiredKey;

        /// <summary>
        /// Unlock this door without a key. Used for external unlock triggers.
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

        /// <summary>
        /// If locked, checks inventory for key. If unlocked, delegates to DoorController.
        /// </summary>
        public void Interact(GameObject interactor)
        {
            if (!_isUnlocked)
            {
                if (_requiredKey != null && Player.PlayerInventory.Instance != null
                    && Player.PlayerInventory.Instance.HasKey(_requiredKey))
                {
                    _isUnlocked = true;
                    GameEvents.DoorUnlocked(_requiredKey, transform.position);
                    _doorController.Interact(interactor);
                }
                else
                {
                    GameEvents.LockedDoorAttempt(_requiredKey);
                }
                return;
            }

            _doorController.Interact(interactor);
        }

        /// <summary>Returns locked prompt or delegates to DoorController.</summary>
        public string GetPromptText()
        {
            if (!_isUnlocked)
            {
                if (_requiredKey != null)
                    return $"{_lockedPrompt} — requires {_requiredKey.DisplayName}";
                return _lockedPrompt;
            }

            return _doorController.GetPromptText();
        }

        #endregion
    }
}