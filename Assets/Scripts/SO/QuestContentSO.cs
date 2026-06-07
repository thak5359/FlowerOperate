using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct QuestObjective
{
    [field: SerializeField] public QuestContentType ContentType { get; private set; }
    [field: SerializeField] public int TargetID { get; private set; } // 대상 ItemID 등
    [field: SerializeField] public int TargetAmount { get; private set; } // 요구 수량
}

// 3. [수정] struct 선언부에는 [SerializeField]가 아니라 [Serializable]을 써야 인스펙터에 노출됩니다.
[Serializable]
public struct QuestReward
{
    [field: SerializeField] public RewardType RewardType { get; private set; }
    [field: SerializeField] public int RewardID { get; private set; }
    [field: SerializeField] public int RewardAmount { get; private set; }
}


[Serializable]
public struct QuestContent
{
    public int QuestId;
    public string QuestTitle;
    [TextArea] public string QuestDescription;

    // 이 퀘스트가 요구하는 실제 행동 목표들 (예: 물주기 10번, 감자 5개 납품)
    public QuestObjective[] QuestObjectives;
    public QuestReward[] QuestRewards;

    public NPCname Publisher; // 퀘스트를 주는 NPC
    public NPCname Rewarder; // 퀘스트 보상을 주는 NPC (보통은 Publisher와 같지만, 다를 수도 있음)
}

[CreateAssetMenu(fileName = "QuestContentSO", menuName = "Quest/QuestContentSO", order = 2)]
public class QuestContentSO : ScriptableObject
{
    [SerializeField] public QuestContent[] questContents;

    public ref QuestContent GetQuestContentById(int questId)
    {
        int left = 0;
        int right = questContents.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int midId = questContents[mid].QuestId;

            if (midId == questId)
            {
                return ref questContents[mid]; // 구조체 원본 참조 반환
            }
            else if (midId < questId)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        throw new KeyNotFoundException($"Quest ID {questId}를 데이터셋에서 찾을 수 없습니다.");
    }

    public NPCname GetQuestPublisher(int questId)
    {
        int left = 0;
        int right = questContents.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int midId = questContents[mid].QuestId;

            if (midId == questId)
            {
                return questContents[mid].Publisher; // 구조체 원본 참조 반환
            }
            else if (midId < questId)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        // ref 반환 구조상 찾지 못했을 때는 null을 줄 수 없으므로 안전하게 예외를 던집니다.
        // (기존 코드에서 -1 인덱스 참조로 에러가 나던 예외 상황을 안전하게 명시적 예외로 대체해요)
        throw new KeyNotFoundException($"Quest ID {questId}를 데이터셋에서 찾을 수 없습니다.");
    }

}
