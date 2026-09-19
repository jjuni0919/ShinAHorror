using ShinA.Maps;
using UnityEngine;
using UnityEngine.UI;

namespace ShinA.DebugUI
{
    public sealed class MapDebugPanel : DebugPanelBase
    {
        private static MapDebugPanel instance;

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
            Image background = contentRoot.gameObject.AddComponent<Image>();
            background.color = new Color(0.035f, 0.04f, 0.045f, 0.98f);

            VerticalLayoutGroup layout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(42, 42, 38, 38);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText(contentRoot, $"MAP DEBUG  //  SHIFT + CTRL + {ToggleKey}", 26,
                FontStyle.Bold);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 58f;

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
            if (MapDatabase.Instance.TravelToMap(mapId))
            {
                SetOpen(false);
                return;
            }

            Debug.LogWarning($"Failed to load map '{mapId}'.", this);
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
