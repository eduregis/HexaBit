using UnityEngine;

namespace HexaBit.Core {
    public class ActiveBuff {
        public BuffData data;

        public int currentLevel;

        public ActiveBuff(BuffData data, int levelIndex = 0) {
            this.data = data;
            this.currentLevel = Mathf.Clamp(levelIndex, 0, data.MaxLevel - 1);
        }

        public void LevelUp() {
            if (currentLevel < data.MaxLevel - 1) {
                currentLevel++;
            }
        }

        public int GetLevelIndex() => currentLevel;

        public int DisplayLevel => currentLevel + 1;
    }
}