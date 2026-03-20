using UnityEngine;
using UnityEngine.InputSystem;

public class RebindUI : MonoBehaviour
{
    // 리바인딩 작업을 관리하는 객체 (메모리 관리를 위해 필요)
    private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    public void StartInteractiveRebind(InputAction actionToRebind, int bindingIndex)
    {
        // 1. 해당 액션이 속한 맵을 잠시 끕니다. (중요! 작동 중에 바꾸면 에러 날 수 있어요)
        actionToRebind.actionMap.Disable();

        // 2. 기존의 리바인딩 작업이 있다면 폐기합니다.
        _rebindOperation?.Dispose();

        // 3. 본격적인 리바인딩 설정
        _rebindOperation = actionToRebind.PerformInteractiveRebinding(bindingIndex)
            // 마우스 이동이나 클릭으로 키가 바뀌는 걸 방지 (원하는 장치만 제외 가능)
            .WithControlsExcluding("<Pointer>")
            // 취소 키 설정 (예: ESC 누르면 취소)
            .WithCancelingThrough("<Keyboard>/escape")
            // 키가 성공적으로 바뀌었을 때 실행할 로직
            .OnComplete(operation =>
            {
                Debug.Log($"{actionToRebind.name}이(가) {actionToRebind.bindings[bindingIndex].effectivePath}로 바뀌었습니다, 파트너!");
                CleanUp(actionToRebind);
            })
            // 유저가 취소했을 때 실행할 로직
            .OnCancel(operation =>
            {
                Debug.Log("리바인딩이 취소되었습니다.");
                CleanUp(actionToRebind);
            })
            // 실제 리바인딩 시작! (유저의 다음 입력을 기다립니다)
            .Start();
    }

    private void CleanUp(InputAction action)
    {
        // 작업을 메모리에서 해제하고 액션 맵을 다시 켭니다.
        _rebindOperation?.Dispose();
        _rebindOperation = null;
        action.actionMap.Enable();
    }
}