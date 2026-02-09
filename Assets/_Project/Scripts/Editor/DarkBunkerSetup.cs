using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheOrder.Editor
{
    /// <summary>
    /// Editor utility to set up dark bunker atmosphere.
    /// Disables all scene lights except the player's flashlight.
    /// Sets ambient lighting to near-black.
    /// </summary>
    public static class DarkBunkerSetup
    {
        [MenuItem("Tools/The Order/Setup Dark Bunker")]
        public static void SetupDarkBunker()
        {
            int disabledCount = 0;

            // Find all lights in the scene
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            foreach (Light light in allLights)
            {
                // Skip the player's flashlight (it's a child of the player)
                if (light.type == LightType.Spot)
                {
                    // Check if this belongs to the player
                    Transform parent = light.transform.parent;
                    while (parent != null)
                    {
                        if (parent.name.Contains("Player") || parent.name.Contains("Flashlight"))
                        {
                            break;
                        }
                        parent = parent.parent;
                    }
                    if (parent != null)
                    {
                        Debug.Log($"[DarkBunker] Keeping player flashlight: {light.gameObject.name}");
                        continue;
                    }
                }

                // Disable shadows and light
                Undo.RecordObject(light, "Disable Scene Light");
                light.shadows = LightShadows.None;
                light.enabled = false;
                disabledCount++;
                Debug.Log($"[DarkBunker] Disabled: {light.gameObject.name} ({light.type})");
            }

            // Set ambient lighting to near-black
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.025f, 1f);
            RenderSettings.reflectionIntensity = 0f;

            // Set skybox to null (solid color)
            RenderSettings.skybox = null;
            RenderSettings.subtractiveShadowColor = Color.black;

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log($"[DarkBunker] Setup complete. Disabled {disabledCount} lights. Ambient set to near-black.");
            Debug.Log("[DarkBunker] Remember to boost the player flashlight intensity if needed.");
        }

        [MenuItem("Tools/The Order/Disable All Light Shadows")]
        public static void DisableAllShadows()
        {
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int count = 0;

            foreach (Light light in allLights)
            {
                if (light.shadows != LightShadows.None)
                {
                    Undo.RecordObject(light, "Disable Shadow");
                    light.shadows = LightShadows.None;
                    EditorUtility.SetDirty(light);
                    count++;
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log($"[DarkBunker] Disabled shadows on {count} lights.");
        }

        [MenuItem("Tools/The Order/Re-enable All Scene Lights")]
        public static void ReenableAllLights()
        {
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int enabledCount = 0;

            foreach (Light light in allLights)
            {
                if (!light.enabled)
                {
                    Undo.RecordObject(light, "Re-enable Light");
                    light.enabled = true;
                    enabledCount++;
                }
            }

            // Restore ambient lighting for editor visibility
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.45f, 1f);
            RenderSettings.reflectionIntensity = 1f;

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log($"[DarkBunker] Re-enabled {enabledCount} lights. Ambient restored for editor.");
        }
    }
}
