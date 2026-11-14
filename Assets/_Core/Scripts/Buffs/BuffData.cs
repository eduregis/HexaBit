using UnityEngine;
using UnityEngine.Localization;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[buff] New Buff", menuName = "HexaBit/Buff Data")]
    public class BuffData : ScriptableObject {
        public LocalizedString localizedName;
        public LocalizedString localizedDescription;
        public Sprite icon;
        public AbilityType type;

        public float[] values = new float[6];

        public float GetValue(int level) {
            if (level <= 0) return 0f;
            int index = Mathf.Clamp(level - 1, 0, values.Length - 1);
            return values[index];
        }

        public int MaxLevel => values.Length;

        // Generates a LevelUpOption for this buff based on the hero's current state
        public GameplayManager.LevelUpOption GetUpgradeOption(HeroController hero) {
            string displayName = localizedName.GetLocalizedString();
            string description = localizedDescription.GetLocalizedString();
            int targetLevel = 1;
            bool isWeapon = false;
            System.Action action = null;

            ActiveBuff existing = hero.activeBuffs.Find(b => b.data == this);
            if (existing != null) {
                targetLevel = existing.currentLevel + 2;

                if (targetLevel > MaxLevel) targetLevel = MaxLevel;

                action = () => hero.ApplyBuff(this);
            } else {
                targetLevel = 1;
                action = () => hero.ApplyBuff(this);
            }

            return new GameplayManager.LevelUpOption {
                displayName = displayName,
                description = description,
                icon = icon,
                isWeapon = isWeapon,
                targetLevel = targetLevel,
                onSelected = action
            };
        }
    }
}