using System;
using System.Collections.Generic;
using ShinA.Managers;
using UnityEngine;

namespace ShinA.Maps
{
    [Serializable]
    public sealed class MapRecord
    {
        public string mapId;
        public string displayName;
        public string sceneName;
        [TextArea] public string description;
    }

    public sealed class MapDatabase : MonoBehaviour
    {
        private static MapDatabase instance;
        private readonly Dictionary<string, MapRecord> mapsById = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<MapRecord> maps = new();

        public static MapDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MapDatabase>();
                    if (instance == null)
                    {
                        instance = new GameObject("MapDatabase").AddComponent<MapDatabase>();
                    }
                }

                return instance;
            }
        }

        public IReadOnlyList<MapRecord> Maps => maps;

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
            RegisterBuiltInMaps();
        }

        public void Register(MapRecord map)
        {
            if (map == null || string.IsNullOrWhiteSpace(map.mapId) || string.IsNullOrWhiteSpace(map.sceneName))
            {
                return;
            }

            if (mapsById.TryGetValue(map.mapId, out MapRecord previous))
            {
                maps.Remove(previous);
            }

            mapsById[map.mapId] = map;
            maps.Add(map);
        }

        public bool TryGetMap(string mapId, out MapRecord map)
        {
            if (string.IsNullOrWhiteSpace(mapId))
            {
                map = null;
                return false;
            }

            return mapsById.TryGetValue(mapId, out map);
        }

        public bool TravelToMap(string mapId)
        {
            return TryGetMap(mapId, out MapRecord map) && SceneLoader.Instance.LoadScene(map.sceneName);
        }

        private void RegisterBuiltInMaps()
        {
            maps.Clear();
            mapsById.Clear();
            Register(new MapRecord
            {
                mapId = "waiting_room",
                displayName = "본부 대기실",
                sceneName = "WaitingScene",
                description = "튜토리얼과 임무 준비를 위한 본부 공간"
            });
            Register(new MapRecord
            {
                mapId = "salt_farm",
                displayName = "흐린 염전",
                sceneName = "SaltFarmScene",
                description = "안개와 잿빛 하늘로 뒤덮인 버려진 염전"
            });
        }
    }
}
