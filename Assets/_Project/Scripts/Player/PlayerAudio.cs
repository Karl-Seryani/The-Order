using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Manages player (John) breathing audio based on game state.
    /// Idle breathing when stationary, shocked gasp on chase detection,
    /// and relief breathing after escaping a chase.
    /// Communicates via GameEvents only.
    /// </summary>
    public class PlayerAudio : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Breathing Clips")]
        [SerializeField] private AudioClip _chaseShockClip;
        [SerializeField] private AudioClip _idleBreathingClip;
        [SerializeField] private AudioClip _postChaseBreathClip;

        [Header("Settings")]
        [SerializeField] private float _idleDelay = 5f;
        [SerializeField] private float _postChaseDuration = 4f;
        [SerializeField] [Range(0f, 1f)] private float _breathVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float _shockVolume = 0.9f;

        [Header("Audio Source")]
        [SerializeField] private AudioSource _breathSource;

        #endregion

        #region Private Fields

        private float _idleTimer;
        private float _postChaseTimer;
        private BreathState _state = BreathState.Moving;
        private float _currentSpeed;
        private bool _hasMovedOnce;

        private const float SPEED_THRESHOLD = 0.5f;

        #endregion

        #region Breath State Enum

        private enum BreathState
        {
            Moving,
            Idle,
            InChase,
            PostChase
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_breathSource != null)
            {
                _breathSource.spatialBlend = 0f;
                _breathSource.playOnAwake = false;
                _breathSource.loop = false;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerMoved += HandlePlayerMoved;
            GameEvents.OnPlayerDetected += HandlePlayerDetected;
            GameEvents.OnPlayerLost += HandlePlayerLost;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerMoved -= HandlePlayerMoved;
            GameEvents.OnPlayerDetected -= HandlePlayerDetected;
            GameEvents.OnPlayerLost -= HandlePlayerLost;
        }

        private void Update()
        {
            switch (_state)
            {
                case BreathState.Moving:
                    UpdateMovingState();
                    break;

                case BreathState.Idle:
                    UpdateIdleState();
                    break;

                case BreathState.InChase:
                    // Nothing to update — waiting for chase to end via event
                    break;

                case BreathState.PostChase:
                    UpdatePostChaseState();
                    break;
            }
        }

        #endregion

        #region State Updates

        private void UpdateMovingState()
        {
            // Don't start idle breathing until the player has actually moved at least once
            if (!_hasMovedOnce) return;

            if (_currentSpeed <= SPEED_THRESHOLD)
            {
                _idleTimer += Time.deltaTime;

                if (_idleTimer >= _idleDelay)
                {
                    TransitionTo(BreathState.Idle);
                }
            }
            else
            {
                _idleTimer = 0f;
            }
        }

        private void UpdateIdleState()
        {
            // If player starts moving, stop idle breathing
            if (_currentSpeed > SPEED_THRESHOLD)
            {
                TransitionTo(BreathState.Moving);
            }
        }

        private void UpdatePostChaseState()
        {
            _postChaseTimer -= Time.deltaTime;

            if (_postChaseTimer <= 0f)
            {
                TransitionTo(BreathState.Moving);
            }
        }

        #endregion

        #region State Transitions

        private void TransitionTo(BreathState newState)
        {
            // Exit current state
            ExitState(_state);

            _state = newState;

            // Enter new state
            EnterState(newState);
        }

        private void EnterState(BreathState state)
        {
            switch (state)
            {
                case BreathState.Moving:
                    _idleTimer = 0f;
                    break;

                case BreathState.Idle:
                    StartBreathingLoop(_idleBreathingClip);
                    break;

                case BreathState.InChase:
                    // Stop any current loop and play shock one-shot
                    StopBreathing();
                    PlayShockGasp();
                    break;

                case BreathState.PostChase:
                    _postChaseTimer = _postChaseDuration;
                    StartBreathingLoop(_postChaseBreathClip);
                    break;
            }
        }

        private void ExitState(BreathState state)
        {
            switch (state)
            {
                case BreathState.Idle:
                case BreathState.PostChase:
                    StopBreathing();
                    break;
            }
        }

        #endregion

        #region Audio Playback

        private void StartBreathingLoop(AudioClip clip)
        {
            if (_breathSource == null || clip == null) return;

            _breathSource.clip = clip;
            _breathSource.loop = true;
            _breathSource.volume = _breathVolume;
            _breathSource.Play();
        }

        private void PlayShockGasp()
        {
            if (_breathSource == null || _chaseShockClip == null) return;

            _breathSource.PlayOneShot(_chaseShockClip, _shockVolume);
        }

        private void StopBreathing()
        {
            if (_breathSource == null) return;

            _breathSource.loop = false;
            _breathSource.Stop();
        }

        #endregion

        #region Event Handlers

        private void HandlePlayerMoved(Vector3 position, float speed)
        {
            _currentSpeed = speed;

            // Track that the player has pressed WASD at least once
            if (!_hasMovedOnce && speed > SPEED_THRESHOLD)
            {
                _hasMovedOnce = true;
            }
        }

        private void HandlePlayerDetected()
        {
            // Chase just started — shocked gasp
            TransitionTo(BreathState.InChase);
        }

        private void HandlePlayerLost()
        {
            // Chase ended — relief breathing
            if (_state == BreathState.InChase)
            {
                TransitionTo(BreathState.PostChase);
            }
        }

        #endregion
    }
}
