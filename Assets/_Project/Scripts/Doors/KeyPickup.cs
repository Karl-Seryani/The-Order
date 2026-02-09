using UnityEngine;

namespace TheOrder.Doors
{
    /// <summary>
    /// World-space interactable key object. Single press to collect.
    /// Fires GameEvents.KeyCollected on pickup.
    /// </summary>
    public class KeyPickup : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Key Data")]
        [SerializeField] private KeyData _keyData;

        #endregion

        #region IInteractable

        /// <summary>Collect the key and destroy the pickup.</summary>
        public void Interact(GameObject interactor)
        {
            if (_keyData == null)
            {
                Debug.LogWarning($"[KeyPickup] No KeyData assigned on {gameObject.name}");
                return;
            }

            GameEvents.KeyCollected(_keyData);
            Destroy(gameObject);
        }

        /// <summary>Returns pickup prompt with key name.</summary>
        public string GetPromptText()
        {
            if (_keyData == null) return "Pick up key";
            return $"Pick up {_keyData.DisplayName}";
        }

        #endregion
    }
}
