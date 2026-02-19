using System.Collections;
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
        [SerializeField] private AudioSource _stingerSource;

        #endregion

        #region Private Fields

        private float _footstepTimer;
        private float _lastMoveTime;

        private float _lastNoiseTime;

        private float _randomStingerTimer;
        private bool _isPlaying;
        private Coroutine _activeStingerCoroutine;

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
            GameEvents.OnHunterStateChanged += HandleHunterStateChanged;
            GameEvents.OnItemPickedUp += HandleItemPickedUp;
            GameEvents.OnWakeUpStarted += HandleWakeUpStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnDoorOpened -= HandleDoorOpened;
            GameEvents.OnDoorClosed -= HandleDoorClosed;
            GameEvents.OnPlayerMoved -= HandlePlayerMoved;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnInteractableNoise -= HandleInteractableNoise;
            GameEvents.OnHunterStateChanged -= HandleHunterStateChanged;
            GameEvents.OnItemPickedUp -= HandleItemPickedUp;
            GameEvents.OnWakeUpStarted -= HandleWakeUpStarted;
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

            // Random stinger timer
            if (_isPlaying && _config != null && _config.RandomStingerClip != null)
            {
                _randomStingerTimer -= Time.deltaTime;
                if (_randomStingerTimer <= 0f)
                {
                    PlayTimedStinger(_config.RandomStingerClip, _config.RandomStingerVolume,
                        _config.RandomStingerDuration);
                    ResetRandomStingerTimer();
                }
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
                bool isSprinting = speed >= _config.SprintSpeedThreshold;
                PlayRandomFootstep(isSprinting);
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
                    _isPlaying = true;
                    ResetRandomStingerTimer();
                    break;
                case GameState.Paused:
                case GameState.Death:
                    _ambientSource.Pause();
                    _isPlaying = false;
                    break;
            }
        }

        private void HandleWakeUpStarted()
        {
            if (_config != null && _config.WakeUpStingerClip != null)
            {
                PlayTimedStinger(_config.WakeUpStingerClip, _config.WakeUpStingerVolume,
                    _config.WakeUpStingerDuration);
            }
        }

        private void HandleHunterStateChanged(HunterState newState)
        {
            if (_config == null || _stingerSource == null || _config.ChaseMusicClip == null) return;

            if (newState == HunterState.Chase)
            {
                // Stop any one-shot stinger so chase music takes over
                StopActiveStinger();
                _stingerSource.clip = _config.ChaseMusicClip;
                _stingerSource.volume = _config.ChaseMusicVolume;
                _stingerSource.loop = true;
                _stingerSource.Play();
            }
            else
            {
                // Stop chase music when leaving Chase state
                if (_stingerSource.isPlaying && _stingerSource.clip == _config.ChaseMusicClip)
                {
                    _stingerSource.Stop();
                    _stingerSource.loop = false;
                }
            }
        }

        private int _keyPickupCount;

        private void HandleItemPickedUp(Items.ItemData item)
        {
            if (_config == null || _config.SecondKeyStingerClip == null) return;
            if (item == null || item.Type != ItemType.Key) return;

            _keyPickupCount++;

            // Play stinger on the second key pickup
            if (_keyPickupCount == 2)
            {
                PlayTimedStinger(_config.SecondKeyStingerClip, _config.SecondKeyStingerVolume,
                    _config.SecondKeyStingerDuration);
            }
        }

        #endregion

        #region Audio Playback

        private void PlayRandomFootstep(bool isSprinting)
        {
            if (_config == null || _sfxSource == null) return;

            AudioClip[] clips = isSprinting ? _config.SprintFootstepClips : _config.WalkFootstepClips;
            if (clips == null || clips.Length == 0) return;

            var clip = clips[Random.Range(0, clips.Length)];
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

        #region Stinger Playback

        private void PlayTimedStinger(AudioClip clip, float volume, float duration)
        {
            if (_stingerSource == null || clip == null) return;

            // Don't interrupt chase music
            if (_stingerSource.isPlaying && _stingerSource.loop) return;

            StopActiveStinger();
            _stingerSource.clip = clip;
            _stingerSource.volume = volume;
            _stingerSource.loop = false;
            _stingerSource.Play();
            _activeStingerCoroutine = StartCoroutine(StopAfterDuration(duration));
        }

        private IEnumerator StopAfterDuration(float duration)
        {
            float fadeDuration = 0.5f;
            yield return new WaitForSecondsRealtime(duration - fadeDuration);

            // Fade out to avoid hard cut
            if (_stingerSource != null && _stingerSource.isPlaying && !_stingerSource.loop)
            {
                float startVolume = _stingerSource.volume;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _stingerSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                _stingerSource.Stop();
                _stingerSource.volume = startVolume;
            }
            _activeStingerCoroutine = null;
        }

        private void StopActiveStinger()
        {
            if (_activeStingerCoroutine != null)
            {
                StopCoroutine(_activeStingerCoroutine);
                _activeStingerCoroutine = null;
            }
            if (_stingerSource != null && _stingerSource.isPlaying)
            {
                _stingerSource.Stop();
                _stingerSource.loop = false;
            }
        }

        private void ResetRandomStingerTimer()
        {
            if (_config == null) return;
            _randomStingerTimer = Random.Range(_config.RandomStingerMinInterval,
                _config.RandomStingerMaxInterval);
        }

        #endregion
    }
}
