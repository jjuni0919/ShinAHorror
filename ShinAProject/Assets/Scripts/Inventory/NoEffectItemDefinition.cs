using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "NoEffectItem", menuName = "ShinA/Items/No Effect Item")]
    public sealed class NoEffectItemDefinition : ItemDefinition
    {
        public override ItemEffectType EffectType => ItemEffectType.None;

        public override bool Use(ItemUseContext context)
        {
            base.Use(context);
            Debug.Log($"{ItemName} has no usable effect.", context.User);
            return false;
        }
    }
}
