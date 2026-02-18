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

        [Header("Note Mode")]
        [Tooltip("If true, this is a re-readable note that is never collected or destroyed.")]
        [SerializeField] private bool _isNote;

        #endregion

        #region Private Fields

        private bool _isReading;

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
                GameEvents.ClueViewed(_clueData);
            }
            else
            {
                if (_isNote)
                {
                    // Note mode — dismiss reading panel, stay in world
                    _isReading = false;
                    GameEvents.ClueCollected(_clueData);
                }
                else
                {
                    // Clue mode — collect and destroy
                    if (string.IsNullOrEmpty(_clueData.Id))
                    {
                        Debug.LogWarning($"[CluePickup] Clue '{_clueData.Title}' has no ID — not collecting.", this);
                        _isReading = false;
                        return;
                    }
                    GameEvents.ClueCollected(_clueData);
                    Destroy(gameObject);
                }
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
