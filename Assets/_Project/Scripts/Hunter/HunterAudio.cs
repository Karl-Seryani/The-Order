using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Manages Hunter audio: footsteps (3D spatial) and proximity breathing.
    /// Footstep interval scales with movement speed.
    /// Breathing volume scales with distance to player.
    /// </summary>
    public class HunterAudio : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Footstep Clips")]
        [SerializeField] private AudioClip[] _walkFootsteps;
        [SerializeField] private AudioClip[] _runFootsteps;

        [Header("Breathing Clips")]
        [SerializeField] private AudioClip _breathingLoopNormal;
        [SerializeField] private AudioClip _breathingLoopChase;

        [Header("Chase Stinger")]
        [SerializeField] private AudioClip _chaseStinger;

        [Header("Footstep Settings")]
        [SerializeField] private float _walkStepInterval = 0.55f;
        [SerializeField] private float _runStepInterval = 0.3f;
        [SerializeField] private float _footstepVolume = 0.6f;
        [SerializeField] private float _speedThreshold = 0.5f;

        [Header("Breathing Settings")]
        [SerializeField] private float _breathingMaxDistance = 8f;
        [SerializeField] private float _breathingMinDistance = 1f;
        [SerializeField] private float _breathingMaxVolume = 0.8f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _footstepSource;
        [SerializeField] private AudioSource _breathingSource;

        #endregion

        #region Private Fields

        private UnityEngine.AI.NavMeshAgent _agent;
        private float _footstepTimer;
        private Vector3 _playerPosition;
        private bool _hasPlayerPosition;
        private bool _isChasing;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            // Setup footstep audio source for 3D spatial sound
            if (_footstepSource != null)
            {
                _footstepSource.spatialBlend = 1f;
                _footstepSource.minDistance = 1f;
                _footstepSource.maxDistance = 20f;
                _footstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
                _footstepSource.playOnAwake = false;
            }

            // Setup breathing audio source
            if (_breathingSource != null)
            {
                _breathingSource.spatialBlend = 1f;
                _breathingSource.minDistance = 1f;
                _breathingSource.maxDistance = _breathingMaxDistance;
                _breathingSource.rolloffMode = AudioRolloffMode.Linear;
                _breathingSource.loop = true;
                _breathingSource.playOnAwake = false;
                _breathingSource.volume = 0f;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerMoved += HandlePlayerMoved;
            GameEvents.OnHunterStateChanged += HandleHunterStateChanged;
            GameEvents.OnPlayerDetected += HandlePlayerDetected;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerMoved -= HandlePlayerMoved;
            GameEvents.OnHunterStateChanged -= HandleHunterStateChanged;
            GameEvents.OnPlayerDetected -= HandlePlayerDetected;
        }

        private void Update()
        {
            UpdateFootsteps();
            UpdateBreathing();
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

            // Determine interval based on speed
            float interval = speed >= 4f ? _runStepInterval : _walkStepInterval;

            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0f;
                PlayFootstep(speed >= 4f);
            }
        }

        private void PlayFootstep(bool isRunning)
        {
            AudioClip[] clips = isRunning ? _runFootsteps : _walkFootsteps;

            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
                _footstepSource.PlayOneShot(clip, _footstepVolume);
            }
        }

        #endregion

        #region Breathing

        private void UpdateBreathing()
        {
            if (_breathingSource == null || !_hasPlayerPosition) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _playerPosition);

            if (distanceToPlayer > _breathingMaxDistance)
            {
                // Too far — silence breathing
                _breathingSource.volume = 0f;
                if (_breathingSource.isPlaying)
                {
                    _breathingSource.Pause();
                }
                return;
            }

            // Start playing if not already
            if (!_breathingSource.isPlaying)
            {
                AudioClip targetClip = _isChasing ? _breathingLoopChase : _breathingLoopNormal;
                if (targetClip != null)
                {
                    _breathingSource.clip = targetClip;
                    _breathingSource.Play();
                }
                else
                {
                    return;
                }
            }

            // Scale volume by distance (louder when closer)
            float t = Mathf.InverseLerp(_breathingMaxDistance, _breathingMinDistance, distanceToPlayer);
            _breathingSource.volume = Mathf.Lerp(0f, _breathingMaxVolume, t);
        }

        #endregion

        #region Event Handlers

        private void HandlePlayerMoved(Vector3 position, float speed)
        {
            _playerPosition = position;
            _hasPlayerPosition = true;
        }

        private void HandleHunterStateChanged(HunterState newState)
        {
            bool wasChasing = _isChasing;
            _isChasing = newState == HunterState.Chase;

            // Swap breathing clip if chase state changed
            if (_breathingSource != null && wasChasing != _isChasing)
            {
                AudioClip targetClip = _isChasing ? _breathingLoopChase : _breathingLoopNormal;
                if (targetClip != null && _breathingSource.clip != targetClip)
                {
                    _breathingSource.clip = targetClip;
                    if (_breathingSource.volume > 0f)
                    {
                        _breathingSource.Play();
                    }
                }
            }
        }

        private void HandlePlayerDetected()
        {
            // Play chase stinger when detection triggers chase
            if (_chaseStinger != null && _footstepSource != null)
            {
                _footstepSource.PlayOneShot(_chaseStinger, 0.8f);
            }
        }

        #endregion
    }
}
