using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class GadgetGunWeapon : WeaponBase {

        [Header("Events")]
        [Tooltip("Called when the weapon fires. Useful for playing audio or triggering VFX.")]
        public UnityEvent OnFireEvent;
        protected override void Fire() {
            if (data.attackPrefab == null) return;

            OnFireEvent?.Invoke();

            float range = data.GetFloat(currentLevel, DynamicParameter.Range);
            Transform target = FindClosestEnemy(range);

            // Determina a direção do tiro
            Vector2 direction;
            if (target != null) {
                direction = (target.position - transform.position).normalized;
            } else {
                // Se não houver alvo, atira em uma direção aleatória
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                Debug.Log("No enemy in range. Firing randomly.");
            }

            int projectileCount = data.GetInt(currentLevel, DynamicParameter.Projectiles);

            for (int i = 0; i < projectileCount; i++) {
                GameObject proj = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
                GadgetGunProjectile effect = proj.GetComponent<GadgetGunProjectile>();
                if (effect != null) {
                    Vector2 dir = direction;
                    // Aplica offset angular para múltiplos projéteis
                    if (i > 0) {
                        float angleOffset = (i % 2 == 0 ? 15f : -15f) * ((i + 1) / 2);
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
                        dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    }
                    effect.Initialize(data, currentLevel, dir, hero);
                }
            }
        }

        private Transform FindClosestEnemy(float range) {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform closest = null;
            float minDist = Mathf.Infinity;

            foreach (var enemy in enemies) {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDist && dist <= range) {
                    minDist = dist;
                    closest = enemy.transform;
                }
            }
            return closest;
        }
    }
}