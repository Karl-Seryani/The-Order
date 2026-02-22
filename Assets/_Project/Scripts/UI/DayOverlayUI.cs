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

        #endregion

        #region Private Fields

        private Coroutine _showCoroutine;
        private bool _hasShown;

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

            // Freeze player camera/input during the overlay without triggering audio
            var cam = FindFirstObjectByType<PlayerCamera.FirstPersonCamera>();
            if (cam != null) cam.IsEnabled = false;

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
                _canvasGroup.alpha = 0f;

            // Ensure the black background covers the screen and renders behind text
            if (_blackBackground != null)
            {
                Color bg = _blackBackground.color;
                bg.a = 1f;
                _blackBackground.color = bg;
                _blackBackground.transform.SetAsFirstSibling();
            }
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

            // Wait on black screen before showing text
            yield return new WaitForSecondsRealtime(_initialDelay);

            // Fade in
            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(_holdDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeOutDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            _showCoroutine = null;

            // Brief pause before wake-up begins
            yield return new WaitForSecondsRealtime(_postDelay);

            OnComplete?.Invoke();
        }

        #endregion
    }
}
