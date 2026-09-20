using System;
using System.Collections.Generic;
using ShinA.Player;
using ShinA.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShinA.Inventory
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        private static readonly Key[] NumberKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };

        [SerializeField, Min(1)] private int capacity = 6;

        private readonly List<ItemDefinition> items = new();
        private Camera viewCamera;
        private PlayerAppearance appearance;
        private FirstPersonController controller;
        private int selectedIndex;
        private float nextUseTime;
        private bool cursorWasLocked;

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
            controller = GetComponent<FirstPersonController>();
            capacity = Mathf.Max(1, slotCount);
            selectedIndex = 0;
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
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

        public List<ItemDefinition> TakeAllItems()
        {
            List<ItemDefinition> removedItems = new(items);
            items.Clear();
            selectedIndex = 0;
            EquipSelected();
            InventoryChanged?.Invoke();
            return removedItems;
        }

        public ItemDefinition TakeSelected()
        {
            ItemDefinition item = SelectedItem;
            if (item == null) return null;
            items.RemoveAt(selectedIndex);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, items.Count - 1));
            EquipSelected();
            InventoryChanged?.Invoke();
            return item;
        }

        public bool DropSelected()
        {
            if (SelectedItem == null || viewCamera == null || (controller != null && !controller.CanAct)) return false;
            Vector3 origin = viewCamera.transform.position;
            Vector3 direction = viewCamera.transform.forward;
            if (Physics.SphereCast(origin, 0.25f, direction, out _, 0.85f, ~0, QueryTriggerInteraction.Ignore))
            {
                NotifyItemResponse("아이템을 내려놓을 공간이 없습니다.");
                return false;
            }
            ItemPickup pickup = ItemPickup.Spawn(SelectedItem, origin + direction * 0.85f, Quaternion.identity);
            if (pickup == null) return false;
            TakeSelected();
            pickup.GetComponent<Rigidbody>().linearVelocity = direction;
            return true;
        }

        public void SetCapacity(int slotCount)
        {
            capacity = Mathf.Max(items.Count, Mathf.Max(1, slotCount));
            int clampedIndex = Mathf.Clamp(selectedIndex, 0, capacity - 1);
            if (selectedIndex != clampedIndex)
            {
                selectedIndex = clampedIndex;
                EquipSelected();
            }

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
            if (item == null || Time.time < nextUseTime || (controller != null && !controller.CanAct))
            {
                return false;
            }

            ItemUseContext context = new(gameObject, viewCamera, this);
            if (!item.Use(context))
            {
                return false;
            }

            nextUseTime = item is WeaponItemDefinition weapon
                ? Time.time + weapon.UseCooldown
                : Time.time + 0.2f;
            return true;
        }

        public void NotifyItemResponse(string message)
        {
            ItemResponse?.Invoke(message);
        }

        public void PlayWeaponAttack(bool ranged)
        {
            appearance?.PlayWeaponAttack(ranged);
        }

        private void OnEnable()
        {
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
        }

        private void Update()
        {
            if (controller != null && !controller.CanAct)
            {
                return;
            }

            HandleSlotSelection();

            if (PlayerInputBindings.WasPressedThisFrame(PlayerAction.DropItem))
            {
                DropSelected();
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                cursorWasLocked && Cursor.lockState == CursorLockMode.Locked)
            {
                UseSelected();
            }
        }

        private void LateUpdate()
        {
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
        }

        private void HandleSlotSelection()
        {
            if (Keyboard.current != null)
            {
                int keyCount = Mathf.Min(capacity, NumberKeys.Length);
                for (int i = 0; i < keyCount; i++)
                {
                    if (Keyboard.current[NumberKeys[i]].wasPressedThisFrame)
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
