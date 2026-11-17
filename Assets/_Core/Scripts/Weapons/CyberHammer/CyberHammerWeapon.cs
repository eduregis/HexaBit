using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class CyberHammerWeapon : WeaponBase {

        [Header("Events")]
        [Tooltip("Called when the weapon fires.")]
        public UnityEvent OnFireEvent;

        protected override void Fire() {
            if (data.attackPrefab == null) return;

            GameObject effectGO = Instantiate(data.attackPrefab, transform.position, Quaternion.identity);
            CyberHammerEffect effect = effectGO.GetComponent<CyberHammerEffect>();
            if (effect != null) {
                OnFireEvent?.Invoke();
                Vector2 direction = hero.FacingDirection;
                effect.Initialize(data, currentLevel, direction, hero);
            }
        }
    }
}