using UnityEngine;

namespace TheOrder
{
    /// <summary>
    /// Disables all scene lights at runtime except the player's flashlight.
    /// Lights stay enabled in the Editor for visibility while working.
    /// </summary>
    public class RuntimeLightManager : MonoBehaviour
    {
        [SerializeField] private Color _ambientColor = new Color(0.02f, 0.02f, 0.025f, 1f);

        private void Awake()
        {
            DisableSceneLights();
            SetDarkAmbient();
        }

        private void DisableSceneLights()
        {
            int count = 0;
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

            foreach (Light light in allLights)
            {
                // Skip the player's flashlight
                if (light.type == LightType.Spot && IsPlayerFlashlight(light.transform))
                    continue;

                light.enabled = false;
                count++;
            }

            Debug.Log($"[RuntimeLightManager] Disabled {count} scene lights.");
        }

        private static bool IsPlayerFlashlight(Transform t)
        {
            Transform parent = t.parent;
            while (parent != null)
            {
                if (parent.name.Contains("Player") || parent.name.Contains("Flashlight"))
                    return true;
                parent = parent.parent;
            }
            return false;
        }

        private void SetDarkAmbient()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = _ambientColor;
            RenderSettings.reflectionIntensity = 0f;
        }
    }
}