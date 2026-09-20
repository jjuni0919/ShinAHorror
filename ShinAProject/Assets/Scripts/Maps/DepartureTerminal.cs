using ShinA.Inventory;
using ShinA.Missions;
using ShinA.Player;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Maps
{
    public sealed class DepartureTerminal : MonoBehaviour, IPlayerInteractable
    {
        public string InteractionPrompt => MissionSession.Instance.Progress.IsCompanyDay
            ? "태블릿에서 수익 정산 필요" : "선택한 탐험지로 출발";

        public void Interact(GameObject player)
        {
            if (MissionSession.Instance.Progress.IsCompanyDay || MissionSession.Instance.Progress.IsGameOver)
            {
                player.GetComponent<PlayerInventory>()?.NotifyItemResponse("회수 박스에 물품을 맡기고 태블릿에서 정산해 주세요.");
                return;
            }
            string mapId = SaveManager.Instance.CurrentData.selectedMapId;
            if (!MapDatabase.Instance.TravelToMap(mapId))
                player.GetComponent<PlayerInventory>()?.NotifyItemResponse("태블릿에서 방문할 장소를 선택해 주세요.");
        }
    }
}
