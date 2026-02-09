using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Manages the minimal dark HUD: interaction prompt, clue notification,
    /// clue reading panel, objective display (fade in/out), and clue counter.
    /// No health bar, no stamina bar, no journal.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Interaction Prompt")]
        [SerializeField] private Text _interactionPromptText;
        [SerializeField] private GameObject _interactionPromptPanel;

        [Header("Clue Notification")]
        [SerializeField] private Text _clueNotificationText;
        [SerializeField] private CanvasGroup _clueNotificationGroup;

        [Header("Clue Reading")]
        [SerializeField] private GameObject _clueReadingPanel;
        [SerializeField] private Text _clueReadingTitle;
        [SerializeField] private Text _clueReadingCategory;
        [SerializeField] private Text _clueReadingContent;

        [Header("Objective")]
        [SerializeField] private Text _objectiveText;
        [SerializeField] private CanvasGroup _objectiveGroup;
        [SerializeField] private float _objectiveDisplayDuration = 3f;
        [SerializeField] private float _objectiveFadeDuration = 0.8f;

        [Header("Clue Counter")]
        [SerializeField] private Text _clueCounterText;

        #endregion

        #region Private Fields

        private Player.PlayerInteraction _playerInteraction;
        private Player.PlayerInputHandler _input;
        private Coroutine _notificationCoroutine;
        private Coroutine _objectiveCoroutine;
        private string _currentObjective = "Explore the bunker for clues or exit";

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _playerInteraction = FindFirstObjectByType<Player.PlayerInteraction>();
            _input = FindFirstObjectByType<Player.PlayerInputHandler>();

            // Hide notification initially
            if (_clueNotificationGroup != null)
            {
                _clueNotificationGroup.alpha = 0f;
            }

            // Hide objective initially
            if (_objectiveGroup != null)
            {
                _objectiveGroup.alpha = 0f;
            }

            // Hide interaction prompt initially
            if (_interactionPromptPanel != null)
            {
                _interactionPromptPanel.SetActive(false);
            }

            // Hide clue reading panel initially
            if (_clueReadingPanel != null)
            {
                _clueReadingPanel.SetActive(false);
            }

            // Set initial objective text
            if (_objectiveText != null)
            {
                _objectiveText.text = _currentObjective;
            }

            UpdateClueCounter();

            // Show objective on start — only if already Playing (not during wake-up)
            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
            {
                ShowObjective();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnClueViewed += HandleClueViewed;
            GameEvents.OnClueCollected += HandleClueCollected;
            GameEvents.OnObjectiveChanged += HandleObjectiveChanged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnKeyCollected += HandleKeyCollected;
            GameEvents.OnLockedDoorAttempt += HandleLockedDoorAttempt;
        }

        private void OnDisable()
        {
            GameEvents.OnClueViewed -= HandleClueViewed;
            GameEvents.OnClueCollected -= HandleClueCollected;
            GameEvents.OnObjectiveChanged -= HandleObjectiveChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnKeyCollected -= HandleKeyCollected;
            GameEvents.OnLockedDoorAttempt -= HandleLockedDoorAttempt;
        }

        private void Update()
        {
            UpdateInteractionPrompt();

            // Tab or Escape toggles pause
            if (_input == null)
            {
                _input = FindFirstObjectByType<Player.PlayerInputHandler>();
            }

            if (_input != null && (_input.JournalPressed || _input.PausePressed))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TogglePause();
                }
            }
        }

        #endregion

        #region Interaction Prompt

        private void UpdateInteractionPrompt()
        {
            // Don't show prompts outside of Playing state
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (_interactionPromptPanel != null) _interactionPromptPanel.SetActive(false);
                return;
            }

            if (_playerInteraction == null)
            {
                _playerInteraction = FindFirstObjectByType<Player.PlayerInteraction>();
                if (_playerInteraction == null) return;
            }

            bool hasTarget = _playerInteraction.HasTarget;

            if (_interactionPromptPanel != null)
            {
                _interactionPromptPanel.SetActive(hasTarget);
            }

            if (hasTarget && _interactionPromptText != null)
            {
                _interactionPromptText.text = $"E — {_playerInteraction.PromptText}";
            }
        }

        #endregion

        #region Clue Reading

        private void ShowClueReading(ClueData clue)
        {
            if (_clueReadingPanel == null || clue == null) return;

            _clueReadingPanel.SetActive(true);

            if (_clueReadingTitle != null)
            {
                _clueReadingTitle.text = clue.Title;
            }

            if (_clueReadingCategory != null)
            {
                _clueReadingCategory.text = $"({clue.Category})";
            }

            if (_clueReadingContent != null)
            {
                _clueReadingContent.text = clue.ContentText;
            }
        }

        private void HideClueReading()
        {
            if (_clueReadingPanel != null)
            {
                _clueReadingPanel.SetActive(false);
            }
        }

        #endregion

        #region Objective Display

        /// <summary>
        /// Fades the objective text in at top-center, holds, then fades out.
        /// Called on Tab press and automatically when objective changes.
        /// </summary>
        private void ShowObjective()
        {
            if (_objectiveGroup == null) return;

            if (_objectiveCoroutine != null)
            {
                StopCoroutine(_objectiveCoroutine);
            }

            _objectiveCoroutine = StartCoroutine(FadeObjective());
        }

        private IEnumerator FadeObjective()
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < _objectiveFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _objectiveGroup.alpha = elapsed / _objectiveFadeDuration;
                yield return null;
            }
            _objectiveGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(_objectiveDisplayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < _objectiveFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _objectiveGroup.alpha = 1f - (elapsed / _objectiveFadeDuration);
                yield return null;
            }
            _objectiveGroup.alpha = 0f;
            _objectiveCoroutine = null;
        }

        #endregion

        #region Clue Notification

        private void ShowClueNotification(string clueTitle)
        {
            if (_clueNotificationText == null || _clueNotificationGroup == null) return;

            _clueNotificationText.text = $"{clueTitle} collected";

            if (_notificationCoroutine != null)
            {
                StopCoroutine(_notificationCoroutine);
            }

            _notificationCoroutine = StartCoroutine(FadeNotification());
        }

        private IEnumerator FadeNotification()
        {
            _clueNotificationGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(3f);

            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                _clueNotificationGroup.alpha = 1f - (elapsed / 0.5f);
                yield return null;
            }

            _clueNotificationGroup.alpha = 0f;
            _notificationCoroutine = null;
        }

        #endregion

        #region Clue Counter

        private void UpdateClueCounter()
        {
            if (_clueCounterText == null) return;

            var cm = Clues.ClueManager.Instance;
            if (cm != null)
            {
                int truth = cm.GetCategoryCount(ClueCategory.Truth);
                int truthTotal = cm.GetCategoryTotal(ClueCategory.Truth);
                int mike = cm.GetCategoryCount(ClueCategory.Mike);
                int mikeTotal = cm.GetCategoryTotal(ClueCategory.Mike);
                _clueCounterText.text = $"Truth: {truth}/{truthTotal}\nMike: {mike}/{mikeTotal}";
            }
            else
            {
                _clueCounterText.text = "Truth: 0/11\nMike: 0/7";
            }
        }

        #endregion

        #region Event Handlers

        private void HandleClueViewed(ClueData clue)
        {
            ShowClueReading(clue);
        }

        private void HandleClueCollected(ClueData clue)
        {
            HideClueReading();

            if (clue != null)
            {
                ShowClueNotification(clue.Title);
            }

            // Delay counter update by one frame so ClueManager processes the clue first
            StartCoroutine(UpdateClueCounterNextFrame());
        }

        private IEnumerator UpdateClueCounterNextFrame()
        {
            yield return null;
            UpdateClueCounter();
        }

        private void HandleObjectiveChanged(string objectiveText)
        {
            _currentObjective = objectiveText;

            if (_objectiveText != null)
            {
                _objectiveText.text = objectiveText;
            }

            // Auto-show when objective changes
            ShowObjective();
        }

        private void HandleGameStateChanged(GameState newState)
        {
            bool visible = newState == GameState.Playing;
            if (_clueCounterText != null) _clueCounterText.gameObject.SetActive(visible);
            if (_interactionPromptPanel != null) _interactionPromptPanel.SetActive(visible);
        }

        private void HandleKeyCollected(Doors.KeyData key)
        {
            if (key != null)
            {
                ShowClueNotification($"{key.DisplayName} acquired");
            }
        }

        private void HandleLockedDoorAttempt(Doors.KeyData requiredKey)
        {
            if (requiredKey != null)
            {
                ShowClueNotification($"Locked — requires {requiredKey.DisplayName}");
            }
            else
            {
                ShowClueNotification("This door is locked");
            }
        }

        #endregion
    }
}
