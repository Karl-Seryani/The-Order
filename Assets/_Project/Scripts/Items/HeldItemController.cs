using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// Manages the single item the player can carry in their right hand.
    /// Attach to the Player GameObject alongside PlayerInventory.
    /// </summary>
    public class HeldItemController : MonoBehaviour
    {
        #region Singleton

        /// <summary>Current instance.</summary>
        public static HeldItemController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Hand Point")]
        [SerializeField] private Transform _handPoint;

        [Header("Drop Settings")]
        [SerializeField] private float _dropDistance = 1.5f;
        [SerializeField] private float _dropHeightOffset = 0.3f;

        #endregion

        #region Private Fields

        private ItemData _currentItem;
        private GameObject _heldMeshInstance;

        #endregion

        #region Public API

        /// <summary>The item currently held, or null.</summary>
        public ItemData CurrentItem => _currentItem;

        /// <summary>True if the player is holding an item.</summary>
        public bool HasItem => _currentItem != null;

        /// <summary>
        /// Pick up an item. Instantiates its mesh at the hand point.
        /// </summary>
        public void PickUp(ItemData item)
        {
            if (item == null) return;

            // Drop current item first if holding one
            if (HasItem)
            {
                Drop();
            }

            _currentItem = item;

            if (item.MeshPrefab != null && _handPoint != null)
            {
                _heldMeshInstance = Instantiate(item.MeshPrefab, _handPoint);
                _heldMeshInstance.transform.localPosition = Vector3.zero;
                _heldMeshInstance.transform.localRotation = Quaternion.identity;
                _heldMeshInstance.transform.localScale = item.MeshScale;

                // Disable colliders on held mesh so it doesn't interfere with raycasts
                foreach (var col in _heldMeshInstance.GetComponentsInChildren<Collider>())
                {
                    col.enabled = false;
                }
            }
        }

        /// <summary>
        /// Drop the currently held item at the player's feet.
        /// Spawns a new ItemPickup in the world.
        /// </summary>
        public void Drop()
        {
            if (!HasItem) return;

            var dropPosition = CalculateDropPosition();
            ItemSpawner.SpawnPickup(_currentItem, dropPosition);
            GameEvents.ItemDropped(_currentItem, dropPosition);

            ClearHeldItem();
        }

        /// <summary>
        /// Clear the held item without dropping it (used when item is consumed).
        /// </summary>
        public void ClearHeldItem()
        {
            _currentItem = null;

            if (_heldMeshInstance != null)
            {
                Destroy(_heldMeshInstance);
                _heldMeshInstance = null;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (_handPoint == null)
            {
                Debug.LogWarning("[HeldItemController] No hand point assigned. Items won't be visible in hand.", this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region Private Methods

        private UnityEngine.Camera _cachedCamera;

        private Vector3 CalculateDropPosition()
        {
            if (_cachedCamera == null)
                _cachedCamera = GetComponentInChildren<UnityEngine.Camera>();

            if (_cachedCamera != null)
            {
                var forward = _cachedCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();
                var dropPos = transform.position + forward * _dropDistance;
                dropPos.y = transform.position.y + _dropHeightOffset;
                return dropPos;
            }

            return transform.position + transform.forward * _dropDistance;
        }

        #endregion
    }
}
