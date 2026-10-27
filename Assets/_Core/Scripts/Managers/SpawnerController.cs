using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {
    public class SpawnerController : MonoBehaviour {
        [Header("Spawn Configuration")]
        [Tooltip("Pool of enemy types that can spawn in this level.")]
        [SerializeField] private List<EnemyData> enemyPool;

        [SerializeField] private float spawnInterval = 1.5f;  // Time between spawns (seconds)
        [SerializeField] private int maxEnemies = 20;         // Maximum enemies alive at once
        [SerializeField] private float spawnOffset = 0.5f;    // Distance outside the camera viewport

        [Header("XP Drop")]
        [SerializeField] private GameObject xpDropPrefab;

        [Header("Player Reference")]
        [SerializeField] private Transform player;            // Player transform (auto-found if null)

        // Wave system (optional)
        private int currentWave = 0;
        private float waveTimer = 0f;
        [SerializeField] private float waveInterval = 10f;   // Time between waves (seconds)

        private Camera mainCamera;
        private int currentEnemyCount = 0;

        private void Start() {
            mainCamera = Camera.main;
            if (mainCamera == null) Debug.LogError("SpawnerController: Main Camera not found!");

            // Wait until the hero is spawned by GameplayManager, then start spawning
            StartCoroutine(WaitForPlayerAndStart());
        }

        /// <summary>
        /// Waits until a GameObject with the "Player" tag exists, then starts spawning.
        /// </summary>
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

            // Reset counters and start the spawn loop
            currentEnemyCount = 0;
            currentWave = 0;
            waveTimer = 0f;

            StartCoroutine(SpawnLoop());
        }

        private void Update() {
            // Increment wave every 'waveInterval' seconds
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval) {
                waveTimer = 0f;
                currentWave++;
                Debug.Log($"Wave {currentWave} started!");
            }
        }

        private IEnumerator SpawnLoop() {
            while (true) {
                yield return new WaitForSeconds(spawnInterval);

                if (currentEnemyCount >= maxEnemies) continue;
                if (player == null) continue;

                // Get a random eligible enemy based on current game time
                EnemyData selectedEnemy = GetRandomEligibleEnemy();
                if (selectedEnemy == null) continue; // No enemies available yet

                Vector3 spawnPos = GetSpawnPosition();
                GameObject enemy = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity);

                EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
                if (enemyCtrl != null) {
                    // 1. Apply the Animator Controller from the EnemyData FIRST
                    Animator anim = enemy.GetComponent<Animator>();
                    if (anim != null && selectedEnemy.animatorController != null) {
                        anim.runtimeAnimatorController = selectedEnemy.animatorController;
                    }

                    // 2. Set the player reference
                    HeroController hero = player.GetComponent<HeroController>();
                    if (hero != null) {
                        enemyCtrl.SetPlayerReference(hero);
                    }

                    // 3. Initialize stats (which now plays the Respawn animation safely)
                    enemyCtrl.Initialize(selectedEnemy, currentWave);
                    enemyCtrl.OnDeath.AddListener(() => OnEnemyDeath(enemy));
                }

                currentEnemyCount++;
            }
        }

        /// <summary>
        /// Returns a random EnemyData from the pool whose startTime has been reached.
        /// Uses the GameplayManager's currentTime to ensure it pauses correctly during menus.
        /// </summary>
        private EnemyData GetRandomEligibleEnemy() {
            if (GameplayManager.Instance == null) return null;

            float gameTime = GameplayManager.Instance.CurrentTime;
            List<EnemyData> eligible = new List<EnemyData>();

            foreach (var enemy in enemyPool) {
                if (enemy != null && gameTime >= enemy.startTime) {
                    eligible.Add(enemy);
                }
            }

            if (eligible.Count == 0) return null;
            return eligible[Random.Range(0, eligible.Count)];
        }

        // Handler for the death event
        private void OnEnemyDeath(GameObject enemy) {
            currentEnemyCount--;

            if (xpDropPrefab != null) {
                Instantiate(xpDropPrefab, enemy.transform.position, Quaternion.identity);
            }

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.AddKill();
            }
        }

        // Calculates a spawn position just outside the camera viewport
        private Vector3 GetSpawnPosition() {
            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

            int side = Random.Range(0, 4);
            Vector3 spawnPos = Vector3.zero;

            switch (side) {
                case 0: // Left
                    spawnPos.x = min.x - spawnOffset;
                    spawnPos.y = Random.Range(min.y + spawnOffset, max.y - spawnOffset);
                    break;
                case 1: // Right
                    spawnPos.x = max.x + spawnOffset;
                    spawnPos.y = Random.Range(min.y + spawnOffset, max.y - spawnOffset);
                    break;
                case 2: // Top
                    spawnPos.y = max.y + spawnOffset;
                    spawnPos.x = Random.Range(min.x + spawnOffset, max.x - spawnOffset);
                    break;
                case 3: // Bottom
                    spawnPos.y = min.y - spawnOffset;
                    spawnPos.x = Random.Range(min.x + spawnOffset, max.x - spawnOffset);
                    break;
            }

            return spawnPos;
        }
    }
}