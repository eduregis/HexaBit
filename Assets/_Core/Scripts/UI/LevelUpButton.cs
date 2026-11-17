using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class LevelUpButton : MonoBehaviour {
        [Header("UI References")]
        public Image frameImage;
        public Image iconImage;
        public Image backgroundImage;
        public Image selectionGlow;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI levelText;

        [Header("Events")]
        [Tooltip("Called when the button receives focus.")]
        public UnityEvent OnFocusEvent;

        public Button button;

        public void SetSelected(bool isSelected) {
            if (selectionGlow != null) {
                OnFocusEvent?.Invoke();
                selectionGlow.gameObject.SetActive(isSelected);
            }
        }
    }
}