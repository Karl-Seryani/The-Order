using UnityEngine;

namespace TheOrder.Ending
{
    /// <summary>
    /// IInteractable on the main door for Easy/Medium escape.
    /// Player presses E to escape. Disables itself on Practice/Hard (car repair required).
    /// Lives on the same GO as LockedDoor — PlayerInteraction prioritizes this when enabled.
    /// </summary>
    public class MainDoorEscapeTrigger : MonoBehaviour, IInteractable
    {
        private bool _triggered;

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

            _triggered = true;
#if UNITY_EDITOR
            Debug.Log("[MainDoorEscapeTrigger] Player escaped through the main door!");
#endif
            GameEvents.CarRepairComplete();
        }

        public string GetPromptText()
        {
            return "Escape";
        }

        public bool CanInteract(GameObject interactor)
        {
            return !_triggered;
        }

        public string GetBlockedMessage()
        {
            return "";
        }
    }
}
