using UnityEngine;

namespace HexaBit.Core {
    public partial class EnemyController {
        [Header("Ranged Settings")]
        [SerializeField] private bool isRanged = false;
        [SerializeField] private float rangedAttackRange = 5f;
        [SerializeField] private float rangedAttackCooldown = 2f;
        [SerializeField] private bool wavyMovement = false;

        private float rangedAttackTimer = 0f;
        private float wavyTimer = 0f;
        private bool isCurrentlyAiming = false;

        private void SetupRangedSettings(EnemyData enemyData) {
            isRanged = enemyData.isRanged;
            rangedAttackRange = enemyData.attackRange;
            rangedAttackCooldown = enemyData.attackCooldown;
            wavyMovement = enemyData.wavyMovement;

            rangedAttackTimer = 0f;
            wavyTimer = 0f;
            isCurrentlyAiming = false;

            if (enemyData.passesThroughWalls) {
                int triggerLayer = LayerMask.NameToLayer("Trigger");
                if (triggerLayer != -1) {
                    gameObject.layer = triggerLayer;
                }
            }
        }

        private void UpdateRangedTimers() {
            if (rangedAttackTimer > 0f) {
                rangedAttackTimer -= Time.deltaTime;
            }
        }

        private bool TryRangedAttack() {
            if (!isRanged) return false;

            float distanceToHero = Vector2.Distance(transform.position, player.position);

            if (distanceToHero <= rangedAttackRange) {
                rb.linearVelocity = Vector2.zero; // Always stop moving in range

                // If cooldown is ready and not aiming yet, start aiming
                if (rangedAttackTimer <= 0f && !isCurrentlyAiming) {
                    isCurrentlyAiming = true;
                    if (animator != null) {
                        animator.SetBool("IsAiming", true);
                        animator.SetFloat("Speed", 0f);
                    }
                }

                return true; // Stops normal movement
            }

            // If out of range, ensure aiming is canceled
            if (isCurrentlyAiming) {
                isCurrentlyAiming = false;
                if (animator != null) animator.SetBool("IsAiming", false);
            }
            return false;
        }

        private Vector2 GetMovementDirection(Vector2 directionToPlayer) {
            if (!wavyMovement) return directionToPlayer;

            wavyTimer += Time.fixedDeltaTime;
            Vector2 perpendicular = new Vector2(-directionToPlayer.y, directionToPlayer.x);
            float waveOffset = Mathf.Sin(wavyTimer * 5f) * 2f;

            return (directionToPlayer + perpendicular * waveOffset).normalized;
        }

        /// <summary>
        /// Called by the Animation Event placed inside the "Aiming" animation.
        /// Forces the enemy back to Walk while the cooldown runs.
        /// </summary>
        public void OnShootAnimationEvent() {
            if (data == null) return;

            Debug.Log($"[{data.enemyName}] Atirou!");

            // Spawn the projectile
            if (data.projectilePrefab != null && player != null) {
                Vector2 directionToHero = (player.position - transform.position).normalized;

                GameObject projObj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
                EnemyProjectile projectile = projObj.GetComponent<EnemyProjectile>();

                if (projectile != null) {
                    // Initialize with direction, speed, and damage
                    projectile.Initialize(directionToHero, data.projectileSpeed, currentDamage);
                }
            }

            // Start the cooldown and return to Walk
            rangedAttackTimer = rangedAttackCooldown;
            isCurrentlyAiming = false;

            if (animator != null) {
                animator.SetBool("IsAiming", false);
                animator.SetFloat("Speed", 0.1f);
            }
        }
    }
}