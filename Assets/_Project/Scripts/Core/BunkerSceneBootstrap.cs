using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Bootstrap script for the Bunker scene.
    /// Sets the game state to Playing, shows the day overlay on a black screen,
    /// then starts the wake-up sequence after the overlay finishes.
    /// </summary>
    public class BunkerSceneBootstrap : MonoBehaviour
    {
        private void Start()
        {
            // Always transition to Playing state
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);

            var wakeUp = FindFirstObjectByType<Player.WakeUpSequence>();
            var dayOverlay = FindFirstObjectByType<UI.DayOverlayUI>();

            // If respawning, skip wake-up and just show day overlay
            if (GameManager.Instance != null && GameManager.Instance.SkipWakeUpSequence)
            {
                GameManager.Instance.SetSkipWakeUpSequence(false);
                if (wakeUp != null)
                    wakeUp.Skip();

                // Show day overlay (no wake-up to chain)
                if (dayOverlay != null)
                    dayOverlay.ShowDayText();

                return;
            }

            // Normal first load: show day overlay on black screen, THEN start wake-up
            if (dayOverlay != null)
            {
                dayOverlay.OnComplete = () =>
                {
                    if (wakeUp != null)
                        wakeUp.Begin();
                    else
                        Debug.LogWarning("[BunkerSceneBootstrap] No WakeUpSequence found.");
                };
                dayOverlay.ShowDayText();
            }
            else if (wakeUp != null)
            {
                // No day overlay — just start wake-up directly
                wakeUp.Begin();
            }
            else
            {
                Debug.LogWarning("[BunkerSceneBootstrap] No WakeUpSequence or DayOverlayUI found.");
            }
        }
    }
}
