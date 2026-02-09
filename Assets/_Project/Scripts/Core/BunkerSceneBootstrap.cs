using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Bootstrap script for the Bunker scene.
    /// Sets the game state to Playing and locks the cursor on scene load.
    /// </summary>
    public class BunkerSceneBootstrap : MonoBehaviour
    {
        private void Start()
        {
            // Always transition to Playing state
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);

            var wakeUp = FindFirstObjectByType<Player.WakeUpSequence>();

            // If respawning, skip wake-up and go straight to gameplay
            if (GameManager.Instance != null && GameManager.Instance.SkipWakeUpSequence)
            {
                GameManager.Instance.SetSkipWakeUpSequence(false);
                if (wakeUp != null)
                    wakeUp.Skip();
                return;
            }

            // Start wake-up sequence if present
            if (wakeUp != null)
            {
                wakeUp.Begin();
            }
            else
            {
                Debug.LogWarning("[BunkerSceneBootstrap] No WakeUpSequence found.");
            }
        }
    }
}
