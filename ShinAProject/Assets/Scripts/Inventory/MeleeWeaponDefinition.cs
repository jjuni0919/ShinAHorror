using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "MeleeWeapon", menuName = "ShinA/Items/Melee Weapon")]
    public sealed class MeleeWeaponDefinition : WeaponItemDefinition
    {
        [SerializeField, Min(0.1f)] private float attackRange = 2f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.45f;

        public void ConfigureMelee(float range, float radius)
        {
            attackRange = range;
            hitRadius = radius;
        }

        public override bool Use(ItemUseContext context)
        {
            base.Use(context);
            context.Inventory.PlayWeaponAttack(false);
            if (context.ViewCamera == null)
            {
                return false;
            }

            Ray ray = new(context.ViewCamera.transform.position, context.ViewCamera.transform.forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, hitRadius, attackRange, ~0,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(context.User.transform))
                {
                    continue;
                }

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                damageable.TakeDamage(Damage, context.User);
                return true;
            }

            return true;
        }
    }
}
