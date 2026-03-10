using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    public RectTransform movablePart;
    public Vector2 showPos;
    public Vector2 hidePos;
    public float duration = 0.5f;

    private Coroutine moveCoroutine;

    // 설정 창 숨기기/보이기 (이동 연출 포함) 
    public void MoveSettings(bool Toggle)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        if (Toggle == true)
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos));
            // 켜질 때 슬롯 갱신 등 기존 로직 수행
        }
        else
        {
            moveCoroutine = StartCoroutine(MoveRoutine(hidePos));
        }
    }

    private IEnumerator MoveRoutine(Vector2 targetPos)
    {
        Vector2 startPos = movablePart.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 부드러운 가감속을 위해 SmoothStep 적용
            t = t * t * (3f - 2f * t);

            movablePart.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        movablePart.anchoredPosition = targetPos;
    }
}
