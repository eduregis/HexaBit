using UnityEngine;

namespace HexaBit.Core {
    public class ViralNapalmWeapon : WeaponBase {
        protected override void Fire() {
            if (data.attackPrefab == null) return;

            // Find closest enemy on screen
            float searchRange = data.GetFloat(currentLevel, DynamicParameter.Range);
            Transform target = FindClosestEnemy(searchRange);

            if (target == null) {
                return;
            }

            // Direction towards target
            Vector2 direction = (target.position - transform.position).normalized;

            // Spawn the projectile
            GameObject projectileGO = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
            ViralNapalmProjectile projectile = projectileGO.GetComponent<ViralNapalmProjectile>();
            if (projectile != null) {
                projectile.Initialize(data, currentLevel, direction, hero);
            }
        }

        private Transform FindClosestEnemy(float range) {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform closest = null;
            float minDist = Mathf.Infinity;

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return null;

            foreach (var enemy in enemies) {
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemy.transform.position);
                bool isOnScreen = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                  viewportPos.y >= 0 && viewportPos.y <= 1 &&
                                  viewportPos.z > 0;

                if (!isOnScreen) continue;

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