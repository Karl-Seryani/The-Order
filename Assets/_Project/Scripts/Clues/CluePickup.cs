using UnityEngine;

namespace TheOrder.Clues
{
    /// <summary>
    /// World-space interactable clue/note object. Two-state interaction:
    /// first press shows content, second press collects (clue) or dismisses (note).
    /// Notes stay in the world and can be re-read.
    /// </summary>
    public class CluePickup : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Clue Data")]
        [SerializeField] private ClueData _clueData;

        [Header("Audio")]
        [SerializeField] private AudioClip _paperSound;
        [SerializeField] [Range(0f, 1f)] private float _paperVolume = 0.4f;

        [Header("Note Mode")]
        [Tooltip("If true, this is a re-readable note that is never collected or destroyed.")]
        [SerializeField] private bool _isNote;

        #endregion

        #region Private Fields

        private bool _isReading;

        #endregion

        #region Static Reading State

        private static CluePickup _currentlyReading;

        /// <summary>True if the player is currently reading any clue/note.</summary>
        public static bool IsReading => _currentlyReading != null;

        /// <summary>
        /// Dismisses the currently open clue/note from anywhere.
        /// Called by PlayerInteraction (E press) and PauseMenuUI (Escape press).
        /// </summary>
        public static void DismissCurrentClue()
        {
            if (_currentlyReading == null) return;

            var pickup = _currentlyReading;
            _currentlyReading = null;
            pickup._isReading = false;

            if (pickup._clueData != null)
                GameEvents.ClueCollected(pickup._clueData);

            // Clue mode — destroy after collecting
            if (!pickup._isNote && pickup._clueData != null && !string.IsNullOrEmpty(pickup._clueData.Id))
                Destroy(pickup.gameObject);
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // If this clue was already collected in a previous day, hide it
            if (_clueData != null && !_isNote && !string.IsNullOrEmpty(_clueData.Id))
            {
                if (ClueManager.Instance != null && ClueManager.Instance.IsClueCollected(_clueData.Id))
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (_currentlyReading == this)
                _currentlyReading = null;
        }

        #endregion

        #region IInteractable

        /// <summary>
        /// First interaction: show content on screen.
        /// Second: collect and destroy (clue) or dismiss reading panel (note).
        /// </summary>
        public void Interact(GameObject interactor)
        {
            if (_clueData == null)
            {
                Debug.LogWarning($"[CluePickup] No ClueData assigned on {gameObject.name}");
                return;
            }

            if (!_isReading)
            {
                // First press — show content on screen
                _isReading = true;
                _currentlyReading = this;
                if (_paperSound != null)
                    AudioSource.PlayClipAtPoint(_paperSound, transform.position, _paperVolume);
                GameEvents.ClueViewed(_clueData);
            }
            else
            {
                // Second press — delegate to static dismiss
                DismissCurrentClue();
            }
        }

        /// <summary>Returns contextual prompt based on reading state.</summary>
        public string GetPromptText()
        {
            if (_clueData == null) return "Read note";

            if (_isReading)
                return _isNote ? "Close" : $"Collect {_clueData.Title}";

            return $"Read {_clueData.Title}";
        }

        #endregion
    }
}
