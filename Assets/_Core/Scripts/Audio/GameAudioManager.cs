using UnityEngine;
using UnityEngine.Audio;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "GameAudioManager", menuName = "HexaBit/Audio/GameAudioManager")]
    public class GameAudioManager : ScriptableObject {
        [SerializeField] private AudioMixer mixer;
        public AudioMixer Mixer => mixer;

        public void SetVolume(string channel, float volume) =>
            mixer.SetFloat(channel, volume == 0 ? -144 : 20.0f * Mathf.Log10(volume));

        public float GetVolume(string channel) =>
            mixer.GetFloat(channel, out var volume) ? Mathf.Pow(10.0f, volume / 20.0f) : 0;
    }
}