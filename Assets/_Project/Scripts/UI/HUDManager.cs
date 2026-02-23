using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheOrder.UI
{
    /// <summary>
    /// Manages the minimal dark HUD: crosshair (dot/X), blocked interaction messages,
    /// item notifications, clue reading panel, and objective display (fade in/out).
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

        [Header("Crosshair")]
        [SerializeField] private float _crosshairSize = 7f;
        [SerializeField] private Color _crosshairColor = new Color(1f, 1f, 1f, 0.7f);

        #endregion

        #region Private Fields

        private Player.PlayerInteraction _playerInteraction;
        private Coroutine _notificationCoroutine;
        private Coroutine _objectiveCoroutine;
        private Coroutine _blockedMessageCoroutine;
        private string _currentObjective = "Find a way to escape";
        private bool _hasSearchedForInteraction;
        private Image _crosshairDot;
        private GameObject _crosshairX;
        private CanvasGroup _promptCanvasGroup;
        private bool _isShowingBlockedMessage;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _playerInteraction = FindFirstObjectByType<Player.PlayerInteraction>();

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

            // Repurpose interaction prompt panel for blocked messages (fade via CanvasGroup)
            if (_interactionPromptPanel != null)
            {
                _promptCanvasGroup = _interactionPromptPanel.GetComponent<CanvasGroup>();
                if (_promptCanvasGroup == null)
                {
                    _promptCanvasGroup = _interactionPromptPanel.AddComponent<CanvasGroup>();
                }
                _promptCanvasGroup.alpha = 0f;
                _promptCanvasGroup.blocksRaycasts = false;
                _promptCanvasGroup.interactable = false;
                _interactionPromptPanel.SetActive(true);
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

            // Create crosshair elements
            CreateCrosshair();

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
            GameEvents.OnItemPickedUp += HandleItemPickedUp;
            GameEvents.OnItemDropped += HandleItemDropped;
            GameEvents.OnItemUsed += HandleItemUsed;
            GameEvents.OnInteractionBlocked += HandleInteractionBlocked;
            GameEvents.OnWakeUpStarted += HandleWakeUpStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnClueViewed -= HandleClueViewed;
            GameEvents.OnClueCollected -= HandleClueCollected;
            GameEvents.OnObjectiveChanged -= HandleObjectiveChanged;
            GameEvents.OnItemPickedUp -= HandleItemPickedUp;
            GameEvents.OnItemDropped -= HandleItemDropped;
            GameEvents.OnItemUsed -= HandleItemUsed;
            GameEvents.OnInteractionBlocked -= HandleInteractionBlocked;
            GameEvents.OnWakeUpStarted -= HandleWakeUpStarted;
        }

        private void Update()
        {
            UpdateCrosshair();
        }

        #endregion

        #region Crosshair

        private void CreateCrosshair()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (canvas == null) return;

            // Dot (default state)
            var dotGo = new GameObject("CrosshairDot");
            dotGo.transform.SetParent(canvas.transform, false);

            _crosshairDot = dotGo.AddComponent<Image>();
            _crosshairDot.color = _crosshairColor;
            _crosshairDot.raycastTarget = false;

            var dotRt = _crosshairDot.rectTransform;
            dotRt.anchorMin = new Vector2(0.5f, 0.5f);
            dotRt.anchorMax = new Vector2(0.5f, 0.5f);
            dotRt.pivot = new Vector2(0.5f, 0.5f);
            dotRt.anchoredPosition = Vector2.zero;
            dotRt.sizeDelta = new Vector2(_crosshairSize, _crosshairSize);

            // X (shown when aiming at any interactable)
            float xSpan = _crosshairSize * 3.7f;
            float xThickness = _crosshairSize * 0.43f;

            _crosshairX = new GameObject("CrosshairX");
            _crosshairX.transform.SetParent(canvas.transform, false);

            var xRt = _crosshairX.AddComponent<RectTransform>();
            xRt.anchorMin = new Vector2(0.5f, 0.5f);
            xRt.anchorMax = new Vector2(0.5f, 0.5f);
            xRt.pivot = new Vector2(0.5f, 0.5f);
            xRt.anchoredPosition = Vector2.zero;
            xRt.sizeDelta = Vector2.zero;

            Color xColor = new Color(1f, 1f, 1f, 0.9f);
            CreateXLine(_crosshairX.transform, xColor, xSpan, xThickness, 45f);
            CreateXLine(_crosshairX.transform, xColor, xSpan, xThickness, -45f);

            _crosshairX.SetActive(false);
        }

        private void CreateXLine(Transform parent, Color color, float length, float thickness, float angle)
        {
            var lineGo = new GameObject("XLine");
            lineGo.transform.SetParent(parent, false);

            var img = lineGo.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(length, thickness);
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void UpdateCrosshair()
        {
            // Hide crosshair outside of Playing state
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (_crosshairDot != null) _crosshairDot.gameObject.SetActive(false);
                if (_crosshairX != null) _crosshairX.SetActive(false);
                if (_promptCanvasGroup != null && !_isShowingBlockedMessage)
                    _promptCanvasGroup.alpha = 0f;
                return;
            }

            if (_playerInteraction == null)
            {
                if (_hasSearchedForInteraction) return;
                _playerInteraction = FindFirstObjectByType<Player.PlayerInteraction>();
                _hasSearchedForInteraction = true;
                if (_playerInteraction == null) return;
            }

            bool hasTarget = _playerInteraction.HasTarget;

            // Dot when no target, X when aiming at any interactable
            if (_crosshairDot != null) _crosshairDot.gameObject.SetActive(!hasTarget);
            if (_crosshairX != null) _crosshairX.SetActive(hasTarget);

            // Show prompt text when aiming at interactable (blocked message takes priority)
            if (_interactionPromptText != null && _promptCanvasGroup != null && !_isShowingBlockedMessage)
            {
                if (hasTarget)
                {
                    string prompt = _playerInteraction.PromptText;
                    if (!string.IsNullOrEmpty(prompt))
                    {
                        _interactionPromptText.text = prompt;
                        _promptCanvasGroup.alpha = 1f;
                    }
                    else
                    {
                        _promptCanvasGroup.alpha = 0f;
                    }
                }
                else
                {
                    _promptCanvasGroup.alpha = 0f;
                }
            }
        }

        #endregion

        #region Blocked Message

        private void ShowBlockedMessage(string message)
        {
            if (_interactionPromptText == null || _promptCanvasGroup == null) return;

            _interactionPromptText.text = message;

            if (_blockedMessageCoroutine != null)
            {
                StopCoroutine(_blockedMessageCoroutine);
            }

            _blockedMessageCoroutine = StartCoroutine(FadeBlockedMessage());
        }

        private IEnumerator FadeBlockedMessage()
        {
            _isShowingBlockedMessage = true;

            // Fade in (fast)
            float elapsed = 0f;
            const float FADE_IN_DURATION = 0.15f;
            while (elapsed < FADE_IN_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                _promptCanvasGroup.alpha = elapsed / FADE_IN_DURATION;
                yield return null;
            }
            _promptCanvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(2f);

            // Fade out
            elapsed = 0f;
            const float FADE_OUT_DURATION = 0.5f;
            while (elapsed < FADE_OUT_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                _promptCanvasGroup.alpha = 1f - (elapsed / FADE_OUT_DURATION);
                yield return null;
            }
            _promptCanvasGroup.alpha = 0f;
            _isShowingBlockedMessage = false;
            _blockedMessageCoroutine = null;
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

        private void ShowNotification(string text)
        {
            if (_clueNotificationText == null || _clueNotificationGroup == null) return;

            _clueNotificationText.text = text;

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

        private void HandleItemPickedUp(Items.ItemData item)
        {
            if (item != null)
            {
                ShowNotification($"{item.DisplayName} picked up");
            }
        }

        private void HandleItemDropped(Items.ItemData item, Vector3 position)
        {
            if (item != null)
            {
                ShowNotification($"{item.DisplayName} dropped");
            }
        }

        private void HandleItemUsed(Items.ItemData item)
        {
            if (item != null)
            {
                ShowNotification($"{item.DisplayName} used");
            }
        }

        private void HandleInteractionBlocked(string message)
        {
            ShowBlockedMessage(message);
        }

        private void HandleWakeUpStarted()
        {
            if (_crosshairDot != null) _crosshairDot.gameObject.SetActive(false);
            if (_crosshairX != null) _crosshairX.SetActive(false);
        }

        #endregion
    }
}
