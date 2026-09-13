using System;
using ShinA.Inventory;
using UnityEngine;

namespace ShinA.Player
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsDead => currentHealth <= 0f;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public void TakeDamage(float damage, GameObject source)
        {
            if (damage <= 0f || IsDead)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            if (IsDead)
            {
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Restore(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
