using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Chase state — Hunter pursues the player at full speed.
    /// Catches the player if within CatchDistance (instant death, reload scene).
    /// 3-second LOS grace: if player leaves vision cone, waits 3s before transitioning.
    /// Opens closed doors in the way.
    /// </summary>
    public class ChaseState : IHunterState
    {
        #region Private Fields

        private readonly HunterAI _ai;
        private float _losTimer;
        private bool _hasLos;
        private float _repathTimer;

        private const float REPATH_INTERVAL = 0.2f;

        #endregion

        #region Constructor

        public ChaseState(HunterAI ai)
        {
            _ai = ai;
        }

        #endregion

        #region IHunterState

        public void Enter()
        {
            _ai.Agent.speed = _ai.Config.ChaseSpeed;
            _ai.SetLooking(false);
            _hasLos = true;
            _losTimer = _ai.Config.LosTimeout;
            _repathTimer = 0f;

            GameEvents.PlayerDetected();

            // Immediately path to player
            if (_ai.HasPlayerPosition)
            {
                _ai.NavigateTo(_ai.PlayerPosition);
            }
        }

        public void Update()
        {
            // Check if player is visible
            bool canSee = _ai.CanSeePlayer();

            if (canSee)
            {
                // Update last seen position
                _ai.UpdateLastSeenPosition(_ai.PlayerPosition);
                _hasLos = true;
                _losTimer = _ai.Config.LosTimeout;
            }
            else
            {
                // Start or continue the LOS grace timer
                _hasLos = false;
                _losTimer -= Time.deltaTime;

                if (_losTimer <= 0f)
                {
                    // Lost the player — transition to Investigate at last known position
                    GameEvents.PlayerLost();
                    _ai.TransitionToInvestigate(_ai.GetMostRecentKnownPosition());
                    return;
                }
            }

            // Check for catch
            if (_ai.HasPlayerPosition)
            {
                float distanceToPlayer = Vector3.Distance(_ai.transform.position, _ai.PlayerPosition);
                if (distanceToPlayer <= _ai.Config.CatchDistance)
                {
                    _ai.CatchPlayer();
                    return;
                }
            }

            // Repath to player position periodically
            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = REPATH_INTERVAL;

                if (_hasLos && _ai.HasPlayerPosition)
                {
                    // Path directly to player when visible
                    _ai.NavigateTo(_ai.PlayerPosition);
                }
                else if (_ai.HasLastKnownPosition)
                {
                    // Path to last known position when not visible
                    _ai.NavigateTo(_ai.GetMostRecentKnownPosition());
                }
            }

            // Check for doors
            _ai.CheckForDoors();
        }

        public void Exit()
        {
            // Nothing special on exit
        }

        #endregion
    }
}
