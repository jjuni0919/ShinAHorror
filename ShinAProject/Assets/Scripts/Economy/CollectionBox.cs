using System;
using System.Collections.Generic;
using ShinA.Inventory;
using ShinA.Managers;
using ShinA.Missions;
using ShinA.Player;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Economy
{
    public sealed class CollectionBox : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField, Min(1), InspectorName("정산 수익 할당량")] private int revenueQuota = 500;
        [SerializeField, Min(0), InspectorName("탐험 성공 1회당 보상")] private int rewardPerExpedition = 100;
        private GameObject body;
        private Material material;
        private PlayerProgress progress;
        public int RevenueQuota => Mathf.Max(1, revenueQuota);
        public int QuestReward => MissionSession.Instance.Progress.CompanyRewardClaimed ? 0 :
            (int)Math.Min(int.MaxValue, (long)MissionSession.Instance.Progress.SuccessfulExpeditions * Mathf.Max(0, rewardPerExpedition));
        public string InteractionPrompt => "선택한 아이템 회수 박스에 맡기기";

        private void Start()
        {
            body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "회수 박스";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            body.transform.localScale = new Vector3(1.5f, 1.3f, 1.5f);
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = new Color(0.8f, 0.48f, 0.08f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            progress = MissionSession.Instance.Progress;
            progress.Changed += RefreshVisibility;
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            PlayerProgress progress = MissionSession.Instance.Progress;
            body.SetActive(progress.IsCompanyDay && !progress.IsGameOver);
        }

        public void Interact(GameObject player)
        {
            PlayerProgress progress = MissionSession.Instance.Progress;
            if (!progress.IsCompanyDay || progress.IsGameOver || SceneLoader.Instance.IsLoading) return;
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;
            ItemDefinition item = inventory.TakeSelected();
            if (item == null)
            {
                inventory.NotifyItemResponse("맡길 아이템을 선택해 주세요. 정산 내역은 태블릿에서 확인할 수 있습니다.");
                return;
            }
            List<int> items = SaveManager.Instance.CurrentData.collectionItemNumbers;
            items.Add(item.ItemNumber);
            if (!SaveManager.Instance.SaveCurrent())
            {
                items.RemoveAt(items.Count - 1);
                inventory.TryAdd(item);
                inventory.NotifyItemResponse("저장 실패로 물품을 맡기지 못했습니다.");
                return;
            }
            inventory.NotifyItemResponse($"{item.ItemName} 보관 · 판매가 {item.SalePrice:N0} · 태블릿에서 정산하세요.");
        }

        public bool TryGetRevenue(out int revenue)
        {
            long total = 0;
            foreach (int number in SaveManager.Instance.CurrentData.collectionItemNumbers)
            {
                ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{number:000}");
                if (item == null)
                {
                    revenue = 0;
                    return false;
                }
                total += item.SalePrice;
            }
            revenue = (int)Math.Min(int.MaxValue, total);
            return true;
        }

        public bool TrySettle(out string message)
        {
            PlayerProgress progress = MissionSession.Instance.Progress;
            if (!progress.IsCompanyDay || progress.IsGameOver || SceneLoader.Instance.IsLoading)
            {
                message = "정산은 5일차마다 대기실에서 한 번 진행할 수 있습니다.";
                return false;
            }
            if (!TryGetRevenue(out int revenue))
            {
                message = "보관 물품 정보를 찾을 수 없어 정산하지 못했습니다.";
                return false;
            }
            SaveData previous = new();
            progress.WriteTo(previous);
            SaveData data = SaveManager.Instance.CurrentData;
            List<int> items = new(data.collectionItemNumbers);
            int questReward = QuestReward;
            progress.SettleRevenue(revenue, RevenueQuota);
            if (!progress.IsGameOver) progress.AddCurrency(questReward);
            data.collectionItemNumbers.Clear();
            if (!SaveManager.Instance.SaveCurrent())
            {
                progress.Restore(previous);
                data.collectionItemNumbers.AddRange(items);
                progress.WriteTo(data);
                message = "저장 실패로 정산이 취소되었습니다. 다시 시도해 주세요.";
                return false;
            }
            message = progress.IsGameOver ? $"정산 수익 {revenue:N0} / 할당량 {RevenueQuota:N0}"
                : $"판매 {revenue:N0} + 퀘스트 {questReward:N0} · {progress.Day}일차 시작";
            if (progress.IsGameOver) GameStateManager.Instance.SetState(GameState.GameOver);
            return true;
        }

        private void OnDestroy()
        {
            if (progress != null) progress.Changed -= RefreshVisibility;
            if (material != null) Destroy(material);
        }
    }
}
