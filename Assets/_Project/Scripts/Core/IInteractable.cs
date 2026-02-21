using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Interface for all interactable objects in the game.
    /// Implemented by: CluePickup, DoorController, LockedDoor, SlidableFurniture,
    /// ItemPickup, ScrewInteractable, ToolReceiver, CarPartPickup, CarInstallZone, CarRepairStation.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Called when the player interacts with this object.</summary>
        void Interact(GameObject interactor);

        /// <summary>Returns the UI prompt text shown when aiming at this object.</summary>
        string GetPromptText();

        /// <summary>Whether this interaction can succeed right now. Used to gate Interact() and show blocked messages.</summary>
        bool CanInteract(GameObject interactor) => true;

        /// <summary>Message to display when interaction is blocked (CanInteract returns false).</summary>
        string GetBlockedMessage() => "";
    }
}
