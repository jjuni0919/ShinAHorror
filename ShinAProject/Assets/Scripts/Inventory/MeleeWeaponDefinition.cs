using System;
using UnityEngine;

namespace ShinA.Inventory
{
    [CreateAssetMenu(fileName = "MeleeWeapon", menuName = "ShinA/아이템/근접 무기")]
    public sealed class MeleeWeaponDefinition : WeaponItemDefinition
    {
        [SerializeField, Min(0.1f)] private float attackRange = 2f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.45f;

        public void ConfigureMelee(float range, float radius)
        {
            attackRange = Mathf.Max(0.1f, range);
            hitRadius = Mathf.Max(0.01f, radius);
        }

        public override bool Use(ItemUseContext context)
        {
            context.Inventory.PlayWeaponAttack(false);
            if (context.ViewCamera == null)
            {
                return false;
            }

            Ray ray = new(context.ViewCamera.transform.position, context.ViewCamera.transform.forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, hitRadius, attackRange, ~0,
                QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(context.User.transform))
                {
                    continue;
                }

                hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(Damage, context.User);
                return true;
            }

            return true;
        }
    }
}
