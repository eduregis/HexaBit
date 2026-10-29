using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class LevelUpInitializationEvent : MonoBehaviour {
        public UnityEvent onInitialize;
        public void InvokeEvent() {
            Time.timeScale = 0f;
            onInitialize?.Invoke();
        }
    }
}