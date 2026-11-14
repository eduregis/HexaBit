using UnityEngine;

namespace HexaBit.Core {
    public class WeaponBase : MonoBehaviour {
        [Header("Weapon Data")]
        public WeaponData data;

        public int currentLevel = 0;

        private float timer;
        protected HeroController hero;

        private void Start() {
            if (hero == null) hero = GetComponentInParent<HeroController>();
        }

        public void SetHeroController(HeroController heroController) {
            hero = heroController;
        }

        private void Update() {
            if (data == null || data.levels.Count == 0) return;
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
            currentLevel = Mathf.Clamp(levelIndex, 0, data.levels.Count - 1);
            gameObject.name = weaponData.localizedName.GetLocalizedString();
        }

        public virtual void LevelUp() {
            if (currentLevel < data.levels.Count - 1) {
                currentLevel++;
                Debug.Log($"{data.localizedName.GetLocalizedString()} up to Lv.{currentLevel + 1}");
            } else {
                Debug.Log($"{data.localizedName.GetLocalizedString()} is in maximum level!");
            }
        }

        public int GetLevelIndex() => currentLevel;
    }
}