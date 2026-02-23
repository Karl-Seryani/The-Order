using System.Collections;
using UnityEngine;

namespace TheOrder.Audio
{
    /// <summary>
    /// Manages ambient audio atmosphere: looping drone, door SFX, and footstep sounds.
    /// Subscribes to GameEvents for audio triggers.
    /// Uses dual ambient sources for smooth crossfading between indoor/outdoor zones.
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
        private bool _isOutdoor;
        private Coroutine _activeStingerCoroutine;
        private Coroutine _chaseFadeCoroutine;

        // Dual ambient source crossfade
        private AudioSource _ambientSourceB;
        private AudioSource _activeAmbientSource;
        private Coroutine _ambientCrossfadeCoroutine;

        // Stinger fade-then-play
        private Coroutine _stingerFadeCoroutine;
        private AudioClip _pendingStingerClip;
        private float _pendingStingerVolume;
        private float _pendingStingerDuration;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (_config == null || _ambientSource == null) return;

            // Create second ambient source by cloning settings from the primary
            _ambientSourceB = gameObject.AddComponent<AudioSource>();
            _ambientSourceB.outputAudioMixerGroup = _ambientSource.outputAudioMixerGroup;
            _ambientSourceB.spatialBlend = _ambientSource.spatialBlend;
            _ambientSourceB.playOnAwake = false;
            _ambientSourceB.loop = true;
            _ambientSourceB.volume = 0f;

