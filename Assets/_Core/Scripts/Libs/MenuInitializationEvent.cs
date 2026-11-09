using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class MenuInitializationEvent : MonoBehaviour {
        [Tooltip("If true, freezes the game (Time.timeScale = 0) when this event is triggered.")]
        [SerializeField] private bool pauseGameOnInit = true;

        public UnityEvent onInitialize;

        public void InvokeEvent() {
            if (pauseGameOnInit) {
                Time.timeScale = 0f;
            }
            onInitialize?.Invoke();
        }
    }
}