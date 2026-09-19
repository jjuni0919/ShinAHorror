using ShinA.Inventory;
using UnityEngine;

namespace ShinA.Maps
{
    public sealed class DamageableTarget : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float health = 60f;

        public void TakeDamage(float damage, GameObject source)
        {
            if (damage <= 0f || health <= 0f)
            {
                return;
            }

            health -= damage;
            transform.localScale *= 0.94f;
            if (health <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
