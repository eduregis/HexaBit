using UnityEngine;

namespace HexaBit.Core {
    public class GadgetGunProjectile : WeaponEffect {
        [Header("Projectile Settings")]
        [SerializeField] private float maxLifetime = 3f; // Life time

        private Vector2 direction;
        private float speed;
        private float damage;
        private float lifetimeTimer;

        public override void Initialize(WeaponData data, int levelIndex, Vector2 dir, HeroController hero) {
            damage = data.GetDamage(levelIndex);
            speed = data.GetFloat(levelIndex, DynamicParameter.Speed);
            direction = dir.normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            lifetimeTimer = maxLifetime;
        }

        private void Update() {
            // Movimento
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f) {
                Destroy(gameObject);
                Debug.Log("Destroyed by life time expired.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            // Se colidir com um inimigo, aplica dano e destrói o projétil
            if (other.CompareTag("Enemy")) {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null) {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            } else if (other.CompareTag("Wall")) Destroy(gameObject);
        }
    }
}