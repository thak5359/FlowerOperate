using System;
using UnityEngine;
using R3;

namespace Fungus
{
    /// <summary>
    /// Fungus Flowchart에서 플레이어의 인벤토리에 아이템을 지급하기 위해 사용하는 커스텀 커맨드입니다.
    /// </summary>
    [CommandInfo("Custom", "AddPlayerItem", "플레이어의 인벤토리에 아이템을 추가하고 외부 C# 시스템에 R3 스트림을 전송합니다.")]
    public class AddPlayerItem : Command
    {
        // 1. 아이템ID를 외부(Fungus Inspector)에서 수정해서 받을 수 있는 텍스트 변수 (StringData)
        [Tooltip("지급할 아이템의 ID를 입력하거나 Fungus string 변수를 연결해주세요.")]
        [SerializeField] protected StringData Id = new StringData("");

        [Tooltip("지급할 아이템의 수량을 입력하거나 Fungus int 변수를 연결해주세요.")]
        [SerializeField] protected IntegerData itemCount = new IntegerData(1);


        public override void OnEnter()
        {
            string idStr = Id.Value;

            // 문자열 ID를 정수로 파싱하여 GameItem 객체를 생성합니다.
            if (int.TryParse(idStr, out int idInt))
            {
                int count = Mathf.Max(1, itemCount.Value);

                Fungus.FungusEventBridge.BroadcastItemDeliver(idInt, count);
                
                Debug.Log($"[Fungus.AddPlayerItem] Event Fired: Item ID = {idInt}, Count = {count}");
            }
            else
            {
                Debug.LogError($"[Fungus.AddPlayerItem] 올바르지 않은 아이템 ID 형식입니다 (정수만 가능): '{idStr}'");
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(Id.Value))
            {
                return "Error: No Item ID specified";
            }
            return $"Add Item: ID [{Id.Value}] x{itemCount.Value}";
        }

        public override Color GetButtonColor()
        {
            // 기존 BroadcastMessage(초록색)와 구분되도록 보랏빛 컬러로 지정합니다.
            return new Color32(145, 110, 210, 255);
        }
    }
}

public static class MakeGameItem
{
    
}
