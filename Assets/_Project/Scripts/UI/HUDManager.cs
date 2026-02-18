using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Manages the minimal dark HUD: interaction prompt, item notifications,
    /// clue reading panel, and objective display (fade in/out).
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
        [SerializeField] private Text _clueReadingContent;

        [Header("Objective")]
        [SerializeField] private Text _objectiveText;
        [SerializeField] private CanvasGroup _objectiveGroup;
        [SerializeField] private float _objectiveDisplayDuration = 3f;
        [SerializeField] private float _objectiveFadeDuration = 0.8f;

        [Header("Drop Hint")]
        [SerializeField] private Text _dropHintText;

        #endregion

        #region Private Fields

        private Player.PlayerInteraction _playerInteraction;
        private Player.PlayerInputHandler _input;
        private Coroutine _notificationCoroutine;
        private Coroutine _objectiveCoroutine;
        private string _currentObjective = "Find a way to escape";
        private bool _hasSearchedForReferences;

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

            // Hide drop hint initially
            if (_dropHintText != null)
            {
                _dropHintText.gameObject.SetActive(false);
            }

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
            GameEvents.OnItemPickedUp += HandleItemPickedUp;
            GameEvents.OnItemDropped += HandleItemDropped;
            GameEvents.OnItemUsed += HandleItemUsed;
            GameEvents.OnLockedDoorAttempt += HandleLockedDoorAttempt;
            GameEvents.OnWakeUpStarted += HandleWakeUpStarted;
            GameEvents.OnWakeUpCompleted += HandleWakeUpCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnClueViewed -= HandleClueViewed;
            GameEvents.OnClueCollected -= HandleClueCollected;
            GameEvents.OnObjectiveChanged -= HandleObjectiveChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnItemPickedUp -= HandleItemPickedUp;
            GameEvents.OnItemDropped -= HandleItemDropped;
            GameEvents.OnItemUsed -= HandleItemUsed;
            GameEvents.OnLockedDoorAttempt -= HandleLockedDoorAttempt;
            GameEvents.OnWakeUpStarted -= HandleWakeUpStarted;
            GameEvents.OnWakeUpCompleted -= HandleWakeUpCompleted;
        }

        private void Update()
        {
            UpdateInteractionPrompt();

            // Search for input handler once if not found in Start()
            if (_input == null && !_hasSearchedForReferences)
            {
                _input = FindFirstObjectByType<Player.PlayerInputHandler>();
                _hasSearchedForReferences = true;
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
                if (_hasSearchedForReferences) return;
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

            // Show/hide drop hint based on held item
            UpdateDropHint();
        }

        private void UpdateDropHint()
        {
            if (_dropHintText == null) return;

            var heldItem = Items.HeldItemController.Instance;
            bool holding = heldItem != null && heldItem.HasItem;
            _dropHintText.gameObject.SetActive(holding);
            if (holding)
            {
                _dropHintText.text = "Q — Drop";
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

        #region Event Handlers

        private void HandleClueViewed(ClueData clue)
        {
            ShowClueReading(clue);
        }

        private void HandleClueCollected(ClueData clue)
        {
            HideClueReading();
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
            if (_interactionPromptPanel != null) _interactionPromptPanel.SetActive(visible);
        }

        private void HandleItemPickedUp(Items.ItemData item)
        {
            if (item != null)
            {
                ShowClueNotification($"{item.DisplayName} picked up");
            }
        }

        private void HandleItemDropped(Items.ItemData item, Vector3 position)
        {
            if (item != null)
            {
                ShowClueNotification($"{item.DisplayName} dropped");
            }
        }

        private void HandleItemUsed(Items.ItemData item)
        {
            if (item != null)
            {
                ShowClueNotification($"{item.DisplayName} used");
            }
        }

        private void HandleWakeUpStarted()
        {
            if (_interactionPromptPanel != null) _interactionPromptPanel.SetActive(false);
            if (_dropHintText != null) _dropHintText.gameObject.SetActive(false);
        }

        private void HandleWakeUpCompleted()
        {
            if (_interactionPromptPanel != null) _interactionPromptPanel.SetActive(false);
        }

        private void HandleLockedDoorAttempt(Items.ItemData requiredItem)
        {
            if (requiredItem != null)
            {
                ShowClueNotification($"Locked — requires {requiredItem.DisplayName}");
            }
            else
            {
                ShowClueNotification("This door is locked");
            }
        }

        #endregion
    }
}
