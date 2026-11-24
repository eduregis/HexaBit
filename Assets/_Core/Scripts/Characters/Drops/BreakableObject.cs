using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace HexaBit.Core {
    public class BreakableObject : DamageableCharacter {
        [Header("Data")]
        [SerializeField] private BreakableData data;

        [Header("Events")]
        public UnityEvent OnBreakEvent;

        private SpriteRenderer render;

        protected override void Awake() {
            base.Awake();
            Debug.Log("[Breakable] Awake called.");
            render = GetComponent<SpriteRenderer>();

            if (data != null) {
                SetHealth(data.maxHealth, 0);
                if (data.sprite != null && render != null) {
                    render.sprite = data.sprite;
                }
            }
        }

        public override void TakeDamage(float damage, Color? popupColor = null) {
            Debug.Log($"[Breakable] Took {damage} damage.");
            base.TakeDamage(damage, popupColor);
        }

        protected override void Die() {

            Debug.Log("[Breakable] Destroyed! Spawning drops.");
            OnBreakEvent?.Invoke();
            SpawnDrops();

            // The base class will handle the dissolve effect automatically.
        }

        private void SpawnDrops() {
            if (data == null) return;

            if (data.guaranteedDropPrefab != null) {
                Instantiate(data.guaranteedDropPrefab, transform.position, Quaternion.identity);
                return;
            }

            if (data.randomDropPool.Count > 0 && Random.value < data.randomDropChance) {
                int index = Random.Range(0, data.randomDropPool.Count);
                GameObject dropPrefab = data.randomDropPool[index];

                if (dropPrefab != null) {
                    Instantiate(dropPrefab, transform.position, Quaternion.identity);
                }
            }
        }

        // Trigger the flash effect (inherited from base class)
        protected override void AnimateTakingDamage() {
            if (runtimeMaterial == null) {
                Debug.LogWarning("[Breakable] No runtime material found for flash effect!");
                return;
            }

            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }
}