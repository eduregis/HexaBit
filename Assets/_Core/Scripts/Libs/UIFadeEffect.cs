using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace HexaBit.Core {
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFadeEffect : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        [Header("Events")]
        public UnityEvent OnFadeInComplete;
        public UnityEvent OnFadeOutComplete;

        private CanvasGroup canvasGroup;
        private Coroutine fadeCoroutine;

        private void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        public void FadeIn() {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            gameObject.SetActive(true);
            fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration, OnFadeInComplete));
        }

        public void FadeOut() {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0f, fadeOutDuration, OnFadeOutComplete));
        }

        private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration, UnityEvent onCompleteEvent) {
            float timer = 0f;
            canvasGroup.alpha = startAlpha;

            while (timer < duration) {
                timer += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            fadeCoroutine = null;
            onCompleteEvent?.Invoke();
        }
    }
}