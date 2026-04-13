using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{
    [CommandInfo("Audio",
                 "Play Voice (Custom)",
                 "지정한 AudioSource를 통해 보이스를 순차 재생하며 Voice 믹서 그룹으로 출력합니다.")]
    [AddComponentMenu("")]
    public class PlayVoiceCustom : Command // 클래스 이름을 유니크하게 변경하여 CS0101 에러 방지
    {
        [Tooltip("보이스를 재생할 Hierarchy의 AudioSource 객체 (비워두면 Voice_Source를 찾습니다)")]
        [SerializeField] protected AudioSource targetSource;

        [Tooltip("순서대로 재생할 보이스 클립 리스트")]
        [SerializeField] protected List<AudioClip> soundClips = new List<AudioClip>();

        [Range(0, 1)]
        [SerializeField] protected float volume = 1;

        [Tooltip("모든 보이스가 끝날 때까지 다음 커맨드 실행을 대기합니다.")]
        [SerializeField] protected bool waitUntilFinished = true;

        protected static AudioSource previousSource;

        public override void OnEnter()
        {
            // 1. Target Source 자동 할당 및 기억 로직
            if (targetSource == null)
            {
                GameObject go = GameObject.Find("Voice_Source");
                if (go != null) targetSource = go.GetComponent<AudioSource>();
            }

            if (targetSource == null && previousSource != null)
            {
                targetSource = previousSource;
            }

            if (targetSource == null || soundClips == null || soundClips.Count == 0)
            {
                Debug.LogWarning("Target Source 또는 보이스 클립이 없습니다.");
                Continue();
                return;
            }

            // 2. Voice 믹서 그룹 강제 할당 (MusicManager 간섭 완전 차단)
            var mainMixer = FungusManager.Instance.MainAudioMixer;
            if (mainMixer != null && mainMixer.VoiceGroup != null)
            {
                targetSource.outputAudioMixerGroup = mainMixer.VoiceGroup;
            }

            // 3. 순차 재생 시작 (MusicManager를 거치지 않고 targetSource에서 직접 재생)
            StartCoroutine(PlayVoiceSequence());

            previousSource = targetSource;

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected IEnumerator PlayVoiceSequence()
        {
            foreach (var clip in soundClips)
            {
                if (clip == null) continue;

                // MusicManager를 쓰지 않고 targetSource에서 직접 PlayOneShot 실행
                targetSource.PlayOneShot(clip, volume);

                // 해당 클립이 끝날 때까지 대기
                yield return new WaitForSeconds(clip.length);
            }

            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (soundClips.Count == 0) return "No voice clips selected";
            return $"{soundClips.Count}개의 보이스 순차 재생";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}