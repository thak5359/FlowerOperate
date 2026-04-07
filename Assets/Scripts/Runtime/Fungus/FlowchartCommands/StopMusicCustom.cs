using UnityEngine;
using System.Collections;
using Fungus;
using Fungus.DentedPixel; // LeanTween 네임스페이스 확인

namespace Fungus
{
    [CommandInfo("Audio",
                 "Stop Music (Custom)",
                 "현재 재생 중인 커스텀 배경 음악을 정지합니다.")]
    [AddComponentMenu("")]
    public class StopMusicDirect : Command
    {
        [Tooltip("음악을 정지할 특정 AudioSource (비워두면 마지막으로 재생된 소스를 정지)")]
        [SerializeField] protected AudioSource targetSource;

        [Range(0, 10)]
        [Tooltip("음악이 서서히 사라지는 시간")]
        [SerializeField] protected float fadeDuration = 1f;

        [Tooltip("정지가 완료될 때까지 대기할지 여부")]
        [SerializeField] protected bool waitUntilFinished = true;

        public override void OnEnter()
        {
            // 1. 타겟 소스 결정
            AudioSource sourceToStop = targetSource;
            if (sourceToStop == null)
            {
                sourceToStop = PlayMusicDirect.activeSource;
            }

            if (sourceToStop == null)
            {
                GameObject go = GameObject.Find("BGM_Source");
                if (go != null) sourceToStop = go.GetComponent<AudioSource>();
            }

            // 정지할 소스가 없거나 재생 중이 아니면 즉시 다음으로 넘어감
            if (sourceToStop == null || !sourceToStop.isPlaying)
            {
                Continue();
                return;
            }

            // [추가] 진행 중인 모든 볼륨 관련 트윈 취소 (충돌 방지)
            LeanTween.cancel(sourceToStop.gameObject);

            // 2. 공통 정지 로직 정의
            System.Action stopAction = () => {
                sourceToStop.Stop();
                sourceToStop.volume = 1f; // 다음 재생을 위해 볼륨 초기화
            };

            // 3. 실행 분기
            if (fadeDuration <= 0)
            {
                // 즉시 정지 모드
                stopAction();
                Continue();
            }
            else
            {
                // 페이드 아웃 모드
                LeanTween.value(sourceToStop.gameObject, sourceToStop.volume, 0f, fadeDuration)
                    .setOnUpdate((float v) => {
                        sourceToStop.volume = v;
                    })
                    .setOnComplete(() => {
                        stopAction();
                        if (waitUntilFinished)
                        {
                            Continue();
                        }
                    });

                // 대기하지 않는 옵션이라면 즉시 다음 커맨드로 이동
                if (!waitUntilFinished)
                {
                    Continue();
                }
            }
        }

        public override string GetSummary()
        {
            return fadeDuration > 0 ? $"{fadeDuration}초간 페이드 아웃 후 정지" : "즉시 정지";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }
    }
}