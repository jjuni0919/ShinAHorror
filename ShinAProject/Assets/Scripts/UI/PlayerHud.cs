using System.Collections.Generic;
using ShinA.Inventory;
using ShinA.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ShinA.UI
{
    public sealed class PlayerHud : MonoBehaviour
    {
        private sealed class SlotView
        {
            public Image Background;
            public Image Icon;
            public Text ItemNumber;
        }

        private readonly List<SlotView> slotViews = new();
        private FirstPersonController player;
        private PlayerInventory inventory;
        private RectTransform staminaFill;
        private RectTransform inventoryRoot;
        private Text responseText;
        private Font font;
        private float responseHideTime;

        public static PlayerHud Create(FirstPersonController targetPlayer, PlayerInventory targetInventory)
        {
            GameObject root = new("Player HUD");
            PlayerHud hud = root.AddComponent<PlayerHud>();
            hud.player = targetPlayer;
            hud.inventory = targetInventory;
            hud.BuildInterface();
            hud.Subscribe();
            hud.RefreshInventory();
            return hud;
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.InventoryChanged -= RefreshInventory;
                inventory.ItemResponse -= ShowResponse;
            }
        }

        private void Update()
        {
            if (player != null && staminaFill != null)
            {
                Vector2 anchorMax = staminaFill.anchorMax;
                anchorMax.x = player.StaminaNormalized;
                staminaFill.anchorMax = anchorMax;
            }

            if (responseText != null && responseText.gameObject.activeSelf && Time.unscaledTime >= responseHideTime)
            {
                responseText.gameObject.SetActive(false);
            }
        }

        private void Subscribe()
        {
            inventory.InventoryChanged += RefreshInventory;
            inventory.ItemResponse += ShowResponse;
        }

        private void BuildInterface()
        {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 24);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            CreateCrosshair(transform);
            CreateStaminaBar(transform);
            CreateInventory(transform);

            responseText = CreateText("Item Message", transform, string.Empty, 20, TextAnchor.MiddleCenter,
                new Color(0.92f, 0.92f, 0.9f, 1f), FontStyle.Bold);
            RectTransform responseRect = responseText.rectTransform;
            responseRect.anchorMin = responseRect.anchorMax = responseRect.pivot = new Vector2(0.5f, 0f);
            responseRect.anchoredPosition = new Vector2(0f, 152f);
            responseRect.sizeDelta = new Vector2(760f, 42f);
            responseText.gameObject.SetActive(false);
        }

        private static void CreateCrosshair(Transform parent)
        {
            Image dot = CreateImage("Aim Point", parent, new Color(0.94f, 0.94f, 0.92f, 0.92f));
            RectTransform rect = dot.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(5f, 5f);
        }

        private void CreateInventory(Transform parent)
        {
            GameObject inventoryObject = new("Inventory Slots", typeof(RectTransform));
            inventoryObject.transform.SetParent(parent, false);
            inventoryRoot = inventoryObject.GetComponent<RectTransform>();
            inventoryRoot.anchorMin = inventoryRoot.anchorMax = inventoryRoot.pivot = new Vector2(0.5f, 0f);
            inventoryRoot.anchoredPosition = new Vector2(0f, 42f);

            HorizontalLayoutGroup layout = inventoryObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void RebuildSlots()
        {
            foreach (Transform child in inventoryRoot)
            {
                Destroy(child.gameObject);
            }

            slotViews.Clear();
            const float slotSize = 72f;
            const float spacing = 8f;
            inventoryRoot.sizeDelta = new Vector2(inventory.Capacity * slotSize +
                                                   Mathf.Max(0, inventory.Capacity - 1) * spacing, slotSize);

            for (int i = 0; i < inventory.Capacity; i++)
            {
                Image background = CreateImage($"Slot {i + 1}", inventoryRoot,
                    new Color(0.025f, 0.03f, 0.035f, 0.88f));
                background.rectTransform.sizeDelta = new Vector2(slotSize, slotSize);

                Outline outline = background.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(2f, -2f);
                outline.effectColor = new Color(0.24f, 0.25f, 0.27f, 1f);

                Text keyLabel = CreateText("Key", background.transform, i < 9 ? (i + 1).ToString() : string.Empty,
                    13, TextAnchor.UpperLeft, new Color(0.55f, 0.56f, 0.58f), FontStyle.Bold);
                Stretch(keyLabel.rectTransform);
                keyLabel.rectTransform.offsetMin = new Vector2(6f, 4f);
                keyLabel.rectTransform.offsetMax = new Vector2(-4f, -4f);

                Image icon = CreateImage("Icon", background.transform, Color.clear);
                icon.preserveAspect = true;
                Stretch(icon.rectTransform);
                icon.rectTransform.offsetMin = new Vector2(14f, 14f);
                icon.rectTransform.offsetMax = new Vector2(-14f, -14f);

                Text itemNumber = CreateText("Item Number", background.transform, string.Empty, 12,
                    TextAnchor.LowerRight, Color.white, FontStyle.Bold);
                Stretch(itemNumber.rectTransform);
                itemNumber.rectTransform.offsetMin = new Vector2(4f, 4f);
                itemNumber.rectTransform.offsetMax = new Vector2(-6f, -4f);

                slotViews.Add(new SlotView { Background = background, Icon = icon, ItemNumber = itemNumber });
            }
        }

        private void RefreshInventory()
        {
            if (slotViews.Count != inventory.Capacity)
            {
                RebuildSlots();
            }

            for (int i = 0; i < slotViews.Count; i++)
            {
                SlotView slot = slotViews[i];
                bool selected = i == inventory.SelectedIndex;
                slot.Background.color = selected
                    ? new Color(0.28f, 0.04f, 0.045f, 0.96f)
                    : new Color(0.025f, 0.03f, 0.035f, 0.88f);

                ItemDefinition item = i < inventory.Items.Count ? inventory.Items[i] : null;
                slot.Icon.gameObject.SetActive(item != null);
                slot.ItemNumber.text = item != null ? $"#{item.ItemNumber:00}" : string.Empty;

                if (item != null)
                {
                    slot.Icon.sprite = item.Icon;
                    slot.Icon.color = item.Icon != null ? Color.white : item.IconColor;
                }
            }
        }

        private void ShowResponse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            responseText.text = message;
            responseText.gameObject.SetActive(true);
            responseHideTime = Time.unscaledTime + 2.2f;
        }

        private void CreateStaminaBar(Transform parent)
        {
            GameObject group = new("Stamina", typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform groupRect = group.GetComponent<RectTransform>();
            groupRect.anchorMin = groupRect.anchorMax = groupRect.pivot = Vector2.zero;
            groupRect.anchoredPosition = new Vector2(64f, 58f);
            groupRect.sizeDelta = new Vector2(280f, 30f);

            Image background = CreateImage("Background", group.transform, new Color(0.015f, 0.018f, 0.021f, 0.82f));
            Stretch(background.rectTransform);

            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(background.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            Stretch(fillAreaRect);
            fillAreaRect.offsetMin = new Vector2(4f, 4f);
            fillAreaRect.offsetMax = new Vector2(-4f, -4f);

            Image fill = CreateImage("Fill", fillArea.transform, new Color(0.68f, 0.055f, 0.055f, 0.96f));
            staminaFill = fill.rectTransform;
            Stretch(staminaFill);
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

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
