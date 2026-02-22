using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// In-game pause menu. Escape toggles pause with Resume, Tutorial, Settings, Quit buttons.
    /// Manages sub-panel navigation (Tutorial, Settings) with Escape to back out.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Audio")]
        [SerializeField] private AudioClip _buttonClickSfx;
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.7f;

        #endregion

        #region Private Fields

        private Canvas _canvas;
        private AudioSource _sfxSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;

            if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResumeClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);

            if (_settingsPanel != null)
            {
                var settingsUI = _settingsPanel.GetComponent<SettingsUI>();
                if (settingsUI != null)
                    settingsUI.OnBackAction = ShowPausePanel;
            }
        }

        private void Start()
        {
            HideAll();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // Don't handle Escape during death, ending, or wake-up sequence
            if (GameManager.Instance != null)
            {
                var state = GameManager.Instance.CurrentState;
                if (state == GameState.Death || state == GameState.Ending || state == GameState.MainMenu) return;
            }

            // Block pause during day overlay and wake-up sequence
            var fpsCam = FindFirstObjectByType<PlayerCamera.FirstPersonCamera>();
            if (fpsCam != null && !fpsCam.IsEnabled) return;

            // If settings sub-panel is open, go back to pause panel
            if (_settingsPanel != null && _settingsPanel.activeSelf)
            {
                ShowPausePanel();
                return;
            }

            // Toggle pause
            if (GameManager.Instance != null)
                GameManager.Instance.TogglePause();

            bool isPaused = GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused;

            if (isPaused)
                ShowPausePanel();
            else
                HideAll();
        }

        private void OnDestroy()
        {
            if (_resumeButton != null) _resumeButton.onClick.RemoveAllListeners();
            if (_settingsButton != null) _settingsButton.onClick.RemoveAllListeners();
            if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Panel Navigation

        private void ShowPausePanel()
        {
            PlayClickSfx();
            if (_canvas != null) _canvas.enabled = true;
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_pausePanel != null) _pausePanel.SetActive(true);

            // Force cursor unlock so player can click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HideAll()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_canvas != null) _canvas.enabled = false;

            // Re-lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion

        #region Button Handlers

        private void OnResumeClicked()
        {
            PlayClickSfx();
            HideAll();
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnSettingsClicked()
        {
            PlayClickSfx();
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(true);

            // Keep cursor unlocked for sub-panel interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnQuitClicked()
        {
            PlayClickSfx();
            // Restore time scale before loading main menu
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region Audio

        private void PlayClickSfx()
        {
            if (_buttonClickSfx != null && _sfxSource != null)
                _sfxSource.PlayOneShot(_buttonClickSfx, _sfxVolume);
        }

        #endregion
    }
}
