#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TheOrder.Editor
{
    /// <summary>
    /// Adds the wake-up sequence objects to the active Bunker scene.
    /// Creates a WakeUpCanvas with blink overlay and wires the WakeUpSequence script.
    /// Run via menu: Tools/The Order/Setup Wake Up Sequence
    /// </summary>
    public static class SetupWakeUpSequence
    {
        [MenuItem("Tools/The Order/Setup Wake Up Sequence")]
        public static void Setup()
        {
            // Find existing player camera
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[SetupWakeUp] Player not found. Open the Bunker scene first.");
                return;
            }

            var playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("[SetupWakeUp] Player camera not found.");
                return;
            }

            // Remove existing WakeUpSequence if present
            var existingWakeUp = Object.FindFirstObjectByType<Player.WakeUpSequence>();
            if (existingWakeUp != null)
            {
                Object.DestroyImmediate(existingWakeUp.gameObject);
                Debug.Log("[SetupWakeUp] Removed existing WakeUpSequence.");
            }

            // Remove existing WakeUpCanvas if present
            var existingCanvas = GameObject.Find("WakeUpCanvas");
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas);
                Debug.Log("[SetupWakeUp] Removed existing WakeUpCanvas.");
            }

            // Create WakeUpCanvas (sort order 100 — on top of everything)
            var canvasGo = new GameObject("WakeUpCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Full-screen black overlay image
            var overlayGo = new GameObject("BlinkOverlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);

            var overlayRt = overlayGo.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            var overlayImg = overlayGo.AddComponent<Image>();
            overlayImg.color = Color.black;

            var overlayGroup = overlayGo.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 1f;

            // Create WakeUpSequence GameObject
            var wakeUpGo = new GameObject("WakeUpSequence");
            var wakeUpScript = wakeUpGo.AddComponent<Player.WakeUpSequence>();

            // Wire serialized fields
            var so = new SerializedObject(wakeUpScript);
            so.FindProperty("_playerCamera").objectReferenceValue = playerCamera.transform;
            so.FindProperty("_blinkOverlay").objectReferenceValue = overlayGroup;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(canvasGo);
            EditorUtility.SetDirty(wakeUpGo);

            Debug.Log("[SetupWakeUp] Wake-up sequence created. WakeUpCanvas + WakeUpSequence added to scene. Save the scene to persist.");
        }
    }
}
#endif
