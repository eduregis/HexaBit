using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class HealthPickup : PickupBase {
        [Header("Reward")]
        [Tooltip("Amount of health restored. If 0, it will use a percentage instead.")]
        [SerializeField] private int healAmount = 20;
        [Tooltip("Percentage of max health restored (0 to 1). Used if healAmount is 0.")]
        [SerializeField] private float healPercentage = 0.1f;

        [Header("Events")]
        public UnityEvent OnPickUpEvent;

        public void Setup() {
            isCollected = false;
            isKnockbackActive = false;
            isAttracted = false;
            knockbackTimer = 0f;
            targetHeroController = null;
            targetHeroTransform = null;

            gameObject.SetActive(true);
        }

        protected override void OnCollect() {
            if (targetHeroController != null) {
                int healValue = healAmount > 0 ? healAmount : Mathf.RoundToInt(targetHeroController.GetMaxHealth() * healPercentage);
                targetHeroController.Heal(healValue);
                OnPickUpEvent?.Invoke();
                Debug.Log($"[HealthPickup] Healed {healValue} HP.");
            }
        }
    }
}