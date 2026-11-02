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
                }
            }
        }

        private void OnDisable() {
            if (pauseAction != null) {
                pauseAction.performed -= OnPausePerformed;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context) {
            // Block pause input if the Restart menu is open
            if (FindFirstObjectByType<RestartUIManager>() != null) return;

            PauseUIManager pauseMenu = FindFirstObjectByType<PauseUIManager>();

            if (pauseMenu != null) {
                // If open, start the fade out closing sequence
                pauseMenu.ClosePauseMenu();
            } else {
                // Block pause input if time is frozen by another menu (Level Up)
                if (Time.timeScale == 0f) return;

                // If closed, ask the GameplayManager to instantiate it
                if (GameplayManager.Instance != null) {
                    GameplayManager.Instance.OpenPauseMenu();
                }
            }
        }
    }
}