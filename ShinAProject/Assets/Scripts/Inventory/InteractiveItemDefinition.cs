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
            base.Use(context);
            Debug.Log(string.IsNullOrWhiteSpace(responseMessage) ? $"{ItemName} reacts." : responseMessage,
                context.User);
            context.Inventory.NotifyItemResponse(responseMessage);
            return true;
        }
    }
}
