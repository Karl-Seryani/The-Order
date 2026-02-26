using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Main menu controller. Handles Play, Tutorial, and Quit buttons.
    /// Plays looping background music and button click SFX.
    /// Sets GameState.MainMenu on scene load.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private GameObject _difficultyPanel;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _quitButton;

        [Header("Difficulty Buttons")]
        [SerializeField] private Button _practiceButton;
        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _mediumButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _nightmareButton;
        [SerializeField] private Button _difficultyBackButton;

        [Header("Audio")]
        [SerializeField] private AudioClip _bgMusic;
        [SerializeField] private AudioClip _buttonClickSfx;
        [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.4f;
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.7f;

        [Header("Scene")]
        [SerializeField] private string _gameSceneName = "Bunker";

        #endregion

        #region Private Fields

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Music AudioSource — looping, plays on awake
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;

            // SFX AudioSource — one-shot, no loop
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;

            if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
            if (_tutorialButton != null) _tutorialButton.onClick.AddListener(OnTutorialClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);

            if (_practiceButton != null) _practiceButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Practice));
            if (_easyButton != null) _easyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Easy));
            if (_mediumButton != null) _mediumButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Medium));
            if (_hardButton != null) _hardButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Hard));
            if (_nightmareButton != null) _nightmareButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Nightmare));
            if (_difficultyBackButton != null) _difficultyBackButton.onClick.AddListener(OnDifficultyBackClicked);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.MainMenu);

            if (_menuPanel != null) _menuPanel.SetActive(true);
            if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
            if (_difficultyPanel != null) _difficultyPanel.SetActive(false);

            // Start background music
            if (_bgMusic != null)
            {
                _musicSource.clip = _bgMusic;
                _musicSource.Play();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // Escape backs out of whichever sub-panel is open
            if (_tutorialPanel != null && _tutorialPanel.activeSelf)
            {
                ShowMainMenu();
                return;
            }
            if (_difficultyPanel != null && _difficultyPanel.activeSelf)
            {
                ShowMainMenu();
                return;
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveAllListeners();
            if (_tutorialButton != null) _tutorialButton.onClick.RemoveAllListeners();
            if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
            if (_practiceButton != null) _practiceButton.onClick.RemoveAllListeners();
            if (_easyButton != null) _easyButton.onClick.RemoveAllListeners();
            if (_mediumButton != null) _mediumButton.onClick.RemoveAllListeners();
            if (_hardButton != null) _hardButton.onClick.RemoveAllListeners();
            if (_nightmareButton != null) _nightmareButton.onClick.RemoveAllListeners();
            if (_difficultyBackButton != null) _difficultyBackButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Public API

        /// <summary>Show the main menu panel, hide other panels.</summary>
        public void ShowMainMenu()
        {
            PlayClickSfx();
            if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
            if (_difficultyPanel != null) _difficultyPanel.SetActive(false);
            if (_menuPanel != null) _menuPanel.SetActive(true);
        }

        #endregion

        #region Button Handlers

        private void OnPlayClicked()
        {
            PlayClickSfx();
            if (_difficultyPanel != null)
            {
                if (_menuPanel != null) _menuPanel.SetActive(false);
                _difficultyPanel.SetActive(true);
            }
            else
            {
                // Fallback if no difficulty panel assigned — load directly (Medium default)
                if (GameManager.Instance != null)
                    GameManager.Instance.LoadScene(_gameSceneName);
            }
        }

        private void OnDifficultySelected(DifficultyLevel level)
        {
            PlayClickSfx();

            // Reset run state for a fresh game
            if (RunStateManager.Instance != null)
                RunStateManager.Instance.ResetRun();
            Player.PlayerInventory.ClearKeys();
            if (Clues.ClueManager.Instance != null)
                Clues.ClueManager.Instance.ClearAll();

            if (GameManager.Instance != null)
                GameManager.Instance.SetDifficulty(level);

            // Fade out music, then load scene after fade completes
            StartCoroutine(FadeOutMusicThenLoad(2f));
        }

        private void OnDifficultyBackClicked()
        {
            PlayClickSfx();
            if (_difficultyPanel != null) _difficultyPanel.SetActive(false);
            if (_menuPanel != null) _menuPanel.SetActive(true);
        }

        private void OnTutorialClicked()
        {
            PlayClickSfx();
            if (_menuPanel != null) _menuPanel.SetActive(false);
            if (_tutorialPanel != null) _tutorialPanel.SetActive(true);
        }

        private void OnQuitClicked()
        {
            PlayClickSfx();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Audio

        private void PlayClickSfx()
        {
            if (_buttonClickSfx != null && _sfxSource != null)
                _sfxSource.PlayOneShot(_buttonClickSfx, _sfxVolume);
        }

        /// <summary>Fade out menu music, then load the game scene.</summary>
        private IEnumerator FadeOutMusicThenLoad(float duration)
        {
            // Disable all buttons so player can't double-click
            if (_difficultyPanel != null)
                foreach (var btn in _difficultyPanel.GetComponentsInChildren<Button>())
                    btn.interactable = false;

            if (_musicSource != null && _musicSource.isPlaying)
            {
                float startVolume = _musicSource.volume;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                    yield return null;
                }

                _musicSource.Stop();
            }

            if (GameManager.Instance != null)
                GameManager.Instance.LoadScene(_gameSceneName);
        }

        #endregion
    }
}
