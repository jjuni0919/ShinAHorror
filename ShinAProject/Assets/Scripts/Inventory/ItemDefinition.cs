using ShinA.Player;
using UnityEngine;

namespace ShinA.Inventory
{
    public enum ItemEffectType
    {
        None,
        Interactive,
        Weapon
    }

    public readonly struct ItemUseContext
    {
        public ItemUseContext(GameObject user, Camera viewCamera, PlayerInventory inventory)
        {
            User = user;
            ViewCamera = viewCamera;
            Inventory = inventory;
        }

        public GameObject User { get; }
        public Camera ViewCamera { get; }
        public PlayerInventory Inventory { get; }
    }

    public abstract class ItemDefinition : ScriptableObject
    {
        [SerializeField] private int itemNumber;
        [SerializeField] private string itemName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color iconColor = Color.white;
        [SerializeField] private GameObject equippedPrefab;

        public int ItemNumber => itemNumber;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public Color IconColor => iconColor;
        public GameObject EquippedPrefab => equippedPrefab;
        public abstract ItemEffectType EffectType { get; }

        public virtual bool Use(ItemUseContext context)
        {
            Debug.Log($"Used item: {itemName} ({itemNumber})", context.User);
            return true;
        }

        public void ConfigureSample(int number, string displayName, string itemDescription, Color color)
        {
            itemNumber = number;
            itemName = displayName;
            description = itemDescription;
            iconColor = color;
            name = $"Item_{number:000}_{displayName}";
        }

        public void ConfigureAssets(Sprite itemIcon, GameObject itemEquippedPrefab)
        {
            icon = itemIcon;
            equippedPrefab = itemEquippedPrefab;
        }
    }
}
