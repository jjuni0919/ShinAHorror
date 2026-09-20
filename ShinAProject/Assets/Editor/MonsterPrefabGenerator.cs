using System.IO;
using ShinA.Monsters;
using UnityEditor;
using UnityEngine;

namespace ShinA.Editor
{
    public static class MonsterPrefabGenerator
    {
        private const string DefinitionFolder = "Assets/Resources/Monsters";
        private const string PrefabFolder = "Assets/Resources/Prefabs/Monsters";
        private const string MaterialFolder = "Assets/Resources/Materials/Monsters";
        private const string SpawnerPrefabPath = PrefabFolder + "/MonsterSpawner.prefab";

        [MenuItem("ShinA/몬스터 에셋 생성")]
        public static void Generate()
        {
            Directory.CreateDirectory(DefinitionFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(MaterialFolder);

            MonsterDefinition[] definitions =
            {
                CreateMonster("Hostile", "적대적 개체", MonsterDisposition.Hostile,
                    100f, 15f, 2f, 3.2f, 18f, 1.7f, 1.2f, 4, 0.35f, true,
                    new Color(0.48f, 0.08f, 0.07f)),
                CreateMonster("Friendly", "우호적 개체", MonsterDisposition.Friendly,
                    80f, 0f, 1f, 1.5f, 0f, 1.5f, 1f, 2, 0.15f, true,
                    new Color(0.12f, 0.42f, 0.24f)),
                CreateMonster("NeutralRetaliatory", "반격하는 중립 개체", MonsterDisposition.NeutralRetaliatory,
                    120f, 18f, 4f, 2.8f, 0f, 1.8f, 1.3f, 3, 0.25f, true,
                    new Color(0.62f, 0.42f, 0.08f)),
                CreateMonster("NeutralPassive", "비반격 중립 개체", MonsterDisposition.NeutralPassive,
                    90f, 0f, 1f, 1.2f, 0f, 1.5f, 1f, 2, 0.2f, false,
                    new Color(0.28f, 0.3f, 0.34f))
            };

            CreateSpawnerPrefab(definitions);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("몬스터 정의 4개, 몬스터 프리팹 4개와 스포너 프리팹을 생성했습니다.");
        }

        private static MonsterDefinition CreateMonster(string id, string displayName,
            MonsterDisposition disposition, float health, float attackPower, float defense, float moveSpeed,
            float detectionRange, float attackRange, float attackCooldown, int maxSpawnCount,
            float spawnChance, bool replenishAfterDeath, Color color)
        {
            string definitionPath = $"{DefinitionFolder}/Monster_{id}.asset";
            MonsterDefinition definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(definitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                definition.ConfigureIdentity(id, displayName, disposition);
                definition.ConfigureStats(health, attackPower, defense, moveSpeed, detectionRange,
                    attackRange, attackCooldown);
                definition.ConfigureSpawn(maxSpawnCount, spawnChance, replenishAfterDeath);
                AssetDatabase.CreateAsset(definition, definitionPath);
            }

            GameObject prefab = CreateMonsterPrefab(definition, id, color);
            definition.SetPrefab(prefab);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject CreateMonsterPrefab(MonsterDefinition definition, string id, Color color)
        {
            GameObject root = new($"Monster_{id}");
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.25f;

            MonsterController monster = root.AddComponent<MonsterController>();
            monster.SetDefinition(definition);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "외형";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.transform.localScale = new Vector3(0.72f, 0.9f, 0.72f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = CreateOrUpdateMaterial(id, color);

            string prefabPath = $"{PrefabFolder}/Monster_{id}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Material CreateOrUpdateMaterial(string id, Color color)
        {
            string materialPath = $"{MaterialFolder}/Monster_{id}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateSpawnerPrefab(MonsterDefinition[] definitions)
        {
            GameObject spawnerObject = new("MonsterSpawner");
            MonsterSpawner spawner = spawnerObject.AddComponent<MonsterSpawner>();
            spawner.SetDefinitions(definitions);
            PrefabUtility.SaveAsPrefabAsset(spawnerObject, SpawnerPrefabPath);
            Object.DestroyImmediate(spawnerObject);
        }
    }
}
