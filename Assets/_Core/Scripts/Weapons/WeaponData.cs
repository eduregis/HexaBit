using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;
using System.Linq;

namespace HexaBit.Core {

    [CreateAssetMenu(fileName = "[weapon] New Weapon", menuName = "HexaBit/Weapon Data")]
    public class WeaponData : ScriptableObject {
        [Header("Localization")]
        public LocalizedString localizedName;
        public LocalizedString localizedDescription;
        public Sprite icon;
        public GameObject attackPrefab;
        public GameObject weaponPrefab;

        [Header("Dynamic Fields (Optionals)")]
        public List<DynamicFieldDefinition> customFields = new List<DynamicFieldDefinition>();

        [Header("Status Effect (Optional)")]
        public StatusData statusData;

        [Header("Levels (Lv.1 ao Lv.6)")]
        public List<WeaponLevelData> levels = new List<WeaponLevelData>();

        // Access methods to facilitate code readability
        public float GetDamage(int levelIndex) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0f;
            return levels[levelIndex].damage;
        }

        public float GetCooldown(int levelIndex) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0f;
            return levels[levelIndex].cooldown;
        }

        public int GetInt(int levelIndex, DynamicParameter param) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0;
            return levels[levelIndex].GetInt(param);
        }

        public float GetFloat(int levelIndex, DynamicParameter param) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return 0f;
            return levels[levelIndex].GetFloat(param);
        }

        public bool GetBool(int levelIndex, DynamicParameter param) {
            if (levelIndex < 0 || levelIndex >= levels.Count) return false;
            return levels[levelIndex].GetBool(param);
        }

        // Generates a LevelUpOption for this weapon based on the hero's current state
        public GameplayManager.LevelUpOption GetUpgradeOption(HeroController hero) {
            string displayName = localizedName.GetLocalizedString();
            string description = localizedDescription.GetLocalizedString();
            int targetLevel = 1;
            bool isWeapon = true;
            System.Action action = null;

            if (hero.HasWeapon(this)) {
                int currentLv = hero.weaponSlots.First(x => x?.data == this).currentLevel + 1;
                targetLevel = currentLv + 1;
                if (targetLevel > 6) targetLevel = 6;

                action = () => hero.UpgradeWeapon(this);
            } else {
                targetLevel = 1;
                action = () => hero.EquipWeapon(this);
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

    [System.Serializable]
    public class DynamicFieldDefinition {
        public DynamicParameter parameter;
        public Sprite icon;
        public LocalizedString localizedName; // New field for UI text
    }

    [System.Serializable]
    public class WeaponLevelData {
        // Mandatory fixed fields
        public float damage;
        public float cooldown;

        // Values for dynamic fields (optional)
        public List<DynamicFieldValue> customValues = new List<DynamicFieldValue>();

        public int GetInt(DynamicParameter param) {
            foreach (var v in customValues)
                if (v.parameter == param && v.fieldType == DynamicFieldType.Int)
                    return v.intValue;
            return 0;
        }

        public float GetFloat(DynamicParameter param) {
            foreach (var v in customValues)
                if (v.parameter == param && v.fieldType == DynamicFieldType.Float)
                    return v.floatValue;
            return 0f;
        }

        public bool GetBool(DynamicParameter param) {
            foreach (var v in customValues)
                if (v.parameter == param && v.fieldType == DynamicFieldType.Bool)
                    return v.boolValue;
            return false;
        }
    }

    [System.Serializable]
    public class DynamicFieldValue {
        public DynamicParameter parameter;
        public DynamicFieldType fieldType;
        public int intValue;
        public float floatValue;
        public bool boolValue;
    }
}