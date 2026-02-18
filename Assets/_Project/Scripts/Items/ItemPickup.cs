using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// World-space item pickup. Press E to pick up into the player's hand.
    /// If the player is already holding an item, shows a "hands full" message.
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

        /// <summary>Pick up the item into the player's hand.</summary>
        public void Interact(GameObject interactor)
        {
            if (_itemData == null)
            {
                Debug.LogWarning($"[ItemPickup] No ItemData assigned on {gameObject.name}");
                return;
            }

            var heldItem = interactor.GetComponent<HeldItemController>();
            if (heldItem == null)
            {
                Debug.LogWarning("[ItemPickup] No HeldItemController on interactor.");
                return;
            }

            if (heldItem.HasItem)
            {
                // Hands full — can't pick up
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

            if (HeldItemController.Instance != null && HeldItemController.Instance.HasItem)
                return "Hands full — drop current item first (Q)";

            return $"Pick up {_itemData.DisplayName}";
        }

        #endregion
    }
}
