using System;
using System.Collections.Generic;
using ShinA.Inventory;
using ShinA.Managers;
using ShinA.Maps;
using ShinA.Player;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Missions
{
    public enum MissionOutcome
    {
        None,
        Completed,
        TimeExpired,
        PlayerDied
    }

    public sealed class MissionSession : MonoBehaviour
    {
        private const string WaitingSceneName = "WaitingScene";
        private static MissionSession instance;

        [SerializeField, Min(0)] private int failureCurrencyPenalty = 100;

        private readonly List<int> fieldStorageItemNumbers = new();
        private PlayerHealth playerHealth;
        private string activeMapId;
        private string activeMapName;
        private string activeSceneName;
        private int activeSeed;
        private int pendingCurrency;
        private float remainingTime;
        private float accumulatedPlayTime;
        private bool running;
        private bool ending;

        public static MissionSession Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MissionSession>();
                    if (instance == null)
                    {
                        instance = new GameObject("MissionSession").AddComponent<MissionSession>();
                    }
                }

                return instance;
            }
        }

        public PlayerProgress Progress { get; } = new();
        public bool IsActive => !string.IsNullOrEmpty(activeMapId);
        public bool IsRunning => running;
        public string ActiveMapName => activeMapName;
        public float RemainingTime => Mathf.Max(0f, remainingTime);
        public int FieldStorageCount => fieldStorageItemNumbers.Count;
        public IReadOnlyList<int> FieldStorageItemNumbers => fieldStorageItemNumbers;
        public int PendingCurrency => pendingCurrency;
        public MissionOutcome LastOutcome { get; private set; }

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

            SaveManager saveManager = SaveManager.Instance;
            SaveData data = saveManager.Load(out SaveData loadedData)
                ? loadedData
                : saveManager.CreateNewData();
            RestoreProgress(data);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            accumulatedPlayTime += Time.deltaTime;
            if (!running || ending)
            {
                return;
            }

            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                EndMission(MissionOutcome.TimeExpired);
            }
        }

        public bool Prepare(MapRecord map, int seed)
        {
            if (map == null || map.missionDurationSeconds <= 0f || IsActive || Progress.IsCompanyDay || Progress.IsGameOver)
            {
                return false;
            }

            activeMapId = map.mapId;
            activeMapName = map.displayName;
            activeSceneName = map.sceneName;
            activeSeed = seed;
            remainingTime = map.missionDurationSeconds;
            fieldStorageItemNumbers.Clear();
            pendingCurrency = 0;
            running = false;
            ending = false;
            LastOutcome = MissionOutcome.None;
            SaveData data = SaveManager.Instance.CurrentData;
            if (data != null && data.activeMissionMapId == map.mapId && data.activeMissionSeed == seed)
            {
                remainingTime = Mathf.Max(0f, data.missionRemainingTime);
                pendingCurrency = data.missionCurrency;
                fieldStorageItemNumbers.AddRange(data.fieldStorageItems);
            }
            return true;
        }

        public bool EnsurePrepared(MapRecord map, int seed)
        {
            return IsActive
                ? string.Equals(activeSceneName, map.sceneName, StringComparison.Ordinal)
                : Prepare(map, seed);
        }

        public void CancelPreparedMission()
        {
            if (running)
            {
                return;
            }

            ClearMission();
        }

        public void BindPlayer(GameObject player)
        {
            if (!IsActive || player == null)
            {
                return;
            }

            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
            }

            running = true;
        }

        public int StoreInventory(PlayerInventory inventory)
        {
            if (!IsActive || ending || inventory == null)
            {
                return 0;
            }

            List<ItemDefinition> storedItems = inventory.TakeAllItems();
            foreach (ItemDefinition item in storedItems)
            {
                fieldStorageItemNumbers.Add(item.ItemNumber);
            }

            return storedItems.Count;
        }

        public void AddCurrencyReward(int amount)
        {
            if (amount <= 0 || ending)
            {
                return;
            }

            if (!IsActive)
            {
                Progress.AddCurrency(amount);
                return;
            }

            pendingCurrency = (int)Math.Min(int.MaxValue, (long)pendingCurrency + amount);
        }

        public bool CompleteMission()
        {
            if (!IsActive || ending)
            {
                return false;
            }

            EndMission(MissionOutcome.Completed);
            return true;
        }

        internal void RestoreProgress(SaveData data)
        {
            if (data == null || IsActive)
            {
                return;
            }

            Progress.Restore(data);
            accumulatedPlayTime = Mathf.Max(0f, data.playTimeSeconds);
            LastOutcome = ParseOutcome(data.lastMissionResult);
        }

        internal void WriteTo(SaveData data)
        {
            Progress.WriteTo(data);
            data.playTimeSeconds = accumulatedPlayTime;
            data.activeMissionMapId = activeMapId;
            data.activeMissionSeed = activeSeed;
            data.missionRemainingTime = remainingTime;
            data.missionCurrency = pendingCurrency;
            data.fieldStorageItems.Clear();
            data.fieldStorageItems.AddRange(fieldStorageItemNumbers);
        }

        private void OnPlayerDied()
        {
            EndMission(MissionOutcome.PlayerDied);
        }

        private void EndMission(MissionOutcome outcome)
        {
            if (!IsActive || ending)
            {
                return;
            }

            ending = true;
            running = false;
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
                playerHealth = null;
            }

            if (outcome == MissionOutcome.Completed)
            {
                Progress.CompleteDay(fieldStorageItemNumbers, pendingCurrency);
            }
            else
            {
                Progress.FailDay(failureCurrencyPenalty);
                FindFirstObjectByType<PlayerInventory>()?.TakeAllItems();
            }

            LastOutcome = outcome;
            SaveData data = SaveManager.Instance.CurrentData ?? SaveManager.Instance.CreateNewData();
            data.currentScene = WaitingSceneName;
            data.playTimeSeconds = accumulatedPlayTime;
            data.lastMissionMapId = activeMapId;
            data.lastMissionResult = outcome.ToString();
            data.lastMissionSeed = activeSeed;
            data.player.position = Vector3.zero;
            data.player.eulerAngles = Vector3.zero;
            if (data.pendingOrders.Count > 0) data.deliveryBoxOpened = false;
            data.deliveredOrders.AddRange(data.pendingOrders);
            data.pendingOrders.Clear();
            data.selectedMapId = null;
            ClearMission();
            if (!SceneLoader.Instance.LoadScene(WaitingSceneName))
            {
                ending = false;
                Debug.LogError("임무가 종료되었지만 대기실 씬을 불러오지 못했습니다.", this);
            }
        }

        private void ClearMission()
        {
            activeMapId = null;
            activeMapName = null;
            activeSceneName = null;
            remainingTime = 0f;
            pendingCurrency = 0;
            fieldStorageItemNumbers.Clear();
            running = false;
        }

        private static MissionOutcome ParseOutcome(string value)
        {
            return Enum.TryParse(value, out MissionOutcome outcome) ? outcome : MissionOutcome.None;
        }
    }
}
