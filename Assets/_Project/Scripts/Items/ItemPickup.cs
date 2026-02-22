using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// World-space item pickup. Press E to pick up.
    /// Keys go straight to inventory (pocket). Tools go to hand.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Item Data")]
        [SerializeField] private ItemData _itemData;

        [Header("Audio")]
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] [Range(0f, 1f)] private float _pickupVolume = 0.6f;

        #endregion

        #region Private Fields

        private string _persistenceId;

        #endregion

        #region Public API

        /// <summary>The item data this pickup represents.</summary>
        public ItemData ItemData => _itemData;

        /// <summary>
        /// Initialize this pickup at runtime (used when dropping items).
        /// </summary>
        public void Initialize(ItemData itemData)
        {
            _itemData = itemData;
        }

        /// <summary>Override the persistence ID (used for spawned items that need to track a source).</summary>
        public void SetPersistenceId(string id) => _persistenceId = id;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (string.IsNullOrEmpty(_persistenceId))
                _persistenceId = transform.GetPersistenceId();
        }

        private void Start()
        {
            RestoreRunState();
        }

        #endregion

        #region IInteractable

        /// <summary>Pick up the item — keys to inventory, tools to hand.</summary>
        public void Interact(GameObject interactor)
        {
            if (_itemData == null)
            {
                Debug.LogWarning($"[ItemPickup] No ItemData assigned on {gameObject.name}");
                return;
            }

            if (_itemData.Type == ItemType.Key)
            {
                var inventory = Player.PlayerInventory.Instance;
                if (inventory == null)
                {
                    inventory = interactor.GetComponent<Player.PlayerInventory>();
                }
                if (inventory == null)
                {
                    Debug.LogWarning("[ItemPickup] No PlayerInventory found.");
                    return;
                }

                inventory.AddKey(_itemData);

                // Mark key as consumed so it stays gone on respawn
                if (RunStateManager.Instance != null && !string.IsNullOrEmpty(_itemData.Id))
                    RunStateManager.Instance.MarkKeyConsumed(_itemData.Id);

                if (_pickupSound != null)
                    AudioSource.PlayClipAtPoint(_pickupSound, transform.position, _pickupVolume);
                GameEvents.ItemPickedUp(_itemData);
                Destroy(gameObject);
                return;
            }

            // Tool — goes to hand
            var heldItem = interactor.GetComponent<HeldItemController>();
            if (heldItem == null)
            {
                Debug.LogWarning("[ItemPickup] No HeldItemController on interactor.");
                return;
            }

            if (heldItem.HasItem)
            {
                return;
            }

            heldItem.PickUp(_itemData);
            heldItem.SetHeldItemPersistenceId(!string.IsNullOrEmpty(_itemData.Id) ? _itemData.Id : _persistenceId);

            if (_pickupSound != null)
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position, _pickupVolume);
            GameEvents.ItemPickedUp(_itemData);
            Destroy(gameObject);
        }

        /// <summary>Returns pickup prompt with item name.</summary>
        public string GetPromptText()
        {
            if (_itemData == null) return "Pick up item";

            if (_itemData.Type == ItemType.Key)
                return $"Pick up {_itemData.DisplayName}";

            if (HeldItemController.Instance != null && HeldItemController.Instance.HasItem)
                return "Hands full";

            return $"Pick up {_itemData.DisplayName}";
        }

        /// <summary>Keys always pickable. Tools blocked when hands full.</summary>
        public bool CanInteract(GameObject interactor)
        {
            if (_itemData == null) return false;
            if (_itemData.Type == ItemType.Key) return true;
            var heldItem = HeldItemController.Instance;
            return heldItem == null || !heldItem.HasItem;
        }

        /// <summary>Returns blocked reason.</summary>
        public string GetBlockedMessage()
        {
            if (_itemData != null && _itemData.Type != ItemType.Key)
            {
                var heldItem = HeldItemController.Instance;
                if (heldItem != null && heldItem.HasItem) return "Hands full";
            }
            return "";
        }

        #endregion

        #region Run State Persistence

        private void RestoreRunState()
        {
            if (RunStateManager.Instance == null || _itemData == null) return;

            string id = !string.IsNullOrEmpty(_itemData.Id) ? _itemData.Id : _persistenceId;
            if (string.IsNullOrEmpty(id)) return;

            // If key was consumed (picked up in a previous life), destroy this pickup
            if (_itemData.Type == ItemType.Key && RunStateManager.Instance.IsKeyConsumed(id))
            {
                Destroy(gameObject);
                return;
            }

            // If this item was dropped somewhere, teleport to that position
            if (RunStateManager.Instance.TryGetItemDropPosition(id, out Vector3 dropPos))
            {
                transform.position = dropPos;
            }
        }

        #endregion
    }
}
