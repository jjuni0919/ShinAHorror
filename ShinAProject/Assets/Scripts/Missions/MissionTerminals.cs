using ShinA.Inventory;
using ShinA.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ShinA.Missions
{
    public sealed class MissionStorageTerminal : MonoBehaviour, IPlayerInteractable
    {
        public string InteractionPrompt => "현장 창고에 보관";

        public void Interact(GameObject player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            int storedCount = MissionSession.Instance.StoreInventory(inventory);
            inventory?.NotifyItemResponse(storedCount > 0
                ? $"아이템 {storedCount}개를 현장 창고에 보관했습니다."
                : "보관할 아이템이 없습니다.");
        }
    }

    public sealed class WaitingWarehouseTerminal : MonoBehaviour, IPlayerInteractable
    {
        public string InteractionPrompt => "대기실 창고 확인";

        public void Interact(GameObject player)
        {
            PlayerProgress progress = MissionSession.Instance.Progress;
            player.GetComponent<PlayerInventory>()?.NotifyItemResponse(
                $"창고 {progress.WarehouseItemNumbers.Count}개  ·  보유 재화 {progress.Currency:N0}");
        }
    }

    public sealed class MissionCompletionTerminal : MonoBehaviour, IPlayerInteractable
    {
        private GameObject confirmationRoot;
        private FirstPersonController controller;
        private CursorLockMode previousLockMode;
        private bool previousCursorVisible;

        public string InteractionPrompt => "임무 완료 처리";

        public void Interact(GameObject player)
        {
            if (!MissionSession.Instance.IsActive || confirmationRoot != null && confirmationRoot.activeSelf)
            {
                return;
            }

            controller = player.GetComponent<FirstPersonController>();
            if (confirmationRoot == null)
            {
                BuildConfirmation();
            }

            previousLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            controller?.SetGameplayInputBlocked(this, true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            confirmationRoot.SetActive(true);
        }

        private void Update()
        {
            if (confirmationRoot != null && confirmationRoot.activeSelf &&
                Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseConfirmation();
            }
        }

        private void CompleteMission()
        {
            CloseConfirmation();
            MissionSession.Instance.CompleteMission();
        }

        private void CloseConfirmation()
        {
            if (confirmationRoot == null || !confirmationRoot.activeSelf)
            {
                return;
            }

            confirmationRoot.SetActive(false);
            controller?.SetGameplayInputBlocked(this, false);
            controller = null;
            Cursor.lockState = previousLockMode;
            Cursor.visible = previousCursorVisible;
        }

        private void BuildConfirmation()
        {
            confirmationRoot = new GameObject("Mission Completion Confirmation", typeof(RectTransform));
            Canvas canvas = confirmationRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            CanvasScaler scaler = confirmationRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            confirmationRoot.AddComponent<GraphicRaycaster>();

            Image dim = confirmationRoot.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.72f);
            RectTransform rootRect = confirmationRoot.GetComponent<RectTransform>();
            Stretch(rootRect);

            GameObject panelObject = new("Panel", typeof(RectTransform));
            panelObject.transform.SetParent(confirmationRoot.transform, false);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(620f, 300f);
            Image panel = panelObject.AddComponent<Image>();
            panel.color = new Color(0.035f, 0.045f, 0.048f, 0.98f);

            Font font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 28);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Text message = CreateText("Message", panelRect, font,
                "임무를 완료하고 본부로 귀환하시겠습니까?\n현장 창고의 아이템만 대기실 창고로 이동합니다.", 25);
            message.rectTransform.anchorMin = new Vector2(0.08f, 0.38f);
            message.rectTransform.anchorMax = new Vector2(0.92f, 0.9f);
            StretchOffsets(message.rectTransform);

            Button confirm = CreateButton("Confirm", panelRect, font, "임무 완료",
                new Vector2(0.12f, 0.1f), new Vector2(0.47f, 0.32f));
            confirm.onClick.AddListener(CompleteMission);
            Button cancel = CreateButton("Cancel", panelRect, font, "계속 탐험",
                new Vector2(0.53f, 0.1f), new Vector2(0.88f, 0.32f));
            cancel.onClick.AddListener(CloseConfirmation);
            confirmationRoot.SetActive(false);

            if (EventSystem.current == null)
            {
                Debug.LogError("임무 확인 창을 사용하려면 EventSystem이 필요합니다.", this);
            }
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            StretchOffsets(rect);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.32f, 0.31f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Label", rect, font, label, 22);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(string name, Transform parent, Font font, string value, int size)
        {
            GameObject textObject = new(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            StretchOffsets(rect);
        }

        private static void StretchOffsets(RectTransform rect)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void OnDisable()
        {
            CloseConfirmation();
        }
    }
}
