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
        [SerializeField] private Vector3 _meshScale = Vector3.one;
        [SerializeField] private ItemType _itemType;

        [Header("Audio")]
        [SerializeField] private AudioClip _impactClip;
        [SerializeField] [Range(0.1f, 5f)] private float _impactVolumeMultiplier = 1f;

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

        /// <summary>Scale applied when instantiating the mesh (hand or drop).</summary>
        public Vector3 MeshScale => _meshScale == Vector3.zero ? Vector3.one : _meshScale;

        /// <summary>Whether this is a Tool (reusable) or Key.</summary>
        public ItemType Type => _itemType;

        /// <summary>Sound played when this item hits a surface after being dropped.</summary>
        public AudioClip ImpactClip => _impactClip;

        /// <summary>Volume multiplier for impact sound (heavier items = louder).</summary>
        public float ImpactVolumeMultiplier => _impactVolumeMultiplier;
    }
}
