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
        public int Currency { get; private set; }
        public int MissionFailureCount { get; private set; }
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
            MissionFailureCount = Math.Max(0, data.missionFailureCount);
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
            data.day = Day;
            data.currency = Currency;
            data.missionFailureCount = MissionFailureCount;
            data.warehouseItemNumbers.Clear();
            data.warehouseItemNumbers.AddRange(warehouseItemNumbers);
        }
    }
}
