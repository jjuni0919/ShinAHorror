using ShinA.Inventory;
using ShinA.UI;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Player
{
    public sealed class PlayerRuntimeSetup : MonoBehaviour
    {
        [SerializeField, Min(1)] private int inventorySlotCount = 6;
        [SerializeField] private PlayerSkinDefinition initialSkin;
        [SerializeField, Range(0f, 0.4f)] private float cameraForwardOffset = 0.18f;

        private bool initialized;

        public void Configure(PlayerSkinDefinition skin, int slotCount, float forwardOffset)
        {
            initialSkin = skin;
            inventorySlotCount = Mathf.Max(1, slotCount);
            cameraForwardOffset = Mathf.Clamp(forwardOffset, 0f, 0.4f);
        }

        private void Start()
        {
            InitializeLocalPlayer();
        }

        public void InitializeLocalPlayer()
        {
            if (initialized)
            {
                return;
            }

            Camera playerCamera = GetComponentInChildren<Camera>(true);
            FirstPersonController controller = GetComponent<FirstPersonController>();
            PlayerAppearance appearance = GetComponent<PlayerAppearance>();
            PlayerHealth health = GetComponent<PlayerHealth>();
            PlayerInventory inventory = GetComponent<PlayerInventory>();
            PlayerItemInteractor interactor = GetComponent<PlayerItemInteractor>();
            PlayerWorldInteractor worldInteractor = GetComponent<PlayerWorldInteractor>();
            if (worldInteractor == null)
            {
                worldInteractor = gameObject.AddComponent<PlayerWorldInteractor>();
            }
            PlayerTabletController tablet = GetComponent<PlayerTabletController>();

            if (playerCamera == null || controller == null || appearance == null || health == null ||
                inventory == null || interactor == null || tablet == null)
            {
                Debug.LogError("플레이어 프리팹에 필수 컴포넌트가 하나 이상 없습니다.", this);
                return;
            }

            initialized = true;
            Vector3 cameraPosition = playerCamera.transform.localPosition;
            cameraPosition.z = cameraForwardOffset;
            playerCamera.transform.localPosition = cameraPosition;

            controller.Initialize(playerCamera);
            appearance.Initialize(playerCamera.transform, controller, initialSkin, true);
            inventory.Initialize(playerCamera, appearance, inventorySlotCount);
            SaveData data = SaveManager.Instance.CurrentData;
            if (data != null)
            {
                foreach (int number in data.inventoryItemNumbers)
                {
                    ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{number:000}");
                    if (item != null && !inventory.TryAdd(item))
                    {
                        ItemPickup.Spawn(item, transform.position + Vector3.up, Quaternion.identity);
                    }
                }
                inventory.SelectSlot(Mathf.Clamp(data.player.selectedInventorySlot, 0, inventory.Capacity - 1));
            }
            interactor.Initialize(playerCamera, inventory);
            worldInteractor.Initialize(playerCamera, controller);
            tablet.Initialize(playerCamera, controller);
            PlayerHud.Create(controller, health, inventory, interactor, worldInteractor);
        }
    }
}
