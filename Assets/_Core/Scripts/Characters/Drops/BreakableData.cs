using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[breakable] New Breakable", menuName = "HexaBit/Breakable Data")]
    public class BreakableData : ScriptableObject {
        [Header("General")]
        public string objectName;
        public Sprite sprite;
        public int maxHealth = 20;

        [Header("Guaranteed Drop")]
        [Tooltip("If set, this drop is ALWAYS spawned when the object breaks.")]
        public GameObject guaranteedDropPrefab;

        [Header("Random Drop Pool")]
        [Tooltip("If the guaranteed drop is empty, one of these will be chosen (can include 'nothing' by leaving an entry empty).")]
        public List<GameObject> randomDropPool = new List<GameObject>();
        [Range(0f, 1f)]
        public float randomDropChance = 0.5f;
    }
}