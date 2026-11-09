using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace HexaBit.Core {
    public class PauseUIManager : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private HUDButton resumeButton;
        [SerializeField] private HUDButton restartButton;
        [SerializeField] private HUDButton quitButton;

        private HUDButton[] _buttons;
        private bool _isClosing = false;

        private void Awake() {
            _buttons = new HUDButton[] { resumeButton, restartButton, quitButton };

            if (pausePanel != null) pausePanel.SetActive(false);

            if (resumeButton != null && resumeButton.button != null) {
                resumeButton.button.onClick.RemoveAllListeners();
                resumeButton.button.onClick.AddListener(ClosePauseMenu);
            }

            if (restartButton != null && restartButton.button != null) {
                restartButton.button.onClick.RemoveAllListeners();
                restartButton.button.onClick.AddListener(RestartGame);
            }

            if (quitButton != null && quitButton.button != null) {
                quitButton.button.onClick.RemoveAllListeners();
                quitButton.button.onClick.AddListener(QuitGame);
            }

            SetupSelectionListeners();
        }

        private void Start() {
            if (pausePanel != null) pausePanel.SetActive(true);

            if (resumeButton != null && resumeButton.button != null && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(resumeButton.button.gameObject);
                SetSelectedButton(0);
            }
        }

        private void SetupSelectionListeners() {
            for (int i = 0; i < _buttons.Length; i++) {
                if (_buttons[i] == null || _buttons[i].button == null) continue;

                EventTrigger trigger = _buttons[i].button.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = _buttons[i].button.gameObject.AddComponent<EventTrigger>();

                trigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry();
                selectEntry.eventID = EventTriggerType.Select;
                int capturedIndex = i;
                selectEntry.callback.AddListener((data) => {
                    SetSelectedButton(capturedIndex);
                });
                trigger.triggers.Add(selectEntry);
            }
        }

        private void SetSelectedButton(int index) {
            for (int i = 0; i < _buttons.Length; i++) {
                if (_buttons[i] != null) {
                    _buttons[i].SetSelected(i == index);
                }
            }
        }

        public void ClosePauseMenu() {
            if (_isClosing) return;
            _isClosing = true;

            UIFadeEffect fade = GetComponent<UIFadeEffect>();
            if (fade != null) {
                fade.FadeOut();
            } else {
                FinishClose();
            }
        }

        public void FinishClose() {
            Time.timeScale = 1f;

            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null);
            }

            Destroy(gameObject);
        }

        public void RestartGame() {
            Time.timeScale = 1f;
            if (GameplayManager.Instance != null) GameplayManager.Instance.SetTimerPaused(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame() {
            Debug.Log("PauseUIManager: QuitGame called");
        }
    }
}