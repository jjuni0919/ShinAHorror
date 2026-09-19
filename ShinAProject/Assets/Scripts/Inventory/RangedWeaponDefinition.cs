using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "RangedWeapon", menuName = "ShinA/Items/Ranged Weapon")]
    public sealed class RangedWeaponDefinition : WeaponItemDefinition
    {
        [SerializeField, Min(0.1f)] private float attackRange = 40f;

        public void ConfigureRange(float range)
        {
            attackRange = Mathf.Max(0.1f, range);
        }

        public override bool Use(ItemUseContext context)
        {
            context.Inventory.PlayWeaponAttack(true);
            DamageFirstHit(context, attackRange, Damage);
            return true;
        }
    }
}
