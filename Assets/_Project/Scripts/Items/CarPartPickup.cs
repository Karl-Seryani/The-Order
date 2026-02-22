using UnityEngine;

namespace TheOrder.Items
{
    /// <summary>
    /// Pickup for car parts and tools (like the drill) that exist as scene objects.
    /// Hides the mesh on pickup, re-shows it on drop with physics so it falls naturally.
    /// For car parts: install snaps them to their assembled world position.
    /// </summary>
    public class CarPartPickup : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Item Data")]
        [SerializeField] private ItemData _itemData;

        [Header("Installation")]
        [Tooltip("If true, this part must be drilled after placement to count as installed.")]
        [SerializeField] private bool _requiresDrill;

        [Tooltip("World position this part snaps to when installed on the car.")]
        [SerializeField] private Vector3 _homePosition;

        [Tooltip("World rotation (euler) this part snaps to when installed on the car.")]
        [SerializeField] private Vector3 _homeRotation;

        [Header("Audio")]
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] [Range(0f, 1f)] private float _pickupVolume = 0.6f;

        #endregion

        #region Private Fields

        private Renderer[] _renderers;
        private Collider[] _colliders;
        private bool _isCollected;
        private bool _isPlaced;
        private bool _isInstalled;
        private Rigidbody _dropRigidbody;

        /// <summary>Tracks which CarPartPickup instance is currently held by the player.</summary>
        private static CarPartPickup _currentlyHeld;

        #endregion

        #region Public API

        /// <summary>The item data for this car part.</summary>
        public ItemData ItemData => _itemData;

        /// <summary>Whether this part must be drilled after placement.</summary>
        public bool RequiresDrill => _requiresDrill;

        /// <summary>Whether this part has been picked up from the world.</summary>
        public bool IsCollected => _isCollected;

        /// <summary>Whether this part has been placed on the car (visible but not yet drilled if wheel).</summary>
        public bool IsPlaced => _isPlaced;

        /// <summary>Whether this part is fully installed (placed + drilled if needed).</summary>
        public bool IsInstalled => _isInstalled;

        /// <summary>
        /// Place this part on the car — snap to assembled world position and show it.
        /// For non-drill parts this also marks as installed.
        /// For wheels this marks as placed (needs drilling).
        /// </summary>
        public void Place()
        {
            RemoveDropPhysics();

            transform.position = _homePosition;
            transform.rotation = Quaternion.Euler(_homeRotation);

            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = true;
            }

            _isPlaced = true;

            if (!_requiresDrill)
            {
                _isInstalled = true;
            }

            if (_currentlyHeld == this)
            {
                _currentlyHeld = null;
            }
        }

        /// <summary>
        /// Drill this placed wheel to secure it. Only valid for parts with RequiresDrill.
        /// </summary>
        public void Drill()
        {
            _isInstalled = true;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _colliders = GetComponentsInChildren<Collider>();
        }

        private void OnEnable()
        {
            GameEvents.OnItemDropped += HandleItemDropped;
        }

        private void OnDisable()
        {
            GameEvents.OnItemDropped -= HandleItemDropped;
        }

        #endregion

        #region IInteractable

        /// <summary>Pick up this car part — goes to hand via HeldItemController.</summary>
        public void Interact(GameObject interactor)
        {
            if (_isCollected || _isInstalled) return;

            var heldItem = HeldItemController.Instance;
            if (heldItem == null)
            {
                heldItem = interactor.GetComponent<HeldItemController>();
            }

            if (heldItem == null || heldItem.HasItem) return;

            heldItem.PickUp(_itemData);
            _currentlyHeld = this;

            // Remove physics from previous drop
            RemoveDropPhysics();

            if (_pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position, _pickupVolume);
            }

            // Hide the mesh and disable colliders
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = false;
            }
            for (int i = 0; i < _colliders.Length; i++)
            {
                _colliders[i].enabled = false;
            }

            _isCollected = true;
            GameEvents.ItemPickedUp(_itemData);
        }

        /// <summary>Returns pickup prompt with part name.</summary>
        public string GetPromptText()
        {
            if (_isCollected || _isInstalled) return "";

            if (HeldItemController.Instance != null && HeldItemController.Instance.HasItem)
            {
                return "Hands full";
            }

            return _itemData != null ? $"Pick up {_itemData.DisplayName}" : "Pick up car part";
        }

        /// <summary>Blocked when hands full.</summary>
        public bool CanInteract(GameObject interactor)
        {
            if (_isCollected || _isInstalled) return true;
            var heldItem = HeldItemController.Instance;
            return heldItem == null || !heldItem.HasItem;
        }

        /// <summary>Returns blocked reason.</summary>
        public string GetBlockedMessage()
        {
            if (_isCollected || _isInstalled) return "";
            var heldItem = HeldItemController.Instance;
            if (heldItem != null && heldItem.HasItem) return "Hands full";
            return "";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// When the player drops a car part, move it to the drop position,
        /// re-show it, and add physics so it falls naturally like tools.
        /// Also destroys the invisible phantom that ItemSpawner created.
        /// </summary>
        private void HandleItemDropped(ItemData item, Vector3 dropPosition)
        {
            if (_currentlyHeld != this) return;
            _currentlyHeld = null;

            // Move to drop position
            transform.position = dropPosition;

            // Re-enable renderers and colliders
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = true;
            }
            for (int i = 0; i < _colliders.Length; i++)
            {
                _colliders[i].enabled = true;
            }

            _isCollected = false;

            // Add physics so it falls like a real dropped tool
            AddDropPhysics();

            // Destroy the phantom ItemPickup that ItemSpawner spawned (has no mesh)
            var allPickups = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
            float closestSqr = float.MaxValue;
            ItemPickup phantom = null;
            for (int i = 0; i < allPickups.Length; i++)
            {
                if (allPickups[i].ItemData != _itemData) continue;
                float sqr = (allPickups[i].transform.position - dropPosition).sqrMagnitude;
                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    phantom = allPickups[i];
                }
            }

            if (phantom != null)
            {
                Destroy(phantom.gameObject);
            }
        }

        private void AddDropPhysics()
        {
            if (_dropRigidbody != null) return;

            _dropRigidbody = gameObject.AddComponent<Rigidbody>();
            _dropRigidbody.mass = 1.5f;
            _dropRigidbody.linearDamping = 0.05f;
            _dropRigidbody.angularDamping = 0.5f;
            _dropRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void RemoveDropPhysics()
        {
            if (_dropRigidbody != null)
            {
                Destroy(_dropRigidbody);
                _dropRigidbody = null;
            }
        }

        #endregion
    }
}
