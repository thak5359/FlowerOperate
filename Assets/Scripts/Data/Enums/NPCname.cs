using Cysharp.Threading.Tasks;
using R3;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using VContainer.Unity;

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
    Dot_InProgress = 2,
    QuestMark_Finishable = 3,
    Escort_Sprite = 4
}

public class NPC : MonoBehaviour
{
    public NPCname npcName;
    [field:SerializeField]
    public SpriteRenderer npcSpriteRenderer {get; set;}
    // int3(Available, InProgress,Finishable) 퀘스트상태의 갯수 저장
    int3 numberOfState = new int3(0, 0, 0);
    Subject<QuestLabel> questLabel = new Subject<QuestLabel>();

    [Inject]
    private NPCManager _npcManager;

    void Awake()
    {
        //npcSpriteRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
        questLabel.Subscribe(label => SetQuestStateSprite(label).Forget());
    }

    void Start()
    {
        // VContainer 수동 주입 (자동 주입 설정이 안 되어 있을 경우를 대비)
        var scope = LifetimeScope.Find<LifetimeScope>();
        if (scope != null && scope.Container != null)
        {
            scope.Container.Inject(this);
        }

        if (_npcManager != null)
        {
            _npcManager.RegisterNPC(npcName, this);
        }
    }

    void OnDestroy()
    {
        if (_npcManager != null)
        {
            _npcManager.UnregisterNPC(npcName);
        }
    }

    public void ChangeQuestSign(QuestState questState)
    {
        switch(questState)
        {
            case QuestState.Available:
                questLabel.OnNext(QuestLabel.QuestMark_CanReceive);
                break;
            case QuestState.InProgress:
                questLabel.OnNext(QuestLabel.Dot_InProgress);
                break;
            case QuestState.Finishable:
                questLabel.OnNext(QuestLabel.QuestMark_Finishable);
                break;
            case QuestState.Completed:
                questLabel.OnNext(QuestLabel.None);
                break;
            default:
                questLabel.OnNext(QuestLabel.None);
                break;
        }
    }

    async UniTaskVoid SetQuestStateSprite(QuestLabel label)
    {
        if(label == QuestLabel.None)
        {
            npcSpriteRenderer.sprite = null;
            numberOfState[2]--;
            return;
        }
        if(label != QuestLabel.QuestMark_CanReceive)
            numberOfState[((int)label)-2]--;
        numberOfState[((int)label)-1]++;
        if (numberOfState.z != 0)
            npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_Finishable));
        else if (numberOfState.y != 0)
            npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.Dot_InProgress));
        else if(numberOfState.x != 0)
            npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_CanReceive));
    }
}
