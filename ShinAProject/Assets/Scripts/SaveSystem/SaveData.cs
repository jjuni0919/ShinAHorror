using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShinA.SaveSystem
{
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentSchemaVersion = 4;
        public const int StartingCurrency = 500;

        public int schemaVersion = CurrentSchemaVersion;
        public string saveId = "main";
        public string savedAtUtc;
        public string currentScene;
        public float playTimeSeconds;
        public int day = 1;
        public int currency = StartingCurrency;
        public List<int> collectionItemNumbers = new();
        public bool gameOver;
        public int lastSettlementRevenue;
        public int missionFailureCount;
        public string lastMissionMapId;
        public string lastMissionResult;
        public int lastMissionSeed;
        public PlayerSaveData player = new();
        public List<int> inventoryItemNumbers = new();
        public List<int> warehouseItemNumbers = new();
        public List<WorldItemSceneData> worldItems = new();
        public List<int> pendingOrders = new();
        public List<int> deliveredOrders = new();
        public bool deliveryBoxOpened;
        public int successfulExpeditions;
        public bool companyRewardClaimed;
        public string selectedMapId;
        public string activeMissionMapId;
        public int activeMissionSeed;
        public float missionRemainingTime;
        public int missionCurrency;
        public List<int> fieldStorageItems = new();
        public DictionaryData customData = new();

        public void MarkSavedNow()
        {
            savedAtUtc = DateTime.UtcNow.ToString("O");
        }
    }

    [Serializable]
    public sealed class WorldItemSceneData
    {
        public string sceneName;
        public int seed;
        public List<WorldItemData> items = new();
    }

    [Serializable]
    public sealed class WorldItemData
    {
        public int itemNumber;
        public SerializableVector3 position;
        public SerializableVector3 eulerAngles;
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public SerializableVector3 position;
        public SerializableVector3 eulerAngles;
        public string selectedSkinId = "default";
        public int selectedInventorySlot;
        public float currentStamina;
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator SerializableVector3(Vector3 value) => new(value);
        public static implicit operator Vector3(SerializableVector3 value) => value.ToVector3();
    }

    // JsonUtility는 Dictionary를 직렬화할 수 없으므로 확장 데이터의 키와 값을 병렬 목록으로 저장한다.
    [Serializable]
    public sealed class DictionaryData
    {
        public List<string> keys = new();
        public List<string> values = new();

        public void Set(string key, string value)
        {
            keys ??= new List<string>();
            values ??= new List<string>();
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                while (values.Count <= index)
                {
                    values.Add(null);
                }

                values[index] = value;
                return;
            }

            keys.Add(key);
            values.Add(value);
        }

        public bool TryGet(string key, out string value)
        {
            if (keys == null || values == null)
            {
                value = null;
                return false;
            }

            int index = keys.IndexOf(key);
            if (index >= 0 && index < values.Count)
            {
                value = values[index];
                return true;
            }

            value = null;
            return false;
        }
    }
}
