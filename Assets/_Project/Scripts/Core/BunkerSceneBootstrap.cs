using UnityEngine;

namespace TheOrder.Core
{
    /// <summary>
    /// Bootstrap script for the Bunker scene.
    /// Sets the game state to Playing and locks the cursor on scene load.
    /// </summary>
    public class BunkerSceneBootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Playing);
            }
            else
            {
                Debug.LogWarning("[BunkerSceneBootstrap] GameManager not found. Creating temporary instance.");
                // Cursor lock fallback if no GameManager exists (e.g., testing scene directly)
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
