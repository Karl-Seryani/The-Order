using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOrder.Clues
{
    /// <summary>
    /// Singleton manager that tracks collected clues and calculates knowledge levels.
    /// Subscribes to GameEvents.OnClueCollected. Persists across scenes.
    /// </summary>
    public class ClueManager : MonoBehaviour
    {
        #region Singleton

        public static ClueManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Clue Totals Per Category")]
        [SerializeField] private int _totalTruthClues = 11;
        [SerializeField] private int _totalMikeClues = 6;

        #endregion

        #region Private Fields

        private readonly HashSet<string> _collectedClueIds = new HashSet<string>();
        private readonly List<ClueData> _collectedClues = new List<ClueData>();

        #endregion

        #region Public API

        /// <summary>Total number of clues collected.</summary>
        public int CollectedCount => _collectedClueIds.Count;

        /// <summary>Total number of clues in the game.</summary>
        public int TotalClues => _totalTruthClues + _totalMikeClues;

        /// <summary>Returns all collected clues in order of collection.</summary>
        public IReadOnlyList<ClueData> GetCollectedClues() => _collectedClues;

        /// <summary>Returns collected clues filtered by category.</summary>
        public List<ClueData> GetCollectedCluesByCategory(ClueCategory category)
        {
            return _collectedClues.Where(c => c.Category == category).ToList();
        }

        /// <summary>Returns true if the specified clue has been collected.</summary>
        public bool IsClueCollected(string clueId)
        {
            return _collectedClueIds.Contains(clueId);
        }

        /// <summary>Returns how many clues have been collected in the given category.</summary>
        public int GetCategoryCount(ClueCategory category)
        {
            return _collectedClues.Count(c => c.Category == category);
        }

        /// <summary>Returns the total number of clues in the given category.</summary>
        public int GetCategoryTotal(ClueCategory category)
        {
            switch (category)
            {
                case ClueCategory.Truth: return _totalTruthClues;
                case ClueCategory.Mike: return _totalMikeClues;
                default: return 0;
            }
        }

        /// <summary>
        /// Calculates knowledge level for a category based on clues collected.
        /// None = 0, Low = less than half, Medium = half or more, High = all.
        /// </summary>
        public KnowledgeLevel GetKnowledgeLevel(ClueCategory category)
        {
            int count = GetCategoryCount(category);
            int total = GetCategoryTotal(category);

            if (count == 0) return KnowledgeLevel.None;
            if (count >= total) return KnowledgeLevel.High;
            if (count >= total / 2) return KnowledgeLevel.Medium;
            return KnowledgeLevel.Low;
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

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnClueCollected += HandleClueCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnClueCollected -= HandleClueCollected;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Process a collected clue. Called by event handler and available for testing.
        /// </summary>
        internal void HandleClueCollected(ClueData clue)
        {
            if (clue == null) return;

            // Prevent duplicate collection
            if (!_collectedClueIds.Add(clue.Id)) return;

            _collectedClues.Add(clue);
        }

        #endregion
    }
}
