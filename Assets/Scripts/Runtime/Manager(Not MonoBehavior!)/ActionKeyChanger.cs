using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public interface IActionKeyChanger
{
    public void rebindAction(string mapName, string actionName, int bindingIndex);

}

public class ActionKeyChanger : IActionKeyChanger
{
    private PlayerInput _playerInput;

    [Inject]
    public void Construct(PlayerInput input_playerInput)
    {
        _playerInput = input_playerInput;
    }

    public void rebindAction(string mapName,string actionName, int bindingIndex)
    {
        if (_playerInput == null || _playerInput.actions == null)
        {
            Debug.Log("_playerINput is Null!");
            return;
        }

        InputActionMap targetMap = _playerInput.actions.FindActionMap(mapName);
        if (targetMap == null)
        {
            Debug.Log("targetMap is Null!");
            return;
        }

        InputAction  actionToRebind = targetMap.FindAction(actionName);
        if (actionToRebind == null)
        {
            Debug.Log("actionToRebind is Null!");
            return;
        }

        InputActionRebindingExtensions.RebindingOperation rebindingOperation =
            actionToRebind.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Pointer>")
            .WithCancelingThrough("<Keyboard>/escape")
            // 키가 바뀌면 실행될 콜백
            .OnComplete(operation => {
                Debug.Log($"성공! 새로운 키: {actionToRebind.bindings[bindingIndex].effectivePath}");

                // 2. 작업 종료 후 정리 및 맵 다시 켜기
                operation.Dispose();
                actionToRebind.actionMap.Enable();
            })
            .OnCancel(operation =>
            {
                Debug.Log($"<color=orange>[Rebind]</color> 리바인딩이 취소되었습니다.");
                operation.Dispose();
                actionToRebind.actionMap.Enable();
            })
            .Start(); // 여기서부터 입력을 기다리기 시작합니다.
        rebindingOperation.Start();
    }
}