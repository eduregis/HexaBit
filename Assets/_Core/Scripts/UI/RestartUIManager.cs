using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace HexaBit.Core {
    public class RestartUIManager : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private GameObject restartPanel;
        [SerializeField] private HUDButton restartButton;
        [SerializeField] private HUDButton quitButton;

        private HUDButton[] _buttons;

        private void Awake() {
            _buttons = new HUDButton[] { restartButton, quitButton };

            if (restartPanel != null) restartPanel.SetActive(false);

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
            if (restartPanel != null) restartPanel.SetActive(true);

            if (restartButton != null && restartButton.button != null && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(restartButton.button.gameObject);
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

        public void RestartGame() {
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame() {
            Debug.Log("RestartUIManager: QuitGame called");
        }
    }
}