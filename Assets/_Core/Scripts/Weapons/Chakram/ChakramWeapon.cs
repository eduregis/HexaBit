using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class ChakramWeapon : WeaponBase {

        [Header("Events")]
        [Tooltip("Called when the weapon fires.")]
        public UnityEvent OnFireEvent;

        private bool isThrowing = false;

        protected override void Fire() {
            if (isThrowing) return;
            if (data.attackPrefab == null) return;

            // Use MaxDistance as search range
            float searchRange = data.GetFloat(currentLevel, DynamicParameter.MaxDistance);
            Transform target = FindClosestEnemy(searchRange);

            Vector2 direction;
            if (target != null) {
                direction = (target.position - transform.position).normalized;
                Debug.Log($"Chakram aiming at {target.name}");
            } else {
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                Debug.Log("No enemy on screen, firing randomly");
            }

            GameObject chakramGO = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
            ChakramProjectile chakram = chakramGO.GetComponent<ChakramProjectile>();
            if (chakram != null) {
                OnFireEvent.Invoke();
                chakram.Initialize(data, currentLevel, direction, hero);
                isThrowing = true;
            }
        }

        // Called by the projectile when it is destroyed (collected or out of bounds)
        public void OnChakramDestroyed() {
            isThrowing = false;
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