using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheOrder
{
    /// <summary>
    /// Singleton game manager. Owns game state FSM, scene transitions, and cursor management.
    /// Persists across scenes via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;

        public GameState CurrentState => _currentState;
        public bool SkipWakeUpSequence { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion

        #region State Machine

        /// <summary>Transition to a new game state.</summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            ExitState(_currentState);
            _currentState = newState;
            EnterState(newState);

            GameEvents.GameStateChanged(newState);
        }

        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    SetCursorLock(false);
                    break;

                case GameState.Prologue:
                    Time.timeScale = 1f;
                    SetCursorLock(true);
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    SetCursorLock(true);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    SetCursorLock(false);
                    break;

                case GameState.Ending:
                    Time.timeScale = 1f;
                    SetCursorLock(false);
                    break;
            }
        }

        private void ExitState(GameState state)
        {
            // Reserved for cleanup when leaving a state
        }

        /// <summary>Toggle between Playing and Paused states.</summary>
        public void TogglePause()
        {
            if (_currentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (_currentState == GameState.Paused)
                SetState(GameState.Playing);
        }

        /// <summary>Skip the wake-up sequence on the next bunker load (e.g., after respawn).</summary>
        public void SetSkipWakeUpSequence(bool skip)
        {
            SkipWakeUpSequence = skip;
        }

        #endregion

        #region Scene Management

        /// <summary>Load a scene by name with optional async loading.</summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>Load a scene asynchronously. Returns the AsyncOperation for progress tracking.</summary>
        public AsyncOperation LoadSceneAsync(string sceneName)
        {
            return SceneManager.LoadSceneAsync(sceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Scene-specific state setup can be added here
        }

        #endregion

        #region Cursor

        private void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        #endregion
    }
}
