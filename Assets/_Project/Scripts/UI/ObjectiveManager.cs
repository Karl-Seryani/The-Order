using UnityEngine;

namespace TheOrder.UI
{
    /// <summary>
    /// Tracks and broadcasts the current objective text.
    /// Subscribes to clue collection to auto-update the objective counter.
    /// </summary>
    public class ObjectiveManager : MonoBehaviour
    {
        #region Singleton

        public static ObjectiveManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Settings")]
        [SerializeField] private string _initialObjective = "Explore the bunker for clues or exit";

        #endregion

        #region Private Fields

        private string _currentObjective;

        #endregion

        #region Public API

        /// <summary>The current objective text displayed on the HUD.</summary>
        public string CurrentObjective => _currentObjective;

        /// <summary>Set a new objective and fire the event.</summary>
        public void SetObjective(string newObjective)
        {
            _currentObjective = newObjective;
            GameEvents.ObjectiveChanged(_currentObjective);
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            string objective = GetDifficultyObjective();
            SetObjective(objective);
        }

        private string GetDifficultyObjective()
        {
            if (GameManager.Instance == null) return _initialObjective;

            return GameManager.Instance.CurrentDifficulty switch
            {
                DifficultyLevel.Practice => "Find car parts and repair the car to escape",
                DifficultyLevel.Easy => "Find the main door and escape",
                DifficultyLevel.Medium => "Find the main door and escape",
                DifficultyLevel.Hard => "Find car parts and repair the car to escape",
                DifficultyLevel.Nightmare => "Find car parts and repair the car to escape",
                _ => _initialObjective
            };
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

    }
}
