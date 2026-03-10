using UnityEngine;
using Fungus;

namespace Fungus
{
    [CommandInfo("Audio", "Play Sound (Custom)", "7가지 채널 중 원하는 위치와 용도를 선택해 소리를 재생합니다.")]
    public class PlaySoundCustom : Command
    {
        // 7종의 채널을 드롭다운 메뉴로 정의
        public enum SoundChannel
        {
            MidSFX,
            MidVoice,
            MidBGM,
            MidAmbience,
            MidWriterSFX,
            Left,
            Right
        }

        [SerializeField] protected AudioClip soundClip;
        [SerializeField] protected SoundChannel channel = SoundChannel.MidSFX;
        [Range(0, 1)][SerializeField] protected float volume = 1f;
        [SerializeField] protected bool waitUntilFinished;

        public override void OnEnter()
        {
            if (soundClip == null)
            {
                Continue();
                return;
            }

            // 개조한 CustomMusicManager 인스턴스 가져오기
            var musicManager = FungusManager.Instance.MusicManager as CustomMusicManager;

            if (musicManager != null)
            {
                AudioSource targetSource = null;

                // Enum 값에 따라 정확한 AudioSource 매칭
                switch (channel)
                {
                    case SoundChannel.Left: targetSource = musicManager.leftSFX; break;
                    case SoundChannel.Right: targetSource = musicManager.rightSFX; break;
                    case SoundChannel.MidVoice: targetSource = musicManager.midVoice; break;
                    case SoundChannel.MidBGM: targetSource = musicManager.midBGM; break;
                    case SoundChannel.MidAmbience: targetSource = musicManager.midAmbience; break;
                    case SoundChannel.MidWriterSFX: targetSource = musicManager.midWriterSFX; break;
                    case SoundChannel.MidSFX:
                    default: targetSource = musicManager.midSFX; break;
                }

                // Null 체크 후 재생
                if (targetSource != null)
                {
                    targetSource.PlayOneShot(soundClip, volume);

                    if (waitUntilFinished)
                    {
                        Invoke("DoWait", soundClip.length);
                    }
                    else
                    {
                        Continue();
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlaySoundCustom] {channel} 채널의 AudioSource가 설정되지 않았습니다.");
                    Continue();
                }
            }
            else
            {
                Continue();
            }
        }

        protected virtual void DoWait() => Continue();

        public override string GetSummary()
        {
            string clipName = (soundClip == null) ? "None" : soundClip.name;
            return $"{clipName} ({channel})";
        }

        public override Color GetButtonColor() => new Color32(242, 209, 176, 255); // 오디오 커맨드와 유사한 색상
    }
}