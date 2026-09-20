using ShinA.Economy;
using UnityEngine;

namespace ShinA.Maps
{
    public sealed class CompanyMapInitializer : MapSceneInitializer
    {
        [SerializeField, Min(0), InspectorName("탐험 성공 1회당 정산 보상")] private int rewardPerExpedition = 100;
        private Material wall;
        private Material counter;

        protected override void InitializeEnvironment()
        {
            RenderSettings.fog = false;
            RenderSettings.ambientLight = new Color(0.7f, 0.7f, 0.75f);
            GameObject building = new("회사 본부");
            wall = CreateMaterial(new Color(0.23f, 0.28f, 0.32f));
            counter = CreateMaterial(new Color(0.16f, 0.45f, 0.4f));
            CreateBlock("바닥", building.transform, new Vector3(0f, -0.25f, 0f), new Vector3(18f, 0.5f, 18f), wall);
            CreateBlock("본부 뒷벽", building.transform, new Vector3(0f, 2f, 8f), new Vector3(18f, 4f, 0.3f), wall);
            CreateBlock("본부 왼쪽벽", building.transform, new Vector3(-9f, 2f, 0f), new Vector3(0.3f, 4f, 18f), wall);
            CreateBlock("본부 오른쪽벽", building.transform, new Vector3(9f, 2f, 0f), new Vector3(0.3f, 4f, 18f), wall);
            CreateBlock("본부 정면벽", building.transform, new Vector3(0f, 2f, -9f), new Vector3(18f, 4f, 0.3f), wall);
            GameObject sale = CreateBlock("물품 매입 창구", building.transform, new Vector3(0f, 0.7f, 4f), new Vector3(3f, 1.4f, 1f), counter);
            sale.AddComponent<CompanyTerminal>().Configure(CompanyTerminalAction.Sell);
            GameObject reward = CreateBlock("퀘스트 정산 창구", building.transform, new Vector3(-4f, 0.7f, 4f), new Vector3(2f, 1.4f, 1f), counter);
            reward.AddComponent<CompanyTerminal>().Configure(CompanyTerminalAction.Reward, rewardPerExpedition);
            GameObject exit = CreateBlock("대기실 복귀", building.transform, new Vector3(4f, 0.7f, 4f), new Vector3(2f, 1.4f, 1f), counter);
            exit.AddComponent<CompanyTerminal>().Configure(CompanyTerminalAction.Return);
            GameObject finish = CreateBlock("다음 탐험 주기", building.transform, new Vector3(4f, 0.7f, -4f), new Vector3(2f, 1.4f, 1f), counter);
            finish.AddComponent<CompanyTerminal>().Configure(CompanyTerminalAction.FinishDay);
            Light light = new GameObject("본부 조명").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void OnDestroy()
        {
            Destroy(wall);
            Destroy(counter);
        }
    }
}
