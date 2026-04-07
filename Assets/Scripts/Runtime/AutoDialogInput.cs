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

        var inputListeners = gameObject.GetComponentsInChildren<IDialogInputListener>();
        for (int i = 0; i < inputListeners.Length; i++)
        {
            inputListeners[i].OnNextLineEvent();
        }
    }
}