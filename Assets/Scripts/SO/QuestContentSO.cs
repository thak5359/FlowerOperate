using System;
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

    public NPC Publisher; // 퀘스트를 주는 NPC
    public NPC Rewarder; // 퀘스트 보상을 주는 NPC (보통은 Publisher와 같지만, 다를 수도 있음)
}

[CreateAssetMenu(fileName ="QuestContentSO", menuName = "Quest/QuestContentSO", order = 2)]
public class QuestContentSO : ScriptableObject
{
    [SerializeField] public QuestContent[] questContents;
}
