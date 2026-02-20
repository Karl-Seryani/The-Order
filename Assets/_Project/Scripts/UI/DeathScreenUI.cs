using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Death screen overlay — fades to black with "YOU DIED" text, then reloads the scene.
    /// Subscribes to GameEvents.OnDeathCinematicComplete (after cinematic plays).
    /// </summary>
    public class DeathScreenUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Canvas _deathCanvas;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Text _deathText;

        [Header("Audio")]
        [SerializeField] private AudioClip _deathStinger;
        [SerializeField] [Range(0f, 1f)] private float _deathStingerVolume = 0.8f;

        [Header("Timing")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _textFadeDuration = 0.3f;
        [SerializeField] private float _holdDuration = 1.5f;

        #endregion

        #region Private Fields

        private bool _isDying;
        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            if (_deathCanvas != null)
            {
                _deathCanvas.enabled = false;
            }

            // Ensure fade image starts transparent
            if (_fadeImage != null)
            {
                Color c = _fadeImage.color;
                c.a = 0f;
                _fadeImage.color = c;
            }

            // Ensure death text starts transparent
            if (_deathText != null)
            {
                Color c = _deathText.color;
                c.a = 0f;
                _deathText.color = c;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnDeathCinematicComplete += HandleDeathCinematicComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnDeathCinematicComplete -= HandleDeathCinematicComplete;
        }

        #endregion

        #region Event Handlers

        private void HandleDeathCinematicComplete()
        {
            if (_isDying) return;
            StartCoroutine(DeathSequence());
        }

        #endregion

        #region Death Sequence

        private IEnumerator DeathSequence()
        {
            _isDying = true;

            // Play death stinger
            if (_deathStinger != null && _audioSource != null)
                _audioSource.PlayOneShot(_deathStinger, _deathStingerVolume);

            // Disable player input via GameManager (keeps state in sync for scene reload)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Death);
            }

            // Enable the death canvas — screen is already black from cinematic
            if (_deathCanvas != null)
            {
                _deathCanvas.enabled = true;
            }

            // Set black background instantly (cinematic already cut to black)
            if (_fadeImage != null)
            {
                Color c = _fadeImage.color;
                c.a = 1f;
                _fadeImage.color = c;
            }

            // Fade in "YOU DIED" text
            yield return StartCoroutine(FadeText(_deathText, 0f, 1f, _textFadeDuration));

            // Hold
            yield return new WaitForSecondsRealtime(_holdDuration);

            // Reload scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetSkipWakeUpSequence(true);
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
        {
            if (image == null) yield break;

            float elapsed = 0f;
            Color color = image.color;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                image.color = color;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;
        }

        private IEnumerator FadeText(Text text, float startAlpha, float endAlpha, float duration)
        {
            if (text == null) yield break;

            float elapsed = 0f;
            Color color = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                text.color = color;
                yield return null;
            }

            color.a = endAlpha;
            text.color = color;
        }

        #endregion
    }
}
