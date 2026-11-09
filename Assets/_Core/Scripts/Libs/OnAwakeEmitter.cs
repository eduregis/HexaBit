using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class OnAwakeEmitter : MonoBehaviour {
        [SerializeField] private UnityEvent onAwake;
        private void Awake() => onAwake?.Invoke();
    }

    public class DelayedEventHolder : MonoBehaviour {
        [SerializeField] private UnityEvent onInvoke;
        [SerializeField] private float delay = 1f;

        public void Invoke() {
            Invoke(nameof(ExecuteEvent), delay);
        }

        private void ExecuteEvent() {
            onInvoke?.Invoke();
        }
    }
}