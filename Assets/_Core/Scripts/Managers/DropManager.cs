using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class DropManager : MonoBehaviour {
        public static DropManager Instance { get; private set; }

        [System.Serializable]
        public class XPDropTier {
            public string tierName = "Basic";
            public int minXP = 10;
            public Color outlineColor = Color.green;
        }

        [Header("Base XP Scaling")]
        [Tooltip("Base XP value at the start of the game.")]
        [SerializeField] private int startingBaseXP = 1;

        [Tooltip("How much the Base XP increases per minute.")]
        [SerializeField] private float baseXPIncreasePerMinute = 2f;

        [Tooltip("The randomized variance around the Base XP.")]
        [SerializeField] private int marginOfError = 0;

        [Header("XP Drop Tiers")]
        [Tooltip("Must be ordered from lowest minXP to highest.")]
        [SerializeField]
        private List<XPDropTier> tiers = new List<XPDropTier>() {
            new XPDropTier { tierName = "Basic", minXP = 10, outlineColor = Color.green },
            new XPDropTier { tierName = "Great", minXP = 50, outlineColor = Color.cyan },
            new XPDropTier { tierName = "Ultra", minXP = 150, outlineColor = Color.red }
        };

        // Master is special: Only appears when the screen is full.
        [Header("Master Tier (Accumulation)")]
        [SerializeField] private Color masterColor = Color.yellow;

        [Header("Base Prefab")]
        [SerializeField] private GameObject defaultXPDropPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int maxActiveDrops = 400;

        private List<XPDrop> activeDrops = new List<XPDrop>();
        private Queue<XPDrop> dropPool = new Queue<XPDrop>();
        private int accumulatedXP = 0;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            activeDrops.Clear();
            dropPool.Clear();
            accumulatedXP = 0;
        }

        public void SpawnXPDrop(Vector3 position) {
            if (GameplayManager.Instance == null || defaultXPDropPrefab == null) return;

            // 1. Get current base XP based on time
            float gameTime = GameplayManager.Instance.CurrentTime;
            int currentBaseXP = Mathf.RoundToInt(startingBaseXP + (gameTime / 60f) * baseXPIncreasePerMinute);

            // 2. Roll the randomized XP
            int randomRoll = Random.Range(-marginOfError, marginOfError + 1);
            int finalXP = Mathf.Max(1, currentBaseXP + randomRoll);

            // 3. Determine which tier to use based on the rolled XP
            XPDropTier chosenTier = GetTierForXP(finalXP);
            Color dropColor = chosenTier.outlineColor;

            // 4. Handle the max drops limit
            if (activeDrops.Count >= maxActiveDrops) {
                accumulatedXP += finalXP;
                Debug.Log($"[DropManager] MAX DROPS REACHED. Accumulated: {accumulatedXP} XP. Active: {activeDrops.Count}");
                return;
            }

            if (accumulatedXP > 0) {
                finalXP += accumulatedXP;
                accumulatedXP = 0;
                dropColor = masterColor;
                Debug.Log($"[DropManager] Spawning MASTER Drop with {finalXP} XP.");
            }

            XPDrop drop = GetDropFromPool();
            drop.transform.position = position;

            drop.Setup(finalXP, dropColor);
            activeDrops.Add(drop);
        }


        public void ReturnDropToPool(XPDrop drop) {
            if (drop == null) return;

            activeDrops.Remove(drop);
            drop.gameObject.SetActive(false);
            dropPool.Enqueue(drop);
        }

        private XPDrop GetDropFromPool() {
            if (dropPool.Count > 0) {
                return dropPool.Dequeue();
            }

            GameObject dropObj = Instantiate(defaultXPDropPrefab);
            return dropObj.GetComponent<XPDrop>();
        }

        private XPDropTier GetTierForXP(int xpValue) {
            for (int i = tiers.Count - 1; i >= 0; i--) {
                if (xpValue >= tiers[i].minXP) {
                    return tiers[i];
                }
            }
            return tiers[0];
        }
    }
}