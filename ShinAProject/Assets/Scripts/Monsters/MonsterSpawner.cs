using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinA.Monsters
{
    public class MonsterSpawner : MonoBehaviour
    {
        private readonly struct PopulationKey : IEquatable<PopulationKey>
        {
            public PopulationKey(int sceneHandle, MonsterDefinition definition)
            {
                SceneHandle = sceneHandle;
                Definition = definition;
            }

            private int SceneHandle { get; }
            private MonsterDefinition Definition { get; }

            public bool Equals(PopulationKey other)
            {
                return SceneHandle == other.SceneHandle && Definition == other.Definition;
            }

            public override bool Equals(object obj)
            {
                return obj is PopulationKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(SceneHandle, Definition);
            }

            public bool BelongsTo(Scene scene)
            {
                return SceneHandle == scene.handle;
            }
        }

        private sealed class Population
        {
            public int AliveCount;
            public int TotalSpawned;
        }

        private static readonly Dictionary<PopulationKey, Population> Populations = new();

        [Header("스폰 대상")]
        [SerializeField] private MonsterDefinition[] monsters = Array.Empty<MonsterDefinition>();

        [Header("스폰 주기")]
        [SerializeField, Min(0.1f)] private float tickInterval = 5f;
        [SerializeField, Min(0f)] private float minimumSpawnRadius = 10f;
        [SerializeField, Min(0.1f)] private float maximumSpawnRadius = 28f;
        [SerializeField, Min(1)] private int positionSearchAttempts = 8;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private LayerMask obstacleMask = ~0;

        private float remainingTickTime;

        public void SetDefinitions(MonsterDefinition[] definitions)
        {
            monsters = definitions ?? Array.Empty<MonsterDefinition>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPopulations()
        {
            Populations.Clear();
            SceneManager.sceneUnloaded -= RemoveScenePopulation;
            SceneManager.sceneUnloaded += RemoveScenePopulation;
        }

        protected virtual void OnEnable()
        {
            remainingTickTime = UnityEngine.Random.Range(0f, Mathf.Max(0.1f, tickInterval));
        }

        protected virtual void Update()
        {
            remainingTickTime -= Time.deltaTime;
            if (remainingTickTime > 0f)
            {
                return;
            }

            remainingTickTime = Mathf.Max(0.1f, tickInterval);
            TickSpawn();
        }

        protected virtual bool MeetsAdditionalSpawnConditions(MonsterDefinition definition)
        {
            return true;
        }

        protected virtual bool IsSpawnPositionAllowed(MonsterDefinition definition, Vector3 position)
        {
            return true;
        }

        private void TickSpawn()
        {
            foreach (MonsterDefinition definition in monsters ?? Array.Empty<MonsterDefinition>())
            {
                if (!CanAttemptSpawn(definition) || UnityEngine.Random.value > definition.SpawnChancePerTick)
                {
                    continue;
                }

                TrySpawn(definition);
            }
        }

        private bool CanAttemptSpawn(MonsterDefinition definition)
        {
            if (definition == null || definition.Prefab == null || definition.MaxSpawnCount <= 0 ||
                !MeetsAdditionalSpawnConditions(definition))
            {
                return false;
            }

            Population population = GetPopulation(definition);
            int currentCount = definition.ReplenishAfterDeath
                ? population.AliveCount
                : population.TotalSpawned;
            return currentCount < definition.MaxSpawnCount;
        }

        private void TrySpawn(MonsterDefinition definition)
        {
            if (!TryFindSpawnPosition(definition, out Vector3 position))
            {
                return;
            }

            GameObject monsterObject = Instantiate(definition.Prefab, position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(monsterObject, gameObject.scene);
            MonsterController monster = monsterObject.GetComponent<MonsterController>();
            if (monster == null)
            {
                Debug.LogError($"몬스터 프리팹에 MonsterController가 없습니다: {definition.Prefab.name}", definition.Prefab);
                Destroy(monsterObject);
                return;
            }

            PopulationKey key = new(gameObject.scene.handle, definition);
            Population population = GetPopulation(definition);
            population.AliveCount++;
            population.TotalSpawned++;
            monster.Removed += _ => ReleasePopulation(key);
        }

        private bool TryFindSpawnPosition(MonsterDefinition definition, out Vector3 position)
        {
            float minRadius = Mathf.Min(minimumSpawnRadius, maximumSpawnRadius);
            float maxRadius = Mathf.Max(minimumSpawnRadius, maximumSpawnRadius);
            for (int i = 0; i < positionSearchAttempts; i++)
            {
                Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
                if (direction.sqrMagnitude <= Mathf.Epsilon)
                {
                    direction = Vector2.right;
                }
                float distance = UnityEngine.Random.Range(minRadius, maxRadius);
                Vector3 rayOrigin = transform.position +
                                    new Vector3(direction.x * distance, 30f, direction.y * distance);

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 60f, groundMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                Vector3 candidate = hit.point + Vector3.up * 0.05f;
                Vector3 capsuleBottom = candidate + Vector3.up * 0.45f;
                Vector3 capsuleTop = candidate + Vector3.up * 1.45f;
                if (Physics.CheckCapsule(capsuleBottom, capsuleTop, 0.4f, obstacleMask,
                        QueryTriggerInteraction.Ignore) || !IsSpawnPositionAllowed(definition, candidate))
                {
                    continue;
                }

                position = candidate;
                return true;
            }

            position = default;
            return false;
        }

        private Population GetPopulation(MonsterDefinition definition)
        {
            PopulationKey key = new(gameObject.scene.handle, definition);
            if (!Populations.TryGetValue(key, out Population population))
            {
                population = new Population();
                Populations.Add(key, population);
            }

            return population;
        }

        private static void ReleasePopulation(PopulationKey key)
        {
            if (Populations.TryGetValue(key, out Population population))
            {
                population.AliveCount = Mathf.Max(0, population.AliveCount - 1);
            }
        }

        private static void RemoveScenePopulation(Scene scene)
        {
            List<PopulationKey> keysToRemove = new();
            foreach (PopulationKey key in Populations.Keys)
            {
                if (key.BelongsTo(scene))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (PopulationKey key in keysToRemove)
            {
                Populations.Remove(key);
            }
        }
    }
}
