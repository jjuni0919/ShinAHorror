using ShinA.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ShinA.Player
{
    public sealed class PlayerTabletController : MonoBehaviour
    {
        private FirstPersonController controller;
        private Transform tabletRoot;
        private GameObject screenObject;
        private Material bodyMaterial;
        private Vector3 loweredPosition = new(0.48f, -0.65f, 0.75f);
        private Vector3 raisedPosition = new(0f, -0.12f, 0.62f);

        public bool IsOpen { get; private set; }

        public void Initialize(Camera playerCamera, FirstPersonController playerController)
        {
            controller = playerController;
            BuildTablet(playerCamera.transform);
            SetOpen(false, true);
        }

        private void Update()
        {
            if (PlayerInputBindings.WasPressedThisFrame(PlayerAction.Tablet) &&
                (IsOpen || controller == null || controller.CanAct))
            {
                SetOpen(!IsOpen, false);
            }

            if (tabletRoot != null)
            {
                Vector3 target = IsOpen ? raisedPosition : loweredPosition;
                tabletRoot.localPosition = Vector3.Lerp(tabletRoot.localPosition, target,
                    1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            }
        }

        public void SetOpen(bool open, bool immediate = false)
        {
            IsOpen = open;
            controller?.SetGameplayInputBlocked(this, open);

            if (screenObject != null)
            {
                screenObject.SetActive(open);
            }

            if (immediate && tabletRoot != null)
            {
                tabletRoot.localPosition = open ? raisedPosition : loweredPosition;
            }
        }

        private void OnDisable()
        {
            controller?.SetGameplayInputBlocked(this, false);
        }

        private void OnEnable()
        {
            if (IsOpen)
            {
                controller?.SetGameplayInputBlocked(this, true);
            }
        }

        private void BuildTablet(Transform cameraTransform)
        {
            GameObject root = new("Player Tablet");
            tabletRoot = root.transform;
            tabletRoot.SetParent(cameraTransform, false);
            tabletRoot.localRotation = Quaternion.Euler(8f, 0f, 0f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Tablet Body";
            body.transform.SetParent(tabletRoot, false);
            body.transform.localScale = new Vector3(0.72f, 0.44f, 0.035f);
            Destroy(body.GetComponent<Collider>());
            bodyMaterial = CreateMaterial(new Color(0.025f, 0.028f, 0.032f));
            body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;

            GameObject canvasObject = new("Tablet Screen", typeof(RectTransform));
            canvasObject.transform.SetParent(tabletRoot, false);
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.localPosition = new Vector3(0f, 0f, -0.021f);
            canvasRect.localRotation = Quaternion.Euler(0f, 180f, 0f);
            canvasRect.sizeDelta = new Vector2(640f, 380f);
            canvasRect.localScale = Vector3.one * 0.001f;

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cameraTransform.GetComponent<Camera>();
            canvasObject.AddComponent<GraphicRaycaster>();

            Image screen = canvasObject.AddComponent<Image>();
            screen.color = new Color(0.025f, 0.12f, 0.14f, 1f);
            screen.raycastTarget = true;

            GameObject textObject = new("Status", typeof(RectTransform));
            textObject.transform.SetParent(canvasObject.transform, false);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "SHIN A // TABLET ONLINE";
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.55f, 0.92f, 0.88f);
            text.raycastTarget = false;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            screenObject = canvasObject;
            EnsureEventSystem();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { color = color };
        }

        private void OnDestroy()
        {
            if (bodyMaterial != null)
            {
                Destroy(bodyMaterial);
            }
        }
    }
}
