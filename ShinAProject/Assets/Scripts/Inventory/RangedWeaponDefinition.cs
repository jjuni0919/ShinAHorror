using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "RangedWeapon", menuName = "ShinA/아이템/원거리 무기")]
    public sealed class RangedWeaponDefinition : WeaponItemDefinition
    {
        [SerializeField, Min(0.1f)] private float attackRange = 40f;
        [SerializeField, Min(1)] private int magazineSize = 12;
        [SerializeField, Min(0f)] private float reloadDuration = 1.5f;

        public int MagazineSize => magazineSize;
        public float ReloadDuration => reloadDuration;

        public void ConfigureRange(float range)
        {
            attackRange = Mathf.Max(0.1f, range);
        }

        public void ConfigureMagazine(int capacity, float reloadSeconds)
        {
            magazineSize = Mathf.Max(1, capacity);
            reloadDuration = Mathf.Max(0f, reloadSeconds);
        }

        public override bool Use(ItemUseContext context)
        {
            context.Inventory.PlayWeaponAttack(true);
            DamageFirstHit(context, attackRange, Damage);
            return true;
        }
    }
}
