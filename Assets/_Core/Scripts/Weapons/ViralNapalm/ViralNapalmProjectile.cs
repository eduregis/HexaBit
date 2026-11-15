using UnityEngine;

namespace HexaBit.Core {
    public class ViralNapalmProjectile : WeaponEffect {
        [Header("Projectile Settings")]
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Puddle")]
        [SerializeField] private GameObject puddlePrefab;

        private WeaponData data;
        private int level;
        private Vector2 direction;
        private HeroController hero;
        private Vector3 targetPosition;
        private bool hasReachedTarget = false;
        private bool hasCollided = false;

        // Collision detection radius
        [SerializeField] private float collisionRadius = 0.4f;

        public override void Initialize(WeaponData weaponData, int levelIndex, Vector2 dir, HeroController hero) {
            data = weaponData;
            level = levelIndex;
            direction = dir.normalized;
            this.hero = hero;

            float range = data.GetFloat(levelIndex, DynamicParameter.Range);
            targetPosition = transform.position + (Vector3)(direction * range);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            hasCollided = false;
            hasReachedTarget = false;

            Debug.Log($"ViralNapalm projectile launched towards {targetPosition}");
        }

        private void Update() {
            if (hasReachedTarget || hasCollided) return;

            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, projectileSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            // Check for collision with enemies along the way
            CheckCollision();

            // Check if reached target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
                hasReachedTarget = true;
                CreatePuddle(targetPosition);
                Destroy(gameObject);
            }
        }

        private void CheckCollision() {
            // Overlap circle to check for enemies in the projectile's path
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, collisionRadius);

            foreach (var hit in hitColliders) {
                if (hit.CompareTag("Enemy")) {
                    EnemyController enemy = hit.GetComponent<EnemyController>();
                    if (enemy != null && !enemy.IsDead) {
                        // Hit an enemy! Create puddle at this position
                        hasCollided = true;
                        Vector3 collisionPoint = transform.position;
                        CreatePuddle(collisionPoint);
                        Destroy(gameObject);
                        Debug.Log($"ViralNapalm hit enemy {enemy.name} at {collisionPoint}, creating puddle");
                        return;
                    }
                }
            }
        }

        private void CreatePuddle(Vector3 position) {
            if (puddlePrefab == null) {
                return;
            }

            GameObject puddleGO = Instantiate(puddlePrefab, position, Quaternion.identity);
            ViralNapalmPuddle puddle = puddleGO.GetComponent<ViralNapalmPuddle>();
            if (puddle != null) {
                puddle.Initialize(data, level, hero);
            } else {
                Destroy(puddleGO);
            }
        }
    }
}