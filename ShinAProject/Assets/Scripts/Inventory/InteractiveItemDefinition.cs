using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "InteractiveItem", menuName = "ShinA/Items/Interactive Item")]
    public sealed class InteractiveItemDefinition : ItemDefinition
    {
        [SerializeField, TextArea] private string responseMessage = "The item reacts.";

        public override ItemEffectType EffectType => ItemEffectType.Interactive;

        public void ConfigureResponse(string message)
        {
            responseMessage = message;
        }

        public override bool Use(ItemUseContext context)
        {
            string message = string.IsNullOrWhiteSpace(responseMessage)
                ? $"{ItemName}이(가) 반응한다."
                : responseMessage;
            context.Inventory.NotifyItemResponse(message);
            return true;
        }
    }
}
