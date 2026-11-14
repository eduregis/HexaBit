using UnityEngine;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "New AudioData", menuName = "HexaBit/Audio/AudioData")]
    public class AudioData : ScriptableObject {
        [Header("Settings")]
        public string channel = "SFX"; // Must match the AudioMixer Group name
        public bool isOneShot = true;
        public bool loop = false;
        public bool stopPrevious = false;
        public bool replaceable = false;

        [Header("Clips")]
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioClip[] randomClips;

        [Header("Properties")]
        [Range(0, 1)] public float volume = 1;
        public float pitch = 1;

        public void Play() => GameAudioHandler.PlayAudio(this);

        public AudioClip GetClip() {
            if (randomClips != null && randomClips.Length > 0)
                return randomClips[Random.Range(0, randomClips.Length)];
            return clip;
        }

        public float GetPitch() => pitch;
    }
}