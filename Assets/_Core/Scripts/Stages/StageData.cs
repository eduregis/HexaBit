using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[stage] New Stage", menuName = "HexaBit/Stage Data")]
    public class StageData : ScriptableObject {
        [System.Serializable]
        public class EnemySpawnInfo {
            public EnemyData enemy;
            public float startTime = 0f; // Seconds
            public float endTime = 9999f; // Seconds (default means it never stops)
        }

        [System.Serializable]
        public class SubBossSpawnInfo {
            public float interval = 60f; // Time between sub-boss spawns
            public float scaleMultiplier = 1.5f;
            public float healthMultiplier = 5f;
            public float damageMultiplier = 2f;
            public List<StatusData> statusPool;
        }

        [Header("Wave Settings")]
        public float spawnInterval = 1.5f;
        public int maxEnemies = 20;
        public float waveInterval = 10f;

        [Header("Enemy Pool")]
        public List<EnemySpawnInfo> enemyPool = new List<EnemySpawnInfo>();

        [Header("Sub-Boss Settings")]
        public SubBossSpawnInfo subBossSettings;
    }
}