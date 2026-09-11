using System;
using System.Collections.Generic;
using ShinA.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShinA.Inventory
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 6;

        private readonly List<ItemDefinition> items = new();
        private Camera viewCamera;
        private PlayerAppearance appearance;
        private int selectedIndex;
        private float nextUseTime;

        public event Action InventoryChanged;
        public event Action<string> ItemResponse;

        public IReadOnlyList<ItemDefinition> Items => items;
        public int Capacity => capacity;
        public int SelectedIndex => selectedIndex;
        public bool IsFull => items.Count >= capacity;
        public ItemDefinition SelectedItem => selectedIndex >= 0 && selectedIndex < items.Count
            ? items[selectedIndex]
            : null;

        public void Initialize(Camera playerCamera, PlayerAppearance playerAppearance, int slotCount = 6)
        {
            viewCamera = playerCamera;
            appearance = playerAppearance;
            capacity = Mathf.Max(1, slotCount);
            selectedIndex = 0;
        }

        public bool TryAdd(ItemDefinition item)
        {
            if (item == null || IsFull)
            {
                return false;
            }

            items.Add(item);
            if (items.Count == 1)
            {
                selectedIndex = 0;
                EquipSelected();
            }

            InventoryChanged?.Invoke();
            return true;
        }

        public void SetCapacity(int slotCount)
        {
            capacity = Mathf.Max(items.Count, Mathf.Max(1, slotCount));
            InventoryChanged?.Invoke();
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= capacity || selectedIndex == index)
            {
                return;
            }

            selectedIndex = index;
            EquipSelected();
            InventoryChanged?.Invoke();
        }

        public bool UseSelected()
        {
            ItemDefinition item = SelectedItem;
            if (item == null || Time.time < nextUseTime)
            {
                return false;
            }

            ItemUseContext context = new(gameObject, viewCamera, this);
            bool used = item.Use(context);
            nextUseTime = item is WeaponItemDefinition weapon
                ? Time.time + weapon.UseCooldown
                : Time.time + 0.2f;
            return used;
        }

        public void NotifyItemResponse(string message)
        {
            ItemResponse?.Invoke(message);
        }

        private void Update()
        {
            HandleSlotSelection();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                Cursor.lockState == CursorLockMode.Locked)
            {
                UseSelected();
            }
        }

        private void HandleSlotSelection()
        {
            if (Keyboard.current != null)
            {
                Key[] numberKeys =
                {
                    Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
                    Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
                };

                int keyCount = Mathf.Min(capacity, numberKeys.Length);
                for (int i = 0; i < keyCount; i++)
                {
                    if (Keyboard.current[numberKeys[i]].wasPressedThisFrame)
                    {
                        SelectSlot(i);
                        break;
                    }
                }
            }

            if (Mouse.current == null)
            {
                return;
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                int direction = scroll > 0f ? -1 : 1;
                SelectSlot((selectedIndex + direction + capacity) % capacity);
            }
        }

        private void EquipSelected()
        {
            appearance?.EquipItem(SelectedItem);
        }
    }
}
