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

        private DifficultyLevel _currentDifficulty = DifficultyLevel.Medium;

        public GameState CurrentState => _currentState;
        public bool SkipWakeUpSequence { get; private set; }

        /// <summary>The selected difficulty level. Persists across scenes.</summary>
        public DifficultyLevel CurrentDifficulty => _currentDifficulty;

        /// <summary>True unless Practice mode (Hunter is deactivated).</summary>
        public bool HunterEnabled => _currentDifficulty != DifficultyLevel.Practice;

        /// <summary>True for Medium, Hard, and Nightmare (Hunter hears footsteps, doors, noise).</summary>
        public bool HunterFullDetection => _currentDifficulty >= DifficultyLevel.Medium;

        /// <summary>True for Practice, Hard, and Nightmare (car repair escape required).</summary>
        public bool RequiresCarRepair => _currentDifficulty == DifficultyLevel.Hard || _currentDifficulty == DifficultyLevel.Practice || _currentDifficulty == DifficultyLevel.Nightmare;

        /// <summary>Set the difficulty level before loading the game scene.</summary>
        public void SetDifficulty(DifficultyLevel level)
        {
            _currentDifficulty = level;
        }

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

        #endregion

        #region State Machine

        /// <summary>Transition to a new game state.</summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

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

                case GameState.Playing:
                    Time.timeScale = 1f;
                    SetCursorLock(true);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    SetCursorLock(false);
                    break;

                case GameState.Death:
                    Time.timeScale = 0f;
                    SetCursorLock(true);
                    break;

                case GameState.Ending:
                    Time.timeScale = 1f;
                    SetCursorLock(false);
                    break;
            }
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
