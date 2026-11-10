using UnityEngine;

namespace HexaBit.Core {
    public class DropManager : MonoBehaviour {
        public static DropManager Instance { get; private set; }

        [System.Serializable]
        public class XPDropTier {
            public string tierName = "Basic";
            public float startTime = 0f; // When this tier starts appearing
            public int xpValue = 1;
            public Color outlineColor = Color.green;
            public GameObject prefab; // Optional: use a different prefab per tier
        }

        [Header("XP Drop Tiers")]
        [SerializeField] private XPDropTier basicTier = new XPDropTier { tierName = "Basic", startTime = 0f, xpValue = 1, outlineColor = Color.green };
        [SerializeField] private XPDropTier greatTier = new XPDropTier { tierName = "Great", startTime = 120f, xpValue = 5, outlineColor = Color.cyan };
        [SerializeField] private XPDropTier ultraTier = new XPDropTier { tierName = "Ultra", startTime = 300f, xpValue = 10, outlineColor = Color.red };

        [Header("Base Prefab")]
        [Tooltip("If a tier doesn't have a specific prefab, this one will be used.")]
        [SerializeField] private GameObject defaultXPDropPrefab;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Spawns an XP drop at the specified position, choosing the tier based on current game time.
        /// </summary>
        public void SpawnXPDrop(Vector3 position) {
            if (GameplayManager.Instance == null) return;

            float currentTime = GameplayManager.Instance.CurrentTime;
            XPDropTier chosenTier = GetTierForTime(currentTime);

            if (chosenTier == null) return;

            // Use tier-specific prefab if available, otherwise fall back to default
            GameObject prefabToSpawn = chosenTier.prefab != null ? chosenTier.prefab : defaultXPDropPrefab;
            if (prefabToSpawn == null) return;

            GameObject dropObj = Instantiate(prefabToSpawn, position, Quaternion.identity);

            XPDrop xpDrop = dropObj.GetComponent<XPDrop>();
            if (xpDrop != null) {
                // Send the tier data to the drop
                xpDrop.Setup(chosenTier.xpValue, chosenTier.outlineColor);
            }
        }

        private XPDropTier GetTierForTime(float time) {
            // Check from highest to lowest to get the most appropriate tier
            if (time >= ultraTier.startTime) return ultraTier;
            if (time >= greatTier.startTime) return greatTier;
            return basicTier;
        }
    }
}