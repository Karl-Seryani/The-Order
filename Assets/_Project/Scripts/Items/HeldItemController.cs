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

        [Header("Item Pickup Prefab")]
        [SerializeField] private GameObject _itemPickupPrefab;

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
            SpawnDroppedPickup(_currentItem, dropPosition);
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

        private Vector3 CalculateDropPosition()
        {
            var cam = GetComponentInChildren<UnityEngine.Camera>();
            if (cam != null)
            {
                // Drop in front of the player, on the ground
                var forward = cam.transform.forward;
                forward.y = 0;
                forward.Normalize();
                var dropPos = transform.position + forward * _dropDistance;
                dropPos.y = transform.position.y + _dropHeightOffset;
                return dropPos;
            }

            return transform.position + Vector3.forward * _dropDistance;
        }

        private void SpawnDroppedPickup(ItemData item, Vector3 position)
        {
            GameObject pickupGo;

            if (_itemPickupPrefab != null)
            {
                pickupGo = Instantiate(_itemPickupPrefab, position, Quaternion.identity);
            }
            else
            {
                // Fallback: create a simple pickup from the mesh prefab
                if (item.MeshPrefab != null)
                {
                    pickupGo = Instantiate(item.MeshPrefab, position, Quaternion.identity);
                }
                else
                {
                    pickupGo = new GameObject($"Dropped_{item.DisplayName}");
                    pickupGo.transform.position = position;
                }

                // Ensure it has a collider for raycasting
                if (pickupGo.GetComponentInChildren<Collider>() == null)
                {
                    pickupGo.AddComponent<BoxCollider>();
                }
            }

            // Add physics so the item falls with gravity
            var rb = pickupGo.GetComponent<Rigidbody>();
            if (rb == null)
                rb = pickupGo.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Ensure MeshColliders are convex (required for non-kinematic Rigidbody)
            foreach (var mc in pickupGo.GetComponentsInChildren<MeshCollider>())
                mc.convex = true;

            // Add or configure ItemPickup component
            var pickup = pickupGo.GetComponent<ItemPickup>();
            if (pickup == null)
            {
                pickup = pickupGo.AddComponent<ItemPickup>();
            }
            pickup.Initialize(item);

            pickupGo.name = $"Dropped_{item.DisplayName}";
        }

        #endregion
    }
}
