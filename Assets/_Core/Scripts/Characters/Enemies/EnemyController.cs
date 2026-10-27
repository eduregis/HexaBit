using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class EnemyController : DamageableCharacter {
        [Header("Data")]
        [SerializeField] private EnemyData data;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool hasDeathAnimation = true;

        private bool isDead = false;
        private Transform player;
        private Rigidbody2D rb;

        [Header("Attack")]
        [SerializeField] private float attackCooldown = 0.5f;
        private float attackTimer = 0f;

        // Sub-Boss Variables
        private int currentDamage;          // Local damage value (so we don't modify the ScriptableObject)
        private StatusData contactStatus;   // Status to apply on contact (for sub-bosses)

        [Header("Events")]
        public UnityEvent OnDeath;

        public EnemyData Data => data;
        public bool IsDead => isDead;

        private HeroController heroController;

        // Knockback state
        private float knockbackTimer = 0f;
        private const float knockbackDuration = 0.2f;

        protected override void Awake() {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) Debug.LogError("EnemyController: Rigidbody2D not found!");
            if (animator == null) animator = GetComponent<Animator>();
        }

        public void Initialize(EnemyData enemyData, int wave, StatusData statusToApply = null) {
            data = enemyData;
            contactStatus = statusToApply; // Store the status (will be null for normal enemies)

            int scaledHealth = data.baseHealth;
            if (!data.healthScalesWithPlayerLevel) {
                scaledHealth += Mathf.RoundToInt(data.baseHealth * 0.1f * wave);
            }

            if (data.isBoss)
                scaledHealth *= 3;

            SetHealth(scaledHealth, 0);

            baseSpeed = data.moveSpeed;
            if (data.isBoss)
                baseSpeed *= 1.2f;
            else
                baseSpeed += data.moveSpeed * 0.05f * wave;

            CurrentSpeed = baseSpeed;

            // Set the base damage
            currentDamage = data.damage;

            isDead = false;
            attackTimer = 0f;
            knockbackTimer = 0f;

            // Check if animator has a controller before playing animations
            if (animator != null && animator.runtimeAnimatorController != null) {
                animator.SetBool("IsDead", false);
                animator.SetTrigger("Respawn");
            }
        }

        /// <summary>
        /// Multiplies the enemy's stats (used by SpawnerController to create sub-bosses).
        /// </summary>
        public void MultiplyStats(float healthMultiplier, float damageMultiplier) {
            // Multiply health and heal to full
            maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
            currentHealth = maxHealth;

            // Multiply damage
            currentDamage = Mathf.RoundToInt(currentDamage * damageMultiplier);

            // Optional: You could also notify the UI or visuals here if needed
            Debug.Log($"[Enemy] Sub-Boss stats applied. HP: {maxHealth}, Damage: {currentDamage}");
        }

        public void SetPlayerReference(HeroController hero) {
            heroController = hero;
            player = hero != null ? hero.transform : null;
        }

        /// <summary>
        /// Applies a knockback force to the enemy and briefly prevents movement override.
        /// </summary>
        public void ApplyKnockback(float force, Vector2 direction) {
            if (isDead) return;
            if (rb == null) return;

            // Reset velocity and wake up
            rb.linearVelocity = Vector2.zero;
            rb.WakeUp();

            // Apply force
            rb.AddForce(direction * force, ForceMode2D.Impulse);

            // Activate knockback timer to prevent movement override
            knockbackTimer = knockbackDuration;
        }

        private void OnEnable() {
            if (data != null) {
                SetHealth(data.baseHealth, 0);
                baseSpeed = data.moveSpeed;
                CurrentSpeed = baseSpeed;
                currentDamage = data.damage;
            } else {
                SetHealth(maxHealth, armor);
                baseSpeed = 3f;
                CurrentSpeed = baseSpeed;
                currentDamage = 5;
            }

            isDead = false;
            attackTimer = 0f;
            knockbackTimer = 0f;

            // Check if animator has a controller before playing animations
            if (animator != null && animator.runtimeAnimatorController != null) {
                animator.SetBool("IsDead", false);
                animator.SetTrigger("Respawn");
            }

            ClearAllStatuses();
        }

        private void Update() {
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            UpdateStatuses();
        }

        private void FixedUpdate() {
            if (isDead) return;
            if (heroController == null || heroController.IsDead) return;

            // If knockback is active, don't override velocity
            if (knockbackTimer > 0f) {
                knockbackTimer -= Time.fixedDeltaTime;
                return; // Skip normal movement while knockback is active
            }

            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * CurrentSpeed;

            if (direction.x != 0) {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            if (animator != null) {
                animator.SetFloat("Speed", rb.linearVelocity.magnitude);
                animator.SetFloat("DirectionX", direction.x);
            }
        }

        // Contact damage
        private void OnTriggerStay2D(Collider2D other) {
            if (isDead || attackTimer > 0f) return;
            if (heroController == null || heroController.IsDead) return;

            if (other.CompareTag("Player")) {
                HeroController hero = other.GetComponent<HeroController>();
                if (hero != null) {
                    // Apply base damage
                    hero.TakeDamage(currentDamage);

                    // Apply status effect if this is a sub-boss with a status
                    if (contactStatus != null) {
                        hero.ApplyStatus(contactStatus);
                    }

                    attackTimer = attackCooldown;
                    Debug.Log($"Enemy dealt {currentDamage} damage to hero. Status: {(contactStatus != null ? contactStatus.statusName : "None")}");
                }
            }
        }

        protected override void AnimateTakingDamage() {
            if (animator == null) return;
            // Adding a damaged animation to enemies
            // animator.SetBool("Damaged", damaged);
        }

        protected override void Die() {
            if (isDead) return;
            isDead = true;

            ClearAllStatuses();
            OnDeath?.Invoke();

            if (hasDeathAnimation && animator != null && animator.HasState(0, Animator.StringToHash("Death"))) {
                animator.SetBool("IsDead", true);
                animator.SetTrigger("Die");
                Destroy(gameObject, 0.5f);
            } else {
                Destroy(gameObject);
            }
        }

        private void OnDisable() {
            OnDeath.RemoveAllListeners();
        }

        public void OnDeathAnimationEnd() {
            Destroy(gameObject);
        }
    }
}