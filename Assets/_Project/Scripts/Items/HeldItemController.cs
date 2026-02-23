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
        [SerializeField] private float _dropDistance = 2.0f;
        [SerializeField] private float _dropHeightOffset = 0.3f;
        [SerializeField] private LayerMask _dropRaycastMask = ~0;

        [Header("Audio")]
        [SerializeField] private AudioClip _dropSound;
        [SerializeField] [Range(0f, 1f)] private float _dropVolume = 0.6f;

        #endregion

        #region Private Fields

        private ItemData _currentItem;
        private GameObject _heldMeshInstance;
        private CharacterController _characterController;
        private Player.PlayerController _playerController;
        private string _heldItemPersistenceId;

        #endregion

        #region Public API

        /// <summary>The item currently held, or null.</summary>
        public ItemData CurrentItem => _currentItem;

        /// <summary>True if the player is holding an item.</summary>
        public bool HasItem => _currentItem != null;

        /// <summary>Set the persistence ID for the currently held item (used for position tracking on drop).</summary>
        public void SetHeldItemPersistenceId(string id) => _heldItemPersistenceId = id;

        /// <summary>
        /// Pick up an item. Instantiates its mesh at the hand point.
        /// </summary>
        public void PickUp(ItemData item)
        {
            if (item == null) return;

            // Keep behavior aligned with interaction prompts: no implicit swap on pickup.
            if (HasItem) return;

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
        /// Spawns a new ItemPickup in the world with inherited player velocity.
        /// If crouched and looking down, places gently instead of dropping.
        /// </summary>
        public void Drop()
        {
            if (!HasItem) return;

            var dropPosition = CalculateDropPosition();
            var droppedItem = ItemSpawner.SpawnPickup(_currentItem, dropPosition);
            
            // Check if we should place gently (crouched + looking down)
            bool shouldPlaceGently = IsPlacingGently();
            
            var rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (shouldPlaceGently)
                {
                    // Gentle placement - no velocity, item just sits there
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                else if (_characterController != null)
                {
                    // Normal drop - inherit player velocity with upward arc
                    Vector3 inheritedVelocity = _characterController.velocity;
                    inheritedVelocity.y = 1.0f;
                    rb.linearVelocity = inheritedVelocity;
                    
                    // Small random spin for variety
                    rb.angularVelocity = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-2f, 2f),
                        Random.Range(-1f, 1f)
                    );
                }
            }
            
            // Play drop sound and alert Hunter (louder if thrown, quieter if placed)
            float loudness = shouldPlaceGently ? 0.2f : 0.7f;
            if (_dropSound != null)
                AudioSource.PlayClipAtPoint(_dropSound, dropPosition, shouldPlaceGently ? _dropVolume * 0.3f : _dropVolume);
            GameEvents.InteractableNoise(dropPosition, loudness);

            // Save drop position for persistence
            SaveDropPosition(dropPosition);

            GameEvents.ItemDropped(_currentItem, dropPosition);
            ClearHeldItem();
        }

        /// <summary>
        /// Drop the currently held item naturally without sound or noise.
        /// Used when the player dies — item tosses forward and falls with gravity.
        /// Still fires ItemDropped so CarPartPickup can intercept.
        /// </summary>
        public void DropSilently()
        {
            if (!HasItem) return;

            // Spawn slightly in front of player at waist height
            Vector3 dropPosition = transform.position + transform.forward * 0.5f;
            var droppedItem = ItemSpawner.SpawnPickup(_currentItem, dropPosition);

            var rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Toss forward with upward arc — same feel as Q drop
                rb.linearVelocity = transform.forward * 1.5f + Vector3.up * 1f;
                rb.linearDamping = 0.05f;
                rb.angularVelocity = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-2f, 2f),
                    Random.Range(-1f, 1f)
                );
            }

            // Save drop position for persistence
            SaveDropPosition(dropPosition);

            GameEvents.ItemDropped(_currentItem, dropPosition);
            ClearHeldItem();
        }

        /// <summary>
        /// Clear the held item without dropping it (used when item is consumed).
        /// </summary>
        public void ClearHeldItem()
        {
            _currentItem = null;
            _heldItemPersistenceId = null;

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

            _characterController = GetComponent<CharacterController>();
            _playerController = GetComponent<Player.PlayerController>();

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

        private void SaveDropPosition(Vector3 position)
        {
            if (RunStateManager.Instance == null || _currentItem == null) return;

            string id = !string.IsNullOrEmpty(_heldItemPersistenceId)
                ? _heldItemPersistenceId
                : _currentItem.Id;

            if (!string.IsNullOrEmpty(id))
                RunStateManager.Instance.SaveItemDropPosition(id, position);
        }

        private UnityEngine.Camera _cachedCamera;

        private bool IsPlacingGently()
        {
            // Check if player is crouched
            if (_playerController == null || !_playerController.IsCrouching)
                return false;

            if (_cachedCamera == null)
                _cachedCamera = GetComponentInChildren<UnityEngine.Camera>();

            if (_cachedCamera == null)
                return false;

            // Check if looking down (camera forward y component is negative and steep)
            float lookDownAngle = _cachedCamera.transform.forward.y;
            return lookDownAngle < -0.7f; // Looking down at ~45 degrees or more
        }

        private Vector3 CalculateDropPosition()
        {
            if (_cachedCamera == null)
                _cachedCamera = GetComponentInChildren<UnityEngine.Camera>();

            if (_cachedCamera != null)
            {
                // Raycast from camera (crosshair) to find drop point
                Ray ray = new Ray(_cachedCamera.transform.position, _cachedCamera.transform.forward);
                
                if (Physics.Raycast(ray, out RaycastHit hit, _dropDistance, _dropRaycastMask, QueryTriggerInteraction.Ignore))
                {
                    // Drop at the surface we're looking at, slightly above it
                    return hit.point + Vector3.up * _dropHeightOffset;
                }
                
                // No surface hit - drop at max distance in front of camera
                return _cachedCamera.transform.position + _cachedCamera.transform.forward * _dropDistance;
            }

            // Fallback to old method if no camera
            return transform.position + transform.forward * _dropDistance;
        }

        #endregion
    }
}
