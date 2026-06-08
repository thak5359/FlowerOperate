using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Audio;
using VContainer.Unity;
using static Constant;

[System.Serializable]
public struct GameSettings
{
    //// 대화 관련
    //public float writingSpeed = 30f;
    //public float puncSliderRaw = 3f;
    ////public bool isAutoMode = false;
    //public float autoWaitTime = 2.0f;

    // 사운드 관련 (0~100)
    public float masterVol;
    public float bgmVol;
    public float sfxVol;
    public float voiceVol;

    // 해상도 및 화면 설정

    public int resWidth;
    public int resHeight;
    public FullScreenMode screenMode;

    public GameSettings(float Vol)
    {
        masterVol = Vol;
        bgmVol = Vol;
        sfxVol = Vol;
        voiceVol = Vol;

        resWidth = 1920;
        resHeight = 1080;

        screenMode = FullScreenMode.FullScreenWindow;
    }

}

public enum PanelMode { Sound, Display, KeyBind }

public struct UIState
{
    public PanelMode usingPanel;
    public bool isTransitioning;
    public FixedString64Bytes currentMap;

    public UIState(PanelMode input_mode, bool transitionCondition, FixedString64Bytes input_currentMap)
    {
        usingPanel = input_mode;
        isTransitioning = transitionCondition;
        currentMap = input_currentMap;
    }
}


public class SettingManager : IStartable, IDisposable
{
    private readonly AudioMixer _masterMixer;
    private GameSettings _settings = new GameSettings(100.0f);

    private struct ResOption
    {
        public int w, h;
    }

    public FullScreenMode screenMode;


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
            _settings = new GameSettings(100.0f);
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

            if (res.width < 1600) continue;
            if (resOptions.Exists(r => r.w == res.width && r.h == res.height)) continue;
            options.Add($"{res.width} x {res.height}");
            resOptions.Add(new ResOption { w = res.width, h = res.height});
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
            _settings.screenMode = FullScreenMode.FullScreenWindow;
        }
    }

    public void ResetToDefault()
    {
        _settings.masterVol = 100.0f;
        _settings.bgmVol = 100.0f;
        _settings.sfxVol = 100.0f;
        _settings.voiceVol = 100.0f;

        SetDefaultHighestResolution();
        ApplyAllSettings();
        SaveSettings();
    }
    #endregion

    #region 실제 설정 적용 (Apply Logic)
    public void ApplyAllSettings()
    {

        //Debug.Log($"{_settings}");
        ApplyResolution(_settings.resWidth, _settings.resHeight);
        ApplyVolume("MasterVolume", _settings.masterVol);
        ApplyVolume("BGMVolume", _settings.bgmVol);
        ApplyVolume("SFXVolume", _settings.sfxVol);
        ApplyVolume("VoiceVolume", _settings.voiceVol);
    }

    public void ChangeResolution(int index)
    {
        if (index < 0 || index >= resOptions.Count) return;
        var opt = resOptions[index];
        ApplyResolution(opt.w, opt.h);
    }

    public void ChangeScreenMode(FullScreenMode input_screenMode)
    {
        _settings.screenMode = input_screenMode;
        Screen.SetResolution(_settings.resWidth,_settings.resHeight, input_screenMode);
        SaveSettings();
    }


    public void ApplyResolution(int width, int height)
    {
        _settings.resWidth = width;
        _settings.resHeight = height;

        Screen.SetResolution(width, height, _settings.screenMode);
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
            r.w == _settings.resWidth && r.h == _settings.resHeight);

        if (currentIndex != -1)
        {
            dropdown.SetValueWithoutNotify(currentIndex); // 이벤트 중복 방지
            dropdown.RefreshShownValue();
        }
    }

    #endregion
}
