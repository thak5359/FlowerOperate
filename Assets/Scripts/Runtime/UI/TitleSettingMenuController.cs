using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;
using static Constant;

public class TitleSettingMenuController : MonoBehaviour
{
    [Header("UI switch")]
    public Button soundButton;
    public Button displayButton;
    public Button etcButton;


    public GameObject soundPanel;        // 사운드 설정 판넬
    public GameObject displayPanel;      // 화면 설정 판넬
    public GameObject keyBindPanel;         // 기타 설정 판넬

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


    [Header("Sound UI Value Shower")]
    public TextValueEdtior masterVolumeText;
    public TextValueEdtior bgmVolumeText;
    public TextValueEdtior sfxVolumeText;
    public TextValueEdtior voiceVolumeText;

    [Header("Resolution UI Reference")]
    public TMP_Dropdown resolutionDropdown;

    public Toggle ExclusiveFullScreenToggle;
    public Toggle FullScreenWindowToggle;
    public Toggle WindowedToggle;

    //-----------------------------------------------------------------------------

    private SettingManager _settingManager;

    private UIState uiState = new UIState(PanelMode.Sound, false, "MAP_TITLE");


    private Canvas settingCanvas;


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
        ExclusiveFullScreenToggle.onValueChanged.AddListener(SetScreenMode_ExclusiveFullScreen);
        FullScreenWindowToggle.onValueChanged.AddListener(SetScreenMode_FullScreenWindow);
        WindowedToggle.onValueChanged.AddListener(SetScreenMode_Windowded);
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

        switch (s.screenMode)
        {
            case (FullScreenMode.ExclusiveFullScreen):
                ExclusiveFullScreenToggle.SetIsOnWithoutNotify(true);

                FullScreenWindowToggle.SetIsOnWithoutNotify(false);
                WindowedToggle.SetIsOnWithoutNotify(false);
                break;
            case (FullScreenMode.FullScreenWindow):
                FullScreenWindowToggle.SetIsOnWithoutNotify(true);

                ExclusiveFullScreenToggle.SetIsOnWithoutNotify(false);
                WindowedToggle.SetIsOnWithoutNotify(false);
                break;
            case (FullScreenMode.Windowed):
                WindowedToggle.SetIsOnWithoutNotify(true);

                ExclusiveFullScreenToggle.SetIsOnWithoutNotify(false);
                FullScreenWindowToggle.SetIsOnWithoutNotify(false);
                break;
        }

        // 해상도 드롭다운 초기화
        _settingManager.InitializeResDropdown(resolutionDropdown);
    }

    #region 설정메뉴 판넬 전환하기
    // 특정 판넬로 갈아끼우기
    private void PanelChange(PanelMode input)
    {
        switch (input)
        {
            case PanelMode.Sound:
                {
                    soundPanel.SetActive(true);
                    displayPanel.SetActive(false);
                    keyBindPanel.SetActive(false);

                    break;
                }
            case PanelMode.Display:
                {
                    soundPanel.SetActive(false);
                    displayPanel.SetActive(true);
                    keyBindPanel.SetActive(false);

                    break;
                }
            case PanelMode.KeyBind:
                {
                    soundPanel.SetActive(false);
                    displayPanel.SetActive(false);
                    keyBindPanel.SetActive(true);

                    break;
                }
        }
    }

    public void OnClickSoundButton() => PanelChange(PanelMode.Sound);
    public void OnClickDisplayButton() => PanelChange(PanelMode.Display);
    public void OnClickKeyBindButton() => PanelChange(PanelMode.KeyBind);



    public void OpenSoundPanel(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            if (uiState.usingPanel != PanelMode.Sound)
            {
                PanelChange(PanelMode.Sound);
            }
        }
    }

    public void OpenDisplayPanel(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            if (uiState.usingPanel != PanelMode.Display)
            {
                PanelChange(PanelMode.Display);
            }
        }
    }

    public void OpenKeyBindPanel(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            if (uiState.usingPanel != PanelMode.KeyBind)
            {
                PanelChange(PanelMode.KeyBind);
            }
        }
    }

    private Coroutine moveCoroutine;

    // 설정 창 숨기기/보이기 (이동 연출 포함) 
    public void OnBackAction(InputAction.CallbackContext context)
    {
        if (this == null || !context.performed && uiState.isTransitioning == true) return;
        HandleBackActionAsync(context).Forget();
    }

    private async UniTaskVoid HandleBackActionAsync(InputAction.CallbackContext context)
    {
        uiState.currentMap = input.getCurrentIAmap();

        if (uiState.currentMap == TITLE_MAP_NAME)
        {
            await OpenSettingMenu();
        }
        else if (uiState.currentMap == SETTING_MAP_NAME)
        {
            await CloseSetttingMenu();
        }
        else
        {
            // 그 외의 경우
            Debug.LogError($"[SettingMenuManager]: {uiState.currentMap}맵에서 해당 동작에 정의되지 않았습니다.");
        }
    }

    private async UniTask MoveRoutine(Vector2 targetPos, CancellationToken token = default)
    {
        if (movablePart == null || settingCanvas == null)
        {
            Debug.LogError("[SettingMenuManager]: MoveRoutine 실행 중 movablePart 또는 settingCanvas가 할당되지 않았습니다.");
            return;
        }
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


    #endregion

    #region 세팅 메뉴 여닫기
    public async UniTask OpenSettingMenu()
    {
        if (uiState.isTransitioning == true) return;
        uiState.isTransitioning = true;

        input.changeIAmapSetting();
        await MoveRoutine(showPos, this.GetCancellationTokenOnDestroy());

        uiState.isTransitioning = false;
    }

    private async UniTask CloseSetttingMenu()
    {
        if (uiState.isTransitioning == true) return;
        uiState.isTransitioning = true;

        input.changeIAmapPrev();
        await MoveRoutine(hidePos, this.GetCancellationTokenOnDestroy());

        uiState.isTransitioning = false;
    }

    public void OnClickSettingOpen()
    {
        if (uiState.isTransitioning == true) return;
        OpenSettingMenu().Forget();
    }

    public void OnClickSettingClose()
    {
        if (uiState.isTransitioning == true) return;
        input.changeIAmapPrev();
        MoveRoutine(hidePos, this.GetCancellationTokenOnDestroy()).Forget();
    }
    #endregion

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
    #endregion

    #region 해상도 조절
    public void OnResolutionChanged(int value)
    {
        _settingManager.ChangeResolution(value);

    }

    public void InitializeResDropdown()
    {
        _settingManager.InitializeResDropdown(resolutionDropdown);
    }
    #endregion

    #region ScreenMode Toggle

    /// <summary>
    /// FullScreen (Blinking when Alt+Tab)
    /// </summary>
    public void SetScreenMode_ExclusiveFullScreen(bool Toggle)
    {
        if (Toggle == true)
            _settingManager.ChangeScreenMode(FullScreenMode.ExclusiveFullScreen);
    }
    /// <summary>
    /// Borderless
    /// </summary>
    public void SetScreenMode_FullScreenWindow(bool Toggle)
    {
        if (Toggle == true)
            _settingManager.ChangeScreenMode(FullScreenMode.FullScreenWindow);
    }

    public void SetScreenMode_Windowded(bool Toggle)
    {
        if (Toggle == true)
            _settingManager.ChangeScreenMode(FullScreenMode.Windowed);
    }

    #endregion
}
