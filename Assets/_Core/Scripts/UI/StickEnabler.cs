using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

namespace HexaBit.Core {
    /// <summary>
    /// Enables or disables mobile-only UI elements based on the current device type.
    /// Attach this to a parent GameObject or any object with references to mobile-only UI.
    /// </summary>
    public class MobileUIEnabler : MonoBehaviour {
        [Header("Mobile-Only UI Elements")]
        [Tooltip("Visual elements of the on-screen stick (background + handle).")]
        [SerializeField] private GameObject[] stickVisuals;

        [Tooltip("Optional: the OnScreenStick component itself (disables interaction).")]
        [SerializeField] private OnScreenStick onScreenStick;

        [Tooltip("The pause button shown only on mobile.")]
        [SerializeField] private GameObject pauseButton;

        [Header("Settings")]
        [Tooltip("If true, elements are only shown on handheld devices (mobile).")]
        [SerializeField] private bool mobileOnly = true;

        private void Awake() {
            bool isMobile = SystemInfo.deviceType == DeviceType.Handheld;

            // If mobileOnly is true, show only on mobile. Otherwise, show only on desktop.
            bool shouldShow = mobileOnly ? isMobile : !isMobile;

            // Toggle stick visuals
            if (stickVisuals != null) {
                foreach (var visual in stickVisuals) {
                    if (visual != null)
                        visual.SetActive(shouldShow);
                }
            }

            // Toggle stick interaction
            if (onScreenStick != null) {
                onScreenStick.enabled = shouldShow;
            }

            // Toggle pause button
            if (pauseButton != null) {
                pauseButton.SetActive(shouldShow);
            }

            Debug.Log($"MobileUIEnabler: Device is {(isMobile ? "mobile" : "desktop")}. UI elements {(shouldShow ? "enabled" : "disabled")}.");
        }
    }
}