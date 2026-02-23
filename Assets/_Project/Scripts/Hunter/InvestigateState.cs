using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Investigate state — Hunter goes to the source of a disturbance.
    /// Plays Looking Around animation at destination.
    /// On completion, returns to last patrol position and transitions to Patrol.
    /// Safety timer only fires if Hunter gets stuck navigating (60s default).
    /// </summary>
    public class InvestigateState : IHunterState
    {
        #region Private Fields

        private readonly HunterAI _ai;
        private float _safetyTimer;
        private bool _hasReachedTarget;
        private bool _isLookingAround;
        private float _lookAroundTimer;
        private bool _isReturningToPatrol;

        #endregion

        #region Constructor

        public InvestigateState(HunterAI ai)
        {
            _ai = ai;
        }

        #endregion

        #region IHunterState

        public void Enter()
        {
            _ai.Agent.speed = _ai.Config.InvestigateSpeed;
            _ai.SetLooking(false);
            _safetyTimer = _ai.Config.InvestigateTimeout;
            _hasReachedTarget = false;
            _isLookingAround = false;
            _isReturningToPatrol = false;

            // Navigate to the most recent known position (seen or heard)
            Vector3 target = _ai.GetMostRecentKnownPosition();
            _ai.NavigateTo(target);
        }

        public void Update()
        {
            _safetyTimer -= Time.deltaTime;

            // Always check for player visibility — transition to Chase
            if (_ai.CanSeePlayer())
            {
                _ai.UpdateLastSeenPosition(_ai.PlayerPosition);
                _ai.TransitionToChase();
                return;
            }

            // Check for doors on the way
            _ai.CheckForDoors();

            // If returning to patrol position
            if (_isReturningToPatrol)
            {
                if (_ai.HasReachedDestination())
                {
                    _ai.TransitionToPatrol();
                }
                return;
            }

            // If looking around at the target
            if (_isLookingAround)
            {
                _lookAroundTimer -= Time.deltaTime;
                if (_lookAroundTimer <= 0f)
                {
                    // Done looking around — return to patrol position
                    StartReturningToPatrol();
                }
                return;
            }

            // If reached the investigation target
            if (!_hasReachedTarget && _ai.HasReachedDestination())
            {
                _hasReachedTarget = true;
                StartLookingAround();
                return;
            }

            // Safety timeout — only if stuck navigating (never reached target)
            if (_safetyTimer <= 0f && !_hasReachedTarget)
            {
                StartReturningToPatrol();
            }
        }

        public void Exit()
        {
            _ai.SetLooking(false);
        }

        #endregion

        #region Investigation Behavior

        private void StartLookingAround()
        {
            _isLookingAround = true;
            _lookAroundTimer = _ai.Config.LookAroundDuration;
            _ai.Agent.ResetPath();
            _ai.SetLooking(true);
        }

        private void StartReturningToPatrol()
        {
            _isReturningToPatrol = true;
            _isLookingAround = false;
            _ai.SetLooking(false);
            _ai.Agent.speed = _ai.Config.PatrolSpeed;
            _ai.NavigateTo(_ai.LastPatrolPosition);
        }

        #endregion
    }
}
