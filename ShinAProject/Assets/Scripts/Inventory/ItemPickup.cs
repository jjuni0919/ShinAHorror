using UnityEngine;

namespace ShinA.Inventory
{
    public sealed class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private float rotationSpeed = 35f;
        [SerializeField] private float bobHeight = 0.12f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;
        private float bobOffset;

        public ItemDefinition Item => item;

        public void SetDefinition(ItemDefinition definition)
        {
            item = definition;
        }

        public void Initialize(ItemDefinition definition)
        {
            item = definition;
            name = $"Pickup - {definition.ItemName}";
            BuildVisual();
        }

        public bool TryCollect(PlayerInventory inventory)
        {
            if (item == null || !inventory.TryAdd(item))
            {
                return false;
            }

            inventory.NotifyItemResponse($"{item.ItemName}을(를) 획득했습니다.");
            Destroy(gameObject);
            return true;
        }

        private void Start()
        {
            startPosition = transform.position;
            bobOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            Vector3 position = startPosition;
            position.y += Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
            transform.position = position;
        }

        private void BuildVisual()
        {
            SphereCollider pickupCollider = gameObject.AddComponent<SphereCollider>();
            pickupCollider.radius = 0.55f;
            pickupCollider.isTrigger = true;

            PrimitiveType primitiveType = item.EffectType == ItemEffectType.Weapon
                ? PrimitiveType.Capsule
                : item.EffectType == ItemEffectType.Interactive ? PrimitiveType.Sphere : PrimitiveType.Cube;
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = "Visual";
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = item.EffectType == ItemEffectType.Weapon
                ? new Vector3(0.12f, 0.42f, 0.12f)
                : Vector3.one * 0.36f;

            Destroy(visual.GetComponent<Collider>());
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            visual.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = item.IconColor };
        }
    }
}
