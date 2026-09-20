using System;
using ShinA.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinA.Maps
{
    [Serializable]
    public sealed class MapItemSpawnEntry
    {
        [SerializeField, InspectorName("아이템")] private ItemDefinition item;
        [SerializeField, Range(0f, 1f), InspectorName("출현 확률")] private float spawnChance = 0.5f;
        [SerializeField, Min(1), InspectorName("최대 출현 수")] private int maxSpawnCount = 1;

        public MapItemSpawnEntry(ItemDefinition item, float spawnChance, int maxSpawnCount)
        {
            this.item = item;
            this.spawnChance = Mathf.Clamp01(spawnChance);
            this.maxSpawnCount = Mathf.Max(1, maxSpawnCount);
        }

        public ItemDefinition Item => item;
        public float SpawnChance => spawnChance;
        public int MaxSpawnCount => maxSpawnCount;
    }

    [DefaultExecutionOrder(100)]
    public sealed class MapItemSpawner : MonoBehaviour
    {
        [Header("아이템 출현 설정")]
        [SerializeField, InspectorName("아이템 목록")] private MapItemSpawnEntry[] items = Array.Empty<MapItemSpawnEntry>();
        [SerializeField, InspectorName("맵 시드 오프셋")] private int seedOffset;

        [Header("출현 범위")]
        [SerializeField, Min(0f), InspectorName("최소 반경")] private float minimumSpawnRadius = 6f;
        [SerializeField, Min(0.1f), InspectorName("최대 반경")] private float maximumSpawnRadius = 24f;
        [SerializeField, Min(1), InspectorName("위치 탐색 횟수")] private int positionSearchAttempts = 12;
        [SerializeField, InspectorName("지면 레이어")] private LayerMask groundMask = ~0;
        [SerializeField, InspectorName("장애물 레이어")] private LayerMask obstacleMask = ~0;

        public void Configure(MapItemSpawnEntry[] spawnItems, int mapSeedOffset,
            float minimumRadius, float maximumRadius)
        {
            items = spawnItems ?? Array.Empty<MapItemSpawnEntry>();
            seedOffset = mapSeedOffset;
            minimumSpawnRadius = Mathf.Max(0f, minimumRadius);
            maximumSpawnRadius = Mathf.Max(0.1f, maximumRadius);
        }

        private void Start()
        {
            MapSceneInitializer initializer = null;
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                initializer = root.GetComponentInChildren<MapSceneInitializer>();
                if (initializer != null)
                {
                    break;
                }
            }
            if (initializer == null)
            {
                Debug.LogError("맵 아이템 스포너가 맵 초기화 정보를 찾지 못했습니다.", this);
                return;
            }

            System.Random random = new(unchecked(initializer.GenerationSeed * 397 ^ seedOffset));
            if (ShinA.SaveSystem.WorldItemPersistence.HasSnapshot(gameObject.scene.name, initializer.GenerationSeed)) return;
            System.Random positionRandom = new(unchecked(initializer.GenerationSeed * 7919 ^ seedOffset));
            Physics.SyncTransforms();
            SpawnItems(random, positionRandom);
        }

        private void SpawnItems(System.Random random, System.Random positionRandom)
        {
            GameObject spawnedItemRoot = new("출현 아이템");
            SceneManager.MoveGameObjectToScene(spawnedItemRoot, gameObject.scene);

            foreach (MapItemSpawnEntry entry in items ?? Array.Empty<MapItemSpawnEntry>())
            {
                if (entry?.Item == null)
                {
                    continue;
                }

                ItemPickup pickupPrefab = Resources.Load<ItemPickup>(
                    $"Prefabs/Items/ItemPickup_{entry.Item.ItemNumber:000}");
                if (pickupPrefab == null)
                {
                    Debug.LogError($"아이템 픽업 프리팹을 찾지 못했습니다: {entry.Item.ItemName}", entry.Item);
                    continue;
                }

                for (int i = 0; i < entry.MaxSpawnCount; i++)
                {
                    if (random.NextDouble() >= entry.SpawnChance ||
                        !TryFindSpawnPosition(positionRandom, out Vector3 position))
                    {
                        continue;
                    }

                    ItemPickup pickup = Instantiate(pickupPrefab, position, Quaternion.identity,
                        spawnedItemRoot.transform);
                    pickup.name = pickupPrefab.name;
                }
            }
        }

        private bool TryFindSpawnPosition(System.Random random, out Vector3 position)
        {
            float minRadius = Mathf.Min(minimumSpawnRadius, maximumSpawnRadius);
            float maxRadius = Mathf.Max(minimumSpawnRadius, maximumSpawnRadius);
            for (int i = 0; i < positionSearchAttempts; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = Mathf.Lerp(minRadius, maxRadius, (float)random.NextDouble());
                Vector3 rayOrigin = transform.position +
                                    new Vector3(Mathf.Cos(angle) * distance, 40f,
                                        Mathf.Sin(angle) * distance);

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 80f, groundMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                Vector3 candidate = hit.point + Vector3.up * 0.65f;
                if (Physics.CheckSphere(candidate, 0.4f, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                position = candidate;
                return true;
            }

            position = default;
            return false;
        }
    }
}
