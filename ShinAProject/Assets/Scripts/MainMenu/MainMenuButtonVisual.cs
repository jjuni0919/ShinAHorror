using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShinA.UI
{
    public sealed class MainMenuButtonVisual : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image accent;
        [SerializeField] private Text numberLabel;
        [SerializeField] private Text menuLabel;

        private bool isHovered;
        private bool isSelected;

        public void Initialize(Image accentImage, Text number, Text label)
        {
            accent = accentImage;
            numberLabel = number;
            menuLabel = label;
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            EventSystem.current?.SetSelectedGameObject(gameObject);
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            Refresh();
        }

        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;
            Refresh();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
            Refresh();
        }

        private void Refresh()
        {
            bool highlighted = isHovered || isSelected;

            if (accent != null)
            {
                accent.color = highlighted
                    ? new Color(0.68f, 0.055f, 0.055f, 1f)
                    : new Color(0.68f, 0.055f, 0.055f, 0f);
            }

            if (menuLabel != null)
            {
                menuLabel.color = highlighted
                    ? new Color(1f, 0.96f, 0.91f, 1f)
                    : new Color(0.72f, 0.73f, 0.74f, 1f);
            }

            if (numberLabel != null)
            {
                numberLabel.color = highlighted
                    ? new Color(0.82f, 0.13f, 0.12f, 1f)
                    : new Color(0.34f, 0.35f, 0.36f, 1f);
            }
        }
    }
}
