using UnityEngine;

namespace HexaBit.Core {
    public class DataStrikeAnimationRelay : MonoBehaviour {
        private DataStrikeEffect effect;

        private void Awake() {
            effect = GetComponentInParent<DataStrikeEffect>();
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