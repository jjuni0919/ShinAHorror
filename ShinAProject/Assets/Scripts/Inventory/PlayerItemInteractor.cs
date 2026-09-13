using ShinA.Player;
using ShinA.Settings;
using UnityEngine;

namespace ShinA.Inventory
{
    public sealed class PlayerItemInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float pickupDistance = 3f;

        private Camera viewCamera;
        private PlayerInventory inventory;
        private FirstPersonController controller;

        public ItemPickup FocusedPickup { get; private set; }

        public void Initialize(Camera playerCamera, PlayerInventory playerInventory)
        {
            viewCamera = playerCamera;
            inventory = playerInventory;
            controller = GetComponent<FirstPersonController>();
        }

        private void Update()
        {
            UpdateFocusedPickup();

            if (!PlayerInputBindings.WasPressedThisFrame(PlayerAction.Interact) ||
                inventory == null || FocusedPickup == null ||
                (controller != null && !controller.CanAct))
            {
                return;
            }

            if (!FocusedPickup.TryCollect(inventory) && inventory.IsFull)
            {
                inventory.NotifyItemResponse("인벤토리에 빈 공간이 없습니다.");
            }
        }

        private void UpdateFocusedPickup()
        {
            FocusedPickup = null;
            if (viewCamera == null || (controller != null && !controller.CanAct))
            {
                return;
            }

            Ray ray = new(viewCamera.transform.position, viewCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, ~0, QueryTriggerInteraction.Collide))
            {
                FocusedPickup = hit.collider.GetComponentInParent<ItemPickup>();
            }
        }
    }
}
