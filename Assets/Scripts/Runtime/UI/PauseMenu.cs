using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using static Constant;

public class PauseMenu : MonoBehaviour
{

    [Header("Pause Menu")]
    public RectTransform movablePart;
    public Vector2 hidePos;
    public Vector2 showPos;
    public Vector2 settingPos;

    public Button buttonResume;
    public Button buttonSetting;
    public Button buttonTitle;
    public Button buttonEnd;
    public Button buttonCloseSetting;

    public SettingMenuManager settingMenuManager;

    [SerializeField] protected const float defaultDuration = 0.5f;

    private Canvas pauseCanvas;
    private float cachedFloat = 0.0f;
    private IMapChangable input;

    private string currentMap;

    private bool isTransitioning;
    private void Awake()
    {
        pauseCanvas = GetComponent<Canvas>();
        settingMenuManager = GetComponent<SettingMenuManager>();

        if (settingMenuManager != null)
        {
            Debug.Log("settingMenuManager is assigned successfully in PauseMenu.");
        }
    }
    
    [Inject]
    public  void Construct(IMapChangable inputManager)
    {
        input = inputManager;
    }

    private void Start()
    {

        buttonResume.onClick.AddListener(() => ClosePauseMain().Forget());
        buttonSetting.onClick.AddListener(() => OpenSettingMenu().Forget());
        buttonTitle.onClick.AddListener(() => OnClickTitleButton());
        buttonEnd.onClick.AddListener(() => OnClickGameEndButton());
        buttonCloseSetting.onClick.AddListener(() => BackToPauseFromSetting().Forget());
    }



    #region PauseMenu, SettingMenu 호출/종료 기능

    public void OnBackAction(InputAction.CallbackContext context)
    {
        // 1. 공통 방어 로직
        if (this == null || isTransitioning == true || !context.performed) return;

        HandleBackActionAsync(context).Forget();

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

        MoveRoutine(showPos).Forget();

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
        settingMenuManager.OnClickSoundButton(); 
        await(MoveRoutine(settingPos));

        isTransitioning = false;
    }



    public async UniTask ClosePauseMain(CancellationToken cancellationToken = default)
    {
        isTransitioning = true;

        await MoveRoutine(hidePos);

        Debug.Log("시간은 다시 움직인다");
        Time.timeScale = 1.0f;
        input.changeIAmapPrev();

        isTransitioning = false;

    }

    public async UniTask BackToPauseFromSetting()
    {
        isTransitioning = true;

        await MoveRoutine(showPos);
        input.changeIAmapPrev();

        isTransitioning = false;
    }


    private async UniTask MoveRoutine(Vector2 targetPos, CancellationToken cancellationToken = default)
    {
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
}
