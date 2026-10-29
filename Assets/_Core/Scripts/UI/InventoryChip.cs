using UnityEngine;
using UnityEngine.UI;

namespace HexaBit.Core {
    public class InventoryChip : MonoBehaviour {
        [Header("UI References")]
        public Image frameImage;
        public Image iconImage;

        public void SetChip(Sprite icon, Sprite frameSprite = null) {
            // Show the chip
            gameObject.SetActive(true);

            // Set icon (always)
            if (iconImage != null) {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            // Set frame sprite (if provided)
            if (frameImage != null) {
                if (frameSprite != null) {
                    Debug.Log($"Frame Sprite: {frameSprite.name}");
                    frameImage.sprite = frameSprite;
                    frameImage.enabled = true;
                } else {
                    // Se não tiver frame, esconde a imagem do frame
                    frameImage.sprite = null;
                    frameImage.enabled = false;
                }
            }
        }

        public void ClearChip() {
            // Hide the chip entirely
            gameObject.SetActive(false);

            // Clear references to avoid lingering data
            if (iconImage != null) {
                iconImage.sprite = null;
            }
            if (frameImage != null) {
                frameImage.sprite = null;
            }
        }
    }
}