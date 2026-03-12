// AutoDialogInput.cs 수정본
using Fungus;
using UnityEngine;

public class AutoDialogInput : DialogInput
{
    public void ForceNextLine()
    {
        // 1. Writer가 대기 중이 아니더라도 강제로 플래그를 세웁니다.
        nextLineInputFlag = true;
        ignoreClickTimer = 0f;

        // 2. [추가] 만약 Writer가 입력을 기다리지 않는 상태라면, 강제로 기다리게 만듭니다.
        // 이 부분은 Writer의 내부 구현에 따라 다를 수 있지만, 
        // 보통 플래그를 세우는 것만으로 부족할 때 '직접' 리스너를 호출하는게 확실합니다.
        var inputListeners = gameObject.GetComponentsInChildren<IDialogInputListener>();
        for (int i = 0; i < inputListeners.Length; i++)
        {
            inputListeners[i].OnNextLineEvent();
        }
    }
}