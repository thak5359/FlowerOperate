using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private static PauseMenu instance;

    [Header("Pause Menu")]
    public RectTransform movablePart;
    public Vector2 hidePos;
    public Vector2 showPos;
    public Vector2 settingPos;

    public Button buttonResume;
    public Button buttonSetting;
    public Button buttonTitle;
    public Button buttonEnd;

    [SerializeField] protected const float defaultDuration = 0.5f;

    private Canvas pauseCanvas;
    private Coroutine moveCoroutine;
    private float cachedFloat = 0.0f;
    private IMapChangable input;

    private bool isTransitioning;
    private void Awake()
    {
        pauseCanvas = GetComponent<Canvas>();

        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {

       input = IAmapManager.Instance();

        buttonResume.onClick.AddListener(() => ClosePauseMain());
        buttonSetting.onClick.AddListener(() => OpenSettingMenu());
        buttonTitle.onClick.AddListener(() => OnClickTitleButton());
        buttonEnd.onClick.AddListener(() => OnClickGameEndButton());
    }

    public static PauseMenu Instance()
    {
        if (instance != null)
            return instance;
        else return null;
    }


    #region PauseMenu, SettingMenu 호출/종료 기능

    public void OnBackAction(InputAction.CallbackContext context)
    {
        // 1. 공통 방어 로직
        if (isTransitioning == true || !context.performed) return;

        string current = input.getCurrentIAmap();


        switch (current)
        {
            case "MAP_FARM":
            case "MAP_SHOP":
                // 농장이나 상점 -> 퍼즈 메뉴 열기
                StartCoroutine(OpenPauseMain());
                break;

            case "MAP_PAUSE":
                // 퍼즈 메인 -> 메뉴 닫고 복귀
                StartCoroutine(ClosePauseMain());
                break;

            case "MAP_SETTING":
                // 세팅 화면 -> 퍼즈 메인으로 돌아가기
                StartCoroutine(BackToPauseFromSetting());
                break;

            default:
                Debug.Log($"[PauseMenu] {current} 맵에서는 해당 동작이 정의되지 않았습니다.");
                break;
        }
    }

    private IEnumerator OpenPauseMain()
    {
        isTransitioning = true;

        input.changeIAmapPauseMenu();

        moveCoroutine = StartCoroutine(MoveRoutine(showPos));

        Debug.Log("시간을 멈춰라 마이 월드야~!");


        cachedFloat = 0.0f;
        while (cachedFloat < 1.0f )
        {
            cachedFloat += Time.unscaledDeltaTime;
            float warpedT = Mathf.Sin(cachedFloat / 1.0f * Mathf.PI * 0.5f);

            Time.timeScale = Mathf.SmoothStep(1, 0, warpedT);
            yield return null;
            
        }
        Time.timeScale = 0f;

        isTransitioning = false;
    }

    public IEnumerator OpenSettingMenu()
    {
        isTransitioning = true;


        yield return StartCoroutine(MoveRoutine(settingPos));
        input.changeIAmapSetting();


        isTransitioning = false;
    }

    public IEnumerator ClosePauseMain()
    {
        isTransitioning = true;


        yield return StartCoroutine(MoveRoutine(hidePos));


        Debug.Log("시간은 다시 움직인다");
        Time.timeScale = 1.0f;
        input.changeIAmapPrev();


        isTransitioning = false;
    }

    public IEnumerator BackToPauseFromSetting()
    {
        isTransitioning = true;
        yield return StartCoroutine(MoveRoutine(showPos));
        input.changeIAmapPrev();
        isTransitioning = false;
    }

    private IEnumerator MoveRoutine(Vector2 targetPos)
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
            yield return null;
        }

        movablePart.anchoredPosition = targetPos;
        if (targetPos == hidePos) { pauseCanvas.enabled = false; }

        moveCoroutine = null;
    }

    #endregion

    #region 버튼 클릭 기능

    public void OnClickSettingButton()
    {
        isTransitioning = true;
        moveCoroutine = StartCoroutine(MoveRoutine(settingPos));

        input.changeIAmapSetting();
        isTransitioning = false;
    }

    public void OnClickTitleButton()
    {
        SceneManager.LoadScene("Main");
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
