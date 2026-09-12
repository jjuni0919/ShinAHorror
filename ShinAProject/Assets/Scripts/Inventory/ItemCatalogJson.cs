using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShinA.Inventory
{
    public enum WeaponAttackType
    {
        None,
        Melee,
        Ranged
    }

    [Serializable]
    public sealed class ItemCatalogFile
    {
        public int schemaVersion = 1;
        public List<ItemRecord> items = new();
    }

    [Serializable]
    public sealed class ItemRecord
    {
        public int itemNumber;
        public string itemName;
        public string description;
        public ItemEffectType effectType;
        public WeaponAttackType weaponType;
        public string responseMessage;
        public float damage;
        public float cooldown = 0.5f;
        public float range = 2f;
        public float hitRadius = 0.45f;
        public Color iconColor = Color.white;
        public string iconResourcePath;
        public string equippedPrefabResourcePath;
    }

    public static class ItemDatabase
    {
        public static IReadOnlyList<ItemDefinition> LoadResourceItems()
        {
            return Resources.LoadAll<ItemDefinition>("Items");
        }

        public static IReadOnlyList<ItemDefinition> CreateFromJson(TextAsset jsonAsset)
        {
            if (jsonAsset == null)
            {
                return Array.Empty<ItemDefinition>();
            }

            ItemCatalogFile catalog = JsonUtility.FromJson<ItemCatalogFile>(jsonAsset.text);
            if (catalog?.items == null)
            {
                return Array.Empty<ItemDefinition>();
            }

            List<ItemDefinition> definitions = new(catalog.items.Count);
            foreach (ItemRecord record in catalog.items)
            {
                ItemDefinition definition = CreateDefinition(record);
                if (definition != null)
                {
                    definitions.Add(definition);
                }
            }

            return definitions;
        }

        private static ItemDefinition CreateDefinition(ItemRecord record)
        {
            ItemDefinition definition;
            if (record.effectType == ItemEffectType.Weapon && record.weaponType == WeaponAttackType.Melee)
            {
                MeleeWeaponDefinition melee = ScriptableObject.CreateInstance<MeleeWeaponDefinition>();
                melee.ConfigureWeapon(record.damage, record.cooldown);
                melee.ConfigureMelee(record.range, record.hitRadius);
                definition = melee;
            }
            else if (record.effectType == ItemEffectType.Weapon && record.weaponType == WeaponAttackType.Ranged)
            {
                RangedWeaponDefinition ranged = ScriptableObject.CreateInstance<RangedWeaponDefinition>();
                ranged.ConfigureWeapon(record.damage, record.cooldown);
                ranged.ConfigureRange(record.range);
                definition = ranged;
            }
            else if (record.effectType == ItemEffectType.Interactive)
            {
                InteractiveItemDefinition interactive = ScriptableObject.CreateInstance<InteractiveItemDefinition>();
                interactive.ConfigureResponse(record.responseMessage);
                definition = interactive;
            }
            else
            {
                definition = ScriptableObject.CreateInstance<NoEffectItemDefinition>();
            }

            definition.ConfigureSample(record.itemNumber, record.itemName, record.description, record.iconColor);
            Sprite icon = string.IsNullOrWhiteSpace(record.iconResourcePath)
                ? null
                : Resources.Load<Sprite>(record.iconResourcePath);
            GameObject equippedPrefab = string.IsNullOrWhiteSpace(record.equippedPrefabResourcePath)
                ? null
                : Resources.Load<GameObject>(record.equippedPrefabResourcePath);
            definition.ConfigureAssets(icon, equippedPrefab);
            return definition;
        }
    }
}
