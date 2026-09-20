using ShinA.Maps;
using UnityEngine;
using UnityEngine.UI;

namespace ShinA.DebugUI
{
    public sealed class MapDebugPanel : DebugPanelBase
    {
        private static MapDebugPanel instance;
        private InputField seedInput;
        private Text seedLabel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnStartup()
        {
            if (instance == null)
            {
                instance = new GameObject("Map Debug UI").AddComponent<MapDebugPanel>();
            }
        }

        protected override void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            base.Awake();
        }

        protected override void BuildContent(Transform contentRoot)
        {
            ((RectTransform)contentRoot).sizeDelta = new Vector2(620f, 760f);
            Image background = contentRoot.gameObject.AddComponent<Image>();
            background.color = new Color(0.035f, 0.04f, 0.045f, 0.98f);

            VerticalLayoutGroup layout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(42, 42, 38, 38);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText(contentRoot, $"맵 디버그  //  SHIFT + CTRL + {ToggleKey}", 26,
                FontStyle.Bold);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 58f;

            seedLabel = CreateText(contentRoot, "생성 시드 · 비우면 무작위", 20, FontStyle.Normal);
            seedLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
            GameObject inputObject = new("Generation Seed", typeof(RectTransform));
            inputObject.transform.SetParent(contentRoot, false);
            inputObject.AddComponent<LayoutElement>().preferredHeight = 44f;
            Image inputBackground = inputObject.AddComponent<Image>();
            inputBackground.color = new Color(0.12f, 0.14f, 0.15f, 1f);
            seedInput = inputObject.AddComponent<InputField>();
            seedInput.targetGraphic = inputBackground;
            Text inputText = CreateText(inputObject.transform, string.Empty, 22, FontStyle.Normal);
            inputText.rectTransform.anchorMin = Vector2.zero;
            inputText.rectTransform.anchorMax = Vector2.one;
            inputText.rectTransform.offsetMin = new Vector2(12f, 0f);
            inputText.rectTransform.offsetMax = new Vector2(-12f, 0f);
            seedInput.textComponent = inputText;
            seedInput.contentType = InputField.ContentType.IntegerNumber;
            seedInput.characterLimit = 11;
            seedInput.text = "12345";

            foreach (MapRecord map in MapDatabase.Instance.Maps)
            {
                MapRecord capturedMap = map;
                Button button = CreateButton(contentRoot, $"{map.displayName}  [{map.sceneName}]");
                button.onClick.AddListener(() => Travel(capturedMap.mapId));
            }

            Button close = CreateButton(contentRoot, "닫기");
            close.onClick.AddListener(() => SetOpen(false));
        }

        private void Travel(string mapId)
        {
            int? seed = null;
            if (!string.IsNullOrWhiteSpace(seedInput.text))
            {
                if (!int.TryParse(seedInput.text, out int parsedSeed))
                {
                    seedLabel.text = "시드는 -2147483648 ~ 2147483647 범위의 정수입니다.";
                    return;
                }

                seed = parsedSeed;
            }

            if (MapDatabase.Instance.TravelToMap(mapId, seed))
            {
                seedInput.text = MapDatabase.Instance.GenerationSeed.ToString();
                seedLabel.text = "생성 시드 · 비우면 무작위";
                SetOpen(false);
                return;
            }

            Debug.LogWarning($"맵을 불러오지 못했습니다: '{mapId}'", this);
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject buttonObject = new(label, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.045f, 0.05f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            buttonObject.AddComponent<LayoutElement>().preferredHeight = 64f;

            Text text = CreateText(buttonObject.transform, label, 20, FontStyle.Bold);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        private Text CreateText(Transform parent, string value, int size, FontStyle style)
        {
            GameObject textObject = new("Label", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = Font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
    }
}
