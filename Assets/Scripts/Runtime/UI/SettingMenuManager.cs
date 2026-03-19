using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Fungus;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using JetBrains.Annotations;

public class SettingMenuManager : MonoBehaviour
{
    public static SettingMenuManager instance;

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
    private bool isShowing = true;
    private Canvas settingCanvas;


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

        if (instance == null)
        {
            instance = this;
            PanelChange(1);
        }
    }




    public void OnClickSoundButton()
    {
        PanelChange(1);
    }
    public void OnClickDisplayButton()
    {
        PanelChange(2);
    }
    public void OnClickEtcButton()
    {
        PanelChange(3);
    }

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
    public void showUI(float input_duration = defaultDuration)
    {
        if (input_duration == 0)
        {
            input_duration = defaultDuration;
        }

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        IMapChangable input = IAmapManager.Instance(); 
        if (isShowing == false) // 보이기
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos, input_duration));

            input.changeIAmapPrev();
        }
        else // 숨기기
        {
            moveCoroutine = StartCoroutine(MoveRoutine(hidePos, input_duration));

            input.changeIAmapPrev();
        }
        isShowing = !isShowing;
    }

    private IEnumerator MoveRoutine(Vector2 targetPos, float input_duration)
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