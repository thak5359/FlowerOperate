// 수정 위치: BroadcastMessage.cs 파일 전체

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
        public static event Action CallReceivedQuestId;
        public static event Action<string> OnFungusMessageBroadcasted;

        public static event Action<int> CallQuestReward;
        public static event Action<int> OnReceivedQuest;
        public static event Action<int> OnCompleteQuest;
        public static event Action<int> OnFinishableQuest;

        private static int[] receivedAvailableQuestId;
        private static int[] receivedFinishableQuestId;


        public static ref int[] getAvailableQuestId => ref receivedAvailableQuestId;
        public static void setAvailableQuestId(ref int[] arr) => receivedAvailableQuestId = arr;

        public static ref int[] getFinishableQuestId => ref receivedFinishableQuestId;
        public static void setFinishableQuestId(ref int[] arr) => receivedFinishableQuestId = arr;

        public static void Broadcast(string message)
        {
            OnFungusMessageBroadcasted?.Invoke(message);
        }
      
        public static void BroadcastCallQuestId()
        {
            CallReceivedQuestId?.Invoke();
        }

        public static void BroadcastCallQuestReward(int questID)
        {
            CallQuestReward?.Invoke(questID);
        }

        public static void BroadcastReceivedQuest(int questId)
        {
            OnReceivedQuest?.Invoke(questId);
        }

        public static void BroadcastCompleteQuest(int questId)
        {
            OnCompleteQuest?.Invoke(questId);
        }

        public static void BroadcastFinishableQuest(int questId)
        {
            OnFinishableQuest?.Invoke(questId);
        }
    }

    public enum FungusBroadcastType
    {
        OpenChatBox,   // 대화창이 열릴 때 쏠 신호
        CloseChatBox,   // 대화창이 닫힐 때 쏠 신호
        OpenShop,   // 상점 오픈 시 사용할 신호
        CallQuestList,   // 수주한 퀘스트 리스트 받아올 때 쏠 신호
        ReceivedQuest,  // 퀘스트 수주 시 쏠 신호 (데이터: 퀘스트 ID)
        CompleteQuest,   // 퀘스트 완료 시 쏠 신호 (데이터: 퀘스트 ID)
        FinishableQuest  // 퀘스트 완료 가능 상태 돌입 시 쏠 신호 (데이터: 퀘스트 ID)
    }


    [CommandInfo("Custom", "BroadcastMessage", "외부 C# 시스템(입력 맵 변경 등)으로 전역 이벤트 신호를 보냅니다.")]
    public class BroadcastMessage : Command
    {
        [Tooltip("보낼 메시지 키워드를 선택해 주세요. 드롭다운으로 표시됩니다.")]
        [SerializeField] private FungusBroadcastType messageTarget = FungusBroadcastType.CloseChatBox;

        [Tooltip("ReceivedQuest, CompleteQuest, FinishableQuest 선택 시 동반해서 전송될 int형 값(예: 퀘스트 ID)입니다.")]
        [SerializeField] private int questId = 0;

        public override void OnEnter()
        {
            // 선택된 enum 값을 문자열(string)로 변환합니다. 
            // ("OpenChatBox" 혹은 "CloseChatBox"라는 string으로 깔끔하게 뽑혀요!)
            string messageKey = messageTarget.ToString();

            switch (messageTarget)
            {
                case FungusBroadcastType.CallQuestList:
                    FungusEventBridge.BroadcastCallQuestId();
                    break;
                case FungusBroadcastType.ReceivedQuest:
                    FungusEventBridge.BroadcastReceivedQuest(questId);
                    break;
                case FungusBroadcastType.CompleteQuest:
                    FungusEventBridge.BroadcastCompleteQuest(questId);
                    break;
                case FungusBroadcastType.FinishableQuest:
                    FungusEventBridge.BroadcastFinishableQuest(questId);
                    break;
                default:
                    FungusEventBridge.Broadcast(messageKey);
                    break;
            }
            // 정적 브릿지를 통해 전역으로 신호를 안전하게 뿜어냅니다.
            Debug.Log($"{messageKey} (Quest ID: {questId})");

            Continue();
        }

        public override string GetSummary()
        {
            // 인스펙터의 블록 흐름창에서도 어떤걸 선택했는지 한눈에 보이게 텍스트를 구성해요.
            if (messageTarget == FungusBroadcastType.ReceivedQuest || 
                messageTarget == FungusBroadcastType.CompleteQuest ||
                messageTarget == FungusBroadcastType.FinishableQuest)
            {
                return $"Broadcast: [{messageTarget}] (Quest ID: {questId})";
            }
            return $"Broadcast: [{messageTarget}]";
        }

        public override Color GetButtonColor()
        {
            // Fungus 에디터에서 다른 기본 커맨드와 구별하기 쉽게 산뜻한 초록색으로 칠해줄게요.
            return new Color32(130, 200, 130, 255);
        }
    }
}