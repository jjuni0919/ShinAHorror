using ShinA.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ShinA.DebugUI
{
    public abstract class DebugPanelBase : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.F1;
        private GameObject panelRoot;
        private CursorLockMode previousLockMode;
        private bool previousCursorVisible;
        private FirstPersonController controlledPlayer;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
        protected Font Font { get; private set; }
        protected Key ToggleKey => toggleKey;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BuildPanel();
            panelRoot.SetActive(false);
        }

        protected virtual void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || toggleKey == Key.None || !System.Enum.IsDefined(typeof(Key), toggleKey))
            {
                return;
            }

            bool shift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            bool control = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            if (shift && control && keyboard[toggleKey].wasPressedThisFrame)
            {
                SetOpen(!IsOpen);
            }
        }

        public void SetToggleKey(Key key)
        {
            toggleKey = System.Enum.IsDefined(typeof(Key), key) ? key : Key.None;
        }

        public void SetOpen(bool open)
        {
            if (panelRoot == null || panelRoot.activeSelf == open)
            {
                return;
            }

            if (open)
            {
                EnsureEventSystem();
                previousLockMode = Cursor.lockState;
                previousCursorVisible = Cursor.visible;
                controlledPlayer = FindFirstObjectByType<FirstPersonController>();
                if (controlledPlayer != null)
                {
                    controlledPlayer.SetGameplayInputBlocked(this, true);
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (controlledPlayer != null)
                {
                    controlledPlayer.SetGameplayInputBlocked(this, false);
                    controlledPlayer = null;
                }

                Cursor.lockState = previousLockMode;
                Cursor.visible = previousCursorVisible;
            }

            panelRoot.SetActive(open);
        }

        protected virtual void OnDestroy()
        {
            if (controlledPlayer != null)
            {
                controlledPlayer.SetGameplayInputBlocked(this, false);
            }
        }

        protected virtual void OnDisable()
        {
            if (IsOpen)
            {
                SetOpen(false);
            }
        }

        protected abstract void BuildContent(Transform contentRoot);

        private void BuildPanel()
        {
            Font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 24);
            if (Font == null)
            {
                Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("Debug Panel", typeof(RectTransform));
            panelRoot.transform.SetParent(transform, false);
            RectTransform rect = panelRoot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image dim = panelRoot.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.76f);

            GameObject content = new("Content", typeof(RectTransform));
            content.transform.SetParent(panelRoot.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = contentRect.anchorMax = contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(620f, 520f);
            BuildContent(content.transform);

            EnsureEventSystem();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new("EventSystem");
            DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }
}
