using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace Fungus
{
    [CommandInfo("Audio",
                 "Play Music (Custom)",
                 "지정한 AudioSource를 통해 배경 음악을 재생하고 이전 소스를 관리합니다.")]
    [AddComponentMenu("")]
    public class PlayMusicDirect : Command
    {
        [Tooltip("음악을 재생할 Hierarchy의 AudioSource 객체")]
        [SerializeField] protected AudioSource targetSource;

        [Tooltip("재생할 음악 클립")]
        [SerializeField] protected AudioClip musicClip;

        [Tooltip("무한 반복 여부")]
        [SerializeField] protected bool loop = true;

        [Range(0, 10)]
        [Tooltip("기존 음악을 정지시키고 새 음악이 커지는 페이드 시간")]
        [SerializeField] protected float fadeDuration = 1f;

        // 이전에 사용한 소스를 기억하기 위한 스태틱 변수
        public static AudioSource activeSource;
        public override void OnEnter()
        {
            if (targetSource == null)
            {
                // 씬에서 "BGM_Source"라는 이름을 가진 객체를 찾아서 자동으로 연결
                GameObject go = GameObject.Find("BGM_Source");
                if (go != null) targetSource = go.GetComponent<AudioSource>();
            }

            // 1. 만약 이번 커맨드에서 Target Source가 비어있다면, 이전에 썼던 소스를 가져옵니다.
            if (targetSource == null && activeSource != null)
            {
                targetSource = activeSource;
            }

            // 2. 여전히 null이라면 (첫 실행 등) 에러를 방지하고 다음으로 넘어갑니다.
            if (targetSource == null)
            {
                Debug.LogWarning("Target AudioSource가 지정되지 않았고, 이전 소스 기록도 없습니다.");
                Continue();
                return;
            }

            if (musicClip == null)
            {
                Continue();
                return;
            }

            // --- 이하 로직 동일 ---
            var mainMixer = FungusManager.Instance.MainAudioMixer;
            if (mainMixer != null && mainMixer.BGMGroup != null)
            {
                targetSource.outputAudioMixerGroup = mainMixer.BGMGroup;
            }

            // 이전 소스와 다른 소스를 쓸 경우에만 이전 소스를 정지
            if (activeSource != null && activeSource != targetSource && activeSource.isPlaying)
            {
                activeSource.Stop();
            }

            targetSource.clip = musicClip;
            targetSource.loop = loop;

            // 재생 로직 (FadeIn 코루틴 호출 등)
            StartCoroutine(FadeIn(targetSource, fadeDuration));

            // 현재 사용한 소스를 전역(static) 변수에 저장
            activeSource = targetSource;

            Continue();
        }
        protected IEnumerator FadeIn(AudioSource source, float duration)
        {
            float currentTime = 0;
            source.volume = 0;
            source.Play();

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                source.volume = Mathf.Lerp(0, 1, currentTime / duration);
                yield return null;
            }
            source.volume = 1f;
        }

        public override string GetSummary()
        {
            string sourceName = targetSource != null ? targetSource.name : "None";
            string clipName = musicClip != null ? musicClip.name : "None";
            return $"[{sourceName}] 에서 {clipName} 재생";
        }
    }
}