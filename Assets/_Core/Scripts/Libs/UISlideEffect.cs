using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class UISlideEffect : MonoBehaviour {
        public enum Direction { Right, Left, Top, Bottom }

        [Header("Settings")]
        [SerializeField] private Direction enterFrom = Direction.Right;
        [SerializeField] private float offset = 800f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float stagger = 0.05f;

        [Header("References")]
        [SerializeField] private List<RectTransform> targets;

        [Header("Events")]
        public UnityEvent OnEnterComplete;
        public UnityEvent OnExitComplete;

        private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
        private Coroutine animCoroutine;

        private void Awake() {
            foreach (var rt in targets) {
                if (rt != null) originalPositions[rt] = rt.anchoredPosition;
            }
        }

        public void PlayEnter() {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(Animate(true, OnEnterComplete));
        }

        public void PlayExit() {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(Animate(false, OnExitComplete));
        }

        private IEnumerator Animate(bool entering, UnityEvent onComplete) {
            Vector2 dir = enterFrom switch {
                Direction.Right => Vector2.right,
                Direction.Left => Vector2.left,
                Direction.Top => Vector2.up,
                Direction.Bottom => Vector2.down,
                _ => Vector2.right
            };

            Vector2 startOffset = entering ? dir * offset : Vector2.zero;
            Vector2 endOffset = entering ? Vector2.zero : dir * offset;

            foreach (var kvp in originalPositions) kvp.Key.anchoredPosition = kvp.Value + startOffset;

            float timer = 0f;
            while (timer < duration + (originalPositions.Count * stagger)) {
                timer += Time.unscaledDeltaTime;
                int i = 0;
                foreach (var kvp in originalPositions) {
                    float t = Mathf.Clamp01((timer - (i * stagger)) / duration);
                    t = 1f - Mathf.Pow(1f - t, 3f); // Ease Out
                    kvp.Key.anchoredPosition = Vector2.Lerp(kvp.Value + startOffset, kvp.Value + endOffset, t);
                    i++;
                }
                yield return null;
            }

            foreach (var kvp in originalPositions) kvp.Key.anchoredPosition = kvp.Value + endOffset;

            animCoroutine = null;
            onComplete?.Invoke();
        }
    }
}