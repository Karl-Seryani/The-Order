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
            // If respawning, skip wake-up and go straight to gameplay
            if (GameManager.Instance != null && GameManager.Instance.SkipWakeUpSequence)
            {
                GameManager.Instance.SetSkipWakeUpSequence(false);
                GameManager.Instance.SetState(GameState.Playing);
                var wakeUpSequence = FindFirstObjectByType<Player.WakeUpSequence>();
                if (wakeUpSequence != null)
                {
                    wakeUpSequence.Skip();
                }
                return;
            }

            // Set Prologue state — input disabled, cursor locked, Hunter paused
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Prologue);

            // Start wake-up sequence if present
            var wakeUp = FindFirstObjectByType<Player.WakeUpSequence>();
            if (wakeUp != null)
            {
                wakeUp.Begin();
            }
            else
            {
                // Fallback: no wake-up sequence, go straight to Playing
                Debug.LogWarning("[BunkerSceneBootstrap] No WakeUpSequence found. Setting Playing immediately.");
                if (GameManager.Instance != null)
                    GameManager.Instance.SetState(GameState.Playing);
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }
}
