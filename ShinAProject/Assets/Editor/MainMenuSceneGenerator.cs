using System.IO;
using ShinA.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShinA.Editor
{
    public static class MainMenuSceneGenerator
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("ShinA/Generate Main Menu Scene")]
        public static void Generate()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            CreateCamera();
            CreateEventSystem();

            GameObject controllerObject = new GameObject("MainMenuController");
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();

            Canvas canvas = CreateCanvas(controllerObject.transform);
            CreateBackdrop(canvas.transform);
            CreateDecorations(canvas.transform);

            Text status = CreateStatus(canvas.transform);
            Button firstButton = CreateMenu(canvas.transform, controller);
            CreateFooter(canvas.transform);

            controller.Initialize(status, firstButton, "WaitingScene");

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Main menu scene generated: {ScenePath}");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.011f, 0.014f, 1f);
            camera.cullingMask = 0;
            cameraObject.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateBackdrop(Transform parent)
        {
            Image background = CreateImage("Background", parent, new Color(0.012f, 0.016f, 0.02f, 1f));
            Stretch(background.rectTransform);

            Image rightField = CreateImage("RightField", parent, new Color(0.12f, 0.018f, 0.02f, 0.18f));
            SetRect(rightField.rectTransform, new Vector2(0.54f, 0f), Vector2.one, Vector2.zero, Vector2.zero);

            Image topShade = CreateImage("TopShade", parent, new Color(0f, 0f, 0f, 0.38f));
            SetRect(topShade.rectTransform, new Vector2(0f, 0.7f), Vector2.one, Vector2.zero, Vector2.zero);

            Image bottomShade = CreateImage("BottomShade", parent, new Color(0f, 0f, 0f, 0.58f));
            SetRect(bottomShade.rectTransform, Vector2.zero, new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero);

            Image divider = CreateImage("VerticalDivider", parent, new Color(0.45f, 0.045f, 0.045f, 0.42f));
            RectTransform dividerRect = divider.rectTransform;
            dividerRect.anchorMin = new Vector2(0f, 0.12f);
            dividerRect.anchorMax = new Vector2(0f, 0.88f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.anchoredPosition = new Vector2(718f, 0f);
            dividerRect.sizeDelta = new Vector2(1f, 0f);
        }

        private static void CreateDecorations(Transform parent)
        {
            Text giantTitle = CreateText("BackgroundTitle", parent, "SHIN A", 148, TextAnchor.MiddleRight,
                new Color(0.36f, 0.37f, 0.38f, 0.085f), FontStyle.Bold);
            SetRect(giantTitle.rectTransform, new Vector2(0.52f, 0.26f), new Vector2(0.94f, 0.7f), Vector2.zero, Vector2.zero);

            Text index = CreateText("BackgroundIndex", parent, "01", 22, TextAnchor.UpperRight,
                new Color(0.62f, 0.08f, 0.08f, 0.55f), FontStyle.Bold);
            SetRect(index.rectTransform, new Vector2(0.88f, 0.82f), new Vector2(0.94f, 0.88f), Vector2.zero, Vector2.zero);
        }

        private static Button CreateMenu(Transform parent, MainMenuController controller)
        {
            GameObject menuObject = new GameObject("Menu", typeof(RectTransform));
            menuObject.transform.SetParent(parent, false);
            RectTransform menuRect = menuObject.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0f, 0.5f);
            menuRect.anchorMax = new Vector2(0f, 0.5f);
            menuRect.pivot = new Vector2(0f, 0.5f);
            menuRect.anchoredPosition = new Vector2(144f, 0f);
            menuRect.sizeDelta = new Vector2(500f, 650f);

            Text eyebrow = CreateText("Eyebrow", menuRect, "MAIN MENU", 18, TextAnchor.MiddleLeft,
                new Color(0.62f, 0.08f, 0.08f, 1f), FontStyle.Bold);
            SetAnchored(eyebrow.rectTransform, 0f, 590f, 500f, 28f);

            Text title = CreateText("Title", menuRect, "SHIN A", 70, TextAnchor.MiddleLeft,
                new Color(0.91f, 0.91f, 0.89f, 1f), FontStyle.Bold);
            SetAnchored(title.rectTransform, 0f, 506f, 500f, 84f);

            Text subtitle = CreateText("Subtitle", menuRect, "어둠이 당신을 기억한다", 18, TextAnchor.MiddleLeft,
                new Color(0.42f, 0.43f, 0.44f, 1f), FontStyle.Normal);
            SetAnchored(subtitle.rectTransform, 2f, 469f, 498f, 32f);

            Image rule = CreateImage("Rule", menuRect, new Color(0.32f, 0.033f, 0.033f, 0.72f));
            SetAnchored(rule.rectTransform, 0f, 436f, 78f, 2f);

            GameObject listObject = new GameObject("MenuItems", typeof(RectTransform));
            listObject.transform.SetParent(menuRect, false);
            RectTransform listRect = listObject.GetComponent<RectTransform>();
            SetAnchored(listRect, 0f, 50f, 500f, 350f);

            VerticalLayoutGroup layout = listObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            Button start = CreateButton(listRect, "01", "게임 시작");
            Button online = CreateButton(listRect, "02", "온라인 모드");
            Button settings = CreateButton(listRect, "03", "설정");
            Button quit = CreateButton(listRect, "04", "게임 종료");

            UnityEventTools.AddPersistentListener(start.onClick, controller.StartGame);
            UnityEventTools.AddPersistentListener(online.onClick, controller.OpenOnlineMode);
            UnityEventTools.AddPersistentListener(settings.onClick, controller.OpenSettings);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.QuitGame);

            Navigation startNav = start.navigation;
            startNav.mode = Navigation.Mode.Explicit;
            startNav.selectOnUp = quit;
            startNav.selectOnDown = online;
            start.navigation = startNav;

            Navigation onlineNav = online.navigation;
            onlineNav.mode = Navigation.Mode.Explicit;
            onlineNav.selectOnUp = start;
            onlineNav.selectOnDown = settings;
            online.navigation = onlineNav;

            Navigation settingsNav = settings.navigation;
            settingsNav.mode = Navigation.Mode.Explicit;
            settingsNav.selectOnUp = online;
            settingsNav.selectOnDown = quit;
            settings.navigation = settingsNav;

            Navigation quitNav = quit.navigation;
            quitNav.mode = Navigation.Mode.Explicit;
            quitNav.selectOnUp = settings;
            quitNav.selectOnDown = start;
            quit.navigation = quitNav;

            return start;
        }

        private static Button CreateButton(Transform parent, string number, string label)
        {
            Image background = CreateImage(label, parent, new Color(0.025f, 0.029f, 0.033f, 0.72f));
            background.raycastTarget = true;
            RectTransform rect = background.rectTransform;
            rect.sizeDelta = new Vector2(0f, 72f);

            LayoutElement layout = background.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 72f;

            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.22f, 1.22f, 1.22f, 1f);
            colors.selectedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            Image accent = CreateImage("Accent", rect, new Color(0.68f, 0.055f, 0.055f, 0f));
            SetRect(accent.rectTransform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, 0f));
            accent.rectTransform.pivot = new Vector2(0f, 0.5f);

            Text numberText = CreateText("Number", rect, number, 15, TextAnchor.MiddleCenter,
                new Color(0.34f, 0.35f, 0.36f, 1f), FontStyle.Bold);
            SetRect(numberText.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(22f, 0f), new Vector2(58f, 0f));

            Text labelText = CreateText("Label", rect, label, 28, TextAnchor.MiddleLeft,
                new Color(0.72f, 0.73f, 0.74f, 1f), FontStyle.Bold);
            SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(84f, 0f), new Vector2(-20f, 0f));

            MainMenuButtonVisual visual = background.gameObject.AddComponent<MainMenuButtonVisual>();
            visual.Initialize(accent, numberText, labelText);
            return button;
        }

        private static Text CreateStatus(Transform parent)
        {
            Text status = CreateText("StatusMessage", parent, string.Empty, 18, TextAnchor.MiddleLeft,
                new Color(0.75f, 0.76f, 0.77f, 1f), FontStyle.Normal);
            status.rectTransform.anchorMin = new Vector2(0f, 0f);
            status.rectTransform.anchorMax = new Vector2(0f, 0f);
            status.rectTransform.pivot = new Vector2(0f, 0f);
            status.rectTransform.anchoredPosition = new Vector2(146f, 50f);
            status.rectTransform.sizeDelta = new Vector2(520f, 36f);
            return status;
        }

        private static void CreateFooter(Transform parent)
        {
            Text footer = CreateText("Footer", parent, "SHIN A  /  PROTOTYPE BUILD", 13, TextAnchor.MiddleRight,
                new Color(0.28f, 0.29f, 0.3f, 1f), FontStyle.Normal);
            footer.rectTransform.anchorMin = new Vector2(1f, 0f);
            footer.rectTransform.anchorMax = new Vector2(1f, 0f);
            footer.rectTransform.pivot = new Vector2(1f, 0f);
            footer.rectTransform.anchoredPosition = new Vector2(-72f, 48f);
            footer.rectTransform.sizeDelta = new Vector2(430f, 28f);
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int size,
            TextAnchor alignment, Color color, FontStyle style)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            Text text = gameObject.AddComponent<Text>();
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetAnchored(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBuildScenes()
        {
            EditorBuildSettingsScene mainMenu = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettingsScene game = new EditorBuildSettingsScene("Assets/Scenes/WaitingScene.unity", true);
            EditorBuildSettings.scenes = new[] { mainMenu, game };
        }
    }
}
