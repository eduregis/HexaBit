using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class ViralNapalmPuddle : MonoBehaviour {
        [Header("Puddle Settings")]
        [SerializeField] private CircleCollider2D puddleCollider;
        [SerializeField] private SpriteRenderer puddleOutline;

        private WeaponData data;
        private int level;
        private float duration;
        private float radius;
        private int damagePerTick;
        private float tickInterval;
        private float timer;
        private float lifetimeTimer;

        private Transform heroTransform;
        private List<EnemyController> enemiesInPuddle = new List<EnemyController>();

        // Status data from weapon
        private StatusData corruptedStatusData;

        public void Initialize(WeaponData weaponData, int levelIndex, HeroController hero) {
            data = weaponData;
            level = levelIndex;
            heroTransform = hero.transform;

            duration = data.GetFloat(level, DynamicParameter.Duration);
            radius = data.GetFloat(level, DynamicParameter.Radius) * hero.GlobalAreaModifier;
            damagePerTick = Mathf.RoundToInt(data.GetDamage(level));
            tickInterval = data.GetFloat(level, DynamicParameter.Tick);

            // Get the status data from the weapon (if any)
            if (data.statusData != null) {
                corruptedStatusData = data.statusData;
                Debug.Log($"ViralNapalm puddle will apply status: {corruptedStatusData.statusName}");
            }

            if (puddleCollider != null)
                puddleCollider.radius = radius;

            if (puddleOutline != null) {
                float scale = radius * 2f;
                puddleOutline.transform.localScale = new Vector3(scale, scale, 1f);
            }

            timer = 0f;
            lifetimeTimer = 0f;
            enemiesInPuddle.Clear();

            Debug.Log($"ViralNapalm puddle created: radius={radius}, duration={duration}, tickInterval={tickInterval}");
        }

        private void Update() {
            lifetimeTimer += Time.deltaTime;

            if (lifetimeTimer >= duration) {
                // Remove status from all enemies still in the puddle
                RemoveStatusFromAllEnemies();
                enemiesInPuddle.Clear();
                Destroy(gameObject);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= tickInterval) {
                timer = 0f;
                ApplyDamageToEnemies();
            }
        }

        private void ApplyDamageToEnemies() {
            List<EnemyController> enemiesCopy = new List<EnemyController>(enemiesInPuddle);

            foreach (var enemy in enemiesCopy) {
                if (enemy != null && !enemy.IsDead) {
                    // Apply damage
                    enemy.TakeDamage(damagePerTick);

                    // Apply corrupted status if available (only once, when enemy enters)
                    // We apply it on entry, not on every tick.
                }
            }

            enemiesInPuddle.RemoveAll(e => e == null || e.IsDead);
        }

        private void ApplyStatusToEnemy(EnemyController enemy) {
            if (corruptedStatusData != null && enemy != null && !enemy.IsDead) {
                enemy.ApplyStatus(corruptedStatusData);
                Debug.Log($"Applied {corruptedStatusData.statusName} to {enemy.name}");
            }
        }

        private void RemoveStatusFromAllEnemies() {
            if (corruptedStatusData == null) return;

            foreach (var enemy in enemiesInPuddle) {
                if (enemy != null) {
                    enemy.RemoveStatus(corruptedStatusData);
                }
            }
        }

        // Trigger events
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInPuddle.Contains(enemy) && !enemy.IsDead) {
                    enemiesInPuddle.Add(enemy);

                    // Apply damage immediately on entry
                    enemy.TakeDamage(damagePerTick);

                    // Apply corrupted status on entry (once per enemy entry)
                    ApplyStatusToEnemy(enemy);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && enemiesInPuddle.Contains(enemy)) {
                    enemiesInPuddle.Remove(enemy);

                    // Remove corrupted status when enemy leaves the puddle
                    if (corruptedStatusData != null) {
                        enemy.RemoveStatus(corruptedStatusData);
                    }
                }
            }
        }

        private void OnDestroy() {
            RemoveStatusFromAllEnemies();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}