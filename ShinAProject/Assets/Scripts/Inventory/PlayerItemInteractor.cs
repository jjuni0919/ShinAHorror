using ShinA.Settings;
using UnityEngine;

namespace ShinA.Inventory
{
    public sealed class PlayerItemInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float pickupDistance = 3f;

        private Camera viewCamera;
        private PlayerInventory inventory;

        public void Initialize(Camera playerCamera, PlayerInventory playerInventory)
        {
            viewCamera = playerCamera;
            inventory = playerInventory;
        }

        private void Update()
        {
            if (!PlayerInputBindings.WasPressedThisFrame(PlayerAction.Interact) ||
                viewCamera == null || inventory == null)
            {
                return;
            }

            Ray ray = new(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance, ~0, QueryTriggerInteraction.Collide))
            {
                return;
            }

            ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>();
            if (pickup == null)
            {
                return;
            }

            if (!pickup.TryCollect(inventory) && inventory.IsFull)
            {
                inventory.NotifyItemResponse("인벤토리에 빈 공간이 없습니다.");
            }
        }
    }
}
