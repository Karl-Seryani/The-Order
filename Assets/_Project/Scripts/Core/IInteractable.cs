using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Interface for all interactable objects in the game.
    /// Implemented by: CluePickup, DoorController, HidingSpot, CameraTerminal.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Called when the player interacts with this object.</summary>
        void Interact(GameObject interactor);

        /// <summary>Returns the UI prompt text shown when aiming at this object.</summary>
        string GetPromptText();
    }
}