            _activeAmbientSource = _ambientSource;

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
            GameEvents.OnPlayerMoved += HandlePlayerMoved;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnInteractableNoise += HandleInteractableNoise;
            GameEvents.OnHunterStateChanged += HandleHunterStateChanged;
            GameEvents.OnItemPickedUp += HandleItemPickedUp;
            GameEvents.OnWakeUpStarted += HandleWakeUpStarted;
        }

        private void OnDisable()
        {
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

        private void HandlePlayerMoved(Vector3 position, float speed)
        {
            if (_config == null) return;
            if (_isOutdoor) return;

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
                    if (_ambientSourceB != null) _ambientSourceB.UnPause();
                    _isPlaying = true;
                    ResetRandomStingerTimer();
                    break;
                case GameState.Paused:
                    _ambientSource.Pause();
                    if (_ambientSourceB != null) _ambientSourceB.Pause();
                    _isPlaying = false;
                    break;
                case GameState.Death:
                    _ambientSource.Pause();
                    if (_ambientSourceB != null) _ambientSourceB.Pause();
                    _isPlaying = false;
                    // Fade out chase music on death (Hunter stays in Chase, never triggers state change)
                    if (_stingerSource != null && _stingerSource.isPlaying
                        && _config != null && _stingerSource.clip == _config.ChaseMusicClip)
                    {
                        _stingerSource.loop = false;
                        if (_chaseFadeCoroutine != null)
                            StopCoroutine(_chaseFadeCoroutine);
                        _chaseFadeCoroutine = StartCoroutine(FadeOutChaseMusic(3f));
                    }
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
                // Cancel any active fade-out before starting new chase music
                if (_chaseFadeCoroutine != null)
                {
                    StopCoroutine(_chaseFadeCoroutine);
                    _chaseFadeCoroutine = null;
                }

                // Stop any one-shot stinger so chase music takes over
                StopActiveStinger();
                _stingerSource.clip = _config.ChaseMusicClip;
                _stingerSource.volume = _config.ChaseMusicVolume;
                _stingerSource.loop = true;
                _stingerSource.Play();
            }
            else
            {
                // Fade out chase music when leaving Chase state (3s fade)
                if (_stingerSource.isPlaying && _stingerSource.clip == _config.ChaseMusicClip)
                {
                    _stingerSource.loop = false;
                    _chaseFadeCoroutine = StartCoroutine(FadeOutChaseMusic(3f));
                }
            }
        }

        private int _keyPickupCount;

        private void HandleItemPickedUp(Items.ItemData item)
        {
            if (_config == null || item == null) return;

            // Machine Room Key stinger (specific item match)
            if (_config.MachineRoomKeyItemData != null && item == _config.MachineRoomKeyItemData
                && _config.MachineRoomKeyStingerClip != null)
            {
                PlayTimedStinger(_config.MachineRoomKeyStingerClip, _config.MachineRoomKeyStingerVolume,
                    _config.MachineRoomKeyStingerDuration);
            }

            // Second key stinger (generic key count)
            if (item.Type != ItemType.Key) return;

            _keyPickupCount++;

            if (_keyPickupCount == 2 && _config.SecondKeyStingerClip != null)
            {
                PlayTimedStinger(_config.SecondKeyStingerClip, _config.SecondKeyStingerVolume,
                    _config.SecondKeyStingerDuration);
            }
        }

        #endregion

        #region Config Switching

        /// <summary>
        /// Switch to a different AudioConfig (e.g., indoor → outdoor zone transition).
        /// Crossfades the ambient loop to the new config's clip.
        /// </summary>
        public void SetConfig(AudioConfig newConfig)
        {
            if (newConfig == null || newConfig == _config) return;

            _config = newConfig;

            // Crossfade ambient loop to new config
            if (newConfig.AmbientLoopClip != null)
            {
                CrossfadeAmbient(newConfig.AmbientLoopClip, newConfig.AmbientVolume);
            }

            ResetRandomStingerTimer();
        }

        /// <summary>
        /// Switch ambient source to a random outdoor forest loop with crossfade.
        /// </summary>
        public void PlayOutdoorAmbient()
        {
            if (_config == null || _ambientSource == null) return;

            var clips = _config.OutdoorAmbientClips;
            if (clips == null || clips.Length == 0) return;

            _isOutdoor = true;

            // Don't restart if already playing an outdoor clip
            if (_activeAmbientSource != null && _activeAmbientSource.isPlaying
                && System.Array.IndexOf(clips, _activeAmbientSource.clip) >= 0)
                return;

            var clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            CrossfadeAmbient(clip, _config.OutdoorAmbientVolume);
        }

        /// <summary>
        /// Stop outdoor ambient and crossfade back to the indoor ambient loop.
        /// </summary>
        public void StopOutdoorAmbient()
        {
            _isOutdoor = false;
            if (_config == null) return;

            if (_config.AmbientLoopClip != null)
            {
                CrossfadeAmbient(_config.AmbientLoopClip, _config.AmbientVolume);
            }
            else if (_activeAmbientSource != null)
            {
                _activeAmbientSource.Stop();
            }
        }

        #endregion

        #region Ambient Crossfade

        /// <summary>
        /// Crossfade from the active ambient source to a new clip over the given duration.
        /// Uses dual AudioSources — fades out the active one while fading in the inactive one.
        /// </summary>
        private void CrossfadeAmbient(AudioClip clip, float targetVolume, float duration = 1.5f)
        {
            if (_ambientSourceB == null) return;

            // Cancel any running crossfade
            if (_ambientCrossfadeCoroutine != null)
            {
                StopCoroutine(_ambientCrossfadeCoroutine);
                _ambientCrossfadeCoroutine = null;
            }

            // Pick the inactive source
            AudioSource incoming = (_activeAmbientSource == _ambientSource)
                ? _ambientSourceB
                : _ambientSource;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.loop = true;
            incoming.Play();

            AudioSource outgoing = _activeAmbientSource;

            _ambientCrossfadeCoroutine = StartCoroutine(
                CrossfadeCoroutine(outgoing, incoming, targetVolume, duration));
        }

        private IEnumerator CrossfadeCoroutine(AudioSource outgoing, AudioSource incoming,
            float targetVolume, float duration)
        {
            float elapsed = 0f;
            float startVolume = outgoing.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                outgoing.volume = Mathf.Lerp(startVolume, 0f, t);
                incoming.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            outgoing.volume = 0f;
            outgoing.Stop();
            incoming.volume = targetVolume;

            _activeAmbientSource = incoming;
            _ambientCrossfadeCoroutine = null;
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

        #endregion

        #region Stinger Playback

        private void PlayTimedStinger(AudioClip clip, float volume, float duration)
        {
            if (_stingerSource == null || clip == null) return;

            // Don't interrupt chase music
            if (_stingerSource.isPlaying && _stingerSource.loop) return;

            // If a stinger is already playing, fade it out then play the new one
            if (_stingerSource.isPlaying)
            {
                // Cancel any pending stinger queue
                if (_stingerFadeCoroutine != null)
                {
                    StopCoroutine(_stingerFadeCoroutine);
                    _stingerFadeCoroutine = null;
                }

                _pendingStingerClip = clip;
                _pendingStingerVolume = volume;
                _pendingStingerDuration = duration;
                _stingerFadeCoroutine = StartCoroutine(FadeOutThenPlayStinger(0.5f));
                return;
            }

            StopActiveStinger();
            _stingerSource.clip = clip;
            _stingerSource.volume = volume;
            _stingerSource.loop = false;
            _stingerSource.Play();
            _activeStingerCoroutine = StartCoroutine(StopAfterDuration(duration));
        }

        /// <summary>
        /// Fade out the current stinger, then play the pending one.
        /// Most-recent-wins: if another request arrives during fade, the pending clip is replaced.
        /// </summary>
        private IEnumerator FadeOutThenPlayStinger(float fadeDuration)
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
            _stingerFadeCoroutine = null;

            // Play the pending stinger (most recent request wins)
            if (_pendingStingerClip != null)
            {
                StopActiveStinger();
                _stingerSource.clip = _pendingStingerClip;
                _stingerSource.volume = _pendingStingerVolume;
                _stingerSource.loop = false;
                _stingerSource.Play();
                _activeStingerCoroutine = StartCoroutine(StopAfterDuration(_pendingStingerDuration));

                _pendingStingerClip = null;
            }
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

        private IEnumerator FadeOutChaseMusic(float duration)
        {
            if (_stingerSource == null) yield break;

            float startVolume = _stingerSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _stingerSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _stingerSource.Stop();
            _stingerSource.volume = startVolume;
            _chaseFadeCoroutine = null;
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
