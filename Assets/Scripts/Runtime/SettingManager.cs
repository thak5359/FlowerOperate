using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using UnityEngine.Audio;
using static Constant;
using TMPro;
using UnityEngine.UI;
using System;

[System.Serializable]
public class GameSettings
{
    //// 대화 관련
    //public float writingSpeed = 30f;
    //public float puncSliderRaw = 3f;
    ////public bool isAutoMode = false;
    //public float autoWaitTime = 2.0f;

    // 사운드 관련 (0~100)
    public float masterVol = 100f;
    public float bgmVol = 100f;
    public float sfxVol = 100f;
    public float voiceVol = 100f;

    // 해상도 및 화면 설정

    public int resWidth = 1920;
    public int resHeight = 1080;
    public bool isFullScreen = true;
}



public class SettingManager : IStartable, IDisposable
{
    private readonly AudioMixer _masterMixer;
    private GameSettings _settings;


    private struct ResOption
    {
        public int w, h;
        public bool isFull;
    }
    private List<ResOption> resOptions = new List<ResOption>();
    private List<string> options = new List<string>();

    public GameSettings Settings => _settings;

    public SettingManager(AudioMixer input_audioMixer)
    {
        _masterMixer = input_audioMixer;
    }

    void IStartable.Start()
    {
        LoadSettings();
        InitResolutionOptions();
        ApplyAllSettings();
    }

    void IDisposable.Dispose()
    {
        SaveSettings();
        PlayerPrefs.Save();
        resOptions?.Clear();
        options?.Clear();

        Debug.Log("<color=red>[SettingManager]</color> 모든 자원 해제 및 데이터 최종 저장 완료");
    }



    #region 데이터 관리 (Save/Load)
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(_settings);
        PlayerPrefs.SetString("GameSettings", json);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        string json = PlayerPrefs.GetString("GameSettings", "");

        if (!string.IsNullOrEmpty(json))
        {
            _settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            // 최초 실행 시 기본값 설정
            _settings = new GameSettings();
            SetDefaultHighestResolution();
        }

    }

    private void InitResolutionOptions()
    {
        int maxW = Screen.currentResolution.width;
        int maxH = Screen.currentResolution.height;
        Resolution[] allResolutions = Screen.resolutions;

        resOptions.Clear();
        options.Clear();

        for (int i = allResolutions.Length - 1; i >= 0; i--)
        {
            Resolution res = allResolutions[i];
            string baseText = $"{res.width} x {res.height}";

            if (resOptions.Exists(r => r.w == res.width && r.h == res.height)) continue;

            // 전체화면 추가
            options.Add($"{baseText} 전체화면");
            resOptions.Add(new ResOption { w = res.width, h = res.height, isFull = true });

            // 창모드 추가 (모니터보다 작을 때만)
            if (res.width < maxW && res.height < maxH)
            {
                options.Add($"{baseText} 창모드");
                resOptions.Add(new ResOption { w = res.width, h = res.height, isFull = false });
            }
        }
    }

    private void SetDefaultHighestResolution()
    {
        Resolution[] allRes = Screen.resolutions;
        if (allRes.Length > 0)
        {
            Resolution maxRes = allRes[allRes.Length - 1];
            _settings.resWidth = maxRes.width;
            _settings.resHeight = maxRes.height;
            _settings.isFullScreen = true;
        }
    }
    #endregion

    #region 실제 설정 적용 (Apply Logic)
    public void ApplyAllSettings()
    {
        ApplyResolution(_settings.resWidth, _settings.resHeight, _settings.isFullScreen);
        ApplyVolume("MasterVolume", _settings.masterVol);
        ApplyVolume("BGMVolume", _settings.bgmVol);
        ApplyVolume("SFXVolume", _settings.sfxVol);
        ApplyVolume("VoiceVolume", _settings.voiceVol);
    }

    public void ApplyResolution(int index)
    {
        if (index < 0 || index >= resOptions.Count) return;
        var opt = resOptions[index];
        ApplyResolution(opt.w, opt.h, opt.isFull);
    }

    // 해상도 변경 함수
    public void ApplyResolution(int width, int height, bool isFull)
    {
        _settings.resWidth = width;
        _settings.resHeight = height;
        _settings.isFullScreen = isFull;

        Screen.SetResolution(width, height, isFull);
        SaveSettings();
    }


    // 볼륨 변경 함수
    public void ApplyVolume(string parameterName, float value)
    {
        // 선형 값(0~100)을 데시벨(-80~0)로 변환
        float linearValue = value / 100f;
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;

        if (_masterMixer != null)
        {
            _masterMixer.SetFloat(parameterName, dB);
        }

        // 데이터 갱신
        switch (parameterName)
        {
            case "MasterVolume": _settings.masterVol = value; break;
            case "BGMVolume": _settings.bgmVol = value; break;
            case "SFXVolume": _settings.sfxVol = value; break;
            case "VoiceVolume": _settings.voiceVol = value; break;
        }

        SaveSettings();
    }

    public void InitializeResDropdown(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int currentIndex = resOptions.FindIndex(r =>
            r.w == _settings.resWidth && r.h == _settings.resHeight && r.isFull == _settings.isFullScreen);

        if (currentIndex != -1)
        {
            dropdown.SetValueWithoutNotify(currentIndex); // 이벤트 중복 방지
            dropdown.RefreshShownValue();
        }
    }

    #endregion
}
