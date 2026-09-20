using System;
using ShinA.Inventory;
using UnityEngine;

namespace ShinA.Monsters
{
    [RequireComponent(typeof(CharacterController))]
    public class MonsterController : MonoBehaviour, IDamageable
    {
        private const float DetectionInterval = 0.2f;
        private const float TargetReleaseRangeMultiplier = 1.5f;

        [SerializeField] private MonsterDefinition definition;

        private CharacterController characterController;
        private GameObject target;
        private float currentHealth;
        private float attackCooldown;
        private float detectionCooldown;
        private float verticalVelocity;
        private bool isDead;

        public event Action<MonsterController> Removed;

        public MonsterDefinition Definition => definition;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;

        public void SetDefinition(MonsterDefinition monsterDefinition)
        {
            definition = monsterDefinition;
        }

        protected virtual void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (definition == null)
            {
                Debug.LogError("몬스터 정의가 지정되지 않았습니다.", this);
                enabled = false;
                return;
            }

            currentHealth = definition.MaxHealth;
        }

        protected virtual void Update()
        {
            if (isDead || definition == null || Time.timeScale <= 0f)
            {
                return;
            }

            attackCooldown = Mathf.Max(0f, attackCooldown - Time.deltaTime);
            detectionCooldown = Mathf.Max(0f, detectionCooldown - Time.deltaTime);
            UpdateBehavior();
            ApplyGravity();
        }

        protected virtual void UpdateBehavior()
        {
            if (definition.Disposition == MonsterDisposition.Friendly ||
                definition.Disposition == MonsterDisposition.NeutralPassive)
            {
                return;
            }

            if (definition.Disposition == MonsterDisposition.Hostile && detectionCooldown <= 0f)
            {
                detectionCooldown = DetectionInterval;
                UpdateHostileTarget();
            }

            if (target == null)
            {
                return;
            }

            Vector3 offset = target.transform.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > definition.AttackRange)
            {
                MoveTowardsTarget(offset);
                return;
            }

            TryAttackTarget();
        }

        public void TakeDamage(float damage, GameObject source)
        {
            if (damage <= 0f || isDead || definition == null)
            {
                return;
            }

            if (definition.Disposition == MonsterDisposition.NeutralRetaliatory && IsPlayer(source))
            {
                target = source;
            }

            float appliedDamage = Mathf.Max(0f, damage - definition.Defense);
            currentHealth = Mathf.Max(0f, currentHealth - appliedDamage);
            OnDamaged(source, appliedDamage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual void OnDamaged(GameObject source, float appliedDamage)
        {
        }

        protected virtual void OnDied()
        {
        }

        protected void SetTarget(GameObject newTarget)
        {
            target = newTarget;
        }

        protected GameObject GetTarget()
        {
            return target;
        }

        protected virtual void MoveTowardsTarget(Vector3 offset)
        {
            if (offset.sqrMagnitude <= Mathf.Epsilon || definition.MoveSpeed <= 0f)
            {
                return;
            }

            Vector3 direction = offset.normalized;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), 360f * Time.deltaTime);
            characterController.Move(direction * (definition.MoveSpeed * Time.deltaTime));
        }

        protected virtual void TryAttackTarget()
        {
            if (attackCooldown > 0f || target == null || definition.AttackPower <= 0f ||
                !HasLineOfSight(target))
            {
                return;
            }

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                target = null;
                return;
            }

            Vector3 lookDirection = target.transform.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > Mathf.Epsilon)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
            }

            attackCooldown = definition.AttackCooldown;
            damageable.TakeDamage(definition.AttackPower, gameObject);
        }

        private void UpdateHostileTarget()
        {
            if (target != null)
            {
                float releaseRange = definition.DetectionRange * TargetReleaseRangeMultiplier;
                if (Vector3.Distance(transform.position, target.transform.position) <= releaseRange)
                {
                    return;
                }

                target = null;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || Vector3.Distance(transform.position, player.transform.position) > definition.DetectionRange)
            {
                return;
            }

            if (HasLineOfSight(player))
            {
                target = player;
            }
        }

        private bool HasLineOfSight(GameObject candidate)
        {
            Vector3 origin = transform.position + Vector3.up;
            Vector3 destination = candidate.transform.position + Vector3.up;
            Vector3 direction = destination - origin;
            if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, ~0,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return hit.transform == candidate.transform || hit.transform.IsChildOf(candidate.transform);
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            characterController.Move(Vector3.up * (verticalVelocity * Time.deltaTime));
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            SpawnDrops();
            OnDied();
            Destroy(gameObject);
        }

        private void SpawnDrops()
        {
            foreach (MonsterDropEntry drop in definition.Drops)
            {
                if (drop?.Item == null || UnityEngine.Random.value > drop.DropChance)
                {
                    continue;
                }

                int quantity = UnityEngine.Random.Range(drop.MinQuantity, drop.MaxQuantity + 1);
                for (int i = 0; i < quantity; i++)
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.6f;
                    GameObject pickupObject = new($"드랍 아이템 - {drop.Item.ItemName}");
                    pickupObject.transform.position = transform.position + new Vector3(offset.x, 0.35f, offset.y);
                    pickupObject.AddComponent<ItemPickup>().Initialize(drop.Item);
                }
            }
        }

        private static bool IsPlayer(GameObject source)
        {
            return source != null && source.CompareTag("Player");
        }

        private void OnDestroy()
        {
            Removed?.Invoke(this);
            Removed = null;
        }
    }
}
