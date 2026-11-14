using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HexaBit.Core {
    [DefaultExecutionOrder(-1000)]
    public class GameAudioHandler : MonoBehaviour {
        public static GameAudioHandler Instance { get; private set; }

        [System.Serializable]
        public class AudioSourceLink {
            public string channel;
            public AudioSource source;
            public AudioSource complementary; // Used for crossfading music
        }

        [SerializeField] private AudioSourceLink[] sourceLinks;

        private static Dictionary<string, Coroutine> _runningCoroutines = new Dictionary<string, Coroutine>();

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void PlayAudio(AudioData data) {
            var link = Instance.sourceLinks.FirstOrDefault(src => src.channel == data.channel);
            if (link == null || link.source == null) return;

            if (data.stopPrevious) link.source.Stop();

            link.source.volume = data.volume;
            link.source.loop = data.loop;
            link.source.pitch = data.GetPitch();

            AudioClip clipToPlay = data.GetClip();

            if (data.isOneShot) {
                link.source.PlayOneShot(clipToPlay);
                return;
            }

            if (link.source.clip == clipToPlay && link.source.isPlaying) return;

            // Stop any existing fade coroutine on this channel
            if (_runningCoroutines.ContainsKey(data.channel) && _runningCoroutines[data.channel] != null) {
                Instance.StopCoroutine(_runningCoroutines[data.channel]);
            }

            _runningCoroutines[data.channel] = Instance.StartCoroutine(Instance.FadeTracks(link.source, clipToPlay, data));
        }

        private IEnumerator FadeTracks(AudioSource source, AudioClip clip, AudioData data) {
            // Fade out
            while (source.volume > 0) {
                source.volume -= Time.deltaTime * 2;
                yield return null;
            }

            source.Stop();
            source.clip = clip;
            source.Play();

            // Fade in
            while (source.volume < data.volume) {
                source.volume = Mathf.Min(source.volume + Time.deltaTime * 2, data.volume);
                yield return null;
            }

            _runningCoroutines[data.channel] = null;
        }

        public static void StopAudio(string channel) {
            var link = Instance.sourceLinks.FirstOrDefault(src => src.channel == channel);
            if (link != null && link.source != null) {
                link.source.Stop();
            }
        }
    }
}