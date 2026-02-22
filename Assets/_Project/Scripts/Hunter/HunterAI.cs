using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Main Hunter AI controller. Manages detection (sight + sound),
    /// state machine transitions, and NavMesh navigation.
    /// Communicates via GameEvents only — no direct references to player systems.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class HunterAI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private HunterConfig _config;

        [Header("References")]
        [SerializeField] private Transform _eyePoint;
        [SerializeField] private Transform[] _patrolWaypoints;

        [Header("Detection Layers")]
        [SerializeField] private LayerMask _playerLayer;

        #endregion

        #region Private Fields

        private NavMeshAgent _agent;
        private Animator _animator;
        private HunterStateMachine _stateMachine;

        // State instances
        private PatrolState _patrolState;
        private InvestigateState _investigateState;
        private ChaseState _chaseState;

        // Player tracking — updated via events
        private Vector3 _playerPosition;
        private Vector3 _playerForward;
        private float _playerSpeed;
        private bool _playerFlashlightOn;
        private bool _hasPlayerPosition;

        // Detection tracking
        private Vector3 _lastKnownPlayerPosition;
        private Vector3 _lastHeardPosition;
        private bool _hasLastKnownPosition;
        private bool _hasLastHeardPosition;
        private float _lastHeardTime;
        private float _lastSeenTime;

        // Patrol tracking
        private Vector3 _lastPatrolPosition;
        private int _lastPatrolWaypointIndex;

#if UNITY_EDITOR
        private float _debugTimer;
