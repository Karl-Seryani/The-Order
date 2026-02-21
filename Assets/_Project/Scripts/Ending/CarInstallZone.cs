using UnityEngine;

namespace TheOrder.Ending
{
    /// <summary>
    /// Child collider zone on the car. Each zone maps to a specific CarPartPickup.
    /// When the player interacts with this zone, it delegates to the parent CarRepairStation
    /// but targets only the assigned part.
    /// </summary>
    public class CarInstallZone : MonoBehaviour, IInteractable
    {
        [SerializeField] private Items.CarPartPickup _assignedPart;

        /// <summary>The specific car part this zone handles.</summary>
        public Items.CarPartPickup AssignedPart => _assignedPart;

        private CarRepairStation _station;

        private void Awake()
        {
            _station = GetComponentInParent<CarRepairStation>();
        }

        public void Interact(GameObject interactor)
        {
            if (_station == null) return;
            _station.InteractWithZone(interactor, _assignedPart);
        }

        public string GetPromptText()
        {
            if (_station == null) return "";
            return _station.GetZonePromptText(_assignedPart);
        }

        public bool CanInteract(GameObject interactor)
        {
            if (_station == null) return false;
            return _station.CanInteractWithZone(interactor, _assignedPart);
        }

        public string GetBlockedMessage()
        {
            if (_station == null) return "";
            return _station.GetZoneBlockedMessage(_assignedPart);
        }
    }
}
