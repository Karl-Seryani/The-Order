using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Shows "Day 1" / "Day 2" / "Day 3 - Last Day" text overlay on scene start.
    /// Driven by BunkerSceneBootstrap — shows on black screen BEFORE wake-up begins.
    /// OnComplete callback fires when the overlay finishes so bootstrap can chain wake-up.
    /// </summary>
    public class DayOverlayUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Text _dayText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _blackBackground;

        [Header("Timing")]
        [SerializeField] private float _initialDelay = 2f;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _holdDuration = 3f;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private float _postDelay = 1f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _lastDayColor = new Color(0.75f, 0.08f, 0.08f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioClip _daySoundClip;
        [SerializeField] [Range(0f, 1f)] private float _daySoundVolume = 0.7f;

        #endregion

        #region Private Fields

        private Coroutine _showCoroutine;
        private bool _hasShown;
        private AudioSource _audioSource;

        #endregion

        #region Public API

        /// <summary>Fires when the day overlay finishes (fade-out complete).</summary>
        public Action OnComplete;

        /// <summary>
        /// Trigger the day overlay display. Called by BunkerSceneBootstrap.
        /// </summary>
        public void ShowDayText()
        {
            if (_hasShown) return;
            _hasShown = true;

            // Freeze player camera and input during the overlay without triggering audio
            var cam = FindFirstObjectByType<PlayerCamera.FirstPersonCamera>();
            if (cam != null) cam.IsEnabled = false;
            var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null) playerInput.actions.FindActionMap("Player").Disable();

            UpdateDayLabel();
            if (_showCoroutine != null)
                StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(FadeDayText());
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            // Ensure the black background covers the screen and renders behind text
            if (_blackBackground != null)
            {
                Color bg = _blackBackground.color;
                bg.a = 1f;
                _blackBackground.color = bg;
                _blackBackground.transform.SetAsFirstSibling();
            }

            // Create AudioSource for day sound (ignoreListenerPause for timeScale=0)
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.ignoreListenerPause = true;
        }

        #endregion

        #region Private Methods

        private void UpdateDayLabel()
        {
            if (_dayText == null) return;

            var run = RunStateManager.Instance;
            int day = run != null ? run.CurrentDay : 1;
            bool isLastDay = day >= RunStateManager.MAX_DAYS;

            if (isLastDay)
                _dayText.text = $"Day {day} - Last Day";
            else
                _dayText.text = $"Day {day}";

            // White for normal days, blood red for last day
            _dayText.color = isLastDay ? _lastDayColor : _normalColor;
        }

        private IEnumerator FadeDayText()
        {
            if (_canvasGroup == null) yield break;

            // Block interaction during overlay
            _canvasGroup.blocksRaycasts = true;

            // Wait on black screen before showing text
            yield return new WaitForSecondsRealtime(_initialDelay);

            // Play day sound at start of fade-in
            if (_daySoundClip != null && _audioSource != null)
                _audioSource.PlayOneShot(_daySoundClip, _daySoundVolume);

            // Fade in
            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold — Day 3 gets a dying-light flicker effect
            var run = RunStateManager.Instance;
            bool isLastDay = run != null && run.CurrentDay >= RunStateManager.MAX_DAYS;

            if (isLastDay && _dayText != null)
            {
                float holdElapsed = 0f;
                float flickerSpeed = 8f;
                float phase = 0f;
                while (holdElapsed < _holdDuration)
                {
                    holdElapsed += Time.unscaledDeltaTime;
                    phase += Time.unscaledDeltaTime * flickerSpeed;

                    // Randomly shift speed for irregular feel
                    if (UnityEngine.Random.value < 0.03f)
                        flickerSpeed = UnityEngine.Random.Range(5f, 25f);

                    float alpha;
                    float glitchRoll = UnityEngine.Random.value;

                    if (glitchRoll < 0.04f)
                    {
                        // Hard blackout — text vanishes briefly
                        alpha = 0f;
                    }
                    else if (glitchRoll < 0.08f)
                    {
                        // Dim flicker — barely visible
                        alpha = UnityEngine.Random.Range(0.05f, 0.2f);
                    }
                    else
                    {
                        // Sine wave oscillation like a dying bulb
                        float t = (Mathf.Sin(phase) + 1f) * 0.5f;
                        alpha = Mathf.Lerp(0.15f, 1f, t);
                    }

                    Color c = _dayText.color;
                    c.a = alpha;
                    _dayText.color = c;
                    yield return null;
                }

                // Restore full alpha before fade-out
                Color restore = _dayText.color;
                restore.a = 1f;
                _dayText.color = restore;
            }
            else
            {
                yield return new WaitForSecondsRealtime(_holdDuration);
            }

            // Fade out
            elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeOutDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _showCoroutine = null;

            // Brief pause before wake-up begins
            yield return new WaitForSecondsRealtime(_postDelay);

            OnComplete?.Invoke();
        }

        #endregion
    }
}
