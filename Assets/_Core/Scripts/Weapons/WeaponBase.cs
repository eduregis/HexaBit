using UnityEngine;

namespace HexaBit.Core {
    public class WeaponBase : MonoBehaviour {
        [Header("Weapon Data")]
        public WeaponData data;

        public int currentLevel = 0;

        // Maximum index for levels (0 to 5 = 6 levels)
        public const int MAX_LEVEL_INDEX = 5;

        private float timer;
        protected HeroController hero;

        private void Start() {
            if (hero == null) hero = GetComponentInParent<HeroController>();
        }

        public void SetHeroController(HeroController heroController) {
            hero = heroController;
        }

        private void Update() {
            // Check if data exists instead of checking a list count
            if (data == null) return;
            if (hero == null || hero.IsDead) return;

            timer += Time.deltaTime;
            float baseCooldown = data.GetCooldown(currentLevel);
            float cooldown = baseCooldown * hero.GlobalCooldownModifier;

            if (timer >= cooldown) {
                timer = 0f;
                Fire();
            }
        }

        protected virtual void Fire() {
            Debug.Log($"{data.localizedName.GetLocalizedString()} (Nv.{currentLevel + 1}) Generic fire!");
        }

        public void Initialize(WeaponData weaponData, int levelIndex = 0) {
            data = weaponData;
            // Clamp using the new constant
            currentLevel = Mathf.Clamp(levelIndex, 0, MAX_LEVEL_INDEX);
            gameObject.name = weaponData.localizedName.GetLocalizedString();
        }

        public virtual void LevelUp() {
            // Check against the constant
            if (currentLevel < MAX_LEVEL_INDEX) {
                currentLevel++;
                Debug.Log($"{data.localizedName.GetLocalizedString()} up to Lv.{currentLevel + 1}");
            } else {
                Debug.Log($"{data.localizedName.GetLocalizedString()} is in maximum level!");
            }
        }

        public int GetLevelIndex() => currentLevel;
    }
}