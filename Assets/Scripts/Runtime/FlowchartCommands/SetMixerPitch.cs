using Fungus.DentedPixel;
using UnityEngine;
using UnityEngine.Audio;

namespace Fungus
{
    [CommandInfo("Audio",
                 "Set Mixer Pitch",
                 "LeanTween을 사용하여 믹서 그룹의 피치를 안전하게 조절합니다.")]
    [AddComponentMenu("")]
    public class SetMixerPitch : Command
    {
        public enum AudioGroup { Master, Music, SFX, Voice }

        [SerializeField] protected AudioGroup audioGroup = AudioGroup.Music;
        [Range(0.5f, 2.0f)][SerializeField] protected float targetPitch = 1f;
        [Range(0, 30)][SerializeField] protected float fadeDuration = 1f;
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            AudioMixer mixer = FungusManager.Instance.UnityAudioMixer;
            if (mixer == null)
            {
                Continue();
                return;
            }

            string parameterName = GetParameterName();

            // 1. 페이드 시간이 0일 경우 (즉시 변경)
            if (Mathf.Approximately(fadeDuration, 0f))
            {
                mixer.SetFloat(parameterName, targetPitch);
                // 즉시 변경 시에는 대기 여부와 상관없이 무조건 다음으로 넘어가야 합니다.
                Continue();
                return;
            }

            // 2. 페이드 시간이 있을 경우 (LeanTween 사용)
            float startPitch;
            mixer.GetFloat(parameterName, out startPitch);

            // [추가] 동일한 파라미터에 실행 중인 이전 트윈이 있다면 취소 (충돌 방지)
            LeanTween.cancel(gameObject);

            LeanTween.value(gameObject, startPitch, targetPitch, fadeDuration)
                .setOnUpdate((float p) => {
                    mixer.SetFloat(parameterName, p);
                })
                .setOnComplete(() => {
                    // 페이드가 끝났을 때만 '기다려달라고 했던' 경우에 한해 Continue 호출
                    if (waitUntilFinished)
                    {
                        Continue();
                    }
                })
                .setEase(LeanTweenType.easeInOutQuad);

            // [중요] 기다리지 않는 옵션이라면 LeanTween 시작 직후 즉시 다음으로 이동
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected string GetParameterName()
        {
            switch (audioGroup)
            {
                case AudioGroup.Music: return "BGM Pitch";
                case AudioGroup.SFX: return "SFX Pitch";
                case AudioGroup.Voice: return "Voice Pitch";
                case AudioGroup.Master: return "Master Pitch";
                default: return "";
            }
        }

        public override string GetSummary()
        {
            return $"{audioGroup} 피치를 {targetPitch}로 ({fadeDuration}초)";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}