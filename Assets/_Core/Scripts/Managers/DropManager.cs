using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class DropManager : MonoBehaviour {
        public static DropManager Instance { get; private set; }

        [System.Serializable]
        public class XPDropTier {
            public string tierName = "Basic";
            public int minXP = 1;
            public Color outlineColor = Color.green;
        }

        [Header("Base XP Scaling")]
        [SerializeField] private int startingBaseXP = 1;
        [SerializeField] private float baseXPIncreasePerMinute = 2f;
        [SerializeField] private int marginOfError = 0;

        [Header("XP Drop Tiers")]
        [SerializeField]
        private List<XPDropTier> tiers = new List<XPDropTier>() {
            new XPDropTier { tierName = "Basic", minXP = 1, outlineColor = Color.green },
            new XPDropTier { tierName = "Great", minXP = 5, outlineColor = Color.cyan },
            new XPDropTier { tierName = "Ultra", minXP = 15, outlineColor = Color.red }
        };

        [Header("Master Tier (Accumulation)")]
        [SerializeField] private Color masterColor = Color.yellow;

        [Header("Prefabs")]
        [SerializeField] private GameObject defaultXPDropPrefab;
        [SerializeField] private GameObject healthPickupPrefab;

        [Header("Health Drop Settings")]
        [Tooltip("Chance (0 to 1) that a normal enemy drops a health pickup instead of XP.")]
        [SerializeField] private float healthDropChance = 0.05f;

        [Header("Pool Settings")]
        [SerializeField] private int maxActiveDrops = 400;

        private List<PickupBase> activeDrops = new List<PickupBase>();
        private Dictionary<GameObject, Queue<PickupBase>> pools = new Dictionary<GameObject, Queue<PickupBase>>();
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
            pools.Clear();
            accumulatedXP = 0;
        }

        public void SpawnXPDrop(Vector3 position) {
            if (GameplayManager.Instance == null || defaultXPDropPrefab == null) return;

            // 1. Chance to drop health instead of XP
            if (healthPickupPrefab != null && Random.value < healthDropChance) {
                SpawnHealthPickup(position);
                return;
            }

            // 2. Standard XP logic
            float gameTime = GameplayManager.Instance.CurrentTime;
            int currentBaseXP = Mathf.RoundToInt(startingBaseXP + (gameTime / 60f) * baseXPIncreasePerMinute);
            int randomRoll = Random.Range(-marginOfError, marginOfError + 1);
            int finalXP = Mathf.Max(1, currentBaseXP + randomRoll);

            XPDropTier chosenTier = GetTierForXP(finalXP);
            Color dropColor = chosenTier.outlineColor;

            if (activeDrops.Count >= maxActiveDrops) {
                accumulatedXP += finalXP;
                return;
            }

            if (accumulatedXP > 0) {
                finalXP += accumulatedXP;
                accumulatedXP = 0;
                dropColor = masterColor;
            }

            XPDrop drop = GetFromPool(defaultXPDropPrefab) as XPDrop;
            drop.transform.position = position;
            drop.Setup(finalXP, dropColor);
            activeDrops.Add(drop);
        }

        public void SpawnHealthPickup(Vector3 position) {
            if (healthPickupPrefab == null) return;
            if (activeDrops.Count >= maxActiveDrops) return; // Don't accumulate health drops, just skip

            HealthPickup pickup = GetFromPool(healthPickupPrefab) as HealthPickup;
            pickup.transform.position = position;
            pickup.Setup();
            activeDrops.Add(pickup);
        }

        public void ReturnPickupToPool(PickupBase pickup) {
            if (pickup == null) return;

            activeDrops.Remove(pickup);
            pickup.gameObject.SetActive(false);

            // Find the correct pool based on the prefab type (we can use a simple tag or name match)
            // To keep it simple, we check the type and enqueue to the correct pool.
            if (pickup is XPDrop && defaultXPDropPrefab != null) {
                EnqueueToPool(defaultXPDropPrefab, pickup);
            } else if (pickup is HealthPickup && healthPickupPrefab != null) {
                EnqueueToPool(healthPickupPrefab, pickup);
            } else {
                Destroy(pickup.gameObject);
            }
        }

        private void EnqueueToPool(GameObject prefab, PickupBase instance) {
            if (!pools.ContainsKey(prefab)) {
                pools[prefab] = new Queue<PickupBase>();
            }
            pools[prefab].Enqueue(instance);
        }

        private PickupBase GetFromPool(GameObject prefab) {
            if (pools.ContainsKey(prefab) && pools[prefab].Count > 0) {
                return pools[prefab].Dequeue();
            }
            GameObject obj = Instantiate(prefab);
            return obj.GetComponent<PickupBase>();
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