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
                return "Hands full — drop current item first (Q)";

            return $"Pick up {_itemData.DisplayName}";
        }

        #endregion
    }
}
