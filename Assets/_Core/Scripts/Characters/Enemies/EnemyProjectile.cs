using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class EnemyProjectile : MonoBehaviour {

        [Header("Projectile Settings")]
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float spreadAngle = 15f;

        [Header("Events")]
        [Tooltip("Called when the enemy shoots a projectile.")]
        public UnityEvent OnShootEvent;

        private Vector2 direction;
        private float speed = 6f;
        private int damage = 5;
        private float lifetimeTimer;

        public void Initialize(Vector2 dir, float moveSpeed, int dmg) {
            direction = dir.normalized;
            speed = moveSpeed;
            damage = dmg;

            OnShootEvent?.Invoke();

            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float finalAngle = baseAngle + randomSpread;

            direction = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            transform.rotation = Quaternion.Euler(0, 0, finalAngle);

            lifetimeTimer = maxLifetime;
        }

        private void Update() {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f) {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            // Debug to see exactly what the projectile is hitting
            Debug.Log($"[Projectile] Hit: {other.name} | Tag: {other.tag} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

            if (other.CompareTag("Player")) {
                HeroController hero = other.GetComponent<HeroController>();
                if (hero != null) {
                    hero.TakeDamage(damage);
                    Debug.Log("Damage dealt to player!");
                }
                Destroy(gameObject);
            } else if (other.CompareTag("Wall")) {
                Debug.Log("Hit a wall, destroying projectile.");
                Destroy(gameObject);
            }
        }
    }
}