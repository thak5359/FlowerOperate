using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using VContainer;
using System;
using static Constant;

public class SettingMenuManager : MonoBehaviour
{
    [Header("UI switch")]
    public Button soundButton;
    public Button displayButton;
    public Button etcButton;

    public GameObject soundPanel;        // 사운드 설정 판넬
    public GameObject displayPanel;      // 화면 설정 판넬
    public GameObject etcPanel;         // 기타 설정 판넬

    [Header("On/Off MoveSet")]
    public RectTransform movablePart; // 이동시킬 최상위 부모 패널
    public Vector2 showPos;          // 화면 안 위치 (예: 0,525)
    public Vector2 hidePos;          // 화면 밖 위치 (예: 0, -525)
    [SerializeField] private const float defaultDuration = 0.5f;   

    private int usingPanel = 1;// 사용중인 판넬 표시용 [1: 사운드 | 2: 화면 | 3: 기타 ]
    private Canvas settingCanvas;

    bool isTransitioning = false;
    private string currentMap;

    IMapChangable input; // Injection

    [Inject]
    void Construct(ActionMapChanger inputManager)
    {
        input = inputManager;
    }

    private void Awake()
    {
        settingCanvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        if (soundButton != null)
            soundButton.onClick.AddListener(() => OnClickSoundButton());
        if (displayButton != null)
            displayButton.onClick.AddListener(() => OnClickDisplayButton());
        if (soundButton != null)
            soundButton.onClick.AddListener(() => OnClickSoundButton());
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

    public void OffUI() // 끄기
    {
        movablePart.anchoredPosition = hidePos;
    }

    private Coroutine moveCoroutine;

    // 설정 창 숨기기/보이기 (이동 연출 포함) 
    public void OnBackAction(InputAction.CallbackContext context)
    {
        if (!context.performed && isTransitioning == true) return;
        currentMap = input.getCurrentIAmap();

        // 수정할 위치: SettingMenuManager 스크립트 내부의 맵 분기 처리 로직
        if (currentMap == TITLE_MAP_NAME)
        {
            StartCoroutine(OpenSettingMenu());
        }
        else if (currentMap == SETTING_MAP_NAME)
        {
            StartCoroutine(CloseSetttingMenu());
        }
        else
        {
            // 그 외의 경우 (기존 default 역할)
            Debug.LogError($"[SettingMenuManager]: {currentMap}맵에서 해당 동작에 정의되지 않았습니다.");
        }
    }

    private IEnumerator OpenSettingMenu()
    {
        isTransitioning = true;

        input.changeIAmapSetting();
        yield return StartCoroutine(MoveRoutine(showPos));



        isTransitioning = false;


    }

    private IEnumerator CloseSetttingMenu()
    {

        isTransitioning = true;

        input.changeIAmapPrev();
        yield return StartCoroutine(MoveRoutine(showPos));


        isTransitioning = false;
    }


    public void OnClickSettingOpen()
    {
        OpenSettingMenu();
    }

    public void OnClickSettingClose()
    {
        input.changeIAmapPrev();
        StartCoroutine (MoveRoutine(hidePos));

    }

    private IEnumerator MoveRoutine(Vector2 targetPos)
    {

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
        if (targetPos == hidePos) { settingCanvas.enabled = false; }
        PanelChange(1);
    }
}
