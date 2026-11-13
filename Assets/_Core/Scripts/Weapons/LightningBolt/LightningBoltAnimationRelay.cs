using UnityEngine;

namespace HexaBit.Core {
    public class LightningEventRelay : MonoBehaviour {
        private LightningBoltEffect effect;

        private void Awake() {
            effect = GetComponentInParent<LightningBoltEffect>();
        }

        // This will be called by the Animation Event on the Body's clip
        public void OnImpactFrame() {
            if (effect != null) effect.OnImpactFrame();
        }

        // This will be called by the Animation Event at the end of the Body's clip
        public void OnAnimationEnd() {
            if (effect != null) effect.OnAnimationEnd();
        }
    }
}