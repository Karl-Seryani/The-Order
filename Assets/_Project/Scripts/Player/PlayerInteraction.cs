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
        [SerializeField] private float _interactionRange = 3.0f;
        [SerializeField] private LayerMask _interactionMask = ~0;
        [SerializeField] private float _closePickupRadius = 0.5f;
        [SerializeField] [Range(-1f, 1f)] private float _closePickupMinDot = -0.2f;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _playerCamera;

        #endregion

        #region Private Fields

        private PlayerInputHandler _input;
        private IInteractable _currentTarget;
        private Items.HeldItemController _heldItemController;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
        private readonly Collider[] _overlapBuffer = new Collider[16];

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

            // First check for very close pickups (when standing on top of items)
            if (TryFindNearbyPickup(out var nearbyPickup))
            {
                _currentTarget = nearbyPickup;
                return;
            }

            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

            // Try regular raycast
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionMask, QueryTriggerInteraction.Ignore))
            {
                // Prefer LockedDoor over DoorController when both exist on a door
                var lockedDoor = hit.collider.GetComponentInParent<Doors.LockedDoor>();
                if (lockedDoor != null)
                {
                    _currentTarget = lockedDoor;
                    return;
                }

                // Check for screws behind overlapping furniture colliders.
                // Do NOT do this for ItemPickup, otherwise closed furniture/doors can be bypassed
                // and items inside can be grabbed through blockers.
                int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, hit.distance + 0.15f, _interactionMask);
                Items.ScrewInteractable closestScrew = null;
                float closestScrewDistance = float.MaxValue;
                for (int i = 0; i < hitCount; i++)
                {
                    if (_hitBuffer[i].collider.TryGetComponent(out Items.ScrewInteractable screw))
                    {
                        float screwDistance = _hitBuffer[i].distance;
                        if (screwDistance < closestScrewDistance)
                        {
                            closestScrewDistance = screwDistance;
                            closestScrew = screw;
                        }
                    }
                }

                if (closestScrew != null)
                {
                    _currentTarget = closestScrew;
                    return;
                }

                if (TryFindPickupBehindOpenBlocker(ray, hit, out var blockerPickup))
                {
                    _currentTarget = blockerPickup;
                    return;
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

        private bool TryFindNearbyPickup(out Items.ItemPickup pickup)
        {
            pickup = null;

            if (_closePickupRadius <= 0f) return false;

            Vector3 eyePos = _playerCamera.transform.position;
            Vector3 eyeForward = _playerCamera.transform.forward;

            int overlapCount = Physics.OverlapSphereNonAlloc(
                eyePos,
                _closePickupRadius,
                _overlapBuffer,
                _interactionMask,
                QueryTriggerInteraction.Collide
            );

            float bestSqrDistance = float.MaxValue;
            for (int i = 0; i < overlapCount; i++)
            {
                var overlapCollider = _overlapBuffer[i];
                if (overlapCollider == null) continue;

                var candidatePickup = overlapCollider.GetComponentInParent<Items.ItemPickup>();
                if (candidatePickup == null) continue;

                Vector3 closestPoint = overlapCollider.ClosestPoint(eyePos);
                Vector3 toPickup = closestPoint - eyePos;
                float sqrDistance = toPickup.sqrMagnitude;

                if (sqrDistance > 0.0001f)
                {
                    Vector3 direction = toPickup.normalized;
                    if (Vector3.Dot(eyeForward, direction) < _closePickupMinDot)
                        continue;

                    float distance = Mathf.Sqrt(sqrDistance);
                    if (Physics.Raycast(eyePos, direction, out RaycastHit hit, distance + 0.02f, _interactionMask))
                    {
                        var visiblePickup = hit.collider.GetComponentInParent<Items.ItemPickup>();
                        if (visiblePickup != candidatePickup)
                            continue;
                    }
                }

                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    pickup = candidatePickup;
                }
            }

            return pickup != null;
        }

        private bool TryFindPickupBehindOpenBlocker(Ray ray, RaycastHit firstHit, out Items.ItemPickup pickup)
        {
            pickup = null;

            var openFurniture = firstHit.collider.GetComponentInParent<Doors.SlidableFurniture>();
            Doors.DoorController openDoor = null;

            if (openFurniture != null)
            {
                if (!openFurniture.IsOpen) return false;
            }
            else
            {
                openDoor = firstHit.collider.GetComponentInParent<Doors.DoorController>();
                if (openDoor == null || !openDoor.IsOpen) return false;
            }

            int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, _interactionRange, _interactionMask);
            float firstDistance = firstHit.distance;
            float nearestPickupDistance = float.MaxValue;
            Items.ItemPickup nearestPickup = null;
            float nearestBlockingDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var candidateCollider = _hitBuffer[i].collider;
                if (candidateCollider == null) continue;

                float candidateDistance = _hitBuffer[i].distance;
                if (candidateDistance <= firstDistance + 0.01f) continue;

                var candidatePickup = candidateCollider.GetComponentInParent<Items.ItemPickup>();
                if (candidatePickup != null)
                {
                    if (candidateDistance < nearestPickupDistance)
                    {
                        nearestPickupDistance = candidateDistance;
                        nearestPickup = candidatePickup;
                    }
                    continue;
                }

                if (BelongsToOpenBlocker(candidateCollider, openFurniture, openDoor))
                    continue;

                if (candidateDistance < nearestBlockingDistance)
                    nearestBlockingDistance = candidateDistance;
            }

            if (nearestPickup == null) return false;
            if (nearestPickupDistance > nearestBlockingDistance + 0.001f) return false;

            pickup = nearestPickup;
            return true;
        }

        private static bool BelongsToOpenBlocker(
            Collider collider,
            Doors.SlidableFurniture openFurniture,
            Doors.DoorController openDoor)
        {
            if (openFurniture != null)
                return collider.GetComponentInParent<Doors.SlidableFurniture>() == openFurniture;

            if (openDoor != null)
                return collider.GetComponentInParent<Doors.DoorController>() == openDoor;

            return false;
        }

        #endregion
    }
}
