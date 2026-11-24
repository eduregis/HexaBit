using UnityEngine;

namespace HexaBit.Core {
    public abstract class PickupBase : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] protected float attractSpeed = 5f;
        [SerializeField] protected float knockbackForce = 3f;
        [SerializeField] protected float knockbackDuration = 0.2f;
        [SerializeField] protected float collectDistance = 0.5f;

        protected Transform targetHeroTransform;
        protected HeroController targetHeroController;
        protected Vector2 knockbackDirection;
        protected float knockbackTimer;
        protected bool isKnockbackActive = false;
        protected bool isAttracted = false;
        protected bool isCollected = false;

        public bool IsCollected => isCollected;

        protected virtual void Update() {
            if (isCollected) return;

            if (isKnockbackActive) {
                knockbackTimer -= Time.deltaTime;
                transform.Translate(knockbackDirection * Time.deltaTime, Space.World);

                if (knockbackTimer <= 0f) {
                    isKnockbackActive = false;
                    if (targetHeroController != null) isAttracted = true;
                }
                return;
            }

            if (isAttracted && targetHeroTransform != null) {
                Vector2 direction = (targetHeroTransform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, targetHeroTransform.position);

                transform.Translate(direction * attractSpeed * Time.deltaTime, Space.World);

                if (distance < collectDistance) Collect();
            }
        }

        public virtual void SetTargetHero(HeroController hero) {
            if (isCollected || hero == null || hero.IsDead) return;

            targetHeroController = hero;
            targetHeroTransform = hero.transform;

            if (!isKnockbackActive && !isAttracted) isAttracted = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") && !isCollected && !isKnockbackActive) {
                HeroController touchingHero = other.GetComponent<HeroController>();
                if (touchingHero != null) {
                    Vector2 directionFromPlayer = (transform.position - other.transform.position).normalized;
                    knockbackDirection = directionFromPlayer * knockbackForce;
                    knockbackTimer = knockbackDuration;
                    isKnockbackActive = true;

                    targetHeroController = touchingHero;
                    targetHeroTransform = touchingHero.transform;
                    isAttracted = false;
                }
            }
        }

        protected virtual void Collect() {
            if (isCollected) return;
            isCollected = true;

            // Let the child class handle the specific logic (add XP, heal, etc.)
            OnCollect();

            // Return to pool
            if (DropManager.Instance != null) {
                DropManager.Instance.ReturnPickupToPool(this);
            } else {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the pickup is collected. Implement the specific effect here.
        /// </summary>
        protected abstract void OnCollect();
    }
}