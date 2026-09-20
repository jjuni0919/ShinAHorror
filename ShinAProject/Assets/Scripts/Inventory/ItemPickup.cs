using UnityEngine;

namespace ShinA.Inventory
{
    public sealed class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        private bool collected;
        private Material generatedMaterial;

        public ItemDefinition Item => item;
        public bool IsCollected => collected;

        public static ItemPickup Spawn(ItemDefinition definition, Vector3 position, Quaternion rotation)
        {
            if (definition == null) return null;
            ItemPickup prefab = Resources.Load<ItemPickup>($"Prefabs/Items/ItemPickup_{definition.ItemNumber:000}");
            if (prefab == null)
            {
                Debug.LogError($"아이템 프리팹이 없습니다: {definition.ItemNumber}");
                return null;
            }
            return Instantiate(prefab, position, rotation);
        }

        private void Awake()
        {
            SphereCollider collider = GetComponent<SphereCollider>();
            if (collider == null) collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = false;
            collider.radius = 0.22f;
            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null) body = gameObject.AddComponent<Rigidbody>();
            body.useGravity = true;
            body.isKinematic = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void SetDefinition(ItemDefinition definition)
        {
            item = definition;
        }

        public void Initialize(ItemDefinition definition)
        {
            item = definition;
            if (item == null)
            {
                return;
            }

            name = $"Pickup - {item.ItemName}";
            BuildVisual();
        }

        public bool TryCollect(PlayerInventory inventory)
        {
            if (collected || inventory == null || item == null)
            {
                return false;
            }

            collected = true;
            if (!inventory.TryAdd(item))
            {
                collected = false;
                return false;
            }

            inventory.NotifyItemResponse($"{item.ItemName}을(를) 획득했습니다.");
            gameObject.SetActive(false);
            Destroy(gameObject);
            return true;
        }

        private void BuildVisual()
        {
            SphereCollider pickupCollider = GetComponent<SphereCollider>();
            if (pickupCollider == null)
            {
                pickupCollider = gameObject.AddComponent<SphereCollider>();
            }

            pickupCollider.radius = 0.22f;
            pickupCollider.isTrigger = false;

            Transform existingVisual = transform.Find("Visual");
            if (existingVisual != null)
            {
                Destroy(existingVisual.gameObject);
            }

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
            if (generatedMaterial != null)
            {
                Destroy(generatedMaterial);
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            generatedMaterial = new Material(shader) { color = item.IconColor };
            visual.GetComponent<Renderer>().sharedMaterial = generatedMaterial;
        }

        private void OnDestroy()
        {
            if (generatedMaterial != null)
            {
                Destroy(generatedMaterial);
            }
        }

    }
}
