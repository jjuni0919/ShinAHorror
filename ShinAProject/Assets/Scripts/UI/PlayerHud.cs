using ShinA.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ShinA.UI
{
    public sealed class PlayerHud : MonoBehaviour
    {
        private FirstPersonController player;
        private RectTransform staminaFill;

        public static PlayerHud Create(FirstPersonController targetPlayer)
        {
            GameObject root = new("Player HUD");
            PlayerHud hud = root.AddComponent<PlayerHud>();
            hud.player = targetPlayer;
            hud.BuildInterface();
            return hud;
        }

        private void Update()
        {
            if (player == null || staminaFill == null)
            {
                return;
            }

            Vector2 anchorMax = staminaFill.anchorMax;
            anchorMax.x = player.StaminaNormalized;
            staminaFill.anchorMax = anchorMax;
        }

        private void BuildInterface()
        {
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
        }

        private static void CreateCrosshair(Transform parent)
        {
            Image dot = CreateImage("Aim Point", parent, new Color(0.94f, 0.94f, 0.92f, 0.92f));
            RectTransform rect = dot.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(5f, 5f);
        }

        private void CreateStaminaBar(Transform parent)
        {
            GameObject group = new("Stamina", typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform groupRect = group.GetComponent<RectTransform>();
            groupRect.anchorMin = groupRect.anchorMax = groupRect.pivot = new Vector2(0f, 0f);
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
            staminaFill.anchorMin = Vector2.zero;
            staminaFill.anchorMax = Vector2.one;
            staminaFill.offsetMin = Vector2.zero;
            staminaFill.offsetMax = Vector2.zero;
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
