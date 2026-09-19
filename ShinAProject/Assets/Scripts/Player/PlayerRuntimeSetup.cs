using ShinA.Inventory;
using ShinA.UI;
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
                Debug.LogError("Player prefab is missing one or more required components.", this);
                return;
            }

            initialized = true;
            Vector3 cameraPosition = playerCamera.transform.localPosition;
            cameraPosition.z = cameraForwardOffset;
            playerCamera.transform.localPosition = cameraPosition;

            controller.Initialize(playerCamera);
            appearance.Initialize(playerCamera.transform, controller, initialSkin, true);
            inventory.Initialize(playerCamera, appearance, inventorySlotCount);
            interactor.Initialize(playerCamera, inventory);
            worldInteractor.Initialize(playerCamera, controller);
            tablet.Initialize(playerCamera, controller);
            PlayerHud.Create(controller, health, inventory, interactor, worldInteractor);
        }
    }
}
