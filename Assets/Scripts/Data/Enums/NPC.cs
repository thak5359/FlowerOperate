using R3;
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

    Subject<QuestLabel> questLabel = new Subject<QuestLabel>();
    void Awake()
    {
        npcSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        questLabel.Subscribe(label => SetQuestStateSprite(label.ToString()));
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
                npcSpriteRenderer.sprite = null;
                break;
        }
    }

    void SetQuestStateSprite(string addressLabel)
    {
        npcSpriteRenderer.sprite = AddressableManager.LoadAssetAsync<Sprite>(addressLabel).GetAwaiter().GetResult();
    }
}
