using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheOrder.Ending
{
    /// <summary>
    /// Triggers the escape cutscene when the Main Door is unlocked.
    /// Sequence: disable input → fade to white → show "YOU ESCAPED" → load MainMenu.
    /// Place on a persistent scene object (e.g., GameManager or dedicated EndingManager).
    /// </summary>
    public class EndingCutscene : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI")]
        [SerializeField] private CanvasGroup _fadeOverlay;
        [SerializeField] private Text _escapedText;

        [Header("Timing")]
        [SerializeField] private float _fadeToWhiteDuration = 3f;
        [SerializeField] private float _textDisplayDuration = 3f;
        [SerializeField] private float _fadeToBlackDuration = 2f;

        #endregion

        #region Private Fields

        private bool _isPlaying;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            GameEvents.OnCarRepairComplete += HandleCarRepairComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnCarRepairComplete -= HandleCarRepairComplete;
        }

        private void Start()
        {
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0f;
                _fadeOverlay.gameObject.SetActive(false);
            }

            if (_escapedText != null)
            {
                _escapedText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Event Handlers

        private void HandleCarRepairComplete()
        {
            if (_isPlaying) return;
            StartCoroutine(PlayEndingSequence());
        }

        #endregion

        #region Cutscene Sequence

        private IEnumerator PlayEndingSequence()
        {
            _isPlaying = true;

            // Disable player input
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Ending);
            }

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Brief pause before fade
            yield return new WaitForSecondsRealtime(0.5f);

            // Set black background before fading in
            var bgImage = _fadeOverlay != null ? _fadeOverlay.GetComponentInChildren<Image>() : null;
            if (bgImage != null)
                bgImage.color = Color.black;

            // Fade to black
            if (_fadeOverlay != null)
            {
                _fadeOverlay.gameObject.SetActive(true);
                float elapsed = 0f;
                while (elapsed < _fadeToWhiteDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _fadeOverlay.alpha = Mathf.Clamp01(elapsed / _fadeToWhiteDuration);
                    yield return null;
                }
                _fadeOverlay.alpha = 1f;
            }

            // Show "YOU ESCAPED" text — smaller white text on black
            if (_escapedText != null)
            {
                _escapedText.gameObject.SetActive(true);
                _escapedText.text = "YOU ESCAPED";
                _escapedText.color = Color.white;
                _escapedText.fontSize = 48;
            }

            // Hold text
            yield return new WaitForSecondsRealtime(_textDisplayDuration);

            // Fade text out (overlay stays white, text fades)
            if (_escapedText != null)
            {
                var textColor = _escapedText.color;
                float elapsed = 0f;
                while (elapsed < _fadeToBlackDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / _fadeToBlackDuration);
                    _escapedText.color = new Color(textColor.r, textColor.g, textColor.b, 1f - t);
                    yield return null;
                }
            }

            yield return new WaitForSecondsRealtime(0.5f);

            // Load Main Menu
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}
