using UnityEngine;
using UnityEngine.Audio;

public class VolumeSeparator : MonoBehaviour
{
    // FungusManager의 싱글톤 인스턴스를 통해 믹서에 접근합니다.
    private AudioMixer GetMixer()
    {
        // 런타임에 생성된 FungusManager 내부의 MainAudioMixer 컴포넌트를 가져옵니다.
        // 유니티 인스펙터 구조에 따라 GetComponent의 위치는 달라질 수 있습니다.
        return Fungus.FungusManager.Instance.GetComponent<AudioMixer>();
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        AudioMixer mixer = GetMixer();
        if (mixer == null) return;

        // 슬라이더 0~1 값을 로그 스케일 dB(-80 ~ 0)로 변환
        float dB = sliderValue > 0 ? Mathf.Log10(sliderValue) * 20 : -80f;
        mixer.SetFloat(parameterName, dB);
    }

    // Fungus 'Call Method'에서 호출하기 편하도록 만든 래퍼 함수들
    public void SetMusicVol(float val) => SetVolume("MusicVol", val);
    public void SetSFXVol(float val) => SetVolume("SFXVol", val);
    public void SetVoiceVol(float val) => SetVolume("VoiceVol", val);
}