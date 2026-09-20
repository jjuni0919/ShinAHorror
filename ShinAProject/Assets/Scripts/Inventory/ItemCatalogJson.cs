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
            ItemDefinition[] items = Resources.LoadAll<ItemDefinition>("Items");
            Array.Sort(items, (left, right) => left.ItemNumber.CompareTo(right.ItemNumber));
            return items;
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
            HashSet<int> itemNumbers = new();
            foreach (ItemRecord record in catalog.items)
            {
                if (record == null || record.itemNumber <= 0 || string.IsNullOrWhiteSpace(record.itemName) ||
                    !itemNumbers.Add(record.itemNumber))
                {
                    Debug.LogError("아이템 카탈로그에 null, 중복 또는 불완전한 레코드가 있습니다.");
                    continue;
                }

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
            else if (record.effectType == ItemEffectType.Weapon)
            {
                Debug.LogError($"아이템 {record.itemNumber}에 유효한 무기 공격 유형이 없습니다.");
                return null;
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
