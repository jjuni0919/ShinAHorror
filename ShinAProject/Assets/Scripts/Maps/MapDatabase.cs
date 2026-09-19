using System;
using System.Collections.Generic;
using ShinA.Managers;
using ShinA.Missions;
using UnityEngine;

namespace ShinA.Maps
{
    [Serializable]
    public sealed class MapRecord
    {
        public string mapId;
        public string displayName;
        public string sceneName;
        [Min(0f)] public float missionDurationSeconds;
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
        public int? GenerationSeed { get; private set; }

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

        public bool TryGetMapByScene(string sceneName, out MapRecord map)
        {
            map = maps.Find(record => string.Equals(record.sceneName, sceneName,
                StringComparison.OrdinalIgnoreCase));
            return map != null;
        }

        public bool TravelToMap(string mapId)
        {
            return TravelToMap(mapId, null);
        }

        public bool TravelToMap(string mapId, int? seed)
        {
            if (!TryGetMap(mapId, out MapRecord map) || SceneLoader.Instance.IsLoading)
            {
                return false;
            }

            int? previousSeed = GenerationSeed;
            int resolvedSeed = seed ?? Guid.NewGuid().GetHashCode();
            bool startsMission = map.missionDurationSeconds > 0f;
            if (startsMission && !MissionSession.Instance.Prepare(map, resolvedSeed))
            {
                return false;
            }

            if (!startsMission && MissionSession.Instance.IsActive)
            {
                return false;
            }

            GenerationSeed = resolvedSeed;
            if (SceneLoader.Instance.LoadScene(map.sceneName))
            {
                return true;
            }

            if (startsMission)
            {
                MissionSession.Instance.CancelPreparedMission();
            }

            GenerationSeed = previousSeed;
            return false;
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
                missionDurationSeconds = 600f,
                description = "안개와 잿빛 하늘로 뒤덮인 버려진 염전"
            });
            Register(new MapRecord
            {
                mapId = "desert",
                displayName = "침묵의 사막",
                sceneName = "DesertScene",
                missionDurationSeconds = 720f,
                description = "눈부신 모래 언덕 사이로 검은 석조 유적이 드러나는 사막"
            });
            Register(new MapRecord
            {
                mapId = "forest",
                displayName = "안개의 수해",
                sceneName = "ForestScene",
                missionDurationSeconds = 900f,
                description = "이끼 낀 바위와 빽빽한 나무, 차가운 안개로 둘러싸인 숲"
            });
        }
    }
}
