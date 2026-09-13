using System;
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

            try
            {
                data.schemaVersion = SaveData.CurrentSchemaVersion;
                data.MarkSavedNow();

                string directory = Path.GetDirectoryName(SaveFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                string temporaryPath = SaveFilePath + ".tmp";
                File.WriteAllText(temporaryPath, json);
                File.Copy(temporaryPath, SaveFilePath, true);
                File.Delete(temporaryPath);

                CurrentData = data;
                Saved?.Invoke(data);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save game data: {exception.Message}", this);
                return false;
            }
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
                if (data == null || data.schemaVersion > SaveData.CurrentSchemaVersion)
                {
                    Debug.LogError("Save data is invalid or uses a newer schema version.", this);
                    data = null;
                    return false;
                }

                CurrentData = data;
                Loaded?.Invoke(data);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load game data: {exception.Message}", this);
                data = null;
                return false;
            }
        }

        public SaveData CreateNewData()
        {
            CurrentData = new SaveData();
            return CurrentData;
        }
    }
}
