using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "NoEffectItem", menuName = "ShinA/아이템/효과 없는 아이템")]
    public sealed class NoEffectItemDefinition : ItemDefinition
    {
        public override ItemEffectType EffectType => ItemEffectType.None;

        public override bool Use(ItemUseContext context)
        {
            context.Inventory.NotifyItemResponse($"{ItemName}은(는) 사용할 수 없습니다.");
            return false;
        }
    }
}
