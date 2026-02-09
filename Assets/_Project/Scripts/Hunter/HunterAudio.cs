using UnityEngine;

namespace TheOrder.Hunter
{
    /// <summary>
    /// Manages Hunter footstep audio with 3D spatial sound.
    /// Footstep interval scales with movement speed.
    /// Mike cannot vocalize — his tongue was surgically removed.
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

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _footstepSource;

        #endregion

        #region Private Fields

        private UnityEngine.AI.NavMeshAgent _agent;
        private float _footstepTimer;

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
        }

        private void Update()
        {
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

            // Determine interval based on speed — threshold from config
            float sprintThreshold = _config != null ? _config.SprintSpeedThreshold : 4f;
            float interval = speed >= sprintThreshold ? _runStepInterval : _walkStepInterval;

            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0f;
                PlayFootstep(speed >= sprintThreshold);
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
    }
}
