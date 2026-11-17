using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class ChakramProjectile : WeaponEffect {

        [Header("Chakram Settings")]
        [SerializeField] private float maxLifetime = 5f;

        private Vector2 initialDirection;
        private Vector2 currentDirection;
        private float speed;
        private float maxDistance;
        private float hitboxRadius;
        private int damage;

        private Transform heroTransform;
        private float distanceTraveled = 0f;
        private bool isReturning = false;
        private float currentSpeed;
        private float lifetimeTimer;

        private float returnAcceleration = 15f;

        // Status data from weapon
        private StatusData poisonStatusData;

        public override void Initialize(WeaponData data, int levelIndex, Vector2 dir, HeroController hero) {
            initialDirection = dir.normalized;
            currentDirection = initialDirection;
            heroTransform = hero.transform;

            speed = data.GetFloat(levelIndex, DynamicParameter.Speed);
            maxDistance = data.GetFloat(levelIndex, DynamicParameter.MaxDistance);
            hitboxRadius = data.GetFloat(levelIndex, DynamicParameter.Hitbox) * hero.GlobalAreaModifier;
            damage = Mathf.RoundToInt(data.GetDamage(levelIndex));

            // Get status data from weapon
            if (data.statusData != null) {
                poisonStatusData = data.statusData;
                Debug.Log($"Chakram will apply status: {poisonStatusData.statusName}");
            }

            currentSpeed = speed;
            distanceTraveled = 0f;
            isReturning = false;
            lifetimeTimer = 0f;

            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null)
                col.radius = hitboxRadius;

            returnAcceleration = speed * 2.5f;

            Debug.Log($"Chakram launched: speed={speed}, maxDistance={maxDistance}, damage={damage}");
        }

        private void Update() {
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= maxLifetime) {
                DestroyProjectile();
                return;
            }

            if (heroTransform == null) {
                DestroyProjectile();
                return;
            }

            UpdateMovement();

            if (IsOutOfBounds()) {
                DestroyProjectile();
                return;
            }

            if (isReturning) {
                float distToHero = Vector2.Distance(transform.position, heroTransform.position);
                if (distToHero < 0.8f) {
                    DestroyProjectile();
                    return;
                }
            }
        }

        private void UpdateMovement() {
            if (!isReturning) {
                float progress = Mathf.Clamp01(distanceTraveled / maxDistance);
                currentSpeed = Mathf.Lerp(speed, 0f, progress * progress);

                transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);
                distanceTraveled += currentSpeed * Time.deltaTime;

                if (distanceTraveled >= maxDistance || currentSpeed < 0.05f) {
                    isReturning = true;
                    currentDirection = -initialDirection;
                    currentSpeed = 0f;
                    Debug.Log($"Chakram starting return journey");
                }
            } else {
                currentSpeed = Mathf.Min(currentSpeed + returnAcceleration * Time.deltaTime, speed);
                transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);
            }
        }

        private bool IsOutOfBounds() {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return false;

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            float margin = 2f;
            return viewportPos.x < -margin || viewportPos.x > 1f + margin ||
                   viewportPos.y < -margin || viewportPos.y > 1f + margin ||
                   viewportPos.z < 0;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemy.IsDead) {
                    // Apply damage
                    enemy.TakeDamage(damage);
                    Debug.Log($"Chakram hit enemy for {damage} damage on {(isReturning ? "return" : "outward")} pass!");

                    // Apply poison status if available
                    if (poisonStatusData != null) {
                        enemy.ApplyStatus(poisonStatusData);
                        Debug.Log($"Applied {poisonStatusData.statusName} to {enemy.name}");
                    }
                }
            } else if (other.CompareTag("Wall")) {
                DestroyProjectile();
            }
        }

        private void DestroyProjectile() {
            if (heroTransform != null) {
                ChakramWeapon weapon = heroTransform.GetComponentInChildren<ChakramWeapon>();
                if (weapon != null)
                    weapon.OnChakramDestroyed();
            }
            Destroy(gameObject);
        }
    }
}