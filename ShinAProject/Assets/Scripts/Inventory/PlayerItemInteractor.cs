using ShinA.Player;
using ShinA.Settings;
using UnityEngine;

namespace ShinA.Inventory
{
    public sealed class PlayerItemInteractor : MonoBehaviour
    {
        private const float RayAdvanceDistance = 0.01f;

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
            float remainingDistance = pickupDistance;

            while (remainingDistance > 0f &&
                   Physics.Raycast(ray, out RaycastHit hit, remainingDistance, ~0, QueryTriggerInteraction.Collide))
            {
                ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>();
                if (pickup != null)
                {
                    FocusedPickup = pickup;
                    return;
                }

                if (!hit.collider.isTrigger)
                {
                    return;
                }

                float advance = hit.distance + RayAdvanceDistance;
                remainingDistance -= advance;
                ray.origin = ray.GetPoint(advance);
            }
        }
    }
}
