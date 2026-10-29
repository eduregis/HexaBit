using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace HexaBit.Core {
    public class EnemyController : DamageableCharacter {
        [Header("Data")]
        [SerializeField] private EnemyData data;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool hasDeathAnimation = true;

        [Header("Visual Effects (Shader)")]
        private Material runtimeMaterial;
        private Coroutine flashCoroutine;

        // IDs do Shader para performance
        private int hitFlashID;
        private int dissolveAmountID;

        private bool isDead = false;
        private Transform player;
        private Rigidbody2D rb;

        [Header("Attack")]
        [SerializeField] private float attackCooldown = 0.5f;
        private float attackTimer = 0f;

        // Sub-Boss Variables
        private int currentDamage;
        private StatusData contactStatus;

        [Header("Events")]
        public UnityEvent OnDeath;

        public EnemyData Data => data;
        public bool IsDead => isDead;

        private HeroController heroController;

        // Knockback state
        private float knockbackTimer = 0f;
        private const float knockbackDuration = 0.2f;

        protected override void Awake() {
            base.Awake(); // A classe base já busca o spriteRenderer aqui

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) Debug.LogError("EnemyController: Rigidbody2D not found!");

            if (animator == null) animator = GetComponent<Animator>();

            // Usa a variável da classe base (sem o 'private' local)
            if (spriteRenderer != null && spriteRenderer.material != null) {
                runtimeMaterial = spriteRenderer.material;
                hitFlashID = Shader.PropertyToID("_HitFlash");
                dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
            }
        }

        public void Initialize(EnemyData enemyData, int wave, StatusData statusToApply = null) {
            data = enemyData;
            contactStatus = statusToApply;

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
            currentDamage = data.damage;

            isDead = false;
            attackTimer = 0f;
            knockbackTimer = 0f;

            // Reseta os efeitos visuais
            if (runtimeMaterial != null) {
                runtimeMaterial.SetFloat(hitFlashID, 0f);
                runtimeMaterial.SetFloat(dissolveAmountID, 0f);
            }

            if (animator != null && animator.runtimeAnimatorController != null) {
                animator.SetBool("IsDead", false);
                animator.SetTrigger("Respawn");
            }
        }

        public void MultiplyStats(float healthMultiplier, float damageMultiplier) {
            maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
            currentHealth = maxHealth;
            currentDamage = Mathf.RoundToInt(currentDamage * damageMultiplier);
            Debug.Log($"[Enemy] Sub-Boss stats applied. HP: {maxHealth}, Damage: {currentDamage}");
        }

        public void SetPlayerReference(HeroController hero) {
            heroController = hero;
            player = hero != null ? hero.transform : null;
        }

        public void ApplyKnockback(float force, Vector2 direction) {
            if (isDead) return;
            if (rb == null) return;

            rb.linearVelocity = Vector2.zero;
            rb.WakeUp();
            rb.AddForce(direction * force, ForceMode2D.Impulse);
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

            // Reseta os efeitos visuais ao voltar do pool
            if (runtimeMaterial != null) {
                runtimeMaterial.SetFloat(hitFlashID, 0f);
                runtimeMaterial.SetFloat(dissolveAmountID, 0f);
            }

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

            if (knockbackTimer > 0f) {
                knockbackTimer -= Time.fixedDeltaTime;
                return;
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

        private void OnTriggerStay2D(Collider2D other) {
            if (isDead || attackTimer > 0f) return;
            if (heroController == null || heroController.IsDead) return;

            if (other.CompareTag("Player")) {
                HeroController hero = other.GetComponent<HeroController>();
                if (hero != null) {
                    hero.TakeDamage(currentDamage);

                    if (contactStatus != null) {
                        hero.ApplyStatus(contactStatus);
                    }

                    attackTimer = attackCooldown;
                    Debug.Log($"Enemy dealt {currentDamage} damage to hero. Status: {(contactStatus != null ? contactStatus.statusName : "None")}");
                }
            }
        }

        // --- Flash Effect ---
        protected override void AnimateTakingDamage() {
            if (runtimeMaterial == null) return;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine() {
            // Blink white twice
            for (int i = 0; i < 2; i++) {
                runtimeMaterial.SetFloat(hitFlashID, 1f);
                yield return new WaitForSeconds(0.05f);

                runtimeMaterial.SetFloat(hitFlashID, 0f);
                yield return new WaitForSeconds(0.05f);
            }
            flashCoroutine = null;
        }

        // ---  Dissolve Effect ---
        protected override void Die() {
            if (isDead) return;
            isDead = true;

            ClearAllStatuses();
            OnDeath?.Invoke();

            // Inicia a rotina de dissolução em vez de destruir imediatamente
            StartCoroutine(DissolveRoutine());
        }

        private IEnumerator DissolveRoutine() {
            // Se não houver material ou animação de morte, destrói direto
            if (runtimeMaterial == null) {
                Destroy(gameObject);
                yield break;
            }

            if (rb != null) {
                rb.linearVelocity = Vector2.zero; // Zera a velocidade
                rb.bodyType = RigidbodyType2D.Kinematic; // Opcional: impede que colisões empurrem ele enquanto dissolve
            }

            // Opcional: Toca a animação de morte, se existir
            if (hasDeathAnimation && animator != null && animator.HasState(0, Animator.StringToHash("Death"))) {
                animator.SetBool("IsDead", true);
                animator.SetTrigger("Die");
            }

            float duration = 0.5f;
            float timer = 0f;

            // Aumenta a dissolução de 0 a 1.1 para garantir que suma tudo
            while (timer < duration) {
                timer += Time.deltaTime;
                float progress = timer / duration;

                runtimeMaterial.SetFloat(dissolveAmountID, progress * 1.1f);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnDisable() {
            OnDeath.RemoveAllListeners();
        }

        public void OnDeathAnimationEnd() {
            Destroy(gameObject);
        }
    }
}