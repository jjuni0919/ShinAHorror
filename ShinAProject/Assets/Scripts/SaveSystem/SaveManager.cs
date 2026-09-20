using System;
using System.Collections.Generic;
using System.IO;
using ShinA.Inventory;
using ShinA.Managers;
using ShinA.Missions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinA.SaveSystem
{
    public sealed class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "savegame.json";
        private static SaveManager instance;
        private bool loadingSave;
        private bool restorePlayerTransform;

        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SaveManager>();
                    if (instance == null)
                    {
                        GameObject managerObject = new("SaveManager");
                        instance = managerObject.AddComponent<SaveManager>();
                    }
                }

                return instance;
            }
        }

        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        public bool HasSaveData => File.Exists(SaveFilePath);
        public SaveData CurrentData { get; private set; }

        public event Action<SaveData> Saved;
        public event Action<SaveData> Loaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateOnStartup()
        {
            _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool Save(SaveData data)
        {
            if (data == null)
            {
                return false;
            }

            data.schemaVersion = SaveData.CurrentSchemaVersion;
            data.MarkSavedNow();
            string temporaryPath = SaveFilePath + ".tmp";

            try
            {
                string directory = Path.GetDirectoryName(SaveFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(temporaryPath, json);
                if (File.Exists(SaveFilePath))
                {
                    File.Replace(temporaryPath, SaveFilePath, null);
                }
                else
                {
                    File.Move(temporaryPath, SaveFilePath);
                }
            }
            catch (Exception exception) when (exception is IOException ||
                                               exception is UnauthorizedAccessException ||
                                               exception is NotSupportedException ||
                                               exception is ArgumentException)
            {
                Debug.LogError($"게임 저장 중 오류가 발생했습니다.\n{exception}", this);
                return false;
            }

            CurrentData = data;
            Saved?.Invoke(data);
            return true;
        }

        public bool Load(out SaveData data)
        {
            data = null;
            if (!HasSaveData)
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || !TryMigrate(data))
                {
                    Debug.LogError("저장 데이터가 유효하지 않거나 지원하지 않는 스키마 버전을 사용합니다.", this);
                    data = null;
                    return false;
                }

                data.player ??= new PlayerSaveData();
                data.inventoryItemNumbers ??= new List<int>();
                data.warehouseItemNumbers ??= new List<int>();
                data.worldItems ??= new List<WorldItemSceneData>();
                data.pendingOrders ??= new List<int>();
                data.deliveredOrders ??= new List<int>();
                data.fieldStorageItems ??= new List<int>();
                data.collectionItemNumbers ??= new List<int>();
                data.customData ??= new DictionaryData();
                data.customData.keys ??= new List<string>();
                data.customData.values ??= new List<string>();
            }
            catch (Exception exception) when (exception is IOException ||
                                               exception is UnauthorizedAccessException ||
                                               exception is NotSupportedException ||
                                               exception is ArgumentException)
            {
                Debug.LogError($"저장 데이터 불러오기 중 오류가 발생했습니다.\n{exception}", this);
                data = null;
                return false;
            }

            CurrentData = data;
            restorePlayerTransform = true;
            Loaded?.Invoke(data);
            return true;
        }

        public bool LoadGame()
        {
            MissionSession session = MissionSession.Instance;
            if (session.IsActive || !Load(out SaveData data))
            {
                return false;
            }

            session.RestoreProgress(data);
            loadingSave = true;
            bool loaded = SceneLoader.Instance.LoadScene(data.currentScene);
            if (!loaded) loadingSave = false;
            return loaded;
        }

        public bool SaveCurrent(string destination = null)
        {
            if (CurrentData == null) return false;
            WorldItemPersistence.Active?.Capture(CurrentData);
            MissionSession.Instance.WriteTo(CurrentData);
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory != null)
            {
                CurrentData.inventoryItemNumbers.Clear();
                foreach (ItemDefinition item in inventory.Items) CurrentData.inventoryItemNumbers.Add(item.ItemNumber);
                CurrentData.player.position = inventory.transform.position;
                CurrentData.player.eulerAngles = inventory.transform.eulerAngles;
                CurrentData.player.selectedInventorySlot = inventory.SelectedIndex;
            }
            CurrentData.currentScene = destination ?? SceneManager.GetActiveScene().name;
            if (destination != null)
            {
                CurrentData.player.position = Vector3.zero;
                CurrentData.player.eulerAngles = Vector3.zero;
            }
            return Save(CurrentData);
        }

        internal void SaveForSceneChange(string destination)
        {
            if (loadingSave)
            {
                loadingSave = false;
                return;
            }
            restorePlayerTransform = false;
            if (WorldItemPersistence.Active != null && !SaveCurrent(destination))
                Debug.LogError("씬 이동 전 진행 상황을 저장하지 못했습니다.", this);
        }

        internal void RestorePlayerTransform()
        {
            if (!restorePlayerTransform || CurrentData.currentScene != SceneManager.GetActiveScene().name) return;
            restorePlayerTransform = false;
            PlayerInventory player = FindFirstObjectByType<PlayerInventory>();
            if (player == null) return;
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.SetPositionAndRotation(CurrentData.player.position, Quaternion.Euler(CurrentData.player.eulerAngles));
            controller.enabled = true;
        }

        public SaveData CreateNewData()
        {
            CurrentData = new SaveData();
            CurrentData.currentScene = "WaitingScene";
            restorePlayerTransform = false;
            return CurrentData;
        }

        public bool StartNewGame()
        {
            if (SceneLoader.Instance.IsLoading || MissionSession.Instance.IsActive ||
                !Application.CanStreamedLevelBeLoaded("WaitingScene")) return false;
            SaveData data = new() { currentScene = "WaitingScene" };
            if (!Save(data)) return false;
            MissionSession.Instance.RestoreProgress(data);
            restorePlayerTransform = false;
            loadingSave = true;
            bool loaded = SceneLoader.Instance.LoadScene(data.currentScene);
            if (!loaded) loadingSave = false;
            return loaded;
        }

        private static bool TryMigrate(SaveData data)
        {
            if (data.schemaVersion == 3)
            {
                data.schemaVersion = SaveData.CurrentSchemaVersion;
            }
            if (data.schemaVersion == 1 || data.schemaVersion == 2)
            {
                data.schemaVersion = SaveData.CurrentSchemaVersion;
                data.day = Mathf.Max(1, data.day);
                data.currentScene = "WaitingScene";
            }

            if (data.schemaVersion != SaveData.CurrentSchemaVersion)
            {
                return false;
            }

            if (string.IsNullOrEmpty(data.currentScene)) data.currentScene = "WaitingScene";
            return true;
        }
    }
}
