using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;
using static Constant;

public class TitleSettingMenuManager : MonoBehaviour
{
    [Header("UI switch")]
    public Button soundButton;
    public Button displayButton;
    public Button etcButton;

    public GameObject soundPanel;        // 사운드 설정 판넬
    public GameObject displayPanel;      // 화면 설정 판넬
    public GameObject etcPanel;         // 기타 설정 판넬

    public Button closeButton;        // 설정 창 닫는 버튼


    [Header("On/Off MoveSet")]
    public RectTransform movablePart; // 이동시킬 최상위 부모 패널
    public Vector2 showPos;          // 화면 안 위치 (예: 0,525)
    public Vector2 hidePos;          // 화면 밖 위치 (예: 0, -525)
    [SerializeField] private const float defaultDuration = 0.5f;

    [Header("Sound UI References")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider voiceVolumeSlider;


    [Header ("Sound UI Value Shower")]
    public TextValueEdtior masterVolumeText;
    public TextValueEdtior bgmVolumeText;
    public TextValueEdtior sfxVolumeText;
    public TextValueEdtior voiceVolumeText;

    [Header("Resolution UI Reference")]
    public TMP_Dropdown resolutionDropdown;


    public SettingManager _settingManager; 


    private int usingPanel = 1;// 사용중인 판넬 표시용 [1: 사운드 | 2: 화면 | 3: 기타 ]
    private Canvas settingCanvas;

    bool isTransitioning = false;
    private string currentMap;

    IMapChangable input; // Injection

    [Inject]
    void Construct(ActionMapChanger inputManager, SettingManager input_settingManager)
    {
        input = inputManager;
        _settingManager = input_settingManager;
    }

    private void Awake()
    {
        settingCanvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        SyncUIWithSettings();
        soundButton.onClick.AddListener(() => OnClickSoundButton());
        displayButton.onClick.AddListener(() => OnClickDisplayButton());
        closeButton.onClick.AddListener(() => CloseSetttingMenu().Forget());


        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

    }
    private void SyncUIWithSettings()
    {
        var s = _settingManager.Settings;

        // 슬라이더 값을 저장된 값으로 세팅 (이벤트 호출 방지를 위해 SetValueWithoutNotify 권장)
        masterVolumeSlider.SetValueWithoutNotify(s.masterVol);
        bgmVolumeSlider.SetValueWithoutNotify(s.bgmVol);
        sfxVolumeSlider.SetValueWithoutNotify(s.sfxVol);
        voiceVolumeSlider.SetValueWithoutNotify(s.voiceVol);


        masterVolumeText.changeTextValueInt(s.masterVol);
        bgmVolumeText.changeTextValueInt(s.bgmVol);
        sfxVolumeText.changeTextValueInt(s.sfxVol);
        voiceVolumeText.changeTextValueInt(s.voiceVol);

        // 해상도 드롭다운 초기화
        _settingManager.InitializeResDropdown(resolutionDropdown);
    }


    public void OnClickSoundButton() => PanelChange(1);
    public void OnClickDisplayButton() => PanelChange(2);
    public void OnClickEtcButton() => PanelChange(3);
    

    // 특정 판넬로 갈아끼우기
    private void PanelChange(int num)
    {
        switch (num)
        {
            case 1:
                {
                    soundPanel.SetActive(true);
                    displayPanel.SetActive(false);
                    etcPanel.SetActive(false);

                    usingPanel = num;
                    break;
                }
            case 2:
                {
                    soundPanel.SetActive(false);
                    displayPanel.SetActive(true);
                    etcPanel.SetActive(false);

                    usingPanel = num;
                    break;
                }
            case 3:
                {
                    soundPanel.SetActive(false);
                    displayPanel.SetActive(false);
                    etcPanel.SetActive(true);

                    usingPanel = num;
                    break;
                }
        }
    }

    public void OpenSoundPanel(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            if (usingPanel != 1)
            {
                PanelChange(1);
            }
        }
    }

    public void OpenDisplayPanel(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            if (usingPanel != 2)
            {
                PanelChange(2);
            }
        }
    }

    public void OpenEtcPanel(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            if (usingPanel != 3)
            {
                PanelChange(3);
            }
        }
    }


    private Coroutine moveCoroutine;

    // 설정 창 숨기기/보이기 (이동 연출 포함) 
    public void OnBackAction(InputAction.CallbackContext context)
    {
        if (!context.performed && isTransitioning == true) return;
        HandleBackActionAsync(context).Forget();

       
    }

    private async UniTaskVoid HandleBackActionAsync(InputAction.CallbackContext context)
    {
        currentMap = input.getCurrentIAmap();

        if (currentMap == TITLE_MAP_NAME)
        {
            await OpenSettingMenu();
        }
        else if (currentMap == SETTING_MAP_NAME)
        {
            await CloseSetttingMenu();
        }
        else
        {
            // 그 외의 경우
            Debug.LogError($"[SettingMenuManager]: {currentMap}맵에서 해당 동작에 정의되지 않았습니다.");
        }
    }

    public async UniTask OpenSettingMenu()
    {

        if ( isTransitioning == true) return;
        isTransitioning = true;

        input.changeIAmapSetting();
        await MoveRoutine(showPos);

        isTransitioning = false;

    }

    private async UniTask CloseSetttingMenu()
    {

        if (isTransitioning == true) return;
        isTransitioning = true;

        input.changeIAmapPrev();
        await MoveRoutine(hidePos);


        isTransitioning = false;
    }

    public void OnClickSettingOpen()
    {
        if (isTransitioning == true) return;
        OpenSettingMenu().Forget();
    }

    public void OnClickSettingClose()
    {
        if (isTransitioning == true) return;
        input.changeIAmapPrev();
        MoveRoutine(hidePos).Forget(); 
    }

    private async UniTask MoveRoutine(Vector2 targetPos, CancellationToken token = default)
    {
        if (targetPos == showPos) { settingCanvas.enabled = true; }
        Vector2 startPos = movablePart.anchoredPosition;
        float elapsed = 0;

        while (elapsed < defaultDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / defaultDuration;
            t = t * t * (3f - 2f * t);

            movablePart.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        movablePart.anchoredPosition = targetPos;
        if (targetPos == hidePos) { settingCanvas.enabled = false; }
    }

    #region 볼륨 값 조절

    public void OnMasterVolumeChanged(float value)
    {
        _settingManager.ApplyVolume(MASTER_MIXER_GROUP, value);
    }

    public void OnBGMVolumeChanged(float value)
    {
        _settingManager.ApplyVolume(BGM_MIXER_GROUP, value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        _settingManager.ApplyVolume(SFX_MIXER_GROUP, value);
    }

    public void OnVoiceVolumeChanged(float value)
    {
        _settingManager.ApplyVolume(VOICE_MIXER_GROUP, value);
    }

    public void OnResolutionChanged(int value)
    {
        _settingManager.ApplyResolution(value);

    }

    public void InitializeResDropdown()
    {
        _settingManager.InitializeResDropdown(resolutionDropdown);
    }

    #endregion
}
