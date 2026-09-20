using System;
using System.Collections.Generic;
using ShinA.Inventory;
using UnityEngine;

namespace ShinA.Monsters
{
    public enum MonsterDisposition
    {
        [InspectorName("적대적")]
        Hostile,
        [InspectorName("우호적")]
        Friendly,
        [InspectorName("공격받으면 반격하는 중립")]
        NeutralRetaliatory,
        [InspectorName("공격받아도 반격하지 않는 중립")]
        NeutralPassive
    }

    [Serializable]
    public sealed class MonsterDropEntry
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
        [SerializeField, Min(1)] private int minQuantity = 1;
        [SerializeField, Min(1)] private int maxQuantity = 1;

        public ItemDefinition Item => item;
        public float DropChance => dropChance;
        public int MinQuantity => Mathf.Max(1, minQuantity);
        public int MaxQuantity => Mathf.Max(MinQuantity, maxQuantity);
    }

    [CreateAssetMenu(fileName = "Monster", menuName = "ShinA/몬스터/몬스터 정의")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string monsterId;
        [SerializeField] private string displayName;
        [SerializeField] private MonsterDisposition disposition;
        [SerializeField] private GameObject prefab;

        [Header("전투 스탯")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float attackPower = 10f;
        [SerializeField, Min(0f)] private float defense;
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField, Min(0f)] private float detectionRange = 15f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.5f;
        [SerializeField, Min(0.01f)] private float attackCooldown = 1f;

        [Header("스폰 설정")]
        [Tooltip("같은 씬에 존재할 수 있는 개체 수 또는 누적 스폰 수의 한계입니다.")]
        [SerializeField, Min(0)] private int maxSpawnCount = 3;
        [Tooltip("스포너의 한 번의 틱마다 이 몬스터가 스폰을 시도할 확률입니다.")]
        [SerializeField, Range(0f, 1f)] private float spawnChancePerTick = 0.25f;
        [Tooltip("활성화하면 처치된 개체가 한도에서 빠지고, 비활성화하면 누적 스폰 수가 한도에 포함됩니다.")]
        [SerializeField] private bool replenishAfterDeath = true;

        [Header("처치 보상")]
        [SerializeField] private MonsterDropEntry[] drops = Array.Empty<MonsterDropEntry>();

        public string MonsterId => monsterId;
        public string DisplayName => displayName;
        public MonsterDisposition Disposition => disposition;
        public GameObject Prefab => prefab;
        public float MaxHealth => maxHealth;
        public float AttackPower => attackPower;
        public float Defense => defense;
        public float MoveSpeed => moveSpeed;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public int MaxSpawnCount => maxSpawnCount;
        public float SpawnChancePerTick => spawnChancePerTick;
        public bool ReplenishAfterDeath => replenishAfterDeath;
        public IReadOnlyList<MonsterDropEntry> Drops => drops ?? Array.Empty<MonsterDropEntry>();

        public void ConfigureIdentity(string id, string monsterName, MonsterDisposition monsterDisposition)
        {
            monsterId = id ?? string.Empty;
            displayName = monsterName ?? string.Empty;
            disposition = monsterDisposition;
        }

        public void ConfigureStats(float health, float power, float defenseValue, float speed,
            float detection, float range, float cooldown)
        {
            maxHealth = Mathf.Max(1f, health);
            attackPower = Mathf.Max(0f, power);
            defense = Mathf.Max(0f, defenseValue);
            moveSpeed = Mathf.Max(0f, speed);
            detectionRange = Mathf.Max(0f, detection);
            attackRange = Mathf.Max(0.1f, range);
            attackCooldown = Mathf.Max(0.01f, cooldown);
        }

        public void ConfigureSpawn(int maximumCount, float chancePerTick, bool respawnAfterDeath)
        {
            maxSpawnCount = Mathf.Max(0, maximumCount);
            spawnChancePerTick = Mathf.Clamp01(chancePerTick);
            replenishAfterDeath = respawnAfterDeath;
        }

        public void SetPrefab(GameObject monsterPrefab)
        {
            prefab = monsterPrefab;
        }
    }
}
