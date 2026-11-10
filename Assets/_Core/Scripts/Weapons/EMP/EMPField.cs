using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class EMPField : MonoBehaviour {
        [Header("References")]
        [SerializeField] private CircleCollider2D fieldCollider;
        [SerializeField] private SpriteRenderer fieldSprite;

        private StatusData slowStatusData;
        private bool hasStatusEffect = false;

        private WeaponData data;
        private int currentLevel;
        private HeroController hero;
        private float radius;
        private float slowAmount;
        private int damage;
        private float tickInterval;
        private float timer;

        private List<EnemyController> enemiesInRange = new List<EnemyController>();

        public void Initialize(WeaponData weaponData, int level, HeroController heroController) {
            data = weaponData;
            currentLevel = level;
            hero = heroController;

            // Calculate the actual radius based on level and hero's area modifier
            radius = data.GetFloat(currentLevel, DynamicParameter.Radius) * hero.GlobalAreaModifier;
            slowAmount = data.GetFloat(currentLevel, DynamicParameter.SlowAmount);
            damage = Mathf.RoundToInt(data.GetDamage(currentLevel));
            tickInterval = data.GetCooldown(currentLevel);

            // 1. Adjust the visual Sprite 
            // To make its diameter match (radius * 2), we just use the radius as the scale.
            if (fieldSprite != null) {
                float scale = radius; // Because (radius * 2) diameter / 2 base units = radius
                fieldSprite.transform.localScale = new Vector3(scale, scale, 1f);
            }

            // 2. Adjust the Physics Collider 
            if (fieldCollider != null) {
                fieldCollider.radius = radius;
            }

            // 3. Create a copy of the StatusData to override its duration
            if (data.statusData != null && slowAmount > 0) {
                slowStatusData = ScriptableObject.Instantiate(data.statusData);
                slowStatusData.slowPercentage = slowAmount;
                slowStatusData.duration = 2f;
                hasStatusEffect = true;
            } else {
                slowStatusData = null;
                hasStatusEffect = false;
            }

            timer = 0f;
            enemiesInRange.Clear();
        }

        private void Update() {
            if (hero == null || data == null) return;

            timer += Time.deltaTime;
            if (timer >= tickInterval) {
                timer = 0f;
                ApplyTickEffects();
            }
        }

        private void ApplyTickEffects() {
            EnemyController[] enemiesCopy = enemiesInRange.ToArray();

            foreach (var enemy in enemiesCopy) {
                if (enemy != null && !enemy.IsDead && enemiesInRange.Contains(enemy)) {
                    // Apply damage only
                    enemy.TakeDamage(damage);
                }
            }
        }

        // ---------- TRIGGER EVENTS ----------
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInRange.Contains(enemy)) {
                    enemiesInRange.Add(enemy);

                    // Apply slow ONLY on entry (not on every tick)
                    if (hasStatusEffect && slowStatusData != null) {
                        slowStatusData.slowPercentage = slowAmount;
                        enemy.ApplyStatus(slowStatusData);
                    }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInRange.Contains(enemy) && !enemy.IsDead) {
                    enemiesInRange.Add(enemy);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && enemiesInRange.Contains(enemy)) {
                    enemiesInRange.Remove(enemy);

                    // Remove slow when enemy leaves
                    if (hasStatusEffect && slowStatusData != null) {
                        enemy.RemoveStatus(slowStatusData);
                    }
                }
            }
        }

        private void OnDestroy() {
            foreach (var enemy in enemiesInRange) {
                if (enemy != null && hasStatusEffect && slowStatusData != null)
                    enemy.RemoveStatus(slowStatusData);
            }
            enemiesInRange.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}