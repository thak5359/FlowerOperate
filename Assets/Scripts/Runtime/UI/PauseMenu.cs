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

    private Canvas pauseCanvas;
    private Coroutine moveCoroutine;


    private void Awake()
    {
        pauseCanvas = GetComponent<Canvas>();
    }



    //테스트 기능 A
    public void OnBackAction(InputAction.CallbackContext context)
    {
        // 1. 공통 방어 로직
        if (moveCoroutine != null || !context.performed || !context.ReadValueAsButton()) return;

        IMapChangable input = IAmapManager.Instance;
        string current = input.getCurrentIAmap();

        // 2. 현재 상태에 따른 "길 찾기" (State Machine)
        switch (current)
        {
            case "MAP_FARM":
            case "MAP_SHOP":
                // 농장이나 상점 -> 퍼즈 메뉴 열기
                OpenPauseMain();
                break;

            case "MAP_PAUSE":
                // 퍼즈 메인 -> 메뉴 닫고 복귀
                ClosePauseMain();
                break;

            case "MAP_SETTING":
                // 세팅 화면 -> 퍼즈 메인으로 돌아가기
                BackToPauseFromSetting();
                break;

            default:
                Debug.Log($"[PauseMenu] {current} 맵에서는 ESC 동작이 정의되지 않았습니다.");
                break;
        }
    }

    private void OpenPauseMain()
    {
        moveCoroutine = StartCoroutine(MoveRoutine(showPos));
        IMapChangable input = IAmapManager.Instance;
            
         input.changeIAmapPauseMenu();
    }

    public void ClosePauseMain()
    {
        moveCoroutine = StartCoroutine(MoveRoutine(hidePos));
        IMapChangable input = IAmapManager.Instance;

        input.changeIAmapPrev();
    }

    public void BackToPauseFromSetting()
    {
        moveCoroutine = StartCoroutine(MoveRoutine(showPos));
        IMapChangable input = IAmapManager.Instance;

        input.changeIAmapPrev();
    }

    // 세팅 버튼 클릭 시 호출용 (인풋용 아님)
    public void OpenSettingMenu()
    {
        moveCoroutine = StartCoroutine(MoveRoutine(settingPos));
        IMapChangable input = IAmapManager.Instance;

        input.changeIAmapSetting();
    }


    private IEnumerator MoveRoutine(Vector2 targetPos)
    {

        if (targetPos == showPos) { pauseCanvas.enabled = true; }
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
        if (targetPos == hidePos) { pauseCanvas.enabled = false; }

        moveCoroutine = null;
    }

    public void OnClickCloseButton(int time)
    {
        SettingMenuManager.instance.showUI(time);
    }
}
