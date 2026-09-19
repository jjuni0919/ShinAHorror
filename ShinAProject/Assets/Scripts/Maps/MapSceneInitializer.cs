using UnityEngine;

namespace ShinA.Maps
{
    public abstract class MapSceneInitializer : MonoBehaviour
    {
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, 0.05f, 0f);
        [SerializeField] private Vector3 playerSpawnEulerAngles;

        protected virtual void Awake()
        {
            InitializeEnvironment();
            GameObject player = SpawnPlayer();
            if (player != null)
            {
                ConfigurePlayer(player);
            }
        }

        protected abstract void InitializeEnvironment();

        protected virtual void ConfigurePlayer(GameObject player)
        {
        }

        protected GameObject SpawnPlayer()
        {
            GameObject existingPlayer = GameObject.FindWithTag("Player");
            if (existingPlayer != null)
            {
                existingPlayer.transform.SetPositionAndRotation(playerSpawnPosition,
                    Quaternion.Euler(playerSpawnEulerAngles));
                return existingPlayer;
            }

            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
            if (playerPrefab == null)
            {
                Debug.LogError("Player prefab could not be loaded.", this);
                return null;
            }

            return Instantiate(playerPrefab, playerSpawnPosition, Quaternion.Euler(playerSpawnEulerAngles));
        }

        protected static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { color = color };
        }

        protected static GameObject CreateBlock(string name, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }
    }
}
