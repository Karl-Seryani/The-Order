using UnityEngine;

namespace TheOrder.UI
{
    /// <summary>
    /// Tracks and broadcasts the current objective text.
    /// Subscribes to item/clue/car events to update objectives during gameplay.
    /// </summary>
    public class ObjectiveManager : MonoBehaviour
    {
        #region Singleton

        public static ObjectiveManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Settings")]
        [SerializeField] private string _initialObjective = "Explore the bunker for clues or exit";

        [Header("Key Items")]
        [SerializeField] private Items.ItemData _basementKey;
        [SerializeField] private Items.ItemData _machineRoomKey;
        [SerializeField] private Items.ItemData _mainDoorKey;
        [SerializeField] private Items.ItemData _drillItem;

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

        private void OnEnable()
        {
            GameEvents.OnItemPickedUp += HandleItemPickedUp;
            GameEvents.OnItemUsed += HandleItemUsed;
            GameEvents.OnClueCollected += HandleClueCollected;
            GameEvents.OnCarRepairComplete += HandleCarRepairComplete;
            GameEvents.OnWakeUpCompleted += HandleWakeUpCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnItemPickedUp -= HandleItemPickedUp;
            GameEvents.OnItemUsed -= HandleItemUsed;
            GameEvents.OnClueCollected -= HandleClueCollected;
            GameEvents.OnCarRepairComplete -= HandleCarRepairComplete;
            GameEvents.OnWakeUpCompleted -= HandleWakeUpCompleted;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region Event Handlers

        private void HandleItemPickedUp(Items.ItemData item)
        {
            if (item == null) return;

            if (_basementKey != null && item == _basementKey)
            {
                SetObjective("The basement awaits...");
            }
            else if (_machineRoomKey != null && item == _machineRoomKey)
            {
                SetObjective("The machine room awaits...");
            }
            else if (_mainDoorKey != null && item == _mainDoorKey)
            {
                SetObjective("The way out is near...");
            }
            else if (_drillItem != null && item == _drillItem && RequiresCarRepair())
            {
                SetObjective("The wheels won't hold themselves...");
            }
            else if (item.Type == ItemType.Key)
            {
                SetObjective("Keep searching...");
            }
        }

        private void HandleItemUsed(Items.ItemData item)
        {
        }

        private void HandleClueCollected(ClueData clue)
        {
        }

        private void HandleCarRepairComplete()
        {
            SetObjective("Get out. Now.");
        }

        private void HandleWakeUpCompleted()
        {
            SetObjective(GetDifficultyObjective());
        }

        #endregion

        #region Helpers

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

        private bool RequiresCarRepair()
        {
            return GameManager.Instance != null && GameManager.Instance.RequiresCarRepair;
        }

        #endregion

    }
}
