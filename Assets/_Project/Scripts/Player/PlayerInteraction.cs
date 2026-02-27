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
        [SerializeField] private float _closePickupRadius = 0.9f;
        [SerializeField] [Range(-1f, 1f)] private float _closePickupMinDot = 0.55f;
        [SerializeField] [Range(0f, 1f)] private float _bodyPickupProbeDownFactor = 0.35f;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _playerCamera;

        #endregion

        #region Private Fields

        private PlayerInputHandler _input;
        private IInteractable _currentTarget;
        private Items.HeldItemController _heldItemController;
        private CharacterController _characterController;
        private CapsuleCollider _capsuleCollider;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[64];
        private readonly Collider[] _overlapBuffer = new Collider[64];

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
            _characterController = GetComponent<CharacterController>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

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
            // If reading a clue, E dismisses it from anywhere (no need to look at the note)
            if (_input.InteractPressed && Clues.CluePickup.IsReading)
            {
                Clues.CluePickup.DismissCurrentClue();
                return;
            }

            DetectInteractable();

            if (_input.InteractPressed && _currentTarget != null)
            {
                if (!_currentTarget.CanInteract(gameObject))
                {
                    string blockedMessage = _currentTarget.GetBlockedMessage();
                    if (!string.IsNullOrEmpty(blockedMessage))
                    {
                        GameEvents.InteractionBlocked(blockedMessage);
                    }

                    return;
                }

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

            Vector3 eyePos = _playerCamera.transform.position;
            Vector3 eyeForward = _playerCamera.transform.forward;

            Ray ray = new Ray(eyePos, eyeForward);

            // Try regular raycast (ignore the player's own colliders).
            if (TryFindNearestNonSelfRayHit(ray.origin, ray.direction, _interactionRange, QueryTriggerInteraction.Ignore, out RaycastHit hit))
            {
                // Main door escape — Easy/Medium difficulty (overrides LockedDoor when enabled)
                var escapeTrigger = hit.collider.GetComponentInParent<Ending.MainDoorEscapeTrigger>();
                if (escapeTrigger != null && escapeTrigger.enabled)
                {
                    _currentTarget = escapeTrigger;
                    return;
                }

                // Prefer LockedDoor over DoorController when both exist on a door
                var lockedDoor = hit.collider.GetComponentInParent<Doors.LockedDoor>();
                if (lockedDoor != null)
                {
                    _currentTarget = lockedDoor;
                    return;
                }

                // Car install zones can sit just behind the car body collider.
                // Prefer the closest zone hit in a tiny depth window behind the first hit.
                if (TryFindCarInstallZone(ray, hit.distance + 0.25f, out var carZone))
                {
                    _currentTarget = carZone;
                    return;
                }

                // Check for screws behind overlapping furniture colliders.
                // Do NOT do this for ItemPickup, otherwise closed furniture/doors can be bypassed
                // and items inside can be grabbed through blockers.
                if (TryFindClosestScrewOnRay(ray, hit.distance + 0.15f, out var closestScrew))
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

            // Fallback for very close pickups (standing on top of items).
            // This runs after precise ray targeting so specific interactions
            // (seat/ignition/doors/zones) are not overridden by nearby items.
            if (TryFindNearbyPickup(eyePos, eyePos, eyeForward, out var nearbyPickup) ||
                TryFindNearbyPickup(GetBodyPickupProbeOrigin(), eyePos, eyeForward, out nearbyPickup))
            {
                _currentTarget = nearbyPickup;
                return;
            }

            _currentTarget = null;
        }

        private bool TryFindNearbyPickup(
            Vector3 overlapOrigin,
            Vector3 eyePos,
            Vector3 eyeForward,
            out IInteractable nearbyInteractable)
        {
            nearbyInteractable = null;

            if (_closePickupRadius <= 0f) return false;

            int overlapCount = Physics.OverlapSphereNonAlloc(
                overlapOrigin,
                _closePickupRadius,
                _overlapBuffer,
                _interactionMask,
                QueryTriggerInteraction.Collide
            );

            if (overlapCount == _overlapBuffer.Length)
            {
                Collider[] allOverlaps = Physics.OverlapSphere(
                    overlapOrigin,
                    _closePickupRadius,
                    _interactionMask,
                    QueryTriggerInteraction.Collide);
                return TrySelectNearbyPickup(allOverlaps, allOverlaps.Length, eyePos, eyeForward, out nearbyInteractable);
            }

            return TrySelectNearbyPickup(_overlapBuffer, overlapCount, eyePos, eyeForward, out nearbyInteractable);
        }

        private Vector3 GetBodyPickupProbeOrigin()
        {
            if (_characterController != null)
            {
                Bounds bounds = _characterController.bounds;
                float y = bounds.center.y - (bounds.extents.y * _bodyPickupProbeDownFactor);
                return new Vector3(bounds.center.x, y, bounds.center.z);
            }

            if (_capsuleCollider != null)
            {
                Bounds bounds = _capsuleCollider.bounds;
                float y = bounds.center.y - (bounds.extents.y * _bodyPickupProbeDownFactor);
                return new Vector3(bounds.center.x, y, bounds.center.z);
            }

            return transform.position + (Vector3.up * 0.6f);
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

            float firstDistance = firstHit.distance;
            int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, _interactionRange, _interactionMask);
            if (hitCount == _hitBuffer.Length)
            {
                RaycastHit[] allHits = Physics.RaycastAll(ray, _interactionRange, _interactionMask, QueryTriggerInteraction.UseGlobal);
                return TryFindPickupBehindOpenBlockerFromHits(allHits, allHits.Length, firstDistance, openFurniture, openDoor, out pickup);
            }

            return TryFindPickupBehindOpenBlockerFromHits(_hitBuffer, hitCount, firstDistance, openFurniture, openDoor, out pickup);
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

        private bool TryFindCarInstallZone(Ray ray, float maxDistance, out Ending.CarInstallZone zone)
        {
            zone = null;

            int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, maxDistance, _interactionMask, QueryTriggerInteraction.Ignore);
            if (hitCount == _hitBuffer.Length)
            {
                RaycastHit[] allHits = Physics.RaycastAll(ray, maxDistance, _interactionMask, QueryTriggerInteraction.Ignore);
                return TryFindCarInstallZoneFromHits(allHits, allHits.Length, out zone);
            }

            return TryFindCarInstallZoneFromHits(_hitBuffer, hitCount, out zone);
        }

        private bool TrySelectNearbyPickup(
            Collider[] colliders,
            int colliderCount,
            Vector3 eyePos,
            Vector3 eyeForward,
            out IInteractable nearbyInteractable)
        {
            nearbyInteractable = null;
            float bestSqrDistance = float.MaxValue;

            for (int i = 0; i < colliderCount; i++)
            {
                var overlapCollider = colliders[i];
                if (overlapCollider == null) continue;

                if (!TryGetNearbyPickupCandidate(overlapCollider, out var candidateInteractable))
                    continue;

                Vector3 closestPoint = overlapCollider.ClosestPoint(eyePos);
                float selectionSqrDistance = (closestPoint - eyePos).sqrMagnitude;
                Vector3 toPickup = closestPoint - eyePos;
                if (toPickup.sqrMagnitude <= 0.0001f)
                {
                    // If eye is effectively on/inside collider surface, use bounds center
                    // to preserve directional filtering instead of auto-selecting by distance.
                    toPickup = overlapCollider.bounds.center - eyePos;
                }
                if (toPickup.sqrMagnitude <= 0.0001f)
                {
                    // Final fallback for tiny/degenerate bounds.
                    toPickup = eyeForward;
                }

                Vector3 direction = toPickup.normalized;
                if (Vector3.Dot(eyeForward, direction) < _closePickupMinDot)
                    continue;

                float visibilityDistance = Mathf.Sqrt(toPickup.sqrMagnitude);
                if (visibilityDistance > 0.03f)
                {
                    if (TryFindNearestNonSelfRayHit(
                        eyePos,
                        direction,
                        visibilityDistance + 0.02f,
                        QueryTriggerInteraction.Ignore,
                        out RaycastHit hit))
                    {
                        if (!TryGetNearbyPickupCandidate(hit.collider, out var visibleInteractable))
                            continue;

                        if (visibleInteractable != candidateInteractable)
                            continue;
                    }
                }

                if (selectionSqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = selectionSqrDistance;
                    nearbyInteractable = candidateInteractable;
                }
            }

            return nearbyInteractable != null;
        }

        private bool TryFindClosestScrewOnRay(Ray ray, float maxDistance, out Items.ScrewInteractable screw)
        {
            screw = null;

            int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, maxDistance, _interactionMask);
            if (hitCount == _hitBuffer.Length)
            {
                RaycastHit[] allHits = Physics.RaycastAll(ray, maxDistance, _interactionMask, QueryTriggerInteraction.UseGlobal);
                return TryFindClosestScrewFromHits(allHits, allHits.Length, out screw);
            }

            return TryFindClosestScrewFromHits(_hitBuffer, hitCount, out screw);
        }

        private bool TryFindNearestNonSelfRayHit(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            QueryTriggerInteraction queryTriggerInteraction,
            out RaycastHit nearestHit)
        {
            nearestHit = default;

            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                _hitBuffer,
                maxDistance,
                _interactionMask,
                queryTriggerInteraction);

            if (hitCount == _hitBuffer.Length)
            {
                RaycastHit[] allHits = Physics.RaycastAll(
                    origin,
                    direction,
                    maxDistance,
                    _interactionMask,
                    queryTriggerInteraction);
                return TryFindNearestNonSelfRayHitFromHits(allHits, allHits.Length, out nearestHit);
            }

            return TryFindNearestNonSelfRayHitFromHits(_hitBuffer, hitCount, out nearestHit);
        }

        private bool TryFindNearestNonSelfRayHitFromHits(RaycastHit[] hits, int hitCount, out RaycastHit nearestHit)
        {
            nearestHit = default;
            float nearestDistance = float.MaxValue;
            bool found = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hits[i].collider;
                if (hitCollider == null || IsSelfCollider(hitCollider))
                    continue;

                float distance = hits[i].distance;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestHit = hits[i];
                    found = true;
                }
            }

            return found;
        }

        private bool IsSelfCollider(Collider collider)
        {
            return collider.transform.IsChildOf(transform);
        }

        private static bool TryFindClosestScrewFromHits(RaycastHit[] hits, int hitCount, out Items.ScrewInteractable screw)
        {
            screw = null;
            float closestScrewDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.TryGetComponent(out Items.ScrewInteractable candidateScrew))
                {
                    float screwDistance = hits[i].distance;
                    if (screwDistance < closestScrewDistance)
                    {
                        closestScrewDistance = screwDistance;
                        screw = candidateScrew;
                    }
                }
            }

            return screw != null;
        }

        private static bool TryFindPickupBehindOpenBlockerFromHits(
            RaycastHit[] hits,
            int hitCount,
            float firstDistance,
            Doors.SlidableFurniture openFurniture,
            Doors.DoorController openDoor,
            out Items.ItemPickup pickup)
        {
            pickup = null;
            float nearestPickupDistance = float.MaxValue;
            Items.ItemPickup nearestPickup = null;
            float nearestBlockingDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var candidateCollider = hits[i].collider;
                if (candidateCollider == null) continue;

                float candidateDistance = hits[i].distance;
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

        private static bool TryFindCarInstallZoneFromHits(RaycastHit[] hits, int hitCount, out Ending.CarInstallZone zone)
        {
            zone = null;
            float closestZoneDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var hitCollider = hits[i].collider;
                if (hitCollider == null) continue;

                var candidateZone = hitCollider.GetComponentInParent<Ending.CarInstallZone>();
                if (candidateZone == null) continue;

                float distance = hits[i].distance;
                if (distance < closestZoneDistance)
                {
                    closestZoneDistance = distance;
                    zone = candidateZone;
                }
            }

            return zone != null;
        }

        private static bool TryGetNearbyPickupCandidate(Collider collider, out IInteractable interactable)
        {
            interactable = null;
            if (collider == null) return false;

            var itemPickup = collider.GetComponentInParent<Items.ItemPickup>();
            if (itemPickup != null)
            {
                interactable = itemPickup;
                return true;
            }

            var carPartPickup = collider.GetComponentInParent<Items.CarPartPickup>();
            if (carPartPickup != null)
            {
                interactable = carPartPickup;
                return true;
            }

            var cluePickup = collider.GetComponentInParent<Clues.CluePickup>();
            if (cluePickup != null)
            {
                interactable = cluePickup;
                return true;
            }

            return false;
        }

        #endregion
    }
}
