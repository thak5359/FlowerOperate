using R3;
using Unity.Mathematics;
using UnityEngine;

public enum NPCname 
{
    None = 0,
    Hwaja = 1,
    YeongJoon = 2,
    YeongSook = 3,
    Mago = 4,
    Hex=5,
    Yuuna = 99
}

public enum QuestLabel
{
    None = 0,
    QuestMark_CanReceive = 1,
    QuestMark_Progressing = 2,
    Exclamation_mark = 3,
    Escort_Sprite = 4
}

public class NPC : MonoBehaviour
{
    public NPCname npcName;
    public SpriteRenderer npcSpriteRenderer {get; private set;}
    // int3(Available, InProgress,Finishable) 퀘스트상태의 갯수 저장
    int3 numberOfState = new int3(0, 0, 0);
    Subject<QuestLabel> questLabel = new Subject<QuestLabel>();
    void Awake()
    {
        npcSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        questLabel.Subscribe(label => SetQuestStateSprite(label));
    }

    public void ChangeQuestSign(QuestState questState)
    {
        switch(questState)
        {
            case QuestState.Available:
                questLabel.OnNext(QuestLabel.QuestMark_CanReceive);
                break;
            case QuestState.InProgress:
                questLabel.OnNext(QuestLabel.QuestMark_Progressing);
                break;
            case QuestState.Finishable:
                questLabel.OnNext(QuestLabel.Exclamation_mark);
                break;
            default:
                questLabel.OnNext(QuestLabel.None);
                break;
        }
    }

    void SetQuestStateSprite(QuestLabel label)
    {
        numberOfState[((int)label)-1]++;
        if(numberOfState.x != 0)
            npcSpriteRenderer.sprite = AddressableManager.LoadAssetAsync<Sprite>("Exclamation_mark").GetAwaiter().GetResult();
        else if (numberOfState.y != 0)
            npcSpriteRenderer.sprite = AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_Progressing)).GetAwaiter().GetResult();
        else if (numberOfState.z != 0)
            npcSpriteRenderer.sprite = AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_CanReceive)).GetAwaiter().GetResult();
        else
            npcSpriteRenderer.sprite = null;

    }
}
