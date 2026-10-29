using UnityEngine;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[status] New Status", menuName = "HexaBit/Status Data")]
    public class StatusData : ScriptableObject {
        [Header("General")]
        public string statusName;
        public Sprite icon;

        [Header("Duration")]
        public float duration = 3f; // 0 = infinite (until removed)

        [Header("Effect Parameters")]
        public bool applySlow = false;
        public float slowPercentage = 0.2f; // 0.2 = 20% speed reduction

        public bool applyDamageOverTime = false;
        public float dotDamagePerSecond = 5f;
        public float dotTickInterval = 1f;

        public bool applyStun = false;
        public float stunDuration = 1f;

        public bool applyDamageReduction = false;
        public float damageReductionPercentage = 0.3f; // 0.3 = 30% less damage dealt

        public bool applyHealOverTime = false;
        public float hotHealPerSecond = 3f;
        public float hotTickInterval = 1f;

        [Header("Visual Feedback")]
        public bool hasOverrideColor = false;
        public Color spriteColorOverride = Color.white;
    }
}