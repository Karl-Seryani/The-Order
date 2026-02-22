using System.Collections.Generic;
using UnityEngine;

namespace TheOrder.Clues
{
    /// <summary>
    /// Singleton manager that tracks collected clues.
    /// Subscribes to GameEvents.OnClueCollected. Persists across scenes.
    /// </summary>
    public class ClueManager : MonoBehaviour
    {
        #region Singleton

        public static ClueManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Clue Totals")]
        [SerializeField] private int _totalClues = 2;

        #endregion

        #region Private Fields

        private readonly HashSet<string> _collectedClueIds = new HashSet<string>();
        private readonly List<ClueData> _collectedClues = new List<ClueData>();

        #endregion

        #region Public API

        /// <summary>Total number of clues collected.</summary>
        public int CollectedCount => _collectedClueIds.Count;

        /// <summary>Total number of clues in the game.</summary>
        public int TotalClues => _totalClues;

        /// <summary>
        /// Calculates knowledge level based on total clues collected.
        /// None = 0, Low = less than half, Medium = half or more, High = all.
        /// </summary>
        public KnowledgeLevel GetKnowledgeLevel()
        {
            int count = CollectedCount;
            if (count == 0) return KnowledgeLevel.None;
            if (count >= _totalClues) return KnowledgeLevel.High;
            if (count * 2 >= _totalClues) return KnowledgeLevel.Medium;
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

        internal void HandleClueCollected(ClueData clue)
        {
            if (clue == null) return;

            if (string.IsNullOrEmpty(clue.Id))
            {
                Debug.LogWarning($"[ClueManager] Clue '{clue.Title}' has no ID — skipping collection.", clue);
                return;
            }

            // Prevent duplicate collection
            if (!_collectedClueIds.Add(clue.Id)) return;

            _collectedClues.Add(clue);
        }

        #endregion
    }
}
