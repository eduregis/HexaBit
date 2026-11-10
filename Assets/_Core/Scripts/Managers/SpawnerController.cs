using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {
    public class SpawnerController : MonoBehaviour {
        [Header("Stage Data")]
        [SerializeField] private StageData stageData;

        [Header("Spawn Validation")]
        [Tooltip("Radius to check if the spawn position is blocked by a wall.")]
        [SerializeField] private float spawnCheckRadius = 0.5f;
        [SerializeField] private LayerMask wallLayer;

        [Header("Player Reference")]
        [SerializeField] private Transform player;

        private float subBossTimer = 0f;
        private int currentWave = 0;
        private float waveTimer = 0f;
        private Camera mainCamera;
        private int currentEnemyCount = 0;

        private void Start() {
            mainCamera = Camera.main;
            if (mainCamera == null) Debug.LogError("SpawnerController: Main Camera not found!");
            if (stageData == null) {
                Debug.LogError("SpawnerController: Stage Data is not assigned!");
                return;
            }

            StartCoroutine(WaitForPlayerAndStart());
        }

        private IEnumerator WaitForPlayerAndStart() {
            if (player == null) {
                Debug.Log("SpawnerController: Waiting for player...");
                while (player == null) {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null) {
                        player = playerObj.transform;
                        Debug.Log($"SpawnerController: Player found: {playerObj.name}");
                    }
                    yield return null;
                }
            }

            currentEnemyCount = 0;
            currentWave = 0;
            waveTimer = 0f;
            subBossTimer = 0f;

            StartCoroutine(SpawnLoop());
        }

        private void Update() {
            // Wave timer
            waveTimer += Time.deltaTime;
            if (waveTimer >= stageData.waveInterval) {
                waveTimer = 0f;
                currentWave++;
                Debug.Log($"Wave {currentWave} started!");
            }

            // Sub-boss timer
            if (GameplayManager.Instance != null && !GameplayManager.Instance.IsTimerPaused) {
                subBossTimer += Time.deltaTime;
                if (subBossTimer >= stageData.subBossSettings.interval) {
                    subBossTimer = 0f;
                    SpawnSubBoss();
                }
            }
        }

        private IEnumerator SpawnLoop() {
            while (true) {
                yield return new WaitForSeconds(stageData.spawnInterval);

                if (currentEnemyCount >= stageData.maxEnemies) continue;
                if (player == null) continue;

                EnemyData selectedEnemy = GetRandomEligibleEnemy();
                if (selectedEnemy == null) continue;

                Vector3 spawnPos = GetSafeSpawnPosition();
                GameObject enemy = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity);

                EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
                if (enemyCtrl != null) {
                    Animator anim = enemy.GetComponent<Animator>();
                    if (anim != null && selectedEnemy.animatorController != null) {
                        anim.runtimeAnimatorController = selectedEnemy.animatorController;
                    }

                    HeroController hero = player.GetComponent<HeroController>();
                    if (hero != null) enemyCtrl.SetPlayerReference(hero);

                    enemyCtrl.Initialize(selectedEnemy, currentWave);
                    enemyCtrl.OnDeath.AddListener(() => OnEnemyDeath(enemy));
                }

                currentEnemyCount++;
            }
        }

        private void SpawnSubBoss() {
            if (player == null) return;

            EnemyData selectedEnemy = GetRandomEligibleEnemy();
            if (selectedEnemy == null) return;

            StatusData selectedStatus = null;
            if (stageData.subBossSettings.statusPool != null && stageData.subBossSettings.statusPool.Count > 0) {
                selectedStatus = stageData.subBossSettings.statusPool[Random.Range(0, stageData.subBossSettings.statusPool.Count)];
            }

            Vector3 spawnPos = GetSafeSpawnPosition();
            GameObject enemy = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity);

            EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
            if (enemyCtrl != null) {
                Animator anim = enemy.GetComponent<Animator>();
                if (anim != null && selectedEnemy.animatorController != null) {
                    anim.runtimeAnimatorController = selectedEnemy.animatorController;
                }

                HeroController hero = player.GetComponent<HeroController>();
                if (hero != null) enemyCtrl.SetPlayerReference(hero);

                enemy.transform.localScale *= stageData.subBossSettings.scaleMultiplier;

                enemyCtrl.Initialize(selectedEnemy, currentWave, selectedStatus);
                enemyCtrl.MultiplyStats(stageData.subBossSettings.healthMultiplier, stageData.subBossSettings.damageMultiplier);

                enemyCtrl.OnDeath.AddListener(() => OnEnemyDeath(enemy));

                Debug.Log($"Sub-Boss Spawned! Enemy: {selectedEnemy.enemyName}");
            }

            currentEnemyCount++;
        }

        /// <summary>
        /// Returns a random EnemyData from the StageData pool that is currently eligible.
        /// </summary>
        private EnemyData GetRandomEligibleEnemy() {
            if (GameplayManager.Instance == null) return null;

            float gameTime = GameplayManager.Instance.CurrentTime;
            List<EnemyData> eligible = new List<EnemyData>();

            foreach (var info in stageData.enemyPool) {
                // Check if the enemy exists and if the current time is within its spawn window
                if (info.enemy != null && gameTime >= info.startTime && gameTime <= info.endTime) {
                    eligible.Add(info.enemy);
                }
            }

            if (eligible.Count == 0) return null;
            return eligible[Random.Range(0, eligible.Count)];
        }

        private void OnEnemyDeath(GameObject enemy) {
            currentEnemyCount--;

            if (DropManager.Instance != null) {
                DropManager.Instance.SpawnXPDrop(enemy.transform.position);
            }

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.AddKill();
            }
        }

        private Vector3 GetSpawnPosition() {
            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

            int side = Random.Range(0, 4);
            Vector3 spawnPos = Vector3.zero;

            switch (side) {
                case 0: spawnPos.x = min.x - 0.5f; spawnPos.y = Random.Range(min.y + 0.5f, max.y - 0.5f); break;
                case 1: spawnPos.x = max.x + 0.5f; spawnPos.y = Random.Range(min.y + 0.5f, max.y - 0.5f); break;
                case 2: spawnPos.y = max.y + 0.5f; spawnPos.x = Random.Range(min.x + 0.5f, max.x - 0.5f); break;
                case 3: spawnPos.y = min.y - 0.5f; spawnPos.x = Random.Range(min.x + 0.5f, max.x - 0.5f); break;
            }

            return spawnPos;
        }

        private Vector3 GetSafeSpawnPosition() {
            Vector3 spawnPos = Vector3.zero;
            int maxAttempts = 30;
            bool foundSafeSpot = false;

            for (int i = 0; i < maxAttempts; i++) {
                spawnPos = GetSpawnPosition();
                Collider2D hit = Physics2D.OverlapCircle(spawnPos, spawnCheckRadius, wallLayer);

                if (hit == null) {
                    foundSafeSpot = true;
                    break;
                }
            }

            return spawnPos;
        }
    }
}