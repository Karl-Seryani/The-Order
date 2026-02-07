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
            SetObjective(_initialObjective);
        }

        private void OnEnable()
        {
            GameEvents.OnClueCollected += HandleClueCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnClueCollected -= HandleClueCollected;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region Event Handlers

        private void HandleClueCollected(ClueData clue)
        {
            // Objective only changes for major game events, not per-clue pickup.
            // The clue counter on the HUD tracks collection progress.
        }

        #endregion
    }
}
