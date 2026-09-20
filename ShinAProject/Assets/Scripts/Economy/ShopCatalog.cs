using System;
using System.Collections.Generic;
using ShinA.Inventory;
using ShinA.Missions;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Economy
{
    [Serializable]
    public sealed class ShopOffer
    {
        [InspectorName("아이템")] public ItemDefinition item;
        [Min(0), InspectorName("구매 가격")] public int price = 50;
        [Range(0f, 1f), InspectorName("일일 판매 확률 (1 = 상시)")] public float availability = 1f;
    }

    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "ShinA/상점 상품 목록")]
    public sealed class ShopCatalog : ScriptableObject
    {
        public const int DeliveryCapacity = 8;
        [SerializeField, InspectorName("상품 목록")] private List<ShopOffer> offers = new();
        public IReadOnlyList<ShopOffer> Offers => offers;

        public bool IsAvailable(int index, int day)
        {
            if (index < 0 || index >= offers.Count || offers[index].item == null) return false;
            return new System.Random(unchecked(day * 7919 + index * 397)).NextDouble() < offers[index].availability;
        }

        public bool TryBuy(int index, out string message)
        {
            PlayerProgress progress = MissionSession.Instance.Progress;
            SaveData data = SaveManager.Instance.CurrentData;
            if (!IsAvailable(index, progress.Day))
            {
                message = "오늘 판매하지 않는 상품입니다.";
                return false;
            }
            if (data.pendingOrders.Count + data.deliveredOrders.Count >= DeliveryCapacity)
            {
                message = "배송 대기 공간이 가득 찼습니다. 택배를 먼저 수령하세요.";
                return false;
            }
            ShopOffer offer = offers[index];
            if (Resources.Load<ItemPickup>($"Prefabs/Items/ItemPickup_{offer.item.ItemNumber:000}") == null)
            {
                message = "배송용 아이템 프리팹이 없어 구매할 수 없습니다.";
                return false;
            }
            if (!progress.TrySpendCurrency(offer.price))
            {
                message = "보유 금액이 부족합니다.";
                return false;
            }
            data.pendingOrders.Add(offer.item.ItemNumber);
            if (!SaveManager.Instance.SaveCurrent())
            {
                data.pendingOrders.RemoveAt(data.pendingOrders.Count - 1);
                progress.AddCurrency(offer.price);
                message = "주문 저장에 실패하여 구매를 취소했습니다.";
                return false;
            }
            message = $"{offer.item.ItemName} 주문 완료 · 다음 탐험 종료 후 배송";
            return true;
        }
    }
}
