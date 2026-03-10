using UnityEngine;
using Fungus;

public class CustomWriter : Writer
{
    /// <summary>
    /// 오토 모드 등을 위해 Writer의 상태와 상관없이 
    /// 강제로 입력 신호(inputFlag)를 발생시킵니다.
    /// </summary>
    public void ForceInput()
    {
        // 부모(Writer)의 protected 변수인 inputFlag에 직접 접근
        inputFlag = true;

        // 리스너들에게 신호를 보냄 (소리 재생 등)
        NotifyInput();

    }
}