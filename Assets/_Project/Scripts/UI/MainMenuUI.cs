using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Main menu controller. Handles Play, Tutorial, and Quit buttons.
    /// Sets GameState.MainMenu on scene load.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _tutorialPanel;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _quitButton;

        [Header("Scene")]
        [SerializeField] private string _prologueSceneName = "Prologue";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
            if (_tutorialButton != null) _tutorialButton.onClick.AddListener(OnTutorialClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.MainMenu);

            if (_menuPanel != null) _menuPanel.SetActive(true);
            if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_tutorialButton != null) _tutorialButton.onClick.RemoveListener(OnTutorialClicked);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        #endregion

        #region Public API

        /// <summary>Show the main menu panel, hide the tutorial panel.</summary>
        public void ShowMainMenu()
        {
            if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
            if (_menuPanel != null) _menuPanel.SetActive(true);
        }

        #endregion

        #region Button Handlers

        private void OnPlayClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoadScene(_prologueSceneName);
        }

        private void OnTutorialClicked()
        {
            if (_menuPanel != null) _menuPanel.SetActive(false);
            if (_tutorialPanel != null) _tutorialPanel.SetActive(true);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}
