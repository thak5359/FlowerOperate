using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    public RectTransform movablePart;
    public Vector2 hidePos;
    public Vector2 showPos;
    public Vector2 settingPos;
    [SerializeField] protected const float defaultDuration = 0.5f;

    private Coroutine moveCoroutine;

    //테스트 기능 A
    public void TogglePauseIA(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() &&context.performed) 
        {
            showPausedUI();
        }
    }
    public void ToggleSettingIA(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && context.performed)
        {
            showSettingMenu();
        }
    }

    public void showPausedUI()
    {
        if (moveCoroutine != null) return;
        IMapChangable input = IAmapManager.Instance;
        string current = input.getCurrentIAmap();

        // 1. 농장에서 ESC -> 퍼즈 메뉴 열기
        if (current == "MAP_FARM")
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos));
            input.changeIAmapPauseMenu();
        }
        // 2. 퍼즈 메뉴에서 ESC -> 농장으로 (메뉴 닫기)
        else if (current == "MAP_PAUSE")
        {
            moveCoroutine = StartCoroutine(MoveRoutine(hidePos));
            input.changeIAmapPrev();
        }
    }

    public void showSettingMenu()
    {
        if (moveCoroutine != null) return;
        IMapChangable input = IAmapManager.Instance;
        string current = input.getCurrentIAmap();

        // 1. 퍼즈 메뉴에서 세팅 열기 (주로 버튼 클릭으로 호출)
        if (current == "MAP_PAUSE")
        {
            moveCoroutine = StartCoroutine(MoveRoutine(settingPos));
            input.changeIAmapSetting();
        }
        // 2. 세팅 메뉴에서 ESC -> 퍼즈 메뉴 메인으로
        else if (current == "MAP_SETTING")
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos));
            input.changeIAmapPrev();
        }
    }


    private IEnumerator MoveRoutine(Vector2 targetPos)
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
        moveCoroutine = null;
    }

    public void OnClickCloseButton(int time)
    {
        SettingMenuManager.instance.showUI(time);
    }
}
