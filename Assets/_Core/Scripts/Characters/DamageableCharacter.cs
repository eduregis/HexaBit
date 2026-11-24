using UnityEngine;
using System.Collections;

namespace HexaBit.Core {
    public abstract partial class DamageableCharacter : MonoBehaviour {
        [Header("Health & Armor")]
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected int armor = 0;

        [Header("Damage Feedback")]
        [SerializeField] protected GameObject damagePopupPrefab;

        protected int currentHealth;
        protected SpriteRenderer spriteRenderer;
        protected Color originalColor;

        // Shader Effects
        protected Material runtimeMaterial;
        protected Coroutine flashCoroutine;
        protected int hitFlashID;
        protected int dissolveAmountID;

        public System.Action OnHealthChanged;

        protected virtual void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                originalColor = spriteRenderer.color;

                // Setup Shader
                if (spriteRenderer.material != null) {
                    runtimeMaterial = spriteRenderer.material;
                    hitFlashID = Shader.PropertyToID("_HitFlash");
                    dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
                }
            }
        }

        public int GetMaxHealth() {
            return maxHealth;
        }

        protected void SetHealth(int health, int armorValue) {
            maxHealth = health;
            armor = armorValue;
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(float damage, Color? popupColor = null) {
            float effectiveDamage = damage * DamageMultiplier;
            int finalDamage = (int)Mathf.Max(1, effectiveDamage - armor);
            currentHealth -= finalDamage;

            // Flash Effect
            if (runtimeMaterial != null) {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashRoutine());
            }

            // Animate taking damage (for Animator based characters)
            AnimateTakingDamage();

            if (damagePopupPrefab != null) {
                GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform.parent);
                DamagePopup popupScript = popup.GetComponent<DamagePopup>();
                if (popupScript != null) {
                    popupScript.SetDamage(finalDamage, popupColor);
                }
            }

            Debug.Log($"{gameObject.name} took {finalDamage} damage. Health: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke();

            if (currentHealth <= 0) {
                Die();
                // Start dissolve after death
                StartCoroutine(DissolveRoutine());
            }
        }

        protected IEnumerator FlashRoutine() {
            for (int i = 0; i < 2; i++) {
                runtimeMaterial.SetFloat(hitFlashID, 1f);
                yield return new WaitForSeconds(0.05f);
                runtimeMaterial.SetFloat(hitFlashID, 0f);
                yield return new WaitForSeconds(0.05f);
            }
            flashCoroutine = null;
        }

        protected IEnumerator DissolveRoutine() {
            if (runtimeMaterial == null) {
                Destroy(gameObject);
                yield break;
            }

            // Stop physics movement
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            float duration = 0.5f;
            float timer = 0f;

            while (timer < duration) {
                timer += Time.deltaTime;
                float progress = timer / duration;
                runtimeMaterial.SetFloat(dissolveAmountID, progress * 1.1f);
                yield return null;
            }

            Destroy(gameObject);
        }

        public virtual void Heal(float amount) {
            currentHealth = Mathf.Min(currentHealth + Mathf.RoundToInt(amount), maxHealth);
            OnHealthChanged?.Invoke();
        }

        protected abstract void AnimateTakingDamage();
        protected abstract void Die();
    }
}