#endif

        // Paused state
        private bool _isPaused;

        // Smoothed speed for animator (prevents idle flicker)
        private float _smoothedSpeed;

        // Door self-ignore (prevents investigating own door opens)
        private float _lastDoorOpenTime = -Mathf.Infinity;
        private Vector3 _lastDoorOpenPosition;

        // Preallocated raycast buffer (avoids GC allocs every frame)
        private readonly RaycastHit[] _raycastBuffer = new RaycastHit[16];
        private static readonly HitDistanceComparer _hitDistanceComparer = new();


        #endregion

        #region Public API

        /// <summary>The Hunter's current FSM state type.</summary>
        public HunterState CurrentState => _stateMachine.CurrentStateType;

        /// <summary>The HunterConfig ScriptableObject.</summary>
        public HunterConfig Config => _config;

        /// <summary>The NavMeshAgent on this Hunter.</summary>
        public NavMeshAgent Agent => _agent;

        /// <summary>The Animator on this Hunter.</summary>
        public Animator HunterAnimator => _animator;

        /// <summary>Patrol waypoints assigned in inspector.</summary>
        public Transform[] PatrolWaypoints => _patrolWaypoints;

        /// <summary>Cached player position from last event.</summary>
        public Vector3 PlayerPosition => _playerPosition;

        /// <summary>True if we have received at least one player position.</summary>
        public bool HasPlayerPosition => _hasPlayerPosition;

        /// <summary>Last position where the player was seen or heard.</summary>
        public Vector3 LastKnownPlayerPosition => _lastKnownPlayerPosition;

        /// <summary>True if we have a last known player position.</summary>
        public bool HasLastKnownPosition => _hasLastKnownPosition;

        /// <summary>Last position where a sound was heard.</summary>
        public Vector3 LastHeardPosition => _lastHeardPosition;

        /// <summary>True if we have a last heard position.</summary>
        public bool HasLastHeardPosition => _hasLastHeardPosition;

        /// <summary>Position where the Hunter was when last in Patrol state.</summary>
        public Vector3 LastPatrolPosition => _lastPatrolPosition;

        /// <summary>The waypoint index the Hunter was heading toward when leaving Patrol.</summary>
        public int LastPatrolWaypointIndex => _lastPatrolWaypointIndex;

        /// <summary>Whether the player's flashlight is currently on.</summary>
        public bool PlayerFlashlightOn => _playerFlashlightOn;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            // Ensure root motion is off — NavMeshAgent controls position
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
            }

            _stateMachine = new HunterStateMachine();

            _patrolState = new PatrolState(this);
            _investigateState = new InvestigateState(this);
            _chaseState = new ChaseState(this);
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerMoved += HandlePlayerMoved;
            GameEvents.OnPlayerFacingChanged += HandlePlayerFacingChanged;
            GameEvents.OnFlashlightToggled += HandleFlashlightToggled;
            GameEvents.OnDoorOpened += HandleDoorOpened;
            GameEvents.OnInteractableNoise += HandleInteractableNoise;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            Debug.Log($"[HunterAI] OnEnable — subscribed to events, isPaused={_isPaused}");
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerMoved -= HandlePlayerMoved;
            GameEvents.OnPlayerFacingChanged -= HandlePlayerFacingChanged;
            GameEvents.OnFlashlightToggled -= HandleFlashlightToggled;
            GameEvents.OnDoorOpened -= HandleDoorOpened;
            GameEvents.OnInteractableNoise -= HandleInteractableNoise;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            if (_config == null)
            {
                Debug.LogError("[HunterAI] No HunterConfig assigned!", this);
                _isPaused = true;
                return;
            }

            if (_patrolWaypoints == null || _patrolWaypoints.Length == 0)
            {
                Debug.LogError("[HunterAI] No patrol waypoints assigned!", this);
                _isPaused = true;
                return;
            }

            // Practice mode — no Hunter at all
            if (GameManager.Instance != null && !GameManager.Instance.HunterEnabled)
            {
                Debug.Log("[HunterAI] Practice mode — deactivating Hunter.");
                gameObject.SetActive(false);
                return;
            }

            Debug.Log($"[HunterAI] Starting — {_patrolWaypoints.Length} waypoints, " +
                      $"onNavMesh={_agent.isOnNavMesh}, " +
                      $"pos={transform.position}, " +
                      $"playerLayer={_playerLayer.value}, " +
                      $"eyePoint={((_eyePoint != null) ? "assigned" : "MISSING")}");

            _lastPatrolPosition = transform.position;
            _stateMachine.ChangeState(_patrolState, HunterState.Patrol);
        }

        private void Update()
        {
            if (_isPaused) return;

            _stateMachine.Update();
            UpdateAnimator();

#if UNITY_EDITOR
            // Debug: periodic state info
            _debugTimer -= Time.deltaTime;
            if (_debugTimer <= 0f)
            {
                _debugTimer = 3f;
                Debug.Log($"[HunterAI] State={_stateMachine.CurrentStateType}, " +
                          $"speed={_agent.velocity.magnitude:F1}, " +
                          $"hasPlayerPos={_hasPlayerPosition}, " +
                          $"onNavMesh={_agent.isOnNavMesh}, " +
                          $"canSee={(_hasPlayerPosition ? CanSeePlayer().ToString() : "no_pos")}");
            }
#endif
        }

        #endregion

        #region Detection — Sight

        /// <summary>
        /// Check if the player is currently visible to the Hunter.
        /// Uses distance, angle, and obstruction raycast.
        /// Casts on all layers — first non-self hit determines visibility.
        /// </summary>
        public bool CanSeePlayer()
        {
            if (!_hasPlayerPosition) return false;

            Vector3 eyePos = _eyePoint != null ? _eyePoint.position : transform.position;
            Vector3 playerCenter = _playerPosition + Vector3.up * 0.8f;
            Vector3 toPlayer = playerCenter - eyePos;
            float distance = toPlayer.magnitude;

            // Range check (doubled if player flashlight is on)
            if (distance > GetEffectiveSightRange()) return false;

            // Angle check
            float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
            if (angle > _config.SightAngle * 0.5f) return false;

            // Obstruction check — cast on ALL layers, skip self colliders
            Vector3 dir = toPlayer.normalized;
            int hitCount = Physics.RaycastNonAlloc(eyePos, dir, _raycastBuffer, distance, ~0, QueryTriggerInteraction.Ignore);

            // Sort by distance, find first non-self hit
            System.Array.Sort(_raycastBuffer, 0, hitCount, _hitDistanceComparer);
            for (int i = 0; i < hitCount; i++)
            {
                if (IsSelfCollider(_raycastBuffer[i].collider)) continue;
                // First non-self hit — is it the player?
                return ((1 << _raycastBuffer[i].collider.gameObject.layer) & _playerLayer) != 0;
            }

            // Nothing hit between eye and player — clear line of sight
            return true;
        }

        /// <summary>
        /// Returns the effective sight range, accounting for flashlight multiplier.
        /// </summary>
        public float GetEffectiveSightRange()
        {
            float range = _config.SightRange;
            if (_playerFlashlightOn)
            {
                range *= _config.FlashlightSightMultiplier;
            }
            return range;
        }

        /// <summary>
        /// Check if the player is within the sight cone (angle + range) without obstruction check.
        /// Useful for testing.
        /// </summary>
        public static bool IsInSightCone(Vector3 hunterPos, Vector3 hunterForward, Vector3 playerPos, float sightRange, float sightAngle)
        {
            Vector3 direction = playerPos - hunterPos;
            float distance = direction.magnitude;

            if (distance > sightRange) return false;

            float angle = Vector3.Angle(hunterForward, direction.normalized);
            return angle <= sightAngle * 0.5f;
        }

        #endregion

        #region Detection — Flashlight

        /// <summary>
        /// Check if the player's flashlight beam is hitting the Hunter.
        /// The flashlight is a cone — if the Hunter is inside that cone and within range,
        /// the Hunter senses the light regardless of which direction they're facing.
        /// A flashlight shining on you from behind is still noticeable.
        /// </summary>
        public bool IsFlashlightHittingHunter()
        {
            if (!_hasPlayerPosition || !_playerFlashlightOn) return false;

            Vector3 hunterPos = _eyePoint != null ? _eyePoint.position : transform.position;
            Vector3 playerCenter = _playerPosition + Vector3.up * 0.8f;

            return IsFlashlightHittingTarget(
                hunterPos, playerCenter, _playerForward,
                _config.FlashlightConeAngle, GetEffectiveSightRange());
        }

        /// <summary>
        /// Static testable version: checks if a target is inside the player's flashlight cone.
        /// Only requires the target to be within the flashlight's cone angle and range.
        /// The target's facing direction doesn't matter — light hits you from any angle.
        /// </summary>
        public static bool IsFlashlightHittingTarget(
            Vector3 targetPos, Vector3 playerPos, Vector3 playerForward,
            float flashlightConeAngle, float maxRange)
        {
            Vector3 playerToTarget = targetPos - playerPos;
            float distance = playerToTarget.magnitude;

            // Must be within flashlight range
            if (distance > maxRange) return false;

            // Is the target within the flashlight cone?
            float angle = Vector3.Angle(playerForward, playerToTarget.normalized);
            return angle <= flashlightConeAngle * 0.5f;
        }

        #endregion

        #region Helpers

        private bool IsSelfCollider(Collider collider)
        {
            if (collider == null) return false;
            Transform t = collider.transform;
            return t == transform || t.IsChildOf(transform);
        }

        #endregion

        #region Detection — Sound

        /// <summary>
        /// Check if a sound at a given position and radius can be heard by the Hunter.
        /// </summary>
        public static bool IsInHearingRange(Vector3 hunterPos, Vector3 soundPos, float hearingRadius)
        {
            return Vector3.Distance(hunterPos, soundPos) <= hearingRadius;
        }

        #endregion

        #region State Transitions

        /// <summary>Transition to the Patrol state.</summary>
        public void TransitionToPatrol()
        {
            _stateMachine.ChangeState(_patrolState, HunterState.Patrol);
        }

        /// <summary>Transition to the Investigate state with a target position.</summary>
        public void TransitionToInvestigate(Vector3 targetPosition)
        {
            _lastKnownPlayerPosition = targetPosition;
            _hasLastKnownPosition = true;
            _stateMachine.ChangeState(_investigateState, HunterState.Investigate);
        }

        /// <summary>Transition to the Chase state.</summary>
        public void TransitionToChase()
        {
            _stateMachine.ChangeState(_chaseState, HunterState.Chase);
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate to a target position, validating it against the NavMesh first.
        /// </summary>
        public bool NavigateTo(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                return true;
            }

            Debug.LogWarning("[HunterAI] Could not find NavMesh position near: " + position, this);
            return false;
        }

        /// <summary>Check if the agent has reached its current destination.</summary>
        public bool HasReachedDestination()
        {
            if (_agent.pathPending) return false;
            return _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
        }

        #endregion

        #region Patrol Tracking

        /// <summary>Save the current position as the last patrol position.</summary>
        public void SavePatrolPosition(int waypointIndex)
        {
            _lastPatrolPosition = transform.position;
            _lastPatrolWaypointIndex = waypointIndex;
        }

        #endregion

        #region Door Handling

        /// <summary>
        /// Raycast forward to detect closed doors and open them.
        /// Called from Chase and Investigate states.
        /// Automatically closes doors after passing through.
        /// </summary>
        public void CheckForDoors()
        {
            if (!_config.CanOpenDoors) return;

            Vector3 rayOrigin = transform.position + Vector3.up * 0.8f;
            if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, 2f))
            {
                // Check the hit object and all parents for a DoorController
                Doors.DoorController door = hit.collider.GetComponentInParent<Doors.DoorController>();
                if (door != null && !door.IsOpen && !door.IsAnimating)
                {
                    // Hunter cannot open locked doors
                    var lockedDoor = door.GetComponent<Doors.LockedDoor>();
                    if (lockedDoor != null && !lockedDoor.IsUnlocked) return;

                    _lastDoorOpenTime = Time.time;
                    _lastDoorOpenPosition = door.transform.position;
                    door.OpenDoor();
                    StartCoroutine(DelayedDoorClose(door));
                }
            }
        }

        private IEnumerator DelayedDoorClose(Doors.DoorController door)
        {
            yield return new WaitForSeconds(3f);

            if (door == null || !door.IsOpen || door.IsAnimating) yield break;

            // Only close if the Hunter has moved away from the door
            float distanceToDoor = Vector3.Distance(transform.position, door.transform.position);
            if (distanceToDoor > 3f)
            {
                door.CloseDoor();
            }
        }

        #endregion

        // Animator hashes
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsLookingHash = Animator.StringToHash("IsLooking");

        #region Animator

        private void UpdateAnimator()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;

            float speed = _agent.velocity.magnitude;

            // Clamp minimum speed while agent has a destination to prevent
            // idle flicker during repathing velocity dips
            if (!_agent.isStopped && _agent.hasPath && !_agent.pathPending)
            {
                HunterState state = _stateMachine.CurrentStateType;
                float minSpeed = state switch
                {
                    HunterState.Chase => _config.ChaseSpeed * 0.5f,
                    HunterState.Investigate => _config.InvestigateSpeed * 0.5f,
                    HunterState.Patrol => _config.PatrolSpeed * 0.5f,
                    _ => 0f
                };
                speed = Mathf.Max(speed, minSpeed);
            }

            _animator.SetFloat(SpeedHash, speed);
        }

        /// <summary>Set the Looking Around animation parameter.</summary>
        public void SetLooking(bool isLooking)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            _animator.SetBool(IsLookingHash, isLooking);
        }

        #endregion

        #region Game Over

        /// <summary>
        /// Called when the Hunter catches the player. Fires cinematic event.
        /// DeathCinematic handles the sequence and fires PlayerCaught when done.
        /// </summary>
        public void CatchPlayer()
        {
            Debug.Log("[HunterAI] Player caught! Starting death cinematic.");
            _isPaused = true;
            _agent.isStopped = true;
            GameEvents.DeathCinematicStart();
        }

        /// <summary>
        /// Play the attack animation. Called by DeathCinematic.
        /// </summary>
        public void PlayAttack()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            _animator.Play("attack1", 0, 0f);
        }

        /// <summary>
        /// Returns a transform for the camera to look at during death cinematic.
        /// Tries to find the upper chest bone, falls back to transform + Y offset.
        /// </summary>
        public Vector3 GetLookTarget()
        {
            // Try common Mixamo bone names for upper chest
            string[] boneNames = { "spine_02", "Spine2", "Character1_Spine2", "mixamorig:Spine2" };
            foreach (string boneName in boneNames)
            {
                Transform bone = FindChildRecursive(transform, boneName);
                if (bone != null) return bone.position;
            }

            // Fallback — chest height offset
            return transform.position + Vector3.up * 1.2f;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        #endregion

        #region Event Handlers

        private void HandlePlayerMoved(Vector3 position, float speed)
        {
            if (_config == null || _isPaused) return;

            // Clamp to sane max — CharacterController.velocity can spike on first grounded frame
            speed = Mathf.Min(speed, 10f);

            if (!_hasPlayerPosition)
            {
                Debug.Log($"[HunterAI] First player position received: {position}, speed={speed:F1}");
            }
            _playerPosition = position;
            _playerSpeed = speed;
            _hasPlayerPosition = true;

            // Sound detection (disabled in Easy mode — sight only)
            bool fullDetection = GameManager.Instance == null || GameManager.Instance.HunterFullDetection;

            if (fullDetection)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, position);

                // Sprint footsteps heard within configured radius
                if (speed > _config.SprintSpeedThreshold)
                {
                    if (distanceToPlayer <= _config.SprintHearingRadius)
                    {
                        RegisterSound(position);
                    }
                }
                // Walking footsteps — only heard within 2m
                else if (speed > 0.1f)
                {
                    if (distanceToPlayer <= _config.WalkHearingRadius)
                    {
                        RegisterSound(position);
                    }
                }

                // Flashlight cone intersection — player shining light into Hunter's view
                if (_playerFlashlightOn)
                {
                    CheckFlashlightDetection();
                }
            }
        }

        /// <summary>
        /// Check if player's flashlight is hitting the Hunter.
        /// If so, register the player's position as a sound (triggers investigate).
        /// </summary>
        private void CheckFlashlightDetection()
        {
            if (IsFlashlightHittingHunter())
            {
                RegisterSound(_playerPosition);
            }
        }

        private void HandlePlayerFacingChanged(Vector3 forward)
        {
            _playerForward = forward;
        }

        private void HandleFlashlightToggled(bool isOn)
        {
            _playerFlashlightOn = isOn;

            // If flashlight just turned on, immediately check cone intersection (full detection only)
            if (isOn && _hasPlayerPosition &&
                (GameManager.Instance == null || GameManager.Instance.HunterFullDetection))
            {
                CheckFlashlightDetection();
            }
        }

        private void HandleDoorOpened(Vector3 position)
        {
            // Easy mode — sight only, no sound detection
            if (GameManager.Instance != null && !GameManager.Instance.HunterFullDetection) return;

            // Ignore doors the Hunter opened himself (within 1s and 5m of last self-open)
            if (Time.time - _lastDoorOpenTime < 1f &&
                Vector3.Distance(_lastDoorOpenPosition, position) < 5f)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, position);
            if (distance <= _config.DoorOpenHearingRadius)
            {
                RegisterSound(position);
            }
        }

        private void HandleInteractableNoise(Vector3 position, float loudness)
        {
            // Easy mode — sight only, no sound detection
            if (GameManager.Instance != null && !GameManager.Instance.HunterFullDetection) return;

            if (loudness < 0.5f) return;

            float distance = Vector3.Distance(transform.position, position);
            // Scale hearing range with loudness (quiet = short range, loud = full door range)
            float hearingRange = _config.DoorOpenHearingRadius * loudness;
            if (distance <= hearingRange)
            {
                RegisterSound(position);
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            _isPaused = newState != GameState.Playing;

            if (newState == GameState.Playing)
            {
                // Reset detection state on respawn so Hunter doesn't chase stale data
                _hasLastKnownPosition = false;
                _hasLastHeardPosition = false;
                _lastSeenTime = -Mathf.Infinity;
                _lastHeardTime = -Mathf.Infinity;

                // Return to patrol on respawn
                if (_stateMachine != null && _stateMachine.CurrentStateType != HunterState.Patrol)
                {
                    _stateMachine.ChangeState(_patrolState, HunterState.Patrol);
                }
            }

            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = _isPaused;
            }
        }

        #endregion

        #region Sound Registration

        private void RegisterSound(Vector3 position)
        {
            _lastHeardPosition = position;
            _hasLastHeardPosition = true;
            _lastHeardTime = Time.time;

            // React to sound based on current state
            HunterState currentState = _stateMachine.CurrentStateType;

            if (currentState == HunterState.Patrol || currentState == HunterState.Investigate)
            {
                // Patrol or Investigate — go investigate the new sound source
                TransitionToInvestigate(position);
            }
            // In Chase — already pursuing, sound position updates silently
        }

        /// <summary>
        /// Get the most recent known position (seen or heard).
        /// Returns the one with the most recent timestamp.
        /// </summary>
        public Vector3 GetMostRecentKnownPosition()
        {
            if (_lastSeenTime >= _lastHeardTime && _hasLastKnownPosition)
                return _lastKnownPlayerPosition;
            if (_hasLastHeardPosition)
                return _lastHeardPosition;
            if (_hasLastKnownPosition)
                return _lastKnownPlayerPosition;
            return transform.position;
        }

        /// <summary>Update the last seen position and timestamp.</summary>
        public void UpdateLastSeenPosition(Vector3 position)
        {
            _lastKnownPlayerPosition = position;
            _hasLastKnownPosition = true;
            _lastSeenTime = Time.time;
        }

        #endregion
    }

    /// <summary>Comparer for sorting RaycastHit by distance (avoids lambda alloc).</summary>
    internal class HitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
