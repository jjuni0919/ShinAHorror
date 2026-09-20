using System.Collections.Generic;
using ShinA.Inventory;
using ShinA.Maps;
using ShinA.Missions;
using ShinA.Managers;
using ShinA.Economy;
using UnityEngine;

namespace ShinA.SaveSystem
{
    [DefaultExecutionOrder(200)]
    public sealed class WorldItemPersistence : MonoBehaviour
    {
        public static WorldItemPersistence Active { get; private set; }
        private int seed;
        private float nextSaveTime;

        public static bool HasSnapshot(string sceneName, int generationSeed)
        {
            return SaveManager.Instance.CurrentData?.worldItems.Exists(
                entry => entry.sceneName == sceneName && entry.seed == generationSeed) == true;
        }

        private void Start()
        {
            Active = this;
            MapSceneInitializer initializer = FindFirstObjectByType<MapSceneInitializer>();
            seed = gameObject.scene.name == "WaitingScene" || gameObject.scene.name == "CompanyScene"
                ? 0 : initializer != null ? initializer.GenerationSeed : 0;
            SaveData data = SaveManager.Instance.CurrentData;
            WorldItemSceneData saved = data.worldItems.Find(entry => entry.sceneName == gameObject.scene.name && entry.seed == seed);
            if (saved != null)
            {
                foreach (WorldItemData entry in saved.items)
                {
                    ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{entry.itemNumber:000}");
                    ItemPickup.Spawn(item, entry.position, Quaternion.Euler(entry.eulerAngles));
                }
            }

            if (gameObject.scene.name == "WaitingScene")
            {
                PlayerProgress progress = MissionSession.Instance.Progress;
                List<int> returnedItems = new(progress.WarehouseItemNumbers);
                foreach (int number in returnedItems)
                {
                    ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{number:000}");
                    Vector3 position = new(Random.Range(-3f, 3f), 1f + Random.value, Random.Range(-4f, -2f));
                    if (ItemPickup.Spawn(item, position, Random.rotation) != null)
                        progress.TryRemoveWarehouseItem(number);
                }
                if (data.deliveredOrders.Count > 0 || data.deliveryBoxOpened)
                    DeliveryBox.Create(new Vector3(5.5f, 0f, -5.5f), data.deliveryBoxOpened);
            }

            SaveManager.Instance.RestorePlayerTransform();
            if (MissionSession.Instance.Progress.IsGameOver)
                GameStateManager.Instance.SetState(GameState.GameOver);
            nextSaveTime = Time.unscaledTime + 5f;
        }

        public void Capture(SaveData data)
        {
            WorldItemSceneData saved = data.worldItems.Find(entry => entry.sceneName == gameObject.scene.name && entry.seed == seed);
            if (saved == null)
            {
                saved = new WorldItemSceneData { sceneName = gameObject.scene.name, seed = seed };
                data.worldItems.Add(saved);
            }
            saved.items.Clear();
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (ItemPickup pickup in root.GetComponentsInChildren<ItemPickup>())
            {
                if (pickup.Item == null || pickup.IsCollected) continue;
                saved.items.Add(new WorldItemData
                {
                    itemNumber = pickup.Item.ItemNumber,
                    position = pickup.transform.position,
                    eulerAngles = pickup.transform.eulerAngles
                });
            }
        }

        private void Update()
        {
            if (SceneLoader.Instance.IsLoading || Time.unscaledTime < nextSaveTime) return;
            nextSaveTime = Time.unscaledTime + 5f;
            SaveManager.Instance.SaveCurrent();
        }

        private void OnApplicationQuit()
        {
            if (!SceneLoader.Instance.IsLoading) SaveManager.Instance.SaveCurrent();
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }
    }
}
