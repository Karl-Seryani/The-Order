using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// ScriptableObject containing all Hunter AI configuration parameters.
    /// Tuned via playtesting — no hardcoded AI values in scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "HunterConfig", menuName = "The Order/Hunter Config")]
    public class HunterConfig : ScriptableObject
    {
        [Header("Movement Speeds")]
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _investigateSpeed = 3.5f;
        [SerializeField] private float _chaseSpeed = 5.5f;
        [SerializeField] private float _searchSpeed = 3f;
        [SerializeField] private float _chaseSpeedMultiplier = 1.1f;

        [Header("Detection — Sight")]
        [SerializeField] private float _sightAngle = 110f;
        [SerializeField] private float _sightRange = 15f;
        [SerializeField] private float _flashlightSightMultiplier = 2f;

        [Header("Detection — Hearing")]
        [SerializeField] private float _sprintHearingRadius = 12f;
        [SerializeField] private float _walkHearingRadius = 2f;
        [SerializeField] private float _doorOpenHearingRadius = 15f;

        [Header("Detection — Proximity")]
        [SerializeField] private float _proximityDetectionRange = 2f;

        [Header("Detection — Meter")]
        [SerializeField] private float _detectionFillRate = 1f;
        [SerializeField] private float _detectionDecayRate = 0.5f;
        [SerializeField] private float _detectionThreshold = 1f;

        [Header("Patrol")]
        [SerializeField] private float _waypointIdleMin = 2f;
        [SerializeField] private float _waypointIdleMax = 5f;

        [Header("Investigate")]
        [SerializeField] private float _investigateTimeout = 8f;
        [SerializeField] private int _investigateCheckSpots = 3;

        [Header("Chase")]
        [SerializeField] private float _losTimeout = 3f;
        [SerializeField] private float _catchDistance = 1.5f;

        [Header("Search")]
        [SerializeField] private float _searchTimeout = 15f;
        [SerializeField] private float _searchRadius = 10f;
        [SerializeField] private float _elevatedAlertDuration = 30f;

        #region Public Accessors

        // Movement
        public float PatrolSpeed => _patrolSpeed;
        public float InvestigateSpeed => _investigateSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float SearchSpeed => _searchSpeed;
        public float ChaseSpeedMultiplier => _chaseSpeedMultiplier;

        // Sight
        public float SightAngle => _sightAngle;
        public float SightRange => _sightRange;
        public float FlashlightSightMultiplier => _flashlightSightMultiplier;

        // Hearing
        public float SprintHearingRadius => _sprintHearingRadius;
        public float WalkHearingRadius => _walkHearingRadius;
        public float DoorOpenHearingRadius => _doorOpenHearingRadius;

        // Proximity
        public float ProximityDetectionRange => _proximityDetectionRange;

        // Detection Meter
        public float DetectionFillRate => _detectionFillRate;
        public float DetectionDecayRate => _detectionDecayRate;
        public float DetectionThreshold => _detectionThreshold;

        // Patrol
        public float WaypointIdleMin => _waypointIdleMin;
        public float WaypointIdleMax => _waypointIdleMax;

        // Investigate
        public float InvestigateTimeout => _investigateTimeout;
        public int InvestigateCheckSpots => _investigateCheckSpots;

        // Chase
        public float LosTimeout => _losTimeout;
        public float CatchDistance => _catchDistance;

        // Search
        public float SearchTimeout => _searchTimeout;
        public float SearchRadius => _searchRadius;
        public float ElevatedAlertDuration => _elevatedAlertDuration;

        #endregion
    }
}
