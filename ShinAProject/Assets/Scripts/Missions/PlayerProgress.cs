using System;
using System.Collections.Generic;
using ShinA.SaveSystem;

namespace ShinA.Missions
{
    public sealed class PlayerProgress
    {
        private readonly List<int> warehouseItemNumbers = new();

        public event Action Changed;

        public int Day { get; private set; } = 1;
        public int Currency { get; private set; } = SaveData.StartingCurrency;
        public bool IsGameOver { get; private set; }
        public int LastSettlementRevenue { get; private set; }
        public int MissionFailureCount { get; private set; }
        public bool IsCompanyDay => Day % 5 == 0;
        public int SuccessfulExpeditions { get; private set; }
        public bool CompanyRewardClaimed { get; private set; }
        public IReadOnlyList<int> WarehouseItemNumbers => warehouseItemNumbers;

        public void AddCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Currency = (int)Math.Min(int.MaxValue, (long)Currency + amount);
            Changed?.Invoke();
        }

        public bool TrySpendCurrency(int amount)
        {
            if (amount < 0 || Currency < amount)
            {
                return false;
            }

            if (amount == 0)
            {
                return true;
            }

            Currency -= amount;
            Changed?.Invoke();
            return true;
        }

        public bool TryRemoveWarehouseItem(int itemNumber)
        {
            bool removed = warehouseItemNumbers.Remove(itemNumber);
            if (removed)
            {
                Changed?.Invoke();
            }

            return removed;
        }

        internal void Restore(SaveData data)
        {
            Day = Math.Max(1, data.day);
            Currency = Math.Max(0, data.currency);
            IsGameOver = data.gameOver;
            LastSettlementRevenue = Math.Max(0, data.lastSettlementRevenue);
            MissionFailureCount = Math.Max(0, data.missionFailureCount);
            SuccessfulExpeditions = Math.Max(0, data.successfulExpeditions);
            CompanyRewardClaimed = data.companyRewardClaimed;
            warehouseItemNumbers.Clear();
            if (data.warehouseItemNumbers != null)
            {
                warehouseItemNumbers.AddRange(data.warehouseItemNumbers);
            }

            Changed?.Invoke();
        }

        internal void CompleteDay(IEnumerable<int> storedItems, int currencyReward)
        {
            warehouseItemNumbers.AddRange(storedItems);
            SuccessfulExpeditions++;
            Currency = (int)Math.Min(int.MaxValue, (long)Currency + Math.Max(0, currencyReward));
            Day++;
            Changed?.Invoke();
        }

        internal void FailDay(int currencyPenalty)
        {
            Currency = Math.Max(0, Currency - Math.Max(0, currencyPenalty));
            MissionFailureCount++;
            Day++;
            Changed?.Invoke();
        }

        internal void WriteTo(SaveData data)
        {
            data.successfulExpeditions = SuccessfulExpeditions;
            data.companyRewardClaimed = CompanyRewardClaimed;
            data.day = Day;
            data.gameOver = IsGameOver;
            data.lastSettlementRevenue = LastSettlementRevenue;
            data.currency = Currency;
            data.missionFailureCount = MissionFailureCount;
            data.warehouseItemNumbers.Clear();
            data.warehouseItemNumbers.AddRange(warehouseItemNumbers);
        }

        public bool SettleRevenue(int revenue, int quota)
        {
            if (!IsCompanyDay || IsGameOver || revenue < 0 || quota <= 0) return false;
            LastSettlementRevenue = revenue;
            Currency = (int)Math.Min(int.MaxValue, (long)Currency + revenue);
            IsGameOver = revenue < quota;
            if (!IsGameOver)
            {
                Day++;
                SuccessfulExpeditions = 0;
                CompanyRewardClaimed = false;
            }
            Changed?.Invoke();
            return true;
        }
    }
}
