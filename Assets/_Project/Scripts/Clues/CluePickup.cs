using UnityEngine;

namespace TheOrder.Clues
{
    /// <summary>
    /// World-space interactable clue object. Two-state interaction:
    /// first press shows clue content, second press collects and destroys.
    /// All clues share the same visual type (torn documents/notes).
    /// </summary>
    public class CluePickup : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Clue Data")]
        [SerializeField] private ClueData _clueData;

        #endregion

        #region Private Fields

        private bool _isReading;

        #endregion

        #region IInteractable

        /// <summary>
        /// First interaction: show clue on screen. Second: collect and destroy.
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
                // First press — show clue content on screen
                _isReading = true;
                GameEvents.ClueViewed(_clueData);
            }
            else
            {
                // Second press — collect and destroy
                GameEvents.ClueCollected(_clueData);
                Destroy(gameObject);
            }
        }

        /// <summary>Returns contextual prompt based on reading state.</summary>
        public string GetPromptText()
        {
            if (_clueData == null) return "Pick up clue";

            if (_isReading)
                return $"Collect {_clueData.Title}";

            return $"Read {_clueData.Title}";
        }

        #endregion
    }
}
