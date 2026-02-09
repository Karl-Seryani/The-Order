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

        [Header("Detection — Sight")]
        [SerializeField] private float _sightAngle = 110f;
        [SerializeField] private float _sightRange = 3f;
        [SerializeField] private float _flashlightSightMultiplier = 8f;
        [SerializeField] private float _flashlightConeAngle = 60f;

        [Header("Detection — Hearing")]
        [SerializeField] private float _sprintHearingRadius = 8f;
        [SerializeField] private float _walkHearingRadius = 2f;
        [SerializeField] private float _doorOpenHearingRadius = 15f;
        [SerializeField] private float _sprintSpeedThreshold = 4.0f;

        [Header("Patrol")]
        [SerializeField] private float _waypointIdleMin = 2f;
        [SerializeField] private float _waypointIdleMax = 5f;

        [Header("Investigate")]
        [SerializeField] private float _investigateTimeout = 8f;

        [Header("Chase")]
        [SerializeField] private float _losTimeout = 3f;
        [SerializeField] private float _catchDistance = 1.5f;

        #region Public Accessors

        // Movement
        public float PatrolSpeed => _patrolSpeed;
        public float InvestigateSpeed => _investigateSpeed;
        public float ChaseSpeed => _chaseSpeed;

        // Sight
        public float SightAngle => _sightAngle;
        public float SightRange => _sightRange;
        public float FlashlightSightMultiplier => _flashlightSightMultiplier;
        public float FlashlightConeAngle => _flashlightConeAngle;

        // Hearing
        public float SprintHearingRadius => _sprintHearingRadius;
        public float WalkHearingRadius => _walkHearingRadius;
        public float DoorOpenHearingRadius => _doorOpenHearingRadius;
        public float SprintSpeedThreshold => _sprintSpeedThreshold;

        // Patrol
        public float WaypointIdleMin => _waypointIdleMin;
        public float WaypointIdleMax => _waypointIdleMax;

        // Investigate
        public float InvestigateTimeout => _investigateTimeout;

        // Chase
        public float LosTimeout => _losTimeout;
        public float CatchDistance => _catchDistance;

        #endregion
    }
}
