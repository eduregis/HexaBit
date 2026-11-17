using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class HUDButton : MonoBehaviour {
        [Header("UI References")]
        public Image backgroundImage;
        public Image selectionGlow;
        public TextMeshProUGUI labelText;

        [Header("Events")]
        [Tooltip("Called when the button receives focus.")]
        public UnityEvent OnFocusEvent;

        public Button button;

        /// <summary>
        /// Enables or disables the selection glow to indicate focus.
        /// </summary>
        public void SetSelected(bool isSelected) {
            if (selectionGlow != null) {
                OnFocusEvent?.Invoke();
                selectionGlow.gameObject.SetActive(isSelected);
            }
        }
    }
}