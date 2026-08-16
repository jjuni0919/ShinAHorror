using ShinA.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ShinA.UI
{
    public sealed class MainMenuSettingsPanel : MonoBehaviour
    {
        private Slider volumeSlider;
        private Slider sensitivitySlider;
        private Text volumeValue;
        private Text sensitivityValue;
        private Font font;

        public static MainMenuSettingsPanel Create(Canvas canvas)
        {
            GameObject root = new("Settings Panel", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            MainMenuSettingsPanel panel = root.AddComponent<MainMenuSettingsPanel>();
            panel.BuildInterface();
            root.SetActive(false);
            return panel;
        }

        public void Open()
        {
            volumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            sensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivityNormalized);
            RefreshValues();
            gameObject.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(volumeSlider.gameObject);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        private void BuildInterface()
        {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 32);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            RectTransform rootRect = GetComponent<RectTransform>();
            Stretch(rootRect);

            Image dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.82f);

            Image window = CreateImage("Settings Window", transform, new Color(0.035f, 0.041f, 0.047f, 0.98f));
            RectTransform windowRect = window.rectTransform;
            windowRect.anchorMin = windowRect.anchorMax = windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(720f, 500f);

            Text title = CreateText("Title", windowRect, "설정", 38, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetAnchored(title.rectTransform, 56f, 398f, 610f, 56f);

            CreateLabel(windowRect, "게임 볼륨", 320f);
            volumeValue = CreateValue(windowRect, 320f);
            volumeSlider = CreateSlider(windowRect, 260f);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            CreateLabel(windowRect, "마우스 감도", 174f);
            sensitivityValue = CreateValue(windowRect, 174f);
            sensitivitySlider = CreateSlider(windowRect, 114f);
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

            Button close = CreateButton(windowRect, "닫기");
            close.GetComponent<RectTransform>().anchoredPosition = new Vector2(510f, 32f);
            close.onClick.AddListener(Close);
        }

        private void OnVolumeChanged(float value)
        {
            GameSettings.MasterVolume = value;
            RefreshValues();
        }

        private void OnSensitivityChanged(float value)
        {
            GameSettings.MouseSensitivityNormalized = value;
            RefreshValues();
        }

        private void RefreshValues()
        {
            volumeValue.text = $"{Mathf.RoundToInt(volumeSlider.value * 100f)}";
            sensitivityValue.text = $"{Mathf.RoundToInt(sensitivitySlider.value * 100f)}";
        }

        private void CreateLabel(Transform parent, string value, float y)
        {
            Text label = CreateText(value, parent, value, 22, TextAnchor.MiddleLeft,
                new Color(0.82f, 0.83f, 0.84f), FontStyle.Bold);
            SetAnchored(label.rectTransform, 56f, y, 300f, 40f);
        }

        private Text CreateValue(Transform parent, float y)
        {
            Text value = CreateText("Value", parent, "0", 20, TextAnchor.MiddleRight,
                new Color(0.78f, 0.1f, 0.1f), FontStyle.Bold);
            SetAnchored(value.rectTransform, 576f, y, 88f, 40f);
            return value;
        }

        private Slider CreateSlider(Transform parent, float y)
        {
            GameObject sliderObject = new("Slider", typeof(RectTransform));
            sliderObject.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            SetAnchored(sliderRect, 56f, y, 608f, 32f);

            Image background = CreateImage("Background", sliderRect, new Color(0.12f, 0.13f, 0.14f, 1f));
            Stretch(background.rectTransform);

            Image fill = CreateImage("Fill", sliderRect, new Color(0.68f, 0.055f, 0.055f, 1f));
            Stretch(fill.rectTransform);

            Image handle = CreateImage("Handle", sliderRect, new Color(0.93f, 0.91f, 0.88f, 1f));
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.sizeDelta = new Vector2(18f, 42f);

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private Button CreateButton(Transform parent, string label)
        {
            Image background = CreateImage(label, parent, new Color(0.42f, 0.045f, 0.045f, 1f));
            RectTransform rect = background.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(154f, 54f);

            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            Text text = CreateText("Label", rect, label, 20, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            Stretch(text.rectTransform);
            return button;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string name, Transform parent, string value, int size,
            TextAnchor alignment, Color color, FontStyle style)
        {
            GameObject textObject = new(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.fontStyle = style;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchored(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
