using ShinA.Missions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinA.Maps
{
    public abstract class MapSceneInitializer : MonoBehaviour
    {
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, 0.05f, 0f);
        [SerializeField] private Vector3 playerSpawnEulerAngles;
        [SerializeField] private int generationSeed = 12345;

        public int GenerationSeed { get; private set; }
        protected System.Random GenerationRandom { get; private set; }

        protected virtual void Awake()
        {
            GenerationSeed = MapDatabase.Instance.GenerationSeed ?? generationSeed;
            GenerationRandom = new System.Random(GenerationSeed);
            MapDatabase.Instance.TryGetMapByScene(SceneManager.GetActiveScene().name, out MapRecord currentMap);
            if (currentMap != null && currentMap.missionDurationSeconds > 0f)
            {
                MissionSession.Instance.EnsurePrepared(currentMap, GenerationSeed);
            }

            InitializeEnvironment();
            GameObject player = SpawnPlayer();
            if (player != null)
            {
                ConfigurePlayer(player);
                if (currentMap != null && currentMap.missionDurationSeconds > 0f)
                {
                    MissionSession.Instance.BindPlayer(player);
                }
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
                Debug.LogError("플레이어 프리팹을 불러오지 못했습니다.", this);
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

        protected static void CreateMissionBase(Transform parent, Material wallMaterial,
            Material floorMaterial, Material terminalMaterial)
        {
            GameObject headquarters = new("Field Headquarters");
            headquarters.transform.SetParent(parent, false);

            CreateBlock("Headquarters Floor", headquarters.transform, new Vector3(0f, -0.05f, 0f),
                new Vector3(10f, 0.1f, 10f), floorMaterial);
            CreateBlock("Headquarters Roof", headquarters.transform, new Vector3(0f, 4.5f, 0f),
                new Vector3(10f, 0.25f, 10f), wallMaterial);
            CreateBlock("Headquarters Back Wall", headquarters.transform, new Vector3(0f, 2.2f, -5f),
                new Vector3(10f, 4.5f, 0.25f), wallMaterial);
            CreateBlock("Headquarters Left Wall", headquarters.transform, new Vector3(-5f, 2.2f, 0f),
                new Vector3(0.25f, 4.5f, 10f), wallMaterial);
            CreateBlock("Headquarters Right Wall", headquarters.transform, new Vector3(5f, 2.2f, 0f),
                new Vector3(0.25f, 4.5f, 10f), wallMaterial);
            CreateBlock("Headquarters Front Left", headquarters.transform, new Vector3(-3f, 2.2f, 5f),
                new Vector3(4f, 4.5f, 0.25f), wallMaterial);
            CreateBlock("Headquarters Front Right", headquarters.transform, new Vector3(3f, 2.2f, 5f),
                new Vector3(4f, 4.5f, 0.25f), wallMaterial);

            GameObject completionTerminal = CreateBlock("Mission Completion Terminal", headquarters.transform,
                new Vector3(-2f, 1f, -4.45f), new Vector3(1.3f, 1.7f, 0.65f), terminalMaterial);
            completionTerminal.AddComponent<MissionCompletionTerminal>();
            GameObject storageTerminal = CreateBlock("Field Storage Terminal", headquarters.transform,
                new Vector3(2f, 1f, -4.45f), new Vector3(1.3f, 1.7f, 0.65f), terminalMaterial);
            storageTerminal.AddComponent<MissionStorageTerminal>();

            GameObject lightObject = new("Headquarters Light");
            lightObject.transform.SetParent(headquarters.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 3.8f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.78f, 0.9f, 0.88f);
            light.intensity = 2.4f;
            light.range = 12f;
            light.shadows = LightShadows.Soft;
        }
    }
}
