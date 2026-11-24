using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class CyberHammerEffect : WeaponEffect {
        [Header("Swing Settings")]
        [SerializeField] private float swingDuration = 0.3f;
        [SerializeField] private EasingType easingType = EasingType.Senoidal;

        [Header("References")]
        [SerializeField] private Transform hammerPivot;
        [SerializeField] private GameObject hammerHead;

        private WeaponData data;
        private int level;
        private Vector2 direction;
        private float timer;
        private float totalAngle;
        private float size;
        private int damage;
        private float knockback;
        private Transform heroTransform; // referência ao herói

        private List<DamageableCharacter> hitEnemies = new List<DamageableCharacter>();

        public override void Initialize(WeaponData weaponData, int levelIndex, Vector2 dir, HeroController hero) {
            data = weaponData;
            level = levelIndex;
            direction = -dir.normalized;
            this.heroTransform = hero.transform;

            totalAngle = data.GetFloat(level, DynamicParameter.Angle);
            size = data.GetFloat(level, DynamicParameter.Size);
            damage = Mathf.RoundToInt(data.GetDamage(level));
            knockback = data.GetFloat(level, DynamicParameter.Knockback);

            // Posiciona o pivô inicialmente
            if (hammerPivot != null) {
                hammerPivot.position = heroTransform != null ? heroTransform.position : transform.position;
                hammerPivot.rotation = Quaternion.identity;
            }

            // Ajusta o tamanho baseado no radius
            if (hammerHead != null) {
                hammerHead.transform.localScale = Vector3.one * size;
            }

            SetupHammer();
            timer = 0f;
            hitEnemies.Clear();
        }

        private void SetupHammer() {
            if (hammerPivot == null) return;
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle + 90f - totalAngle / 2f;
            hammerPivot.rotation = Quaternion.Euler(0, 0, startAngle);
        }

        private void Update() {
            if (hammerPivot != null && heroTransform != null) {
                hammerPivot.position = heroTransform.position;
            }

            timer += Time.deltaTime;
            float rawProgress = Mathf.Clamp01(timer / swingDuration);

            float easedProgress = ApplyEasing(rawProgress);

            if (hammerPivot != null) {
                float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float startAngle = baseAngle + 90f - totalAngle / 2f;
                float endAngle = baseAngle + 90f + totalAngle / 2f;
                float currentAngle = Mathf.Lerp(startAngle, endAngle, easedProgress);
                hammerPivot.rotation = Quaternion.Euler(0, 0, currentAngle);
            }

            if (rawProgress >= 1f) {
                Destroy(gameObject);
            }
        }

        private float ApplyEasing(float t) {
            switch (easingType) {
                case EasingType.Linear:
                    return t;

                case EasingType.EaseInOut:
                    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

                case EasingType.Senoidal:
                    return 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);

                case EasingType.EaseOutBack:
                    float c1 = 1.70158f;
                    float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

                default:
                    return t;
            }
        }

        public void OnHammerHit(Collider2D other) {
            DamageableCharacter damageable = other.GetComponent<DamageableCharacter>();

            if (damageable != null && !hitEnemies.Contains(damageable)) {
                // Apply knockback if it's an enemy
                if (knockback > 0 && damageable is EnemyController enemy) {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemy.ApplyKnockback(knockback, knockbackDir);
                }

                // Then apply damage
                damageable.TakeDamage(damage);
                hitEnemies.Add(damageable);
            }
        }
    }
}