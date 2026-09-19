using ShinA.Missions;
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
        private Text missionText;
        private Text timerText;
        private Text progressText;
        private Text instructionText;
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

            if (screenObject != null && screenObject.activeSelf)
            {
                RefreshScreen();
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

            Font font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 32);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Text title = CreateText(canvasObject.transform, font, "SHIN A // FIELD TABLET", 25,
                new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f), TextAnchor.MiddleLeft);
            title.color = new Color(0.55f, 0.92f, 0.88f);
            missionText = CreateText(canvasObject.transform, font, string.Empty, 29,
                new Vector2(0.07f, 0.63f), new Vector2(0.93f, 0.82f), TextAnchor.MiddleLeft);
            timerText = CreateText(canvasObject.transform, font, string.Empty, 54,
                new Vector2(0.07f, 0.35f), new Vector2(0.93f, 0.64f), TextAnchor.MiddleCenter);
            progressText = CreateText(canvasObject.transform, font, string.Empty, 23,
                new Vector2(0.07f, 0.16f), new Vector2(0.93f, 0.36f), TextAnchor.MiddleLeft);
            instructionText = CreateText(canvasObject.transform, font, string.Empty, 17,
                new Vector2(0.07f, 0.03f), new Vector2(0.93f, 0.16f), TextAnchor.MiddleLeft);

            screenObject = canvasObject;
            RefreshScreen();
            EnsureEventSystem();
        }

        private void RefreshScreen()
        {
            MissionSession session = MissionSession.Instance;
            PlayerProgress progress = session.Progress;
            if (session.IsActive)
            {
                int seconds = Mathf.CeilToInt(session.RemainingTime);
                missionText.text = $"DAY {progress.Day}  ·  {session.ActiveMapName}";
                timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
                timerText.color = seconds <= 60
                    ? new Color(1f, 0.28f, 0.2f)
                    : new Color(0.65f, 1f, 0.88f);
                progressText.text = $"현장 창고  {session.FieldStorageCount}개  ·  임무 수익 +{session.PendingCurrency:N0}\n" +
                                    $"보유 재화  {progress.Currency:N0}";
                instructionText.text = "제한 시간 안에 본부 단말기에서 임무 완료 처리를 진행하십시오.";
                return;
            }

            missionText.text = $"DAY {progress.Day}  ·  본부 대기 중";
            timerText.text = "--:--";
            timerText.color = new Color(0.55f, 0.92f, 0.88f);
            progressText.text = $"대기실 창고  {progress.WarehouseItemNumbers.Count}개\n보유 재화  {progress.Currency:N0}";
            instructionText.text = session.LastOutcome switch
            {
                MissionOutcome.Completed => "이전 임무 완료 · 보관 물품 이송 완료",
                MissionOutcome.PlayerDied => "이전 임무 실패 · 플레이어 사망",
                MissionOutcome.TimeExpired => "이전 임무 실패 · 제한 시간 초과",
                _ => "임무를 선택해 탐사를 시작하십시오."
            };
        }

        private static Text CreateText(Transform parent, Font font, string value, int size,
            Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
        {
            GameObject textObject = new("Tablet Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(0.78f, 0.94f, 0.9f);
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
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
