using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Fungus;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SettingMenuManager : MonoBehaviour
{
    public static SettingMenuManager instance;

    [Header("UI Panels")]
    public GameObject SoundPanel;        // 사운드 설정 판넬
    public GameObject DisplayPanel;      // 화면 설정 판넬
    public GameObject EtcPanel;         // 기타 설정 판넬

    public RectTransform movablePart; // 이동시킬 최상위 부모 패널
    public Vector2 showPos;          // 화면 안 위치 (예: 0,525)
    public Vector2 hidePos;          // 화면 밖 위치 (예: 0, -525)
    [SerializeField] private const float defaultDuration = 0.5f;    // 이동 시간


    private string prevIAmap; // 이전에 사용한 맵 저장용

    private int usingPanel = 1;// 사용중인 판넬 표시용 [1: 사운드 | 2: 화면 | 3: 기타 ]

    private bool isShowing = true;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            PanelChange(1);
        }
    }


    // 특정 판넬로 갈아끼우기
    private void PanelChange(int num)
    {
        switch (num)
        {
            case 1:
                {
                    SoundPanel.SetActive(true);
                    DisplayPanel.SetActive(false);
                    EtcPanel.SetActive(false);
                    break;
                }
            case 2:
                {
                    SoundPanel.SetActive(false);
                    DisplayPanel.SetActive(false);
                    EtcPanel.SetActive(true);
                    break;
                }
            case 3:
                {
                    SoundPanel.SetActive(true);
                    DisplayPanel.SetActive(false);
                    EtcPanel.SetActive(false);
                    break;
                }
        }
    }
    // 1. 세이브/로드 버튼을 눌렀을 때 호출
    public void OpenSoundPanel(bool Toggle)
    {
        if (Toggle == true)
        {
            if (usingPanel != 1)
            {
                PanelChange(1);
                usingPanel = 1;
            }
        }
    }

    public void OpenDisplayPanel(bool Toggle)
    {
        if (Toggle == true)
        {
            if (usingPanel != 2)
            {
                PanelChange(2);
                usingPanel = 2;
            }
        }
    }

    public void OpenEtcPanel(bool Toggle)
    {
        if (Toggle == true)
        {
            if (usingPanel != 3)
            {
                PanelChange(3);
                usingPanel = 3;
            }
        }
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
        IMapChangable input = IAmapManager.Instance; // IA맵 변경 함수 접근 권한 취득
        if (isShowing == false) // 보이기
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos, input_duration));

            prevIAmap = input.getCurrentIAmap();
            input.changeIAmapPauseMenu();
        }
        else // 숨기기
        {
            moveCoroutine = StartCoroutine(MoveRoutine(hidePos, input_duration));

            prevIAmap = null;
            input.changeIAmap(prevIAmap);
        }
        isShowing = !isShowing;
    }

    private IEnumerator MoveRoutine(Vector2 targetPos, float input_duration)
    {
        Vector2 startPos = movablePart.anchoredPosition;
        float elapsed = 0;

        while (elapsed < defaultDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / defaultDuration;
            // 부드러운 가감속을 위해 SmoothStep 적용
            t = t * t * (3f - 2f * t);

            movablePart.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        movablePart.anchoredPosition = targetPos;
        PanelChange(1);
    }
}