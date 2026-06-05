using UnityEngine;

public enum NPC 
{
    None = 0,
    Hwaja = 1,
    YeongJoon = 2,
    YeongSook = 3,
    Mago = 4,
    Hex=5,
    Yuuna = 99
}

public class NpcClass : MonoBehaviour
{
    public NPC npcName;
    public SpriteRenderer QuestStateSpriteRenderer {get; private set;}

    void Awake()
    {
        QuestStateSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void ChangeSprite(QuestState state)
    {
        Sprite newSprite = null;
        switch (state)
        {
            case QuestState.Available:
                newSprite = AddressableManager.LoadAssetAsync<Sprite>("QuestMark_CanReceive").GetAwaiter().GetResult();
                break;
            case QuestState.InProgress:
                newSprite = AddressableManager.LoadAssetAsync<Sprite>("QuestMark_InProgress").GetAwaiter().GetResult();
                break;
            case QuestState.Finishable:
                newSprite = AddressableManager.LoadAssetAsync<Sprite>("QuestMark_Finishable").GetAwaiter().GetResult();
                break;
            default:
                Debug.LogError("[Error] NpcClass : ChangeSprite함수에 전달한 퀘스트 상태가 유효하지 않음.");
                newSprite = null;
                break;
        }
        QuestStateSpriteRenderer.sprite = newSprite;
    }
}
