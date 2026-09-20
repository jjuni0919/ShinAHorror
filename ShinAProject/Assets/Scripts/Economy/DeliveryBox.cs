using ShinA.Inventory;
using ShinA.Player;
using ShinA.SaveSystem;
using UnityEngine;

namespace ShinA.Economy
{
    public sealed class DeliveryBox : MonoBehaviour, IPlayerInteractable
    {
        private GameObject lid;
        private bool opened;
        private Material material;
        public string InteractionPrompt => opened ? "택배 수령 완료" : "택배상자 열기";

        public static void Create(Vector3 position, bool opened = false)
        {
            GameObject root = new("택배상자");
            root.transform.position = position;
            DeliveryBox box = root.AddComponent<DeliveryBox>();
            box.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            box.material.color = new Color(0.5f, 0.31f, 0.12f);
            box.AddPart("바닥", new Vector3(0f, 0.05f, 0f), new Vector3(1.15f, 0.1f, 1.15f));
            box.AddPart("왼쪽 벽", new Vector3(-0.55f, 0.55f, 0f), new Vector3(0.05f, 1f, 1.15f));
            box.AddPart("오른쪽 벽", new Vector3(0.55f, 0.55f, 0f), new Vector3(0.05f, 1f, 1.15f));
            box.AddPart("앞쪽 벽", new Vector3(0f, 0.55f, -0.55f), new Vector3(1.15f, 1f, 0.05f));
            box.AddPart("뒤쪽 벽", new Vector3(0f, 0.55f, 0.55f), new Vector3(1.15f, 1f, 0.05f));
            box.lid = box.AddPart("뚜껑", new Vector3(0f, 1.075f, 0f), new Vector3(1.15f, 0.05f, 1.15f));
            box.opened = opened;
            box.lid.SetActive(!opened);
        }

        private GameObject AddPart(string partName, Vector3 position, Vector3 scale)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = partName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        public void Interact(GameObject player)
        {
            if (opened) return;
            SaveData data = SaveManager.Instance.CurrentData;
            int slot = 0;
            for (int i = data.deliveredOrders.Count - 1; i >= 0; i--)
            {
                ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{data.deliveredOrders[i]:000}");
                Vector3 offset = new((slot % 2 == 0 ? -1 : 1) * 0.25f,
                    0.34f + (slot / 4) * 0.46f, ((slot / 2) % 2 == 0 ? -1 : 1) * 0.25f);
                if (ItemPickup.Spawn(item, transform.position + offset, Quaternion.identity) == null) continue;
                data.deliveredOrders.RemoveAt(i);
                slot++;
            }
            opened = data.deliveredOrders.Count == 0;
            data.deliveryBoxOpened = opened;
            lid.SetActive(false);
            SaveManager.Instance.SaveCurrent();
            player.GetComponent<PlayerInventory>()?.NotifyItemResponse("상자 안의 물건을 직접 집어 주세요.");
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
