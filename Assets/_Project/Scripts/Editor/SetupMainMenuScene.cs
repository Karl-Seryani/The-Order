#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TheOrder.Editor
{
    /// <summary>
    /// Creates the Main Menu scene with background image, overlay, and horror-styled UI.
    /// Place your background image at: Assets/_Project/Art/UI/MainMenuBG.png (or .jpg)
    /// Run via menu: Tools/The Order/Setup Main Menu Scene
    /// </summary>
    public static class SetupMainMenuScene
    {
        // Dark overlay on top of background image for readability
        private static readonly Color OVERLAY_COLOR = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color GOLDEN_COLOR = new Color(0.95f, 0.78f, 0.3f, 1f);
        private static readonly Color BODY_TEXT_COLOR = new Color(0.85f, 0.85f, 0.85f, 1f);
        // Buttons with slight dark background for visibility
        private static readonly Color BUTTON_NORMAL = new Color(0f, 0f, 0f, 0.35f);
        private static readonly Color BUTTON_HOVER = new Color(0.95f, 0.78f, 0.3f, 0.25f);
        private static readonly Color BUTTON_PRESSED = new Color(0.95f, 0.78f, 0.3f, 0.15f);
        private static readonly Color BUTTON_TEXT_NORMAL = new Color(1f, 1f, 1f, 1f);
        private static readonly Color BUTTON_TEXT_HOVER_COLOR = new Color(1f, 0.85f, 0.4f, 1f);
        // Tutorial panel background
        private static readonly Color BACKGROUND_COLOR = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        private static readonly Color TAB_ACTIVE = new Color(0.3f, 0.3f, 0.38f, 1f);

        [MenuItem("Tools/The Order/Setup Main Menu Scene")]
        public static void Setup()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var cameraGo = new GameObject("Main Camera");
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            // GameManager
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            // EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();

            // Canvas
            var canvasGo = new GameObject("MainMenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // ============================================
            // MENU PANEL
            // ============================================
            var menuPanel = CreatePanel(canvasGo.transform, "MenuPanel", true);
            // Make MenuPanel's own Image transparent — the bg image handles visuals
            var menuPanelImg = menuPanel.GetComponent<Image>();
            menuPanelImg.color = Color.clear;

            // --- Full-screen background RawImage ---
            var bgGo = new GameObject("BackgroundImage");
            bgGo.transform.SetParent(menuPanel.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var rawImg = bgGo.AddComponent<RawImage>();
            rawImg.color = Color.white;

            // Try to load the background texture from known paths
            Texture2D bgTexture = null;
            string[] possiblePaths = {
                "Assets/_Project/Art/UI/MainMenuBG.png",
                "Assets/_Project/Art/UI/MainMenuBG.jpg",
                "Assets/_Project/Art/UI/MainMenuBG.jpeg",
                "Assets/_Project/Art/UI/menu_bg.png",
                "Assets/_Project/Art/UI/menu_bg.jpg"
            };
            foreach (var path in possiblePaths)
            {
                bgTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (bgTexture != null)
                {
                    Debug.Log($"[SetupMainMenu] Found background image: {path}");
                    break;
                }
            }
            if (bgTexture != null)
            {
                rawImg.texture = bgTexture;
            }
            else
            {
                // Fallback: dark gradient-like color if no image found
                rawImg.color = new Color(0.04f, 0.03f, 0.05f, 1f);
                Debug.LogWarning("[SetupMainMenu] No background image found. Place your image at Assets/_Project/Art/UI/MainMenuBG.png then re-run, or drag it onto the BackgroundImage RawImage component.");
            }

            // --- Dark overlay for text readability ---
            var overlayGo = new GameObject("DarkOverlay");
            overlayGo.transform.SetParent(menuPanel.transform, false);
            var overlayRt = overlayGo.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;
            var overlayImg = overlayGo.AddComponent<Image>();
            overlayImg.color = OVERLAY_COLOR;
            overlayImg.raycastTarget = false;

            // --- Left-side gradient overlay for button area ---
            var leftGradGo = new GameObject("LeftGradient");
            leftGradGo.transform.SetParent(menuPanel.transform, false);
            var leftGradRt = leftGradGo.AddComponent<RectTransform>();
            leftGradRt.anchorMin = new Vector2(0f, 0f);
            leftGradRt.anchorMax = new Vector2(0.35f, 1f);
            leftGradRt.offsetMin = Vector2.zero;
            leftGradRt.offsetMax = Vector2.zero;
            var leftGradImg = leftGradGo.AddComponent<Image>();
            leftGradImg.color = new Color(0f, 0f, 0f, 0.5f);
            leftGradImg.raycastTarget = false;

            // --- Title: "THE ORDER" top-left ---
            var titleGo = CreateTextObject(menuPanel.transform, "TitleText",
                "THE ORDER", 64, GOLDEN_COLOR, TextAnchor.MiddleLeft, FontStyle.Bold);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.75f);
            titleRt.anchorMax = new Vector2(0.35f, 0.88f);
            titleRt.offsetMin = new Vector2(60, 0);
            titleRt.offsetMax = Vector2.zero;

            // --- Subtitle ---
            var subtitleGo = CreateTextObject(menuPanel.transform, "SubtitleText",
                "No one leaves.", 22, new Color(0.6f, 0.55f, 0.45f, 0.9f), TextAnchor.UpperLeft, FontStyle.Italic);
            var subRt = subtitleGo.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0.70f);
            subRt.anchorMax = new Vector2(0.35f, 0.76f);
            subRt.offsetMin = new Vector2(63, 0);
            subRt.offsetMax = Vector2.zero;

            // --- Buttons: left-aligned, stacked vertically ---
            var buttonsGo = new GameObject("Buttons");
            buttonsGo.transform.SetParent(menuPanel.transform, false);
            var buttonsRt = buttonsGo.AddComponent<RectTransform>();
            buttonsRt.anchorMin = new Vector2(0f, 0.25f);
            buttonsRt.anchorMax = new Vector2(0.3f, 0.62f);
            buttonsRt.offsetMin = new Vector2(60, 0);
            buttonsRt.offsetMax = Vector2.zero;
            var vlg = buttonsGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var playBtn = CreateMenuButton(buttonsGo.transform, "PlayButton", "START", 36, FontStyle.Bold);
            var tutorialBtn = CreateMenuButton(buttonsGo.transform, "TutorialButton", "TUTORIAL", 28, FontStyle.Normal);
            var settingsBtn = CreateMenuButton(buttonsGo.transform, "SettingsButton", "SETTINGS", 28, FontStyle.Normal);
            var quitBtn = CreateMenuButton(buttonsGo.transform, "QuitButton", "QUIT", 28, FontStyle.Normal);

            // --- Version / copyright bottom-left ---
            var versionGo = CreateTextObject(menuPanel.transform, "VersionText",
                "v0.1 — The Order", 14, new Color(0.4f, 0.4f, 0.4f, 0.6f), TextAnchor.LowerLeft, FontStyle.Normal);
            var verRt = versionGo.GetComponent<RectTransform>();
            verRt.anchorMin = new Vector2(0f, 0f);
            verRt.anchorMax = new Vector2(0.3f, 0.05f);
            verRt.offsetMin = new Vector2(20, 10);
            verRt.offsetMax = Vector2.zero;

            // ============================================
            // TUTORIAL PANEL (unchanged layout)
            // ============================================
            var tutorialPanel = CreatePanel(canvasGo.transform, "TutorialPanel", false);
            var tutBg = tutorialPanel.GetComponent<Image>();
            tutBg.color = BACKGROUND_COLOR;

            var tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(tutorialPanel.transform, false);
            var tabBarRt = tabBar.AddComponent<RectTransform>();
            tabBarRt.anchorMin = new Vector2(0.1f, 0.88f);
            tabBarRt.anchorMax = new Vector2(0.9f, 0.95f);
            tabBarRt.offsetMin = Vector2.zero;
            tabBarRt.offsetMax = Vector2.zero;
            var hlg = tabBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var controlsTab = CreateButton(tabBar.transform, "ControlsTab", "CONTROLS", 18);
            var survivalTab = CreateButton(tabBar.transform, "SurvivalTab", "SURVIVAL", 18);
            var cluesTab = CreateButton(tabBar.transform, "CluesTab", "CLUES & ENDINGS", 16);

            var sectionTitle = CreateTextObject(tutorialPanel.transform, "SectionTitle",
                "Controls", 36, GOLDEN_COLOR, TextAnchor.MiddleCenter, FontStyle.Bold);
            var stRt = sectionTitle.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.1f, 0.8f);
            stRt.anchorMax = new Vector2(0.9f, 0.87f);
            stRt.offsetMin = Vector2.zero;
            stRt.offsetMax = Vector2.zero;

            var scrollGo = new GameObject("BodyScroll");
            scrollGo.transform.SetParent(tutorialPanel.transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.1f, 0.15f);
            scrollRt.anchorMax = new Vector2(0.9f, 0.78f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            var scrollImg = scrollGo.AddComponent<Image>();
            scrollImg.color = new Color(0, 0, 0, 0.3f);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollGo.AddComponent<Mask>().showMaskGraphic = true;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 500);
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRt;

            var bodyText = CreateTextObject(contentGo.transform, "BodyText",
                "", 20, BODY_TEXT_COLOR, TextAnchor.UpperLeft, FontStyle.Normal);
            var btRt = bodyText.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = new Vector2(1, 1);
            btRt.offsetMin = new Vector2(20, 10);
            btRt.offsetMax = new Vector2(-20, -10);
            btRt.pivot = new Vector2(0.5f, 1);

            var navBar = new GameObject("NavBar");
            navBar.transform.SetParent(tutorialPanel.transform, false);
            var navBarRt = navBar.AddComponent<RectTransform>();
            navBarRt.anchorMin = new Vector2(0.2f, 0.05f);
            navBarRt.anchorMax = new Vector2(0.8f, 0.12f);
            navBarRt.offsetMin = Vector2.zero;
            navBarRt.offsetMax = Vector2.zero;
            var navHlg = navBar.AddComponent<HorizontalLayoutGroup>();
            navHlg.spacing = 20;
            navHlg.childAlignment = TextAnchor.MiddleCenter;
            navHlg.childForceExpandWidth = true;
            navHlg.childForceExpandHeight = true;

            var prevBtn = CreateButton(navBar.transform, "PrevButton", "< PREVIOUS", 18);
            var pageIndicator = CreateTextObject(navBar.transform, "PageIndicator",
                "1/1", 18, BODY_TEXT_COLOR, TextAnchor.MiddleCenter, FontStyle.Normal);
            var nextBtn = CreateButton(navBar.transform, "NextButton", "NEXT >", 18);

            var backBtn = CreateButton(tutorialPanel.transform, "BackButton", "BACK", 20);
            var backRt = backBtn.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.05f, 0.05f);
            backRt.anchorMax = new Vector2(0.15f, 0.12f);
            backRt.offsetMin = Vector2.zero;
            backRt.offsetMax = Vector2.zero;

            // ============================================
            // ATTACH SCRIPTS & WIRE REFERENCES
            // ============================================
            var mainMenuUI = canvasGo.AddComponent<UI.MainMenuUI>();
            var tutorialUI = tutorialPanel.AddComponent<UI.TutorialUI>();

            var mmSO = new SerializedObject(mainMenuUI);
            mmSO.FindProperty("_menuPanel").objectReferenceValue = menuPanel;
            mmSO.FindProperty("_tutorialPanel").objectReferenceValue = tutorialPanel;
            mmSO.FindProperty("_playButton").objectReferenceValue = playBtn.GetComponent<Button>();
            mmSO.FindProperty("_tutorialButton").objectReferenceValue = tutorialBtn.GetComponent<Button>();
            mmSO.FindProperty("_settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            mmSO.FindProperty("_quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();

            // Wire audio clips
            var bgMusic = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/KarpoSoundtracks/FREE Horror Ambient Music Pack - DESPERATION/WAV/LOOPS/there's someone behind you (Without Jumpscare) [LOOP].wav");
            var clickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Free UI Click Sound Effects Pack/AUDIO/Button/SFX_UI_Button_Organic_Plastic_Thin_Negative_Back_2.wav");

            if (bgMusic != null)
                mmSO.FindProperty("_bgMusic").objectReferenceValue = bgMusic;
            else
                Debug.LogWarning("[SetupMainMenu] Background music clip not found.");

            if (clickSfx != null)
                mmSO.FindProperty("_buttonClickSfx").objectReferenceValue = clickSfx;
            else
                Debug.LogWarning("[SetupMainMenu] Button click SFX not found.");

            mmSO.FindProperty("_musicVolume").floatValue = 0.4f;
            mmSO.FindProperty("_sfxVolume").floatValue = 0.7f;

            mmSO.ApplyModifiedPropertiesWithoutUndo();

            var tutSO = new SerializedObject(tutorialUI);
            tutSO.FindProperty("_mainMenuUI").objectReferenceValue = mainMenuUI;
            tutSO.FindProperty("_sectionTitle").objectReferenceValue = sectionTitle.GetComponent<Text>();
            tutSO.FindProperty("_bodyText").objectReferenceValue = bodyText.GetComponent<Text>();
            tutSO.FindProperty("_pageIndicator").objectReferenceValue = pageIndicator.GetComponent<Text>();
            tutSO.FindProperty("_prevButton").objectReferenceValue = prevBtn.GetComponent<Button>();
            tutSO.FindProperty("_nextButton").objectReferenceValue = nextBtn.GetComponent<Button>();
            tutSO.FindProperty("_backButton").objectReferenceValue = backBtn.GetComponent<Button>();
            tutSO.FindProperty("_controlsTab").objectReferenceValue = controlsTab.GetComponent<Button>();
            tutSO.FindProperty("_survivalTab").objectReferenceValue = survivalTab.GetComponent<Button>();
            tutSO.FindProperty("_cluesTab").objectReferenceValue = cluesTab.GetComponent<Button>();
            tutSO.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/MainMenu/MainMenu.unity";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(
                System.IO.Path.Combine(Application.dataPath, "../", scenePath)));
            EditorSceneManager.SaveScene(scene, scenePath);

            UpdateBuildSettings();

            Debug.Log("[SetupMainMenu] Main Menu scene created. Place your background image at Assets/_Project/Art/UI/MainMenuBG.png and re-run if needed.");
        }

        #region Helpers

        private static GameObject CreatePanel(Transform parent, string name, bool active)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>();
            go.SetActive(active);
            return go;
        }

        /// <summary>
        /// Creates a clean, transparent menu button with left-aligned text.
        /// Looks like the reference screenshot — just text, no box.
        /// </summary>
        private static GameObject CreateMenuButton(Transform parent, string name, string label, int fontSize, FontStyle fontStyle = FontStyle.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 50);

            var img = go.AddComponent<Image>();
            img.color = BUTTON_NORMAL; // Transparent

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = BUTTON_NORMAL;
            colors.highlightedColor = BUTTON_HOVER;
            colors.pressedColor = BUTTON_PRESSED;
            colors.selectedColor = BUTTON_HOVER;
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            btn.targetGraphic = img;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            var textGo = CreateTextObject(go.transform, "Text", label, fontSize,
                BUTTON_TEXT_NORMAL, TextAnchor.MiddleLeft, fontStyle);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5, 0);
            textRt.offsetMax = new Vector2(-5, 0);

            return go;
        }

        /// <summary>
        /// Creates a solid-background button (used for tutorial/tab buttons).
        /// </summary>
        private static GameObject CreateButton(Transform parent, string name, string label, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 55);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            colors.selectedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            btn.targetGraphic = img;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 55;

            var textGo = CreateTextObject(go.transform, "Text", label, fontSize,
                new Color(0.9f, 0.9f, 0.9f, 1f), TextAnchor.MiddleCenter, FontStyle.Normal);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);

            return go;
        }

        private static GameObject CreateTextObject(Transform parent, string name,
            string content, int fontSize, Color color, TextAnchor alignment, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return go;
        }

        private static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenu/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Bunker/Bunker.unity", true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[SetupMainMenu] Build settings: MainMenu=0, Bunker=1");
        }

        #endregion
    }
}
#endif
