using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    public RectTransform movablePart;
    public Vector2 showPos;
    public Vector2 hidePos;
    [SerializeField]protected const float defaultDuration = 0.5f;

    private bool isShowing = false; 
    private Coroutine moveCoroutine;


    //호출용, PauseMenu 숨기기/보이기 (이동 연출 포함) 
    public void showUI(float input_duration = defaultDuration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        IMapChangable input = IAmapManager.Instance; // IA맵 변경 함수 접근 권한 취득


        if(input == null)
        {
            Debug.Log("input is null!");
            return;
        }

        if (isShowing == false) // 보이기
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos, input_duration));

            input.changeIAmapPauseMenu();
        }
        else // 숨기기
        {
            moveCoroutine = StartCoroutine(MoveRoutine(hidePos, input_duration));
            input.changeIAmapPrev();
        }
        isShowing = !isShowing; // UI 이동 후 상태 전환
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
    }

    public void OnClickCloseButton(int time)
    {
        SettingMenuManager.instance.showUI(time);
    }
}
