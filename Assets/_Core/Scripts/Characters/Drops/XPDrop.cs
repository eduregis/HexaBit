using UnityEngine;

namespace HexaBit.Core {
    public class XPDrop : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] private float attractSpeed = 5f;
        [SerializeField] private float knockbackForce = 3f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private float collectDistance = 0.5f;

        [Header("Reward")]
        [SerializeField] private int xpValue = 10;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Cache the ID for performance when setting the outline color
        private int outlineColorID;

        private Transform targetHeroTransform;
        private HeroController targetHeroController;

        private Vector2 knockbackDirection;
        private float knockbackTimer;
        private bool isKnockbackActive = false;
        private bool isAttracted = false;
        private bool isCollected = false;

        public bool IsCollected => isCollected;

        private void Awake() {
            // Get SpriteRenderer if not assigned
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            // Cache the shader property ID
            outlineColorID = Shader.PropertyToID("_OutlineColor");
        }

        private void Start() {
            isKnockbackActive = false;
            isAttracted = false;
            isCollected = false;
        }

        /// <summary>
        /// Sets the outline color for this specific drop.
        /// </summary>
        public void SetOutlineColor(Color color) {
            if (spriteRenderer != null && spriteRenderer.material != null) {
                // Note: Setting .material creates a unique instance of the material for this object.
                // This prevents all drops from sharing the same color.
                spriteRenderer.material.SetColor(outlineColorID, color);
            }
        }

        private void Update() {
            if (isCollected) return;

            // Knockback phase
            if (isKnockbackActive) {
                knockbackTimer -= Time.deltaTime;
                transform.Translate(knockbackDirection * Time.deltaTime, Space.World);

                if (knockbackTimer <= 0f) {
                    isKnockbackActive = false;
                    if (targetHeroController != null) {
                        isAttracted = true;
                    }
                }
                return;
            }

            // Attraction phase
            if (isAttracted && targetHeroTransform != null) {
                Vector2 direction = (targetHeroTransform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, targetHeroTransform.position);

                transform.Translate(direction * attractSpeed * Time.deltaTime, Space.World);

                if (distance < collectDistance) {
                    Collect();
                }
            }
        }

        public void SetTargetHero(HeroController hero) {
            if (isCollected) return;
            if (hero == null || hero.IsDead) return;

            targetHeroController = hero;
            targetHeroTransform = hero.transform;

            if (!isKnockbackActive && !isAttracted) {
                isAttracted = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
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

        private void Collect() {
            if (isCollected) return;
            isCollected = true;

            int finalXP = xpValue;
            if (targetHeroController != null) {
                finalXP = Mathf.RoundToInt(xpValue * targetHeroController.GlobalXPMultiplier);
            }

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.AddXP(finalXP, targetHeroController);
            }

            Destroy(gameObject);
        }
    }
}