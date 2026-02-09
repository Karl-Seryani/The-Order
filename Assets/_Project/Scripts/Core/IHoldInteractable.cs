using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Extended interaction interface for hold-based mechanics.
    /// Objects that support gradual open/close/slide implement this.
    /// Extends IInteractable so existing raycast detection still works.
    /// </summary>
    public interface IHoldInteractable : IInteractable
    {
        /// <summary>Called on the frame the player presses E while looking at this object.</summary>
        void HoldStart(GameObject interactor);

        /// <summary>Called every frame while the player holds E.</summary>
        void HoldUpdate(GameObject interactor, float deltaTime);

        /// <summary>Called when the player releases E or looks away.</summary>
        void HoldRelease(GameObject interactor);

        /// <summary>Prompt text shown while hovering (e.g., "Open Door").</summary>
        string GetHoldPromptText();
    }
}
