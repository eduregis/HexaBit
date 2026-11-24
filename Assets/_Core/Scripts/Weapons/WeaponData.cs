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

        // THE NEW CURVE STRUCTURE
        [Header("Level Curves")]
        [Tooltip("Index 0 is Level 1, Index 5 is Level 6. The X axis must go from 0 to 5.")]
        public AnimationCurve damageCurve = AnimationCurve.Linear(0, 10, 5, 50);
        public AnimationCurve cooldownCurve = AnimationCurve.Linear(0, 1.0f, 5, 0.5f);

        public List<DynamicFieldDefinition> dynamicCurves = new List<DynamicFieldDefinition>();

        // Access methods updated to read from curves
        public float GetDamage(int levelIndex) {
            return damageCurve.Evaluate(levelIndex);
        }

        public float GetCooldown(int levelIndex) {
            return cooldownCurve.Evaluate(levelIndex);
        }

        public float GetFloat(int levelIndex, DynamicParameter param) {
            var def = customFields.Find(c => c.parameter == param);
            if (def != null) {
                return def.curve.Evaluate(levelIndex);
            }
            return 0f;
        }

        public int GetInt(int levelIndex, DynamicParameter param) {
            return Mathf.RoundToInt(GetFloat(levelIndex, param));
        }

        public bool GetBool(int levelIndex, DynamicParameter param) {
            // Curves are mostly for numbers. For booleans, we keep the 0 to 1 rule.
            return GetFloat(levelIndex, param) > 0.5f;
        }

        /// <summary>
        /// Generates a description string showing the differences between two levels.
        /// </summary>
        public string GetUpgradeDescription(int currentLevelIndex, int nextLevelIndex) {
            if (nextLevelIndex < 0 || nextLevelIndex > 5) return "";
            if (currentLevelIndex < 0) currentLevelIndex = 0;

            StringBuilder sb = new StringBuilder();

            // 1. Fixed Fields (Damage & Cooldown)
            float currentDamage = GetDamage(currentLevelIndex);
            float nextDamage = GetDamage(nextLevelIndex);
            if (!Mathf.Approximately(currentDamage, nextDamage)) {
                string damageName = GetLocalizedString(DAMAGE_LOC_KEY, "Damage");
                AppendStat(sb, damageName, nextDamage - currentDamage, false);
            }

            float currentCooldown = GetCooldown(currentLevelIndex);
            float nextCooldown = GetCooldown(nextLevelIndex);
            if (!Mathf.Approximately(currentCooldown, nextCooldown)) {
                string cooldownName = GetLocalizedString(COOLDOWN_LOC_KEY, "Cooldown");
                AppendStat(sb, cooldownName, nextCooldown - currentCooldown, true);
            }

            // 2. Dynamic Fields
            foreach (var def in customFields) {
                if (!def.appearsInUpgradeDescription) continue;

                float currentVal = GetFloat(currentLevelIndex, def.parameter);
                float nextVal = GetFloat(nextLevelIndex, def.parameter);

                string paramName = def.localizedName != null && !def.localizedName.IsEmpty
                    ? def.localizedName.GetLocalizedString()
                    : def.parameter.ToString();

                if (def.parameter.GetFieldType() == DynamicFieldType.Int) {
                    int diff = Mathf.RoundToInt(nextVal) - Mathf.RoundToInt(currentVal);
                    if (diff != 0) AppendStat(sb, paramName, diff, false);
                } else if (def.parameter.GetFieldType() == DynamicFieldType.Float) {
                    float diff = nextVal - currentVal;
                    if (!Mathf.Approximately(diff, 0f)) AppendStat(sb, paramName, diff, false);
                }
            }

            return sb.ToString().TrimStart(',', ' ');
        }

        private void AppendStat(StringBuilder sb, string name, float diff, bool isCooldown) {
            if (sb.Length > 0) sb.Append(", ");
            string sign = diff > 0 ? "+" : "";
            string formattedDiff = diff % 1 == 0 ? diff.ToString("0") : diff.ToString("0.##");
            sb.Append($"{sign}{formattedDiff} {name}");
        }

        private string GetLocalizedString(string key, string fallback) {
            var localizedString = new LocalizedString("Weapon_Texts", key);
            string result = localizedString.GetLocalizedString();
            return string.IsNullOrEmpty(result) || result == key ? fallback : result;
        }

        public GameplayManager.LevelUpOption GetUpgradeOption(HeroController hero) {
            string displayName = localizedName.GetLocalizedString();
            int targetLevelDisplay = 1;
            bool isWeapon = true;
            System.Action action = null;
            string finalDescription = "";

            if (hero.HasWeapon(this)) {
                int currentIndex = hero.weaponSlots.First(x => x?.data == this).currentLevel;
                int nextIndex = Mathf.Clamp(currentIndex + 1, 0, 5);
                targetLevelDisplay = nextIndex + 1;
                finalDescription = GetUpgradeDescription(currentIndex, nextIndex);
                action = () => hero.UpgradeWeapon(this);
            } else {
                targetLevelDisplay = 1;
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

        // The curve for this dynamic field
        public AnimationCurve curve = AnimationCurve.Linear(0, 1, 5, 10);
    }
}