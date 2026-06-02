// 수정 위치: BroadcastMessage.cs 파일 전체

using R3;
using System;
using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Fungus 커맨드와 외부 C# 전역 시스템(ActionMapChanger 등)을 중계하는 정적 브릿지 클래스입니다.
    /// </summary>
    public static class FungusEventBridge
    {
        // 외부에서 구독할 전역 이벤트
        public static event Action<string> OnFungusMessageBroadcasted;
        public static event Action<int, int> onItemAdded;

        public static void Broadcast(string message)
        {
            OnFungusMessageBroadcasted?.Invoke(message);
        }
        public static void BroadcastItemDeliver(int id, int count)
        {
            onItemAdded?.Invoke(id, count);
        }
    }
    public enum FungusBroadcastType
    {
        OpenChatBox,   // 대화창이 열릴 때 쏠 신호
        CloseChatBox,   // 대화창이 닫힐 때 쏠 신호
        OpenShop,   // 상점 오픈 시 사용할 신호

    }


    [CommandInfo("Custom", "BroadcastMessage", "외부 C# 시스템(입력 맵 변경 등)으로 전역 이벤트 신호를 보냅니다.")]
    public class BroadcastMessage : Command
    {
        [Tooltip("보낼 메시지 키워드를 선택해 주세요. 드롭다운으로 표시됩니다.")]
        [SerializeField] private FungusBroadcastType messageTarget = FungusBroadcastType.CloseChatBox;

        public override void OnEnter()
        {
            // 선택된 enum 값을 문자열(string)로 변환합니다. 
            // ("OpenChatBox" 혹은 "CloseChatBox"라는 string으로 깔끔하게 뽑혀요!)
            string messageKey = messageTarget.ToString();

            // 정적 브릿지를 통해 전역으로 신호를 안전하게 뿜어냅니다.
            FungusEventBridge.Broadcast(messageKey);
            Debug.Log(messageKey);

            Continue();
        }

        public override string GetSummary()
        {
            // 인스펙터의 블록 흐름창에서도 어떤걸 선택했는지 한눈에 보이게 텍스트를 구성해요.
            return $"Broadcast: [{messageTarget}]";
        }

        public override Color GetButtonColor()
        {
            // Fungus 에디터에서 다른 기본 커맨드와 구별하기 쉽게 산뜻한 초록색으로 칠해줄게요.
            return new Color32(130, 200, 130, 255);
        }
    }
}