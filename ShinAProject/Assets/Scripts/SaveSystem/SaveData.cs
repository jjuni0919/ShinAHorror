using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShinA.SaveSystem
{
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public string saveId = "main";
        public string savedAtUtc;
        public string currentScene;
        public float playTimeSeconds;
        public PlayerSaveData player = new();
        public List<int> inventoryItemNumbers = new();
        public DictionaryData customData = new();

        public void MarkSavedNow()
        {
            savedAtUtc = DateTime.UtcNow.ToString("O");
        }
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

    // JsonUtility cannot serialize Dictionary directly, so extensible values use key/value lists.
    [Serializable]
    public sealed class DictionaryData
    {
        public List<string> keys = new();
        public List<string> values = new();

        public void Set(string key, string value)
        {
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                values[index] = value;
                return;
            }

            keys.Add(key);
            values.Add(value);
        }

        public bool TryGet(string key, out string value)
        {
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
