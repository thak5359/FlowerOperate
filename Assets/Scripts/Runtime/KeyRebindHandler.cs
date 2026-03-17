using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebindHandler : MonoBehaviour
{
    public InputActionReference moveAction; // 리바인딩할 액션 참조

    public void StartRebinding()
    {
        // 1. 기존 액션을 잠시 멈춥니다.
        moveAction.action.Disable();

        // 2. 리바인딩 오퍼레이션 생성
        var rebindOperation = moveAction.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse") // 마우스 클릭은 제외 (선택 사항)
            .OnComplete(operation =>
            {
                Debug.Log("리바인딩 완료!");
                //SaveKeys(); // 성공 시 저장
                moveAction.action.Enable(); // 다시 활성화
                operation.Dispose(); // 메모리 해제
            })
            .Start(); // 입력 대기 시작


    }

}
