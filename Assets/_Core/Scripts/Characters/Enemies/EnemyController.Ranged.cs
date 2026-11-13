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

        /// <summary>
        /// Sets up the ranged/wavy settings from the EnemyData and changes the layer if needed.
        /// </summary>
        private void SetupRangedSettings(EnemyData enemyData) {
            isRanged = enemyData.isRanged;
            rangedAttackRange = enemyData.attackRange;
            rangedAttackCooldown = enemyData.attackCooldown;
            wavyMovement = enemyData.wavyMovement;

            rangedAttackTimer = 0f;
            wavyTimer = 0f;

            // Change layer to pass through walls
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

        /// <summary>
        /// Checks if the enemy is in range to stop and aim/shoot. Returns true if it stopped to attack.
        /// </summary>
        private bool TryRangedAttack() {
            if (!isRanged) return false;

            float distanceToHero = Vector2.Distance(transform.position, player.position);

            if (distanceToHero <= rangedAttackRange) {
                // Stop moving
                rb.linearVelocity = Vector2.zero;

                if (animator != null) {
                    animator.SetBool("IsAiming", true);
                    animator.SetFloat("Speed", 0f);
                }

                // Shoot if cooldown is ready
                if (rangedAttackTimer <= 0f) {
                    if (animator != null) animator.SetTrigger("Shoot");
                    rangedAttackTimer = rangedAttackCooldown;
                }
                return true; // Stops normal movement in FixedUpdate
            }

            // Resume normal movement if out of range
            if (animator != null) animator.SetBool("IsAiming", false);
            return false;
        }

        /// <summary>
        /// Calculates the final movement direction, applying a sine wave if wavy movement is enabled.
        /// </summary>
        private Vector2 GetMovementDirection(Vector2 directionToPlayer) {
            if (!wavyMovement) return directionToPlayer;

            wavyTimer += Time.fixedDeltaTime;
            Vector2 perpendicular = new Vector2(-directionToPlayer.y, directionToPlayer.x);
            float waveOffset = Mathf.Sin(wavyTimer * 5f) * 2f; // 5f is frequency, 2f is amplitude

            return (directionToPlayer + perpendicular * waveOffset).normalized;
        }

        /// <summary>
        /// Called by the Animation Event on the "Shoot" clip.
        /// </summary>
        public void OnShootAnimationEvent() {
            if (data == null) return;
            Debug.Log($"[{data.enemyName}] Atirou!");
            // Future implementation: Instantiate the projectile prefab here.
        }
    }
}