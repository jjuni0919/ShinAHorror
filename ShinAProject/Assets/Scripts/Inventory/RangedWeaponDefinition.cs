using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "RangedWeapon", menuName = "ShinA/Items/Ranged Weapon")]
    public sealed class RangedWeaponDefinition : WeaponItemDefinition
    {
        [SerializeField, Min(0.1f)] private float attackRange = 40f;

        public void ConfigureRange(float range)
        {
            attackRange = range;
        }

        public override bool Use(ItemUseContext context)
        {
            base.Use(context);
            DamageFirstHit(context, attackRange, Damage);
            return true;
        }
    }
}
