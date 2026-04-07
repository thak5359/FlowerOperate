using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using static Constant;

public class IngameSettingMenuManager : MonoBehaviour
{


    [Header("Pause Menu")]
    public RectTransform movablePart;
    public Vector2 hidePos = new Vector2(0, 1000);
    public Vector2 showPos = new Vector2(0, 0);
    public Vector2 settingPos = new Vector2 ( 0, -1000);

    public Button buttonResume;
    public Button buttonSetting;
    public Button buttonTitle;
    public Button buttonEnd;
    public Button buttonCloseSetting;

    [Header("UI switch")]
    public Button soundButton;
    public Button displayButton;
    public Button etcButton;

    public GameObject soundPanel;        // 사운드 설정 판넬
    public GameObject displayPanel;      // 화면 설정 판넬
    public GameObject etcPanel;         // 기타 설정 판넬
    private int usingPanel = 1;// 사용중인 판넬 표시용 [1: 사운드 | 2: 화면 | 3: 기타 ]


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


    [SerializeField] protected const float defaultDuration = 0.5f;

    private Canvas pauseCanvas;
    private float cachedFloat = 0.0f;
    private IMapChangable input;
    private  SettingManager _settingManager;

    private string currentMap;

    private bool isTransitioning;
    private void Awake()
    {
        pauseCanvas = GetComponent<Canvas>();

    }
    
    [Inject]
    public  void Construct(IMapChangable input_Imapchangable, SettingManager input_settingManager)
    {
        input = input_Imapchangable;
        _settingManager = input_settingManager;
    }

    private void Start()
    {

        SyncUIWithSettings();

        buttonResume.onClick.AddListener(() => ClosePauseMain().Forget());
        buttonSetting.onClick.AddListener(() => OpenSettingMenu().Forget());
        buttonTitle.onClick.AddListener(() => OnClickTitleButton());
        buttonEnd.onClick.AddListener(() => OnClickGameEndButton());
        buttonCloseSetting.onClick.AddListener(() => BackToPauseFromSetting().Forget());

        soundButton.onClick.AddListener(() => OnClickSoundButton());
        displayButton.onClick.AddListener(() => OnClickDisplayButton());

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider   .onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider   .onValueChanged.AddListener(OnSFXVolumeChanged);
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

    #region PauseMenu, SettingMenu 호출/종료 기능

    public void OnBackAction(InputAction.CallbackContext context)
    {
        // 1. 공통 방어 로직
        if (this == null || isTransitioning == true || !context.performed) return;

        HandleBackActionAsync(context).Forget();

    }

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









    private async UniTaskVoid HandleBackActionAsync(InputAction.CallbackContext context)
    {
        currentMap = input.getCurrentIAmap();

        // 수정할 위치: PauseMenu UI 매니저 스크립트 내부의 맵 분기 처리 로직
        if (currentMap == FARM_MAP_NAME || currentMap == SHOP_MAP_NAME)
        {
            // 농장이나 상점 -> 퍼즈 메뉴 열기
            OpenPauseMain(this.GetCancellationTokenOnDestroy()).Forget();
        }
        else if (currentMap == PAUSEMENU_MAP_NAME)
        {
            // 퍼즈 메인 -> 메뉴 닫고 복귀
            await ClosePauseMain();
        }
        else if (currentMap == SETTING_MAP_NAME)
        {
            // 세팅 화면 -> 퍼즈 메인으로 돌아가기
            await BackToPauseFromSetting();
        }
        else
        {
            // 그 외의 경우 
            Debug.Log($"[PauseMenu] {currentMap} 맵에서는 해당 동작이 정의되지 않았습니다.");
        }


    }

    private  async UniTask OpenPauseMain(CancellationToken cancellationToken = default)
    {
        isTransitioning = true;

        input.changeIAmapPauseMenu();

        MoveRoutine(showPos, this.GetCancellationTokenOnDestroy()).Forget();

        Debug.Log("시간을 멈춰라 마이 월드야~!");


        cachedFloat = 0.0f;
        while (cachedFloat < 1.0f )
        {
            cachedFloat += Time.unscaledDeltaTime;
            float warpedT = Mathf.Sin(cachedFloat / 1.0f * Mathf.PI * 0.5f);

            Time.timeScale = Mathf.SmoothStep(1, 0, warpedT);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            
        }
        Time.timeScale = 0f;


        isTransitioning = false;
    }

    private async UniTask OpenSettingMenu(CancellationToken cancellationToken = default)
    {
        isTransitioning = true;

        input.changeIAmapSetting();
        PanelChange(1);
        await(MoveRoutine(settingPos, this.GetCancellationTokenOnDestroy()));

        isTransitioning = false;
    }

    public async UniTask ClosePauseMain(CancellationToken cancellationToken = default)
    {
        isTransitioning = true;

        await MoveRoutine(hidePos, this.GetCancellationTokenOnDestroy());

        Debug.Log("시간은 다시 움직인다");
        Time.timeScale = 1.0f;
        input.changeIAmapPrev();

        isTransitioning = false;

    }

    public async UniTask BackToPauseFromSetting()
    {
        isTransitioning = true;

        await MoveRoutine(showPos, this.GetCancellationTokenOnDestroy());
        input.changeIAmapPrev();

        isTransitioning = false;
    }

    private async UniTask MoveRoutine(Vector2 targetPos, CancellationToken cancellationToken = default)
    {
        if (movablePart == null || pauseCanvas == null)
        {
            Debug.LogError("[SettingMenuManager]: MoveRoutine 실행 중 movablePart 또는 settingCanvas가 할당되지 않았습니다.");
            return;
        }

        if (targetPos == showPos) { pauseCanvas.enabled = true; }
        Vector2 startPos = movablePart.anchoredPosition;
        float elapsed = 0;

        while (elapsed < defaultDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / defaultDuration;
            t = t * t * (3f - 2f * t);

            movablePart.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
        movablePart.anchoredPosition = targetPos;
        if (targetPos == hidePos) { pauseCanvas.enabled = false; }
    }

    #endregion

    #region 버튼 클릭 기능

    public void OnClickSoundButton() => PanelChange(1);
    public void OnClickDisplayButton() => PanelChange(2);

    public void OnClickEtcButton() => PanelChange(3);

    public void OnClickTitleButton()
    {
        SceneManager.LoadScene("MainTitle");
    }

    public void OnClickGameEndButton()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
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

    #region 해상도 드롭다운 초기화

    public void OnResolutionChanged(int value)
    {
        _settingManager.ApplyResolution(value);

    }
    #endregion

}
