using Fungus;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance;
    public AudioMixer masterMixer;
    public GameSettings settings = new GameSettings();



    //[Header("Writer UI References")]
    //public Slider writingSpeedSlider;
    //public Slider punctuationSpeedSlider;
    //public Slider autoWaitTimeSlider;
    //public Toggle autoToggle;


    [Header("Sound UI References")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider voiceVolumeSlider;


    [Header("Resolution UI Reference")]
    public TMP_Dropdown resolutionDropdown;
    

    private List<Resolution> resolutions = new List<Resolution>();

    #region 싱글톤 & 초기화

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        InitResolution();
        ApplyToFungus();
    }




    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { ApplyToFungus(); }


    #endregion

    #region UI 이벤트 핸들러

    #region 대화창 UI
    //public void UpdateWritingSpeed(float value)
    //{
    //    settings.writingSpeed = value;
    //    ApplyToFungus();
    //}
    //public void UpdatePunctuationPause(float value)
    //{
    //    settings.puncSliderRaw = value;
    //    ApplyToFungus();
    //}
    //public void UpdateAutoWaitTime(float value)
    //{

    //    settings.autoWaitTime = value;
    //    ApplyToFungus();
    //}
    //public void ToggleAutoMode(bool Toggle)
    //{
    //    settings.isAutoMode = Toggle;
    //    ApplyToFungus();
    //}

    #endregion

    #region 소리 UI

    public void ChangeMasterVolume(float value) => ApplyVolumeToMixer("MasterVolume", value);
    public void ChangeBGMVolume(float value) => ApplyVolumeToMixer("BGMVolume", value);
    public void ChangeSFXVolume(float value) => ApplyVolumeToMixer("SFXVolume", value);
    public void ChangeVoiceVolume(float value) => ApplyVolumeToMixer("VoiceVolume", value);

    private void ApplyVolumeToMixer(string parameterName, float Value)
    {
        float linearValue = Value / 100f;
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;

        if (masterMixer != null) { masterMixer.SetFloat(parameterName, dB); }

        switch (parameterName)
        {
            case "MasterVolume": settings.masterVol = Value; break;
            case "BGMVolume": settings.bgmVol = Value; break;
            case "SFXVolume": settings.sfxVol = Value; break;
            case "VoiceVolume": settings.voiceVol = Value; break;
        }

        ApplyToFungus();

    }
    private void ApplyAllVolumes()
    {
        ApplyVolumeToMixer("MasterVolume", settings.masterVol);
        ApplyVolumeToMixer("BGMVolume", settings.bgmVol);
        ApplyVolumeToMixer("SFXVolume", settings.sfxVol);
        ApplyVolumeToMixer("VoiceVolume", settings.voiceVol);
    }
    #endregion

    #region 해상도 UI

    // 드롭다운에 표시될 해상도와 모드 정보를 묶어두는 헬퍼 클래스
    private struct ResOption
    {
        public int w, h;
        public bool isFull;
    }
    private List<ResOption> resOptions = new List<ResOption>();




    private void InitResolution()
    {
        // 1. 현재 모니터의 최대 사양 확인
        int maxW = Screen.currentResolution.width;
        int maxH = Screen.currentResolution.height;

        Resolution[] allResolutions = Screen.resolutions;
        resOptions.Clear();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        // 2. 역순(높은 해상도부터) 루프
        for (int i = allResolutions.Length - 1; i >= 0; i--)
        {
            Resolution res = allResolutions[i];
            string baseText = $"{res.width} x {res.height}";

            // 중복 해상도 제거
            bool alreadyAdded = resOptions.Exists(r => r.w == res.width && r.h == res.height && r.isFull == true);
            if (alreadyAdded) continue;

            // A. 전체화면 옵션은 무조건 추가
            options.Add(baseText + " 전체화면");
            resOptions.Add(new ResOption { w = res.width, h = res.height, isFull = true });

            // B. 창모드 옵션 추가 (가로 또는 세로가 최대치보다 작을 때만)
            if (res.width < maxW && res.height < maxH)
            {
                options.Add(baseText + " 창모드");
                resOptions.Add(new ResOption { w = res.width, h = res.height, isFull = false });
            }
        }

        resolutionDropdown.AddOptions(options);

        // 3. 현재 설정된 값과 일치하는 항목 찾아 드롭다운 위치 맞추기
        int currentIndex = resOptions.FindIndex(r =>
            r.w == settings.resWidth &&
            r.h == settings.resHeight &&
            r.isFull == settings.isFullScreen);

        if (currentIndex != -1)
        {
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.RefreshShownValue();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(resolutionDropdown.template.GetComponent<RectTransform>());
    }
    public void SetResolutionFromDropdown(int index)
    {
        if (index < 0 || index >= resOptions.Count) return;

        ResOption selected = resOptions[index];

        // 1. 데이터 업데이트
        settings.resWidth = selected.w;
        settings.resHeight = selected.h;
        settings.isFullScreen = selected.isFull;

        // 2. [핵심] 현재 씬 이름 체크
        if (SceneManager.GetActiveScene().name == "MainTitle")
        {
            Screen.SetResolution(selected.w, selected.h, selected.isFull);
            Debug.Log($"[Resolution] '{selected.w}x{selected.h}' 적용 완료 (Scene: MainTitle)");
        }
        else
        {
            Debug.Log("[Resolution] 데이터 저장 완료. 다음 'MainTitle' 진입 시 적용됩니다.");
        }

        SaveSettings();
    }

    public void SetFullScreen(bool isFull)
    {
        settings.isFullScreen = isFull;
        Screen.fullScreen = isFull;
        SaveSettings();
    }

    public void SetWindowded(bool isWindowed)
    {
        settings.isFullScreen = !isWindowed;
        Screen.fullScreen = !isWindowed;
        SaveSettings();
    }


    #endregion



    #endregion

    #region UI 설정 적용, 저장, 불러오기

    public void ApplyToFungus()
    {
        //SayDialog sayDialog = SayDialog.GetSayDialog();
        //if (sayDialog != null)
        //{
        //    var writer = sayDialog.GetComponent<Writer>();
        //    if (writer != null)
        //    {
        //        writer.writingSpeed = settings.writingSpeed;
        //        writer.punctuationPause = (settings.puncSliderRaw + 1) * 0.05f;
        //    }
        //}

        //if (writingSpeedSlider != null) writingSpeedSlider.value = settings.writingSpeed;
        //if (punctuationSpeedSlider != null)punctuationSpeedSlider.value = settings.puncSliderRaw;
        //if (autoWaitTimeSlider != null) autoWaitTimeSlider.value = settings.autoWaitTime;

        if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVol;
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = settings.bgmVol;
        if (sfxVolumeSlider != null)sfxVolumeSlider.value = settings.sfxVol;
        if (voiceVolumeSlider != null) voiceVolumeSlider.value = settings.voiceVol;

        SaveSettings();
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings);
        PlayerPrefs.SetString("GameSettings", json);
        PlayerPrefs.Save();
         Debug.Log("저장된 JSON: " + json); // 확인용
    }

    private void LoadSettings()
    {
        string json = PlayerPrefs.GetString("GameSettings", "");

        if (!string.IsNullOrEmpty(json))
        {
            // 1. 기존 데이터가 있다면 불러오기
            settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            // 2. [최초 가동] 데이터가 없다면 최고 사양으로 세팅
            SetDefaultHighestResolution();

            SaveSettings();
            Debug.Log($"[Init] 최초 가동: 최고 해상도({settings.resWidth}x{settings.resHeight})로 설정되었습니다.");
        }


        if (SceneManager.GetActiveScene().name == "MainTitle")
        {
            Screen.SetResolution(settings.resWidth, settings.resHeight, settings.isFullScreen);
        }

        ApplyAllVolumes();
    }
    private void SetDefaultHighestResolution()
    {
        // Screen.resolutions는 낮은 해상도 -> 높은 해상도 순으로 정렬.
        Resolution[] allRes = Screen.resolutions;

        if (allRes.Length > 0)
        {

            Resolution maxRes = allRes[allRes.Length - 1];
            settings.resWidth = maxRes.width;
            settings.resHeight = maxRes.height;
            settings.isFullScreen = true; 
        }
        else
        {
            settings.resWidth = 1920;
            settings.resHeight = 1080;
            settings.isFullScreen = true;
        }
    }

    #endregion
}
