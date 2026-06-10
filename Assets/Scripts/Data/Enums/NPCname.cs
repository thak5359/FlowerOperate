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

    [Inject]
    private NPCManager _npcManager;

    [Inject]
    private QuestManager _questManager;

    void Awake()
    {
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
            UpdateQuestSign();
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
        UpdateQuestSign();
    }

    public void UpdateQuestSign()
    {
        if (_npcManager == null)
        {
            Debug.Log($"[NPC:{npcName}] UpdateQuestSign: _npcManager is null!");
            return;
        }

        QuestState highestState = QuestState.Unknown;
        bool showEscortSprite = false;

        Debug.Log($"[NPC:{npcName}] UpdateQuestSign called. _questManager is {(_questManager != null ? "not null" : "null")}");

        // 1. Check active progressing quests in QuestManager to see if this NPC is a different Rewarder
        if (_questManager != null)
        {
            Debug.Log($"[NPC:{npcName}] ProgressingQuests count: {_questManager.ProgressingQuests.Count}");
            foreach (var progressingQuest in _questManager.ProgressingQuests)
            {
                var (publisher, rewarder) = _questManager.GetQuestNpcs(progressingQuest.QuestID);
                Debug.Log($"[NPC:{npcName}] Checking progressing Quest {progressingQuest.QuestID}: publisher={publisher}, rewarder={rewarder}, state={progressingQuest.QuestState}");
                if (publisher != rewarder && rewarder == npcName)
                {
                    if (progressingQuest.QuestState == QuestState.InProgress)
                    {
                        showEscortSprite = true;
                        Debug.Log($"[NPC:{npcName}] Setting showEscortSprite to true for Quest {progressingQuest.QuestID}");
                    }
                    else if (progressingQuest.QuestState == QuestState.Finishable)
                    {
                        highestState = QuestState.Finishable;
                        Debug.Log($"[NPC:{npcName}] Setting highestState to Finishable for Quest {progressingQuest.QuestID}");
                    }
                }
            }
        }

        // 2. Check general quest states where this NPC is the publisher
        foreach (var pair in _npcManager.GetReceivedQuestState)
        {
            if (pair.Value.Publisher == npcName)
            {
                QuestState state = pair.Value.State;
                Debug.Log($"[NPC:{npcName}] General received quest {pair.Key}: publisher={pair.Value.Publisher}, state={state}");
                
                // Priority: Finishable > Available > InProgress > Others
                if (state == QuestState.Finishable)
                {
                    highestState = QuestState.Finishable;
                }
                else if (state == QuestState.Available && highestState != QuestState.Finishable)
                {
                    highestState = QuestState.Available;
                }
                else if (state == QuestState.InProgress && highestState != QuestState.Finishable && highestState != QuestState.Available)
                {
                    highestState = QuestState.InProgress;
                }
            }
        }

        Debug.Log($"[NPC:{npcName}] Final quest sign: highestState={highestState}, showEscortSprite={showEscortSprite}");
        SetQuestSignSprite(highestState, showEscortSprite).Forget();
    }

    private async UniTaskVoid SetQuestSignSprite(QuestState state, bool showEscortSprite)
    {
        if (npcSpriteRenderer == null) return;

        if (state == QuestState.Finishable)
        {
            npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_Finishable));
        }
        else if (showEscortSprite)
        {
            npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_Finishable));
        }
        else
        {
            switch (state)
            {
                case QuestState.InProgress:
                    npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.Dot_InProgress));
                    break;
                case QuestState.Available:
                    npcSpriteRenderer.sprite = await AddressableManager.LoadAssetAsync<Sprite>(nameof(QuestLabel.QuestMark_CanReceive));
                    break;
                default:
                    npcSpriteRenderer.sprite = null;
                    break;
            }
        }
    }
}
