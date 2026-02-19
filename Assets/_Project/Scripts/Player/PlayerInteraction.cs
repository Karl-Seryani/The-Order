using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Raycasts from the camera center to detect IInteractable objects.
    /// Press E to interact with the targeted object.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Raycast")]
        [SerializeField] private float _interactionRange = 2.5f;
        [SerializeField] private LayerMask _interactionMask = ~0;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _playerCamera;

        #endregion

        #region Private Fields

        private PlayerInputHandler _input;
        private IInteractable _currentTarget;
        private Items.HeldItemController _heldItemController;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

        #endregion

        #region Public API

        /// <summary>Currently targeted interactable, or null if none.</summary>
        public IInteractable CurrentInteractable => _currentTarget;

        /// <summary>Prompt text for the current target, or empty string.</summary>
        public string PromptText => _currentTarget?.GetPromptText() ?? string.Empty;

        /// <summary>True if currently looking at an interactable.</summary>
        public bool HasTarget => _currentTarget != null;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            _heldItemController = GetComponent<Items.HeldItemController>();

            if (_playerCamera == null)
            {
                _playerCamera = GetComponentInChildren<UnityEngine.Camera>();
            }

            if (_playerCamera == null)
            {
                Debug.LogWarning("[PlayerInteraction] No camera found — interaction disabled.", this);
            }
        }

        private void Update()
        {
            DetectInteractable();

            if (_input.InteractPressed && _currentTarget != null)
            {
                _currentTarget.Interact(gameObject);
            }

            if (_input.DropPressed && _heldItemController != null && _heldItemController.HasItem)
            {
                _heldItemController.Drop();
            }
        }

        #endregion

        #region Detection

        private void DetectInteractable()
        {
            if (_playerCamera == null) { _currentTarget = null; return; }

            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionMask))
            {
                // Prefer LockedDoor over DoorController when both exist on a door
                var lockedDoor = hit.collider.GetComponentInParent<Doors.LockedDoor>();
                if (lockedDoor != null)
                {
                    _currentTarget = lockedDoor;
                    return;
                }

                // Check for screws and item pickups behind overlapping furniture colliders
                int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, hit.distance + 0.15f, _interactionMask);
                for (int i = 0; i < hitCount; i++)
                {
                    if (_hitBuffer[i].collider.TryGetComponent(out Items.ScrewInteractable screw))
                    {
                        _currentTarget = screw;
                        return;
                    }
                }
                for (int i = 0; i < hitCount; i++)
                {
                    var pickup = _hitBuffer[i].collider.GetComponentInParent<Items.ItemPickup>();
                    if (pickup != null)
                    {
                        _currentTarget = pickup;
                        return;
                    }
                }

                // Check hit object first, then parents
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _currentTarget = interactable;
                    return;
                }

                var parentInteractable = hit.collider.GetComponentInParent<IInteractable>();
                if (parentInteractable != null)
                {
                    _currentTarget = parentInteractable;
                    return;
                }
            }

            _currentTarget = null;
        }

        #endregion
    }
}