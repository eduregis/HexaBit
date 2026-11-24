using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class ViralNapalmPuddle : MonoBehaviour {
        [Header("Puddle Settings")]
        [SerializeField] private CircleCollider2D puddleCollider;
        [SerializeField] private SpriteRenderer puddleOutline;

        [Header("Events")]
        [Tooltip("Called when the puddle appears; executes a loop until its duration ends.")]
        public UnityEvent OnActiveLoopEvent;

        [Tooltip("Called when the puddle is initialized. Useful for playing spawn and loop audio.")]
        public UnityEvent OnInitializeEvent;

        [Tooltip("Called when the puddle is destroyed. Useful for stopping loop audio.")]
        public UnityEvent OnDestroyEvent;

        private WeaponData data;
        private int level;
        private float duration;
        private float radius;
        private int damagePerTick;
        private float tickInterval;
        private float timer;
        private float lifetimeTimer;

        private Transform heroTransform;
        private List<DamageableCharacter> enemiesInPuddle = new List<DamageableCharacter>();

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

            // Invoke the initialization event (for audio/VFX)
            OnInitializeEvent?.Invoke();

            OnActiveLoopEvent?.Invoke();

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
            List<DamageableCharacter> enemiesCopy = new List<DamageableCharacter>(enemiesInPuddle);

            foreach (var target in enemiesCopy) {
                if (target != null) {
                    if (target is EnemyController enemy && enemy.IsDead) continue;
                    target.TakeDamage(damagePerTick);
                }
            }

            enemiesInPuddle.RemoveAll(e => e == null || (e is EnemyController enemy && enemy.IsDead));
        }

        private void ApplyStatusToEnemy(DamageableCharacter target) {
            if (corruptedStatusData != null && target != null && target is EnemyController enemy && !enemy.IsDead) {
                enemy.ApplyStatus(corruptedStatusData);
                Debug.Log($"Applied {corruptedStatusData.statusName} to {enemy.name}");
            }
        }

        private void RemoveStatusFromAllEnemies() {
            if (corruptedStatusData == null) return;

            foreach (var target in enemiesInPuddle) {
                if (target != null && target is EnemyController enemy) {
                    enemy.RemoveStatus(corruptedStatusData);
                }
            }
        }

        // Trigger events
        private void OnTriggerEnter2D(Collider2D other) {
            DamageableCharacter damageable = other.GetComponent<DamageableCharacter>();
            if (damageable != null && !enemiesInPuddle.Contains(damageable)) {
                if (damageable is EnemyController enemy && enemy.IsDead) return;

                enemiesInPuddle.Add(damageable);
                damageable.TakeDamage(damagePerTick);
                ApplyStatusToEnemy(damageable);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            DamageableCharacter damageable = other.GetComponent<DamageableCharacter>();
            if (damageable != null && enemiesInPuddle.Contains(damageable)) {
                enemiesInPuddle.Remove(damageable);
                if (corruptedStatusData != null && damageable is EnemyController enemy) {
                    enemy.RemoveStatus(corruptedStatusData);
                }
            }
        }

        private void OnDestroy() {
            RemoveStatusFromAllEnemies();

            // Invoke the destroy event (for stopping audio/VFX)
            OnDestroyEvent?.Invoke();
        }
    }
}