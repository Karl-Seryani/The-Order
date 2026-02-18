using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// ScriptableObject defining an item (tool or key) in the game.
    /// Used by ItemPickup, HeldItemController, ToolReceiver, and LockedDoor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "The Order/Item Data")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] [TextArea(2, 4)] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _meshPrefab;
        [SerializeField] private ItemType _itemType;

        /// <summary>Unique identifier for this item.</summary>
        public string Id => _id;

        /// <summary>Name shown in UI notifications and prompts.</summary>
        public string DisplayName => _displayName;

        /// <summary>Optional flavor text.</summary>
        public string Description => _description;

        /// <summary>Optional icon for UI display.</summary>
        public Sprite Icon => _icon;

        /// <summary>3D model instantiated in the player's hand when held.</summary>
        public GameObject MeshPrefab => _meshPrefab;

        /// <summary>Whether this is a Tool (reusable) or Key.</summary>
        public ItemType Type => _itemType;
    }
}
