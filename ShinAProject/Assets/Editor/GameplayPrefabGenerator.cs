using System.IO;
using ShinA.Inventory;
using ShinA.Player;
using UnityEditor;
using UnityEngine;

namespace ShinA.Editor
{
    public static class GameplayPrefabGenerator
    {
        private const string PlayerPrefabPath = "Assets/Resources/Prefabs/Player/Player.prefab";
        private const string ItemDataFolder = "Assets/Resources/Items";
        private const string ItemModelFolder = "Assets/Resources/Prefabs/ItemModels";
        private const string ItemPickupFolder = "Assets/Resources/Prefabs/Items";
        private const string ItemMaterialFolder = "Assets/Resources/Materials/Items";

        [InitializeOnLoadMethod]
        private static void ScheduleInitialGeneration()
        {
            EditorApplication.delayCall += GenerateIfMissing;
        }

        [MenuItem("ShinA/Generate Gameplay Prefabs")]
        public static void Generate()
        {
            CreateDirectories();
            CreatePlayerPrefab();
            CreateItemAssetsAndPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Player prefab and 15 item prefabs generated.");
        }

        private static void GenerateIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            string[] itemAssets = AssetDatabase.FindAssets("t:ItemDefinition", new[] { ItemDataFolder });
            string[] pickupPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { ItemPickupFolder });
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            bool playerNeedsUpdate = playerPrefab == null ||
                                     playerPrefab.GetComponent<PlayerHealth>() == null ||
                                     playerPrefab.GetComponent<PlayerTabletController>() == null;
            if (playerNeedsUpdate ||
                itemAssets.Length < 15 || pickupPrefabs.Length < 15)
            {
                Generate();
            }
        }

        private static void CreateDirectories()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PlayerPrefabPath) ?? "Assets/Resources/Prefabs/Player");
            Directory.CreateDirectory(ItemDataFolder);
            Directory.CreateDirectory(ItemModelFolder);
            Directory.CreateDirectory(ItemPickupFolder);
            Directory.CreateDirectory(ItemMaterialFolder);
        }

        private static void CreatePlayerPrefab()
        {
            GameObject player = new("Player");
            player.tag = "Player";

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.stepOffset = 0.3f;
            characterController.slopeLimit = 45f;

            player.AddComponent<FirstPersonController>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerAppearance>();
            player.AddComponent<PlayerInventory>();
            player.AddComponent<PlayerItemInteractor>();
            player.AddComponent<PlayerTabletController>();
            player.AddComponent<PlayerRuntimeSetup>();

            GameObject cameraObject = new("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.65f, 0.18f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();

            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
        }

        private static void CreateItemAssetsAndPrefabs()
        {
            var samples = SampleItemCatalog.CreateSamples();
            foreach (ItemDefinition sample in samples)
            {
                string number = sample.ItemNumber.ToString("000");
                string itemAssetPath = $"{ItemDataFolder}/Item_{number}.asset";
                ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemAssetPath);
                if (definition == null || definition.GetType() != sample.GetType())
                {
                    if (definition != null)
                    {
                        AssetDatabase.DeleteAsset(itemAssetPath);
                    }

                    definition = Object.Instantiate(sample);
                    definition.name = sample.name;
                    AssetDatabase.CreateAsset(definition, itemAssetPath);
                }
                else
                {
                    EditorUtility.CopySerialized(sample, definition);
                    definition.name = sample.name;
                }

                GameObject modelPrefab = CreateItemModelPrefab(definition, number);
                definition.ConfigureAssets(definition.Icon, modelPrefab);
                EditorUtility.SetDirty(definition);
                CreatePickupPrefab(definition, modelPrefab, number);
                Object.DestroyImmediate(sample);
            }
        }

        private static GameObject CreateItemModelPrefab(ItemDefinition item, string number)
        {
            PrimitiveType primitive = item is RangedWeaponDefinition
                ? PrimitiveType.Cube
                : item is MeleeWeaponDefinition ? PrimitiveType.Capsule
                : item.EffectType == ItemEffectType.Interactive ? PrimitiveType.Sphere : PrimitiveType.Cube;

            GameObject model = GameObject.CreatePrimitive(primitive);
            model.name = $"Item Model {number}";
            Collider collider = model.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            model.transform.localScale = GetOriginalModelScale(item, primitive);
            Material material = CreateOrUpdateMaterial(item, number);
            model.GetComponent<Renderer>().sharedMaterial = material;

            string path = $"{ItemModelFolder}/ItemModel_{number}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(model, path);
            Object.DestroyImmediate(model);
            return prefab;
        }

        private static Vector3 GetOriginalModelScale(ItemDefinition item, PrimitiveType primitive)
        {
            if (item is MeleeWeaponDefinition)
            {
                return new Vector3(0.09f, 0.38f, 0.09f);
            }

            if (item is RangedWeaponDefinition)
            {
                return new Vector3(0.12f, 0.1f, 0.28f);
            }

            return primitive == PrimitiveType.Sphere ? Vector3.one * 0.22f : Vector3.one * 0.2f;
        }

        private static Material CreateOrUpdateMaterial(ItemDefinition item, string number)
        {
            string path = $"{ItemMaterialFolder}/ItemMaterial_{number}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = item.IconColor;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreatePickupPrefab(ItemDefinition item, GameObject modelPrefab, string number)
        {
            GameObject pickupObject = new($"Pickup_{number}_{item.ItemName}");
            SphereCollider collider = pickupObject.AddComponent<SphereCollider>();
            collider.radius = 0.55f;
            collider.isTrigger = true;

            ItemPickup pickup = pickupObject.AddComponent<ItemPickup>();
            pickup.SetDefinition(item);

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            model.name = "Visual";
            model.transform.SetParent(pickupObject.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            string path = $"{ItemPickupFolder}/ItemPickup_{number}.prefab";
            PrefabUtility.SaveAsPrefabAsset(pickupObject, path);
            Object.DestroyImmediate(pickupObject);
        }
    }
}
