using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaBit.Core {
    public class PauseInputHandler : MonoBehaviour {
        private PlayerInput playerInput;
        private InputAction pauseAction;

        private void Awake() {
            playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable() {
            if (playerInput != null && playerInput.actions != null) {
                pauseAction = playerInput.actions["Pause"];
                if (pauseAction != null) {
                    pauseAction.performed += OnPausePerformed;
                } else {
                    Debug.LogWarning("PauseInputHandler: 'Pause' action not found in PlayerInput actions!");
                }
            }
        }

        private void OnDisable() {
            if (pauseAction != null) {
                pauseAction.performed -= OnPausePerformed;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context) {
            // Check if the Restart menu is currently active in the scene
            RestartUIManager restartMenu = FindFirstObjectByType<RestartUIManager>();
            if (restartMenu != null) {
                return; // Cannot pause while game over screen is open
            }

            // Check if the Pause menu is currently active in the scene
            PauseUIManager pauseMenu = FindFirstObjectByType<PauseUIManager>();

            if (pauseMenu != null) {
                // If it's already open, close it
                pauseMenu.ClosePauseMenu();
            } else {
                // Block pause input if time is frozen by another menu (like Level Up)
                if (Time.timeScale == 0f) return;

                // If it's not open, tell the GameplayManager to instantiate it
                if (GameplayManager.Instance != null) {
                    GameplayManager.Instance.OpenPauseMenu();
                }
            }
        }
    }
}