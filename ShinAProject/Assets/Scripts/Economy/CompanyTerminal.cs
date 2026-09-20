using ShinA.Inventory;
using ShinA.Managers;
using ShinA.Missions;
using ShinA.Player;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Economy
{
    public enum CompanyTerminalAction
    {
        [InspectorName("선택 아이템 판매")] Sell,
        [InspectorName("퀘스트 보상 수령")] Reward,
        [InspectorName("대기실 복귀")] Return,
        [InspectorName("회사 방문일 종료")] FinishDay
    }

    public sealed class CompanyTerminal : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private CompanyTerminalAction action;
        [SerializeField, Min(0), InspectorName("탐험 성공 1회당 보상")] private int rewardPerExpedition = 100;
        public void Configure(CompanyTerminalAction terminalAction, int expeditionReward = 100)
        {
            action = terminalAction;
            rewardPerExpedition = Mathf.Max(0, expeditionReward);
        }
        public string InteractionPrompt => action switch
        {
            CompanyTerminalAction.Sell => "판매는 대기실 회수 박스 이용",
            CompanyTerminalAction.Reward => "보상은 대기실 태블릿 정산 이용",
            _ => "대기실로 복귀"
        };

        public void Interact(GameObject player)
        {
            if (SceneLoader.Instance.IsLoading || MissionSession.Instance.Progress.IsGameOver) return;
            if (action == CompanyTerminalAction.Return || action == CompanyTerminalAction.FinishDay)
            {
                SceneLoader.Instance.LoadScene("WaitingScene");
                return;
            }
            player.GetComponent<PlayerInventory>()?.NotifyItemResponse("판매와 수익 정산은 대기실 회수 박스와 태블릿을 이용해 주세요.");
        }
    }
}
