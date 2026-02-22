using UnityEngine;

namespace TheOrder.Ending
{
    /// <summary>
    /// IInteractable on the "start" GameObject (ignition).
    /// When player is seated and has the car key, starts the engine and triggers ending.
    /// </summary>
    public class CarIgnition : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [SerializeField] private CarRepairStation _station;
        [SerializeField] private Items.ItemData _carKeyItemData;

        #endregion

        #region IInteractable

        public void Interact(GameObject interactor)
        {
            if (_station == null) return;
            _station.StartCarFromIgnition();
        }

        public string GetPromptText()
        {
            if (!HasCarKey())
            {
                return "Ignition";
            }
            return "Start car";
        }

        public bool CanInteract(GameObject interactor)
        {
            return HasCarKey();
        }

        public string GetBlockedMessage()
        {
            if (!HasCarKey())
            {
                return "Need Car Key";
            }
            return "";
        }

        #endregion

        #region Private Methods

        private bool HasCarKey()
        {
            var inventory = Player.PlayerInventory.Instance;
            return inventory != null && inventory.HasKey(_carKeyItemData);
        }

        #endregion
    }
}
