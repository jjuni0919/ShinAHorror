using UnityEngine;

namespace ShinA.Inventory
{
    public interface IDamageable
    {
        void TakeDamage(float damage, GameObject source);
    }

    public abstract class WeaponItemDefinition : ItemDefinition
    {
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.01f)] private float useCooldown = 0.5f;

        public override ItemEffectType EffectType => ItemEffectType.Weapon;
        public float Damage => damage;
        public float UseCooldown => useCooldown;

        public void ConfigureWeapon(float weaponDamage, float cooldown)
        {
            damage = Mathf.Max(0f, weaponDamage);
            useCooldown = Mathf.Max(0.01f, cooldown);
        }

        protected static bool DamageFirstHit(ItemUseContext context, float range, float damage)
        {
            if (context.ViewCamera == null)
            {
                return false;
            }

            Ray ray = new(context.ViewCamera.transform.position, context.ViewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(damage, context.User);
            return damageable != null;
        }
    }
}
