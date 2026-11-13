using UnityEngine;
using System.Collections;

namespace HexaBit.Core {
    public class LightningBoltEffect : WeaponEffect {
        [Header("Visual References")]
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Transform extraPartTransform;
        [SerializeField] private ParticleSystem impactParticles;

        [Header("Settings")]
        [SerializeField] private float damageDelay = 0.3f;
        [SerializeField] private float stackOffset = 1.5f;

        private WeaponData data;
        private int level;
        private float splashArea;
        private int damage;
        private Transform target;
        private bool isDead = false;
        private bool hasAppliedDamage = false;
        private Animator bodyAnimator;

        public override void Initialize(WeaponData weaponData, int levelIndex, Vector2 direction, HeroController hero) {
            data = weaponData;
            level = levelIndex;
            splashArea = data.GetFloat(level, DynamicParameter.SplashArea);
            damage = Mathf.RoundToInt(data.GetDamage(level));
        }

        public void SetTarget(Transform targetTransform) {
            target = targetTransform;
            if (target == null) {
                Destroy(gameObject);
                return;
            }

            transform.position = target.position;
            float baseArea = 1.5f;
            float scaleFactor = splashArea / baseArea;
            transform.localScale = Vector3.one * scaleFactor;

            if (bodyTransform != null) bodyAnimator = bodyTransform.GetComponent<Animator>();

            StackRayToTopOfScreen();

            StartCoroutine(ApplyDamageFallback());
        }

        private void StackRayToTopOfScreen() {
            if (bodyTransform == null || extraPartTransform == null) return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            float topY = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
            float currentY = bodyTransform.position.y;

            float distanceToTop = topY - currentY;
            float heightPerPiece = stackOffset * transform.localScale.y;
            int stackCount = Mathf.CeilToInt(distanceToTop / heightPerPiece);

            for (int i = 1; i <= stackCount; i++) {
                GameObject extraObj = Instantiate(extraPartTransform.gameObject, transform);
                extraObj.SetActive(true);

                // Remove the Relay from the clone if it exists, so it doesn't trigger double damage
                LightningEventRelay cloneRelay = extraObj.GetComponent<LightningEventRelay>();
                if (cloneRelay != null) Destroy(cloneRelay);

                extraObj.transform.localPosition = bodyTransform.localPosition + new Vector3(0, i * stackOffset, 0);

                Animator extraAnim = extraObj.GetComponent<Animator>();
                if (bodyAnimator != null && extraAnim != null) {
                    // Force the clone to use the ExtraPart's Controller (the Override)
                    extraAnim.runtimeAnimatorController = extraPartTransform.GetComponent<Animator>().runtimeAnimatorController;

                    // Sync the frame time
                    float currentFrame = bodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    extraAnim.Play(0, -1, currentFrame);
                }
            }
        }

        private IEnumerator ApplyDamageFallback() {
            yield return new WaitForSeconds(damageDelay);
            OnImpactFrame();
        }

        // --- ANIMATION EVENTS ---
        // These are called by the Body's Animator, because the Body has the animation.
        public void OnImpactFrame() {
            if (hasAppliedDamage || isDead) return;
            hasAppliedDamage = true;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, splashArea);
            foreach (var collider in hitColliders) {
                if (collider.CompareTag("Enemy")) {
                    EnemyController enemy = collider.GetComponent<EnemyController>();
                    if (enemy != null) {
                        enemy.TakeDamage(damage);
                    }
                }
            }

            if (impactParticles != null) {
                Instantiate(impactParticles, transform.position, Quaternion.identity);
            }
        }

        public void OnAnimationEnd() {
            if (isDead) return;
            isDead = true;
            Destroy(gameObject);
        }

        private void Start() {
            Destroy(gameObject, damageDelay + 1.5f);
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, splashArea);
        }
    }
}