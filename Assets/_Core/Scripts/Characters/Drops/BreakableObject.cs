using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class BreakableObject : DamageableCharacter {
        [System.Serializable]
        public class LootDrop {
            [Tooltip("Item prefab (HealthPickup, XP, etc). Leave empty if spawning an enemy.")]
            public GameObject itemPrefab;

            [Tooltip("Enemy data (EnemyData). Use this to spawn enemies using the base prefab.")]
            public EnemyData enemyData;

            [Range(0f, 1f)]
            [Tooltip("Chance to drop. 1 = always, 0 = never.")]
            public float dropChance = 1f;

            [Tooltip("Delay in seconds before the enemy spawns after the object breaks. Only used for enemies.")]
            public float enemySpawnDelay = 0.5f;
        }

        [Header("Data")]
        [SerializeField] private BreakableData data;

        [Header("Loot Settings")]
        [Tooltip("All possible drops. Each entry uses its own drop chance.")]
        [SerializeField] private List<LootDrop> lootTable = new List<LootDrop>();

        [Header("Events")]
        public UnityEvent OnBreakEvent;

        private SpriteRenderer render;

        protected override void Awake() {
            base.Awake();
            render = GetComponent<SpriteRenderer>();

            if (data != null) {
                SetHealth(data.maxHealth, 0);
                if (data.sprite != null && render != null) {
                    render.sprite = data.sprite;
                }
            }
        }

        protected override void Die() {
            OnBreakEvent?.Invoke();
            ProcessLoot();
        }

        private void ProcessLoot() {

            foreach (var loot in lootTable) {

                if (Random.value > loot.dropChance) {
                    continue;
                }

                if (loot.itemPrefab != null) {
                    Instantiate(loot.itemPrefab, transform.position, Quaternion.identity);
                } else if (loot.enemyData != null) {
                    if (loot.enemySpawnDelay > 0f) {
                        StartCoroutine(SpawnEnemyWithDelay(loot.enemyData, loot.enemySpawnDelay, transform.position));
                    } else {
                        SpawnEnemy(loot.enemyData, transform.position);
                    }
                }
            }
        }

        // Coroutine hosted on a temporary runner object so it survives
        // if the breakable object is destroyed by the base class.
        private IEnumerator SpawnEnemyWithDelay(EnemyData enemyData, float delay, Vector3 spawnPosition) {
            GameObject tempRunner = new GameObject("EnemySpawnDelayRunner");
            tempRunner.transform.position = spawnPosition;

            yield return new WaitForSeconds(delay);

            SpawnEnemy(enemyData, spawnPosition);

            Destroy(tempRunner);
        }

        private void SpawnEnemy(EnemyData enemyData, Vector3 spawnPosition) {

            if (enemyData.prefab == null) {
                return;
            }

            GameObject enemyObj = Instantiate(enemyData.prefab, spawnPosition, Quaternion.identity);

            EnemyController enemyCtrl = enemyObj.GetComponent<EnemyController>();
            if (enemyCtrl == null) {
                return;
            }

            Animator anim = enemyObj.GetComponent<Animator>();
            if (anim != null && enemyData.animatorController != null) {
                anim.runtimeAnimatorController = enemyData.animatorController;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                HeroController hero = playerObj.GetComponent<HeroController>();
                if (hero != null) enemyCtrl.SetPlayerReference(hero);
            }

            enemyCtrl.Initialize(enemyData, 0);

            enemyCtrl.OnDeathEvent.AddListener(() => {
                if (GameplayManager.Instance != null) GameplayManager.Instance.AddKill();
            });
        }

        protected override void AnimateTakingDamage() {
            if (runtimeMaterial == null) return;
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }
}