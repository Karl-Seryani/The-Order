using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Patrol state — Hunter walks between waypoints at patrol speed.
    /// Idles briefly at each waypoint before moving on.
    /// Transitions to Investigate on sound, Chase on sight.
    /// </summary>
    public class PatrolState : IHunterState
    {
        #region Private Fields

        private readonly HunterAI _ai;
        private int _currentWaypointIndex;
        private float _idleTimer;
        private float _idleDuration;
        private bool _isIdling;

        #endregion

        #region Constructor

        public PatrolState(HunterAI ai)
        {
            _ai = ai;
        }

        #endregion

        #region IHunterState

        public void Enter()
        {
            _ai.Agent.speed = _ai.Config.PatrolSpeed;
            _ai.SetLooking(false);

            // Resume from last waypoint index if returning from investigate/chase
            _currentWaypointIndex = _ai.LastPatrolWaypointIndex;
            _isIdling = false;

            NavigateToCurrentWaypoint();
        }

        public void Update()
        {
            // Check for player visibility — transition to Chase
            if (_ai.CanSeePlayer())
            {
                _ai.UpdateLastSeenPosition(_ai.PlayerPosition);
                _ai.TransitionToChase();
                return;
            }

            // Check for doors while walking
            _ai.CheckForDoors();

            if (_isIdling)
            {
                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0f)
                {
                    _isIdling = false;
                    _ai.SetLooking(false);
                    AdvanceToNextWaypoint();
                    NavigateToCurrentWaypoint();
                }
                return;
            }

            // Check if reached waypoint
            if (_ai.HasReachedDestination())
            {
                StartIdling();
            }
        }

        public void Exit()
        {
            // Save where we were patrolling so we can return after investigate/chase
            _ai.SavePatrolPosition(_currentWaypointIndex);
        }

        #endregion

        #region Waypoint Navigation

        private void NavigateToCurrentWaypoint()
        {
            if (_ai.PatrolWaypoints == null || _ai.PatrolWaypoints.Length == 0) return;

            Transform waypoint = _ai.PatrolWaypoints[_currentWaypointIndex];
            if (waypoint != null)
            {
                _ai.NavigateTo(waypoint.position);
            }
        }

        private void AdvanceToNextWaypoint()
        {
            if (_ai.PatrolWaypoints == null || _ai.PatrolWaypoints.Length == 0) return;

            _currentWaypointIndex = (_currentWaypointIndex + 1) % _ai.PatrolWaypoints.Length;
        }

        private void StartIdling()
        {
            _isIdling = true;
            _idleDuration = Random.Range(_ai.Config.WaypointIdleMin, _ai.Config.WaypointIdleMax);
            _idleTimer = _idleDuration;
            _ai.Agent.ResetPath();
            _ai.SetLooking(true);
        }

        #endregion
    }
}
