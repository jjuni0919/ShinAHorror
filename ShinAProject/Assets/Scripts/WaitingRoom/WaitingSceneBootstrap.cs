using ShinA.Inventory;
using ShinA.Maps;
using ShinA.Missions;
using ShinA.Player;
using UnityEngine;

namespace ShinA.WaitingRoom
{
    [DefaultExecutionOrder(-1000)]
    public sealed class WaitingSceneBootstrap : MapSceneInitializer
    {
        [SerializeField] private PlayerSkinDefinition initialPlayerSkin;
        [SerializeField, Min(1)] private int inventorySlotCount = 6;
        [SerializeField, Range(0f, 0.4f)] private float cameraForwardOffset = 0.18f;

        protected override void InitializeEnvironment()
        {
            CreateTestRoom();
            CreateSamplePickups();
        }

        protected override void ConfigurePlayer(GameObject player)
        {
            PlayerRuntimeSetup setup = player.GetComponent<PlayerRuntimeSetup>();
            if (setup == null)
            {
                Debug.LogError("Player prefab is missing PlayerRuntimeSetup.", player);
                return;
            }

            setup.Configure(initialPlayerSkin, inventorySlotCount, cameraForwardOffset);

            Camera sceneCamera = GetComponent<Camera>();
            if (sceneCamera != null)
            {
                sceneCamera.enabled = false;
            }

            AudioListener sceneListener = GetComponent<AudioListener>();
            if (sceneListener != null)
            {
                sceneListener.enabled = false;
            }

            Destroy(gameObject);
        }

        private static void CreateSamplePickups()
        {
            GameObject pickupRoot = new("Sample Item Pickups");
            ItemPickup[] pickupPrefabs = Resources.LoadAll<ItemPickup>("Prefabs/Items");

            for (int i = 0; i < pickupPrefabs.Length; i++)
            {
                float angle = (360f / pickupPrefabs.Length) * i + Random.Range(-8f, 8f);
                float radius = Random.Range(3f, 7.2f);
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
                position.y = 0.65f;

                ItemPickup pickup = Instantiate(pickupPrefabs[i], position, Quaternion.identity, pickupRoot.transform);
                pickup.name = pickupPrefabs[i].name;
            }
        }

        private static void CreateTestRoom()
        {
            GameObject room = new("Test Room");
            Material floorMaterial = CreateMaterial(new Color(0.18f, 0.19f, 0.21f));
            Material wallMaterial = CreateMaterial(new Color(0.32f, 0.34f, 0.37f));

            CreateBlock("Floor", room.transform, new Vector3(0f, -0.25f, 0f), new Vector3(18f, 0.5f, 18f), floorMaterial);
            CreateBlock("North Wall", room.transform, new Vector3(0f, 2f, 9f), new Vector3(18f, 4.5f, 0.4f), wallMaterial);
            CreateBlock("South Wall", room.transform, new Vector3(0f, 2f, -9f), new Vector3(18f, 4.5f, 0.4f), wallMaterial);
            CreateBlock("East Wall", room.transform, new Vector3(9f, 2f, 0f), new Vector3(0.4f, 4.5f, 18f), wallMaterial);
            CreateBlock("West Wall", room.transform, new Vector3(-9f, 2f, 0f), new Vector3(0.4f, 4.5f, 18f), wallMaterial);

            CreateBlock("Test Platform", room.transform, new Vector3(3.5f, 0.5f, 3f), new Vector3(4f, 1f, 4f), floorMaterial);
            CreateBlock("Step 1", room.transform, new Vector3(-3f, 0.15f, 2f), new Vector3(2f, 0.3f, 1.5f), wallMaterial);
            CreateBlock("Step 2", room.transform, new Vector3(-3f, 0.45f, 3.2f), new Vector3(2f, 0.9f, 1.5f), wallMaterial);

            CreateBlock("Warehouse Platform", room.transform, new Vector3(-5.8f, 0.05f, -4.8f),
                new Vector3(5f, 0.2f, 5f), floorMaterial);
            CreateBlock("Warehouse Back Wall", room.transform, new Vector3(-5.8f, 1.5f, -7.2f),
                new Vector3(5f, 3f, 0.25f), wallMaterial);
            CreateBlock("Warehouse Shelf Left", room.transform, new Vector3(-7.7f, 1f, -5.4f),
                new Vector3(0.35f, 2f, 2.8f), wallMaterial);
            GameObject warehouseTerminal = CreateBlock("Waiting Warehouse Terminal", room.transform,
                new Vector3(-5.8f, 1f, -6.85f), new Vector3(1.4f, 1.7f, 0.55f), floorMaterial);
            warehouseTerminal.AddComponent<WaitingWarehouseTerminal>();
        }

    }
}
