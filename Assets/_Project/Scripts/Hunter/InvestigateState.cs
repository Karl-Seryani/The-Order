using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Investigate state — Hunter goes to the source of a disturbance.
    /// Plays Looking Around animation at destination.
    /// On timeout, returns to last patrol position and transitions to Patrol.
    /// Also handles post-chase searching (merged Search behavior).
    /// </summary>
    public class InvestigateState : IHunterState
    {
        #region Private Fields

        private readonly HunterAI _ai;
        private float _timer;
        private bool _hasReachedTarget;
        private bool _isLookingAround;
        private float _lookAroundTimer;
        private bool _isReturningToPatrol;

        private const float LOOK_AROUND_DURATION = 4f;

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
            _timer = _ai.Config.InvestigateTimeout;
            _hasReachedTarget = false;
            _isLookingAround = false;
            _isReturningToPatrol = false;

            // Navigate to the most recent known position (seen or heard)
            Vector3 target = _ai.GetMostRecentKnownPosition();
            _ai.NavigateTo(target);
        }

        public void Update()
        {
            _timer -= Time.deltaTime;

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

            // Timeout — give up and return to patrol
            if (_timer <= 0f)
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
            _lookAroundTimer = LOOK_AROUND_DURATION;
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
