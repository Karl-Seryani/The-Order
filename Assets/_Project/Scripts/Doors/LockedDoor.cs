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

        [Header("Audio")]
        [SerializeField] private AudioClip _unlockSound;
        [SerializeField] private AudioClip _rattleSound;
        [SerializeField] [Range(0f, 1f)] private float _soundVolume = 0.7f;

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

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _doorController = GetComponent<DoorController>();
        }

        private void Start()
        {
            RestoreRunState();
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
                    SaveRunState();
                    if (_unlockSound != null)
                        AudioSource.PlayClipAtPoint(_unlockSound, transform.position, _soundVolume);
                    GameEvents.ItemUsed(_requiredItem);
                }
                else
                {
                    if (_rattleSound != null)
                        AudioSource.PlayClipAtPoint(_rattleSound, transform.position, _soundVolume);
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
                    return $"Locked  -  need {_requiredItem.DisplayName}";

                return _lockedPrompt;
            }

            return _doorController.GetPromptText();
        }

        /// <summary>Can interact if already unlocked or player has the key.</summary>
        public bool CanInteract(GameObject interactor)
        {
            if (_isUnlocked) return true;
            var inventory = Player.PlayerInventory.Instance;
            return inventory != null && inventory.HasKey(_requiredItem);
        }

        /// <summary>Returns blocked reason when locked without key.</summary>
        public string GetBlockedMessage()
        {
            if (_isUnlocked) return "";
            if (_requiredItem != null) return $"Need {_requiredItem.DisplayName}";
            return "Locked";
        }

        #endregion

        #region Run State Persistence

        private void SaveRunState()
        {
            if (RunStateManager.Instance == null) return;
            RunStateManager.Instance.MarkDoorUnlocked(transform.GetPersistenceId());
        }

        private void RestoreRunState()
        {
            if (RunStateManager.Instance == null) return;
            string id = transform.GetPersistenceId();
            if (!RunStateManager.Instance.IsDoorUnlocked(id)) return;

            _isUnlocked = true;
        }

        #endregion
    }
}
