using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexaBit.Core {

    [CreateAssetMenu(fileName = "[weapon] New Weapon", menuName = "HexaBit/Weapon Data")]
    public class WeaponData : ScriptableObject {

        private const string DAMAGE_LOC_KEY = "weapon_field_damage";
        private const string COOLDOWN_LOC_KEY = "weapon_field_cooldown";

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
        /// <summary>
        /// Generates a description string showing the differences between two levels.
        /// Example: "+2 Damage, -0.05 Cooldown, +0.2 Range"
        /// </summary>
        public string GetUpgradeDescription(int currentLevelIndex, int nextLevelIndex) {
            if (nextLevelIndex < 0 || nextLevelIndex >= levels.Count) return "";
            if (currentLevelIndex < 0) currentLevelIndex = 0; // Safety fallback

            StringBuilder sb = new StringBuilder();

            // 1. Fixed Fields (Damage & Cooldown)
            float currentDamage = levels[currentLevelIndex].damage;
            float nextDamage = levels[nextLevelIndex].damage;
            if (!Mathf.Approximately(currentDamage, nextDamage)) {
                string damageName = GetLocalizedString(DAMAGE_LOC_KEY, "Damage");
                AppendStat(sb, damageName, nextDamage - currentDamage, false);
            }

            float currentCooldown = levels[currentLevelIndex].cooldown;
            float nextCooldown = levels[nextLevelIndex].cooldown;
            if (!Mathf.Approximately(currentCooldown, nextCooldown)) {
                // Cooldown reduction is usually displayed as negative (faster)
                string cooldownName = GetLocalizedString(COOLDOWN_LOC_KEY, "Cooldown");
                AppendStat(sb, cooldownName, nextCooldown - currentCooldown, true);
            }

            // 2. Dynamic Fields
            foreach (var def in customFields) {
                if (!def.appearsInUpgradeDescription) continue;

                var currentVal = levels[currentLevelIndex].customValues.Find(v => v.parameter == def.parameter);
                var nextVal = levels[nextLevelIndex].customValues.Find(v => v.parameter == def.parameter);

                if (currentVal == null || nextVal == null) continue;

                string paramName = def.localizedName != null && !def.localizedName.IsEmpty
                    ? def.localizedName.GetLocalizedString()
                    : def.parameter.ToString();

                if (currentVal.fieldType == DynamicFieldType.Int) {
                    int diff = nextVal.intValue - currentVal.intValue;
                    if (diff != 0) AppendStat(sb, paramName, diff, false);
                } else if (currentVal.fieldType == DynamicFieldType.Float) {
                    float diff = nextVal.floatValue - currentVal.floatValue;
                    if (!Mathf.Approximately(diff, 0f)) AppendStat(sb, paramName, diff, false);
                }
            }

            return sb.ToString().TrimStart(',', ' ');
        }

        private void AppendStat(StringBuilder sb, string name, float diff, bool isCooldown) {
            if (sb.Length > 0) sb.Append(", ");

            // Format the difference: + or - sign, rounded to 2 decimals if needed
            string sign = diff > 0 ? "+" : "";
            string formattedDiff = diff % 1 == 0 ? diff.ToString("0") : diff.ToString("0.##");

            // For cooldowns, a negative difference (faster) is good, so we keep the minus.
            // For others, a positive difference is good, so we keep the plus.
            sb.Append($"{sign}{formattedDiff} {name}");
        }

        private string GetLocalizedString(string key, string fallback) {
            var localizedString = new LocalizedString("Weapon_Texts", key); // Change "Weapon_Texts" to your actual table name if different
            string result = localizedString.GetLocalizedString();

            // If the key doesn't exist, return the fallback English text
            return string.IsNullOrEmpty(result) || result == key ? fallback : result;
        }

        public GameplayManager.LevelUpOption GetUpgradeOption(HeroController hero) {
            string displayName = localizedName.GetLocalizedString();
            int targetLevelDisplay = 1; // Used for UI (1 to 6)
            bool isWeapon = true;
            System.Action action = null;
            string finalDescription = "";

            if (hero.HasWeapon(this)) {
                // currentLevel is 0-based (0 = Level 1)
                int currentIndex = hero.weaponSlots.First(x => x?.data == this).currentLevel;

                // Next index is simply current + 1
                int nextIndex = Mathf.Clamp(currentIndex + 1, 0, levels.Count - 1);

                // UI display level is index + 1
                targetLevelDisplay = nextIndex + 1;

                // Generate the custom description for upgrades
                finalDescription = GetUpgradeDescription(currentIndex, nextIndex);

                // DEBUG LOG
                Debug.Log($"[WeaponData] {displayName} Upgrade: Index {currentIndex} -> {nextIndex}. Desc: '{finalDescription}'");

                action = () => hero.UpgradeWeapon(this);
            } else {
                targetLevelDisplay = 1;
                // Use the default description for new weapons
                finalDescription = localizedDescription.GetLocalizedString();
                action = () => hero.EquipWeapon(this);
            }

            return new GameplayManager.LevelUpOption {
                displayName = displayName,
                description = finalDescription,
                icon = icon,
                isWeapon = isWeapon,
                targetLevel = targetLevelDisplay,
                onSelected = action
            };
        }
    }

    [System.Serializable]
    public class DynamicFieldDefinition {
        public DynamicParameter parameter;
        public Sprite icon;
        public LocalizedString localizedName;
        public bool appearsInUpgradeDescription = true;
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