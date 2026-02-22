using UnityEngine;

namespace TheOrder.Ending
{
    /// <summary>
    /// IInteractable on the main door for Easy/Medium escape.
    /// Requires the same key as the sibling LockedDoor before allowing escape.
    /// Disables itself on Practice/Hard (car repair required).
    /// Lives on the same GO as LockedDoor — PlayerInteraction prioritizes this when enabled.
    /// </summary>
    public class MainDoorEscapeTrigger : MonoBehaviour, IInteractable
    {
        private bool _triggered;
        private Doors.LockedDoor _lockedDoor;

        private void Awake()
        {
            _lockedDoor = GetComponent<Doors.LockedDoor>();
        }

        private void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.RequiresCarRepair)
            {
                enabled = false;
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_triggered) return;

            // Must have the key first
            if (_lockedDoor != null && !_lockedDoor.IsUnlocked)
            {
                var inventory = Player.PlayerInventory.Instance;
                if (inventory == null || !inventory.HasKey(_lockedDoor.RequiredItem))
                    return;

                // Unlock the door (delegates to LockedDoor for sound + visual)
                _lockedDoor.Interact(interactor);
            }

            _triggered = true;
#if UNITY_EDITOR
            Debug.Log("[MainDoorEscapeTrigger] Player escaped through the main door!");
#endif
            GameEvents.CarRepairComplete();
        }

        public string GetPromptText()
        {
            if (_lockedDoor != null && !_lockedDoor.IsUnlocked)
                return _lockedDoor.GetPromptText();

            return "Escape";
        }

        public bool CanInteract(GameObject interactor)
        {
            if (_triggered) return false;

            // Show as interactable even when locked (so player sees prompt)
            return true;
        }

        public string GetBlockedMessage()
        {
            if (_lockedDoor != null && !_lockedDoor.IsUnlocked)
                return _lockedDoor.GetBlockedMessage();

            return "";
        }
    }
}
