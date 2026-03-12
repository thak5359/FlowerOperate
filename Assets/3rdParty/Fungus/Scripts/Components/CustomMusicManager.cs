using UnityEngine;
using UnityEngine.Audio;
using Fungus;

namespace Fungus
{
    public class CustomMusicManager : MusicManager
    {

        [Header("Center Channel (Original Fungus Sources)")]
        public AudioSource midBGM;
        public AudioSource midAmbience;
        public AudioSource midSFX;
        public AudioSource midVoice;
        public AudioSource midWriterSFX;

        [Header("Spatial Side System")]
        public AudioSource leftSFX;
        public AudioSource rightSFX;

        public override void Init()
        {
            this.transform.position = new Vector3(960f, 540f, 0f);

            base.Init();

            midBGM = audioSourceMusic;
            midAmbience = audioSourceAmbiance;
            midSFX = audioSourceSoundEffect;
            midVoice = audioSourceDefaultVoice;
            midWriterSFX = audioSourceWriterSoundEffect;

            var mixer = FungusManager.Instance.MainAudioMixer;
            midBGM.outputAudioMixerGroup = mixer.BGMGroup;
            midAmbience.outputAudioMixerGroup = mixer.SFXGroup;
            midSFX.outputAudioMixerGroup = mixer.SFXGroup;
            midVoice.outputAudioMixerGroup = mixer.VoiceGroup;
            midWriterSFX.outputAudioMixerGroup = mixer.SFXGroup;

            if (leftSFX == null)
                leftSFX = CreateSpatialSource("LeftAudioSource", new Vector3(-960f, 0f, 0f), mixer.SFXGroup);

            if (rightSFX == null)
                rightSFX = CreateSpatialSource("RightAudioSource", new Vector3(960f, 0f, 0f), mixer.SFXGroup);

        }

        private AudioSource CreateSpatialSource(string name, Vector3 localPos, AudioMixerGroup group)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(this.transform);
            go.transform.localPosition = localPos;

            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = group;
            source.spatialBlend = 1f; // 3D 공간 음향
            source.playOnAwake = false;


            source.minDistance = 1000f;
            source.maxDistance = 2000f;
            source.rolloffMode = AudioRolloffMode.Linear;

            return source;
        }

        // [확장] 특정 채널로 소리를 재생하는 통합 함수
        public void PlayVoice(AudioClip clip, string channel = "MidSFX", float volume = 1f)
        {
            if (clip == null) return;

            switch (channel.ToLower())
            {
                case "left": leftSFX.PlayOneShot(clip, volume); break;
                case "right": rightSFX.PlayOneShot(clip, volume); break;
                case "midbgm": midBGM.PlayOneShot(clip, volume); break;
                case "midambience": midAmbience.PlayOneShot(clip, volume); break;
                case "midvoice": midVoice.PlayOneShot(clip, volume); break;
                case "midwritersfx": midWriterSFX.PlayOneShot(clip, volume); break;
                case "midsfx":
                default: midSFX.PlayOneShot(clip, volume); break;
            }
        }
    }
}