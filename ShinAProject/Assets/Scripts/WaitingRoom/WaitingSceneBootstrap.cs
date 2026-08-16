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

        private void Awake()
        {
            if (GameObject.FindWithTag("Player") != null)
            {
                return;
            }

            CreateTestRoom();
            CreatePlayer();
        }

        private void CreatePlayer()
        {
            GameObject player = new("Player");
            player.tag = "Player";
            player.transform.SetPositionAndRotation(playerSpawnPosition, Quaternion.identity);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;

            Camera playerCamera = GetComponent<Camera>();
            transform.SetParent(player.transform, false);
            transform.localPosition = new Vector3(0f, 1.65f, 0f);
            transform.localRotation = Quaternion.identity;
            playerCamera.nearClipPlane = 0.05f;

            FirstPersonController firstPersonController = player.AddComponent<FirstPersonController>();
            firstPersonController.Initialize(playerCamera);

            PlayerAppearance appearance = player.AddComponent<PlayerAppearance>();
            appearance.Initialize(playerCamera.transform, initialPlayerSkin);

            PlayerHud.Create(firstPersonController);
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
