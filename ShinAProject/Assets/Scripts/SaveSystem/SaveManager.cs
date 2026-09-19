using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShinA.SaveSystem
{
    public sealed class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "savegame.json";
        private static SaveManager instance;

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
                Debug.LogException(exception, this);
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
                if (data == null || data.schemaVersion != SaveData.CurrentSchemaVersion)
                {
                    Debug.LogError("Save data is invalid or uses an unsupported schema version.", this);
                    data = null;
                    return false;
                }

                data.player ??= new PlayerSaveData();
                data.inventoryItemNumbers ??= new List<int>();
                data.customData ??= new DictionaryData();
                data.customData.keys ??= new List<string>();
                data.customData.values ??= new List<string>();
            }
            catch (Exception exception) when (exception is IOException ||
                                               exception is UnauthorizedAccessException ||
                                               exception is NotSupportedException ||
                                               exception is ArgumentException)
            {
                Debug.LogException(exception, this);
                data = null;
                return false;
            }

            CurrentData = data;
            Loaded?.Invoke(data);
            return true;
        }

        public SaveData CreateNewData()
        {
            CurrentData = new SaveData();
            return CurrentData;
        }
    }
}
