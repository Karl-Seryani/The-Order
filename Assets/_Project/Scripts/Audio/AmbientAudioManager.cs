using UnityEngine;

namespace TheOrder.Audio
{
    /// <summary>
    /// Manages ambient audio atmosphere: looping drone, door SFX, and footstep sounds.
    /// Subscribes to GameEvents for audio triggers.
    /// </summary>
    public class AmbientAudioManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private AudioConfig _config;
        [SerializeField] private AudioSource _ambientSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _interactionSource;

        #endregion

        #region Private Fields

        private float _footstepTimer;
        private float _lastMoveTime;

        private float _lastNoiseTime;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (_config == null || _ambientSource == null) return;

            // Start ambient loop
            if (_config.AmbientLoopClip != null)
            {
                _ambientSource.clip = _config.AmbientLoopClip;
                _ambientSource.volume = _config.AmbientVolume;
                _ambientSource.loop = true;
                _ambientSource.Play();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnDoorOpened += HandleDoorOpened;
            GameEvents.OnDoorClosed += HandleDoorClosed;
            GameEvents.OnPlayerMoved += HandlePlayerMoved;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnInteractableNoise += HandleInteractableNoise;
        }

        private void OnDisable()
        {
            GameEvents.OnDoorOpened -= HandleDoorOpened;
            GameEvents.OnDoorClosed -= HandleDoorClosed;
            GameEvents.OnPlayerMoved -= HandlePlayerMoved;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnInteractableNoise -= HandleInteractableNoise;
        }

        private void Update()
        {
            // Reset movement tracking if no move event received recently
            if (Time.time - _lastMoveTime > 0.2f)
            {
                _footstepTimer = 0f;
            }

            // Fade out interaction sound if no noise received recently
            if (_interactionSource != null && _interactionSource.isPlaying
                && Time.time - _lastNoiseTime > 0.2f)
            {
                _interactionSource.Stop();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleDoorOpened(Vector3 position)
        {
            PlaySFXAtPoint(_config != null ? _config.DoorOpenClip : null,
                           position,
                           _config != null ? _config.DoorVolume : 0.7f);
        }

        private void HandleDoorClosed(Vector3 position)
        {
            PlaySFXAtPoint(_config != null ? _config.DoorCloseClip : null,
                           position,
                           _config != null ? _config.DoorVolume : 0.7f);
        }

        private void HandlePlayerMoved(Vector3 position, float speed)
        {
            if (_config == null) return;

            // No footsteps when stationary
            if (speed < 0.1f)
            {
                _footstepTimer = 0f;
                return;
            }

            _lastMoveTime = Time.time;

            // Determine footstep interval based on speed
            float interval = speed >= _config.SprintSpeedThreshold
                ? _config.SprintFootstepInterval
                : _config.WalkFootstepInterval;

            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0f;
                PlayRandomFootstep();
            }
        }

        private void HandleInteractableNoise(Vector3 position, float loudness)
        {
            if (_config == null || _interactionSource == null) return;

            _lastNoiseTime = Time.time;

            // Pick clip — use door creak as default, furniture slide if available
            AudioClip clip = _config.DoorCreakClip;
            if (clip == null) clip = _config.FurnitureSlideClip;
            if (clip == null) return;

            if (!_interactionSource.isPlaying || _interactionSource.clip != clip)
            {
                _interactionSource.clip = clip;
                _interactionSource.loop = true;
                _interactionSource.Play();
            }

            // Scale volume with loudness
            _interactionSource.volume = loudness * _config.InteractionMaxVolume;
            _interactionSource.transform.position = position;
        }

        private void HandleGameStateChanged(GameState newState)
        {
            if (_ambientSource == null) return;

            switch (newState)
            {
                case GameState.Playing:
                    _ambientSource.UnPause();
                    break;
                case GameState.Paused:
                    _ambientSource.Pause();
                    break;
            }
        }

        #endregion

        #region Audio Playback

        private void PlayRandomFootstep()
        {
            if (_config == null || _config.FootstepClips == null || _config.FootstepClips.Length == 0) return;
            if (_sfxSource == null) return;

            var clip = _config.FootstepClips[Random.Range(0, _config.FootstepClips.Length)];
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, _config.FootstepVolume);
            }
        }

        private void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        #endregion
    }
}
