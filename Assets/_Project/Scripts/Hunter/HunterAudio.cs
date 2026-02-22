using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Manages Hunter audio — footsteps with speed-matched intervals,
    /// terrifying sounds during investigation,
    /// and death cinematic sound.
    /// </summary>
    public class HunterAudio : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private HunterConfig _config;

        [Header("Footstep Clips")]
        [SerializeField] private AudioClip[] _walkFootsteps;
        [SerializeField] private AudioClip[] _runFootsteps;

        [Header("Footstep Settings")]
        [SerializeField] private float _walkStepInterval = 0.55f;
        [SerializeField] private float _runStepInterval = 0.3f;
        [SerializeField] private float _footstepVolume = 0.6f;
        [SerializeField] private float _speedThreshold = 0.5f;

        [Header("Investigate / Idle Sounds")]
        [SerializeField] private AudioClip[] _investigateSounds;
        [SerializeField] [Range(0f, 1f)] private float _investigateVolume = 0.7f;

        [Header("Death Cinematic Sound")]
        [SerializeField] private AudioClip _deathCinematicSound;
        [SerializeField] [Range(0f, 1f)] private float _deathCinematicVolume = 0.8f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _footstepSource;

        #endregion

        #region Private Fields

        private UnityEngine.AI.NavMeshAgent _agent;
        private float _footstepTimer;
        private bool _isPaused;
        private bool _isInvestigating;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (_footstepSource == null)
            {
                _footstepSource = gameObject.AddComponent<AudioSource>();
            }
            _footstepSource.spatialBlend = 1f;
            _footstepSource.minDistance = 1f;
            _footstepSource.maxDistance = 20f;
            _footstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _footstepSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            GameEvents.OnHunterStateChanged += HandleHunterStateChanged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnDeathCinematicStart += HandleDeathCinematicStart;
        }

        private void OnDisable()
        {
            GameEvents.OnHunterStateChanged -= HandleHunterStateChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnDeathCinematicStart -= HandleDeathCinematicStart;
        }

        private void Update()
        {
            if (_isPaused) return;
            UpdateFootsteps();
        }

        #endregion

        #region Footsteps

        private void UpdateFootsteps()
        {
            if (_agent == null || _footstepSource == null) return;

            float speed = _agent.velocity.magnitude;

            if (speed < _speedThreshold)
            {
                _footstepTimer = 0f;
                return;
            }

            float sprintThreshold = _config != null ? _config.SprintSpeedThreshold : 4f;
            bool isRunning = speed >= sprintThreshold;
            float interval = isRunning ? _runStepInterval : _walkStepInterval;

            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0f;
                PlayFootstep(isRunning);
            }
        }

        private void PlayFootstep(bool isRunning)
        {
            AudioClip[] clips = isRunning ? _runFootsteps : _walkFootsteps;

            if ((clips == null || clips.Length == 0) && isRunning)
                clips = _walkFootsteps;

            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
                _footstepSource.PlayOneShot(clip, _footstepVolume);
            }
        }

        #endregion

        #region Investigate Sounds

        private void PlayInvestigateSound()
        {
            if (_investigateSounds == null || _investigateSounds.Length == 0) return;
            if (_footstepSource == null) return;

            AudioClip clip = _investigateSounds[Random.Range(0, _investigateSounds.Length)];
            if (clip != null)
            {
                _footstepSource.PlayOneShot(clip, _investigateVolume);
            }
        }

        #endregion

        #region Event Handlers

        private void HandleHunterStateChanged(HunterState newState)
        {
            if (newState == HunterState.Investigate)
            {
                if (!_isInvestigating)
                {
                    _isInvestigating = true;
                    PlayInvestigateSound();
                }
            }
            else
            {
                _isInvestigating = false;
            }
        }

        private void HandleDeathCinematicStart()
        {
            if (_deathCinematicSound != null && _footstepSource != null)
            {
                _footstepSource.PlayOneShot(_deathCinematicSound, _deathCinematicVolume);
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            _isPaused = newState != GameState.Playing;
        }

        #endregion
    }
}
