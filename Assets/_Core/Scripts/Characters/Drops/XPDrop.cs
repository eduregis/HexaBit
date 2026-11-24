using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class XPDrop : PickupBase {
        [Header("Reward")]
        [SerializeField] private int xpValue = 1;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Events")]
        [Tooltip("Called when a hero collect XP.")]
        public UnityEvent OnPickUpEvent;

        private int outlineColorID;

        private void Awake() {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            outlineColorID = Shader.PropertyToID("_OutlineColor");
        }

        /// <summary>
        /// Resets the drop and activates it for reuse.
        /// </summary>
        public void Setup(int value, Color color) {
            xpValue = value;
            isCollected = false;
            isKnockbackActive = false;
            isAttracted = false;
            knockbackTimer = 0f;
            targetHeroController = null;
            targetHeroTransform = null;

            if (spriteRenderer != null && spriteRenderer.material != null) {
                spriteRenderer.material.SetColor(outlineColorID, color);
            }

            gameObject.SetActive(true);
        }

        protected override void OnCollect() {
            int finalXP = xpValue;
            if (targetHeroController != null) {
                finalXP = Mathf.RoundToInt(xpValue * targetHeroController.GlobalXPMultiplier);
            }

            if (GameplayManager.Instance != null) {
                OnPickUpEvent?.Invoke();
                GameplayManager.Instance.AddXP(finalXP, targetHeroController);
            }
        }
    }
}