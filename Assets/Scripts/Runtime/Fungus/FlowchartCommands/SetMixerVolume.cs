using Fungus.DentedPixel;
using UnityEngine;
using UnityEngine.Audio;

namespace Fungus
{
    [CommandInfo("Audio",
                 "Set Mixer Volume",
                 "믹서의 특정 그룹 볼륨을 LeanTween으로 부드럽게 조절합니다.")]
    [AddComponentMenu("")]
    public class SetMixerVolume : Command
    {
        public enum AudioGroup { Master, BGM, SFX, Voice }

        [SerializeField] protected AudioGroup audioGroup = AudioGroup.BGM;
        [Range(0, 1)][SerializeField] protected float targetVolume = 1f;
        [Range(0, 30)][SerializeField] protected float fadeDuration = 1f;
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            AudioMixer mixer = FungusManager.Instance.UnityAudioMixer;

            if (mixer == null)
            {
                Debug.LogWarning("FungusManager에서 UnityAudioMixer를 찾을 수 없습니다.");
                Continue();
                return;
            }

            string parameterName = GetParameterName();
            float targetDB = targetVolume > 0 ? Mathf.Log10(targetVolume) * 20 : -80f;

            // 완료 시 실행할 로직
            System.Action onComplete = () => {
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            // 1. 즉시 변경인 경우
            if (Mathf.Approximately(fadeDuration, 0f))
            {
                mixer.SetFloat(parameterName, targetDB);
                onComplete();
                return;
            }

            // 2. 페이드 변경인 경우 (LeanTween 사용)
            float startDB;
            mixer.GetFloat(parameterName, out startDB);

            LeanTween.value(gameObject, startDB, targetDB, fadeDuration)
                .setOnUpdate((float val) => {
                    mixer.SetFloat(parameterName, val);
                })
                .setOnComplete(() => {
                    onComplete();
                })
                .setEase(LeanTweenType.linear); // 볼륨은 선형(Linear) 페이드가 가장 자연스럽습니다.

            // [핵심 버그 수정] 기다리지 않는 옵션이라면 즉시 다음 명령어로 이동
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected string GetParameterName()
        {
            switch (audioGroup)
            {
                case AudioGroup.BGM: return "BGM Volume";
                case AudioGroup.SFX: return "SFX Volume";
                case AudioGroup.Voice: return "Voice Volume";
                case AudioGroup.Master: return "Master Volume";
                default: return "";
            }
        }

        public override string GetSummary()
        {
            return $"{audioGroup} 볼륨을 {targetVolume}까지 {fadeDuration}초간 조절";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}