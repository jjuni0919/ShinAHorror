using ShinA.Inventory;
using ShinA.Player;
using ShinA.UI;
using UnityEngine;

namespace ShinA.WaitingRoom
{
    [DefaultExecutionOrder(-1000)]
    public sealed class WaitingSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, 0.05f, -5f);
        [SerializeField] private PlayerSkinDefinition initialPlayerSkin;
        [SerializeField, Min(1)] private int inventorySlotCount = 6;
        [SerializeField, Range(0f, 0.4f)] private float cameraForwardOffset = 0.18f;

        private void Awake()
        {
            if (GameObject.FindWithTag("Player") != null)
            {
                return;
            }

            CreateTestRoom();
            CreatePlayerFromPrefab();
            CreateSamplePickups();
            Destroy(gameObject);
        }

        private void CreatePlayerFromPrefab()
        {
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

            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
            if (playerPrefab == null)
            {
                Debug.LogError("Player prefab was not found at Resources/Prefabs/Player/Player.", this);
                return;
            }

            GameObject player = Instantiate(playerPrefab);
            player.transform.SetPositionAndRotation(playerSpawnPosition, Quaternion.identity);
            player.GetComponent<PlayerRuntimeSetup>()?.Configure(initialPlayerSkin, inventorySlotCount, cameraForwardOffset);
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
        }

        private static void CreateBlock(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new(shader);
            material.color = color;
            return material;
        }
    }
}
