
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TheOrder.Editor
{
    /// <summary>
    /// One-shot editor utility to fix HUD canvas layouts.
    /// Run via menu: The Order/Setup UI Layout
    /// </summary>
    public static class UILayoutSetup
    {
        [MenuItem("The Order/Setup UI Layout")]
        public static void SetupUILayout()
        {
            SetupHUD();
            SetupClueReadingPanel();
            SetupClueVisibility();
            Debug.Log("[UILayoutSetup] UI layout setup complete.");
        }

        private static void SetupHUD()
        {
            var hudCanvas = GameObject.Find("HUDCanvas");
            if (hudCanvas == null) { Debug.LogWarning("HUDCanvas not found"); return; }

            var canvas = hudCanvas.GetComponent<Canvas>();
            if (canvas != null) canvas.sortingOrder = 10;

            // --- Objective Text (top-center, with CanvasGroup for fade) ---
            var objectiveGo = GameObject.Find("ObjectiveText");
            if (objectiveGo != null)
            {
                var rt = objectiveGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1);
                rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                rt.anchoredPosition = new Vector2(0, -25);
                rt.sizeDelta = new Vector2(600, 40);

                // Ensure CanvasGroup exists for fade
                var cg = objectiveGo.GetComponent<CanvasGroup>();
                if (cg == null) cg = objectiveGo.AddComponent<CanvasGroup>();
                cg.alpha = 0f;

                var text = objectiveGo.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 22;
                    text.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                    text.alignment = TextAnchor.MiddleCenter;
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    text.text = "Explore the bunker for clues or exit";
                }
            }

            // --- Clue Counter (top-left) ---
            var counterGo = GameObject.Find("ClueCounterText");
            if (counterGo != null)
            {
                var rt = counterGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(30, -30);
                rt.sizeDelta = new Vector2(200, 50);

                var text = counterGo.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 16;
                    text.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    text.alignment = TextAnchor.UpperLeft;
                    text.lineSpacing = 1.1f;
                    text.text = "Truth: 0/11\nMike: 0/7";
                }
            }

            // --- Interaction Prompt Panel (bottom-center) ---
            var promptPanel = GameObject.Find("InteractionPromptPanel");
            if (promptPanel != null)
            {
                var rt = promptPanel.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0);
                rt.anchorMax = new Vector2(0.5f, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.anchoredPosition = new Vector2(0, 60);
                rt.sizeDelta = new Vector2(350, 45);

                var img = promptPanel.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0, 0, 0, 0.6f);
                }
            }

            // --- Interaction Prompt Text (fill parent) ---
            var promptText = GameObject.Find("InteractionPromptText");
            if (promptText != null)
            {
                var rt = promptText.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(10, 5);
                rt.offsetMax = new Vector2(-10, -5);

                var text = promptText.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 20;
                    text.color = Color.white;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.text = "Press E to interact";
                }
            }

            // --- Clue Notification Panel (top-right) ---
            var notifPanel = GameObject.Find("ClueNotificationPanel");
            if (notifPanel != null)
            {
                var rt = notifPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(1, 1);
                    rt.anchoredPosition = new Vector2(-30, -30);
                    rt.sizeDelta = new Vector2(350, 40);
                }

                var cg = notifPanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
            }

            // --- Clue Notification Text (fill parent) ---
            var notifText = GameObject.Find("ClueNotificationText");
            if (notifText != null)
            {
                var rt = notifText.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(10, 5);
                rt.offsetMax = new Vector2(-10, -5);

                var text = notifText.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 20;
                    text.color = new Color(1f, 0.85f, 0.4f, 1f);
                    text.alignment = TextAnchor.MiddleRight;
                }
            }

            Debug.Log("[UILayoutSetup] HUD layout configured.");
        }

        private static void SetupClueReadingPanel()
        {
            var readingPanel = GameObject.Find("ClueReadingPanel");
            if (readingPanel == null)
            {
                Debug.LogWarning("ClueReadingPanel not found — create it first");
                return;
            }

            // Panel: centered, 60% width, 70% height
            var rt = readingPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.15f);
            rt.anchorMax = new Vector2(0.8f, 0.85f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = readingPanel.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.06f, 0.06f, 0.1f, 0.95f);
            }

            // Title
            var titleGo = GameObject.Find("ClueReadingTitle");
            if (titleGo != null)
            {
                var titleRt = titleGo.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0, 1);
                titleRt.anchorMax = new Vector2(1, 1);
                titleRt.pivot = new Vector2(0.5f, 1);
                titleRt.anchoredPosition = new Vector2(0, -20);
                titleRt.sizeDelta = new Vector2(-60, 45);

                var text = titleGo.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 28;
                    text.color = new Color(1f, 0.85f, 0.4f, 1f);
                    text.alignment = TextAnchor.MiddleCenter;
                    text.fontStyle = FontStyle.Bold;
                }
            }

            // Category
            var catGo = GameObject.Find("ClueReadingCategory");
            if (catGo != null)
            {
                var catRt = catGo.GetComponent<RectTransform>();
                catRt.anchorMin = new Vector2(0, 1);
                catRt.anchorMax = new Vector2(1, 1);
                catRt.pivot = new Vector2(0.5f, 1);
                catRt.anchoredPosition = new Vector2(0, -70);
                catRt.sizeDelta = new Vector2(-60, 30);

                var text = catGo.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 16;
                    text.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    text.alignment = TextAnchor.MiddleCenter;
                }
            }

            // Content
            var contentGo = GameObject.Find("ClueReadingContent");
            if (contentGo != null)
            {
                var contentRt = contentGo.GetComponent<RectTransform>();
                contentRt.anchorMin = new Vector2(0, 0);
                contentRt.anchorMax = new Vector2(1, 1);
                contentRt.offsetMin = new Vector2(40, 60);
                contentRt.offsetMax = new Vector2(-40, -110);

                var text = contentGo.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 18;
                    text.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                    text.alignment = TextAnchor.UpperLeft;
                }
            }

            // Hint
            var hintGo = GameObject.Find("ClueReadingHint");
            if (hintGo != null)
            {
                var hintRt = hintGo.GetComponent<RectTransform>();
                hintRt.anchorMin = new Vector2(0.5f, 0);
                hintRt.anchorMax = new Vector2(0.5f, 0);
                hintRt.pivot = new Vector2(0.5f, 0);
                hintRt.anchoredPosition = new Vector2(0, 15);
                hintRt.sizeDelta = new Vector2(300, 30);

                var text = hintGo.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 16;
                    text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    text.alignment = TextAnchor.MiddleCenter;
                    text.text = "Press E to collect";
                }
            }

            Debug.Log("[UILayoutSetup] Clue reading panel configured.");
        }

        private static void SetupClueVisibility()
        {
            // Make clue cubes bigger and give them a visible yellow-ish material
            var cluePickups = Object.FindObjectsByType<TheOrder.Clues.CluePickup>(FindObjectsSortMode.None);

            foreach (var pickup in cluePickups)
            {
                // Scale up to be more visible
                pickup.transform.localScale = new Vector3(0.3f, 0.05f, 0.4f);

                // Create a bright material
                var renderer = pickup.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.SetColor("_BaseColor", new Color(0.9f, 0.85f, 0.6f, 1f));
                    mat.SetFloat("_Smoothness", 0.2f);
                    renderer.sharedMaterial = mat;
                }
            }

            Debug.Log($"[UILayoutSetup] Updated {cluePickups.Length} clue pickup visuals.");
        }
    }
}
#endif
