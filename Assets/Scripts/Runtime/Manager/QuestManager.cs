using Cysharp.Threading.Tasks;
using MemoryPack;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 세이브 데이터 저장용 및 퀘스트 진척도 관리용 구조체입니다
/// </summary>
[MemoryPackable]
[Serializable]
public partial struct QuestLog
{
    [MemoryPackInclude] public int QuestId;
    [MemoryPackInclude] public QuestState State;
    [MemoryPackInclude] public int Progress; // 퀘스트 진행 상황 (예: 물주기 10번 중 3번 완료)
    [MemoryPackInclude] public int CompletedDay; // 퀘스트를 완료한 날짜 (미완료 시 0)
}



[StructLayout(LayoutKind.Sequential)]
[MemoryPackable]
[Serializable]
public partial class QuestObjectiveInProgress : IDisposable
{
    [MemoryPackInclude]
    private int progress;
    [MemoryPackIgnore]
    private int goal;
    [MemoryPackIgnore]
    private DisposableBag bag;
    [MemoryPackIgnore]
    private readonly Subject<Unit> onProgressChanged = new();

    #region Getter
    [MemoryPackIgnore]
    public int Progress => progress;
    [MemoryPackIgnore]
    public int Goal => goal;
    [MemoryPackIgnore]
    public bool IsCompleted => progress >= goal;
    [MemoryPackIgnore]
    public string ProgressString => $"{progress} / {goal}";
    [MemoryPackIgnore]
    public Observable<Unit> OnProgressChanged => onProgressChanged;

    #endregion


    /// <summary>
    /// 밭을 갈거나 물을 주는등의 행위를 할때마다 값을 증가시키기 위한 함수
    /// </summary>
    /// <param name="input"></param>


    public QuestObjectiveInProgress(in QuestObjective baseData)
    {
        progress = 0;
        Rebind(in baseData);
    }


    [MemoryPackConstructor]
    public QuestObjectiveInProgress(int progress)
    {
        this.progress = progress;

        goal = 0;
        bag = default;
    }

    public void Rebind(in QuestObjective baseData)
    {
        ClearSubscriptions();

        goal = baseData.TargetAmount;
        SubscribeR3(in baseData);

        if (progress < 0)
            progress = 0;

        if (progress > goal)
            progress = goal;
    }

    private void ClearSubscriptions()
    {
        bag.Dispose();
        bag = default;
    }

    /// <summary>
    /// R3 기반으로 작성된 반응형 구독을 처리합니다.
    /// </summary>
    /// <param name="targetType"></param>
    public void SubscribeR3(in QuestObjective baseData)
    {
        int targetItemID = baseData.TargetID;

        switch (baseData.ContentType)
        {
            case QuestContentType.PlowPlot:
                SubscribeCount(QuestProgressPublisher.PlowPlot);
                return;

            case QuestContentType.PlotSowing:
                SubscribeCount(QuestProgressPublisher.PlotSowing, targetItemID);
                return;

            case QuestContentType.PlotWatering:
                SubscribeCount(QuestProgressPublisher.PlotWatering, targetItemID);
                return;

            case QuestContentType.PlotHammeringPlot:
                SubscribeCount(QuestProgressPublisher.PlotHammeringPlot);
                return;

            case QuestContentType.PlotReaping:
                SubscribeCount(QuestProgressPublisher.PlotReaping, targetItemID);
                return;

            case QuestContentType.Unknown:
                IncrementCount();
                return;

            default:
                Debug.LogError($"아직 로직화 되지 않은 퀘스트 컨텐츠 타입입니다. ContentType: {baseData.ContentType}, TargetID: {targetItemID}");
                return;
        }
    }

    #region Method for R3 Delegation


    private void SubscribeCount(Subject<Unit> subject)
    {
        subject
            .Subscribe(_ => IncrementCount())
            .AddTo(ref bag);
    }

    private void SubscribeCount(Subject<int> subject, int targetItemID)
    {
        if (targetItemID != 0)
        {
            subject
                .Where(id => id == targetItemID)
                .Subscribe(_ => IncrementCount())
                .AddTo(ref bag);
        }
        else
        {
            subject
                .Subscribe(_ => IncrementCount())
                .AddTo(ref bag);
        }
    }

    public void IncrementCount()
    {
        progress++;
        onProgressChanged.OnNext(Unit.Default);
    }

    /// <summary>
    /// 돈이나 명성 등, 현재의 상태를 추적하기 위한 함수
    /// </summary>
    /// <param name="progress"></param>
    public void SynchronizeCount(int input_progress)
    {
        progress = input_progress;
        onProgressChanged.OnNext(Unit.Default);
    }
    #endregion


    public void Dispose()
    {
        ClearSubscriptions();
        onProgressChanged.Dispose();
    }
}

[StructLayout(layoutKind: LayoutKind.Sequential)]
[Serializable]
[MemoryPackable]

// 수주 받은 이후 현재 진행중인 퀘스트의 정보를 관리하기 위한 클래스입니다.
public partial class QuestInProgress : IDisposable
{

    [MemoryPackInclude]
    private int questID;

    [MemoryPackInclude]
    private QuestState questState;

    [MemoryPackInclude]
    public QuestObjectiveInProgress[] QuestObjectives;

    [MemoryPackIgnore]
    public QuestReward[] QuestRewards;

    [MemoryPackIgnore]
    private readonly Subject<QuestState> onStateChanged = new();

    [MemoryPackIgnore]
    private CompositeDisposable objectivesDisposables = new();

    #region Getter
    [MemoryPackIgnore]
    public int QuestID => questID;
    [MemoryPackIgnore]
    public QuestState QuestState => questState;
    [MemoryPackIgnore]
    public Observable<QuestState> OnStateChanged => onStateChanged;

    #endregion

    public bool IsCompleted
    {
        get
        {
            if (QuestObjectives == null || QuestObjectives.Length == 0)
                return false;

            for (int i = 0; i < QuestObjectives.Length; i++)
            {
                if (QuestObjectives[i] == null || !QuestObjectives[i].IsCompleted)
                    return false;
            }

            return true;
        }
    }



    /// <summary>
    /// 행동 횟수를 추적하기 위한 함수입니다.
    /// </summary>

    public QuestInProgress(in QuestContent inputQuestContent)
    {
        questID = inputQuestContent.QuestId;
        questState = QuestState.InProgress;

        QuestObjectives = new QuestObjectiveInProgress[inputQuestContent.QuestObjectives.Length];

        for (int i = 0; i < inputQuestContent.QuestObjectives.Length; i++)
        {
            QuestObjectives[i] = new QuestObjectiveInProgress(in inputQuestContent.QuestObjectives[i]);
        }

        QuestRewards = inputQuestContent.QuestRewards;
        SetupObjectiveSubscriptions();
    }

    [MemoryPackConstructor]
    public QuestInProgress(
       int questID,
       QuestState questState,
       QuestObjectiveInProgress[] questObjectives)
    {
        this.questID = questID;
        this.questState = questState;
        QuestObjectives = questObjectives;

        QuestRewards = null;
        SetupObjectiveSubscriptions();
    }

    private void SetupObjectiveSubscriptions()
    {
        objectivesDisposables.Dispose();
        objectivesDisposables = new CompositeDisposable();

        if (QuestObjectives == null)
            return;

        for (int i = 0; i < QuestObjectives.Length; i++)
        {
            if (QuestObjectives[i] == null) continue;

            QuestObjectives[i].OnProgressChanged
                .Subscribe(_ => CheckAndUpdateState())
                .AddTo(objectivesDisposables);
        }
    }

    public void CheckAndUpdateState()
    {
        if (questState != QuestState.InProgress && questState != QuestState.Finishable)
            return;

        QuestState newState = IsCompleted ? QuestState.Finishable : QuestState.InProgress;
        if (questState != newState)
        {
            questState = newState;
            onStateChanged.OnNext(questState);
        }
    }

    public void Rebind(in QuestContent content)
    {
        QuestRewards = content.QuestRewards;

        if (QuestObjectives == null ||
            QuestObjectives.Length != content.QuestObjectives.Length)
        {
            ClearSubscriptions(disposeObjectives: true);

            QuestObjectives = new QuestObjectiveInProgress[content.QuestObjectives.Length];

            for (int i = 0; i < QuestObjectives.Length; i++)
            {
                QuestObjectives[i] = new QuestObjectiveInProgress(in content.QuestObjectives[i]);
            }
        }
        else
        {
            ClearSubscriptions(disposeObjectives: false);

            for (int i = 0; i < QuestObjectives.Length; i++)
            {
                QuestObjectives[i].Rebind(in content.QuestObjectives[i]);
            }
        }

        SetupObjectiveSubscriptions();
    }

    private void ClearSubscriptions(bool disposeObjectives)
    {
        objectivesDisposables.Dispose();
        objectivesDisposables = new CompositeDisposable();

        if (QuestObjectives != null && disposeObjectives)
        {
            for (int i = 0; i < QuestObjectives.Length; i++)
            {
                QuestObjectives[i]?.Dispose();
            }
        }
    }

    public void Dispose()
    {
        ClearSubscriptions(disposeObjectives: true);
        onStateChanged.Dispose();
    }

}

// 1. 날짜에 따라 퀘스트를 부른다!  Clear!
// 2. 수주 가능한 퀘스트 목록을 전달한다! Clear!
// 3. 퀘스트 수주를 받으면 처리한다! 
// 3.1 퀘스트 수주 요청을 받는다 Clear!
// 3.2 SO에서 관련데이터를 읽어낸다 Clear!
// 3.3 ReactiveProperty에 수주 퀘스트 값 증가를 구독한다.
// 4. 퀘스트 완료 조건이 달성되면 Finishable 퀘스트 목록에 추가한다! 



public class QuestManager : IInitializable, IDisposable
{
    private PlayerOwnItemDataManager _playerItemManager;
    private NPCManager _npcManager;
    private ItemManager _itemManager;
    private QuestRequirementSO _QuestReqs;
    private QuestContentSO _QuestContents;
    private SaveLoadManager _SaveLoadManager;

    private readonly List<QuestInProgress> progressingQuests = new();
    private readonly List<QuestLog> questLogs = new();
    private readonly Dictionary<int, IDisposable> questStateSubscriptions = new();

    private QuestRequirement[] availableQuestBuffer;
    private int[] availableQuestList = Array.Empty<int>();
    private int[] finishableQuestList = Array.Empty<int>();

    private DisposableBag disposableBag;

    private readonly Subject<Unit> onQuestListChanged = new();
    public Observable<Unit> OnQuestListChanged => onQuestListChanged;

    public IReadOnlyList<QuestInProgress> ProgressingQuests => progressingQuests;
    public IReadOnlyList<QuestLog> QuestLogs => questLogs;

    public int[] AvailableQuestList => availableQuestList;
    public int[] FinishableQuestList => finishableQuestList;

    [Inject]
    public void Construct(PlayerOwnItemDataManager input_POITDM, NPCManager input_NPCM
        , ItemManager input_ITM, SaveLoadManager input_SLM)
    {
        _playerItemManager = input_POITDM;
        _npcManager = input_NPCM;
        _itemManager = input_ITM;
        _SaveLoadManager = input_SLM;
    }
    public void Initialize()
    {
        InitAsync().Forget();
        Fungus.FungusEventBridge.OnReceivedQuest += IgnoreReceiveReturn();
        Fungus.FungusEventBridge.OnCompleteQuest += IgnoreCompleteReturn();
    }

    private Action<int> IgnoreReceiveReturn()
    {
        return questId => ReceiveQuest(questId);
    }

    private Action<int> IgnoreCompleteReturn()
    {
        return questId => CompleteQuest(questId);
    }

    private async UniTaskVoid InitAsync()
    {

        await Addressables.InitializeAsync();   

        _QuestReqs = await AddressableManager
                    .LoadAssetAsync<QuestRequirementSO>("QuestRequirementSO");

        _QuestContents = await AddressableManager
            .LoadAssetAsync<QuestContentSO>("QuestContentSO");

        if (_QuestReqs != null && _QuestReqs.questRequirements != null)
            availableQuestBuffer = new QuestRequirement[_QuestReqs.questRequirements.Length];
        else
            availableQuestBuffer = Array.Empty<QuestRequirement>();

        GlobalEventManager.OnNextDayObservable
            .Subscribe(_ => {
                ProcessDayChangeQuestStatus();
                UpdateAvailableQuest();
                DoRegisterQuestStateInNpcManager();
                SynchonizeAvailableQuestListToFungus();
            })
            .AddTo(ref disposableBag);

        if (_playerItemManager != null)
        {
            _playerItemManager.InventoryRevisionChanged
                .Subscribe(_ => {
                    for (int i = 0; i < progressingQuests.Count; i++)
                    {
                        SynchronizePassiveObjectives(progressingQuests[i]);
                    }
                })
                .AddTo(ref disposableBag);
        }

        UpdateAvailableQuest();
        DoRegisterQuestStateInNpcManager();

        Fungus.FungusEventBridge.CallReceivedQuestId += SynchonizeAvailableQuestListToFungus;
        Fungus.FungusEventBridge.CallReceivedQuestId += SynchonizeFinishableQuestListToFungus;
        Fungus.FungusEventBridge.CallReceivedQuestId += SynchonizeProgressingQuestListToFungus;

        SynchonizeAvailableQuestListToFungus();
        SynchonizeFinishableQuestListToFungus();
        SynchonizeProgressingQuestListToFungus();
    }

    private void EnsureQuestRegisteredInNpcManager(int questId, QuestState initialState = QuestState.Available)
    {
        if (_npcManager != null && !_npcManager.GetReceivedQuestState.ContainsKey(questId))
        {
            string publisher = _QuestContents != null ? _QuestContents.GetQuestPublisher(questId).ToString() : "";
            _npcManager.RegisterQuestState(questId, new QuestProgressState((NPCname)Enum.Parse(typeof(NPCname), publisher), initialState));
        }
    }

    private void DoRegisterQuestStateInNpcManager()
    {
        if (_QuestContents == null || _npcManager == null) return;

        foreach (int id in availableQuestList)
        {
            EnsureQuestRegisteredInNpcManager(id, QuestState.Available);
            Debug.Log($" availableQuest Registered, QuestID : {id} ");
            _npcManager.ChangeQuestState(id, QuestState.Available);
        }

        foreach (var quest in progressingQuests)
        {
            EnsureQuestRegisteredInNpcManager(quest.QuestID, quest.QuestState);
            Debug.Log($" progressingQuest Registered, QuestID : {quest.QuestID}, State: {quest.QuestState} ");
            _npcManager.ChangeQuestState(quest.QuestID, quest.QuestState);
        }

        foreach (int id in finishableQuestList)
        {
            EnsureQuestRegisteredInNpcManager(id, QuestState.Finishable);
            Debug.Log($" finishableQuest Registered, QuestID : {id} ");
            _npcManager.ChangeQuestState(id, QuestState.Finishable);
        }

        _npcManager.UpdateAllNPCSigns();
    }

    public void UpdateAvailableQuest()
    {
        if (_QuestReqs == null || availableQuestBuffer == null)
        {
            availableQuestList = Array.Empty<int>();
            return;
        }

        int currentDay = ProgressManager.getPlayedDayOnGameSystem();

        int rawCount = _QuestReqs.GetValidRequirements(
            currentDay,
            availableQuestBuffer,
            questLogs
        );

        Debug.Log($"[QuestManager] UpdateAvailableQuest - currentDay: {currentDay}, GetValidRequirements count: {rawCount}");

        List<int> validQuestIds = new List<int>(rawCount);

        for (int i = 0; i < rawCount; i++)
        {
            QuestRequirement req = availableQuestBuffer[i];
            bool canReceive = CanReceiveQuest(req);
            Debug.Log($"[QuestManager] Checking Quest {req.QuestId} - PrereqQuestId: {req.PrereqQuestId}, CanReceive: {canReceive}");

            if (!canReceive)
                continue;

            validQuestIds.Add(req.QuestId);
        }


        availableQuestList = validQuestIds.ToArray();
    }



    private bool CanReceiveQuest(QuestRequirement req)
    {
        if (IsQuestInProgress(req.QuestId))
            return false;

        if (HasQuestState(req.QuestId, QuestState.Completed))
            return false;

        if (req.PrereqQuestId != 0)
        {
            if (!HasQuestState(req.PrereqQuestId, req.PrereqQuestState))
                return false;
        }

        return true;
    }

    private bool IsQuestInProgress(int questId)
    {
        for (int i = 0; i < progressingQuests.Count; i++)
        {
            if (progressingQuests[i].QuestID == questId)
                return true;
        }

        return false;
    }

    private bool HasQuestState(int questId, QuestState state)
    {
        for (int i = 0; i < questLogs.Count; i++)
        {
            if (questLogs[i].QuestId == questId &&
                questLogs[i].State == state)
            {
                return true;
            }
        }

        for (int i = 0; i < progressingQuests.Count; i++)
        {
            QuestInProgress quest = progressingQuests[i];

            if (quest.QuestID == questId &&
                quest.QuestState == state)
            {
                return true;
            }
        }

        return false;
    }

    private void SubscribeToQuestState(QuestInProgress quest)
    {
        UnsubscribeFromQuestState(quest.QuestID);
        var sub = quest.OnStateChanged.Subscribe(state => HandleQuestStateChanged(quest.QuestID, state));
        questStateSubscriptions[quest.QuestID] = sub;
    }

    private void UnsubscribeFromQuestState(int questId)
    {
        if (questStateSubscriptions.TryGetValue(questId, out var sub))
        {
            sub.Dispose();
            questStateSubscriptions.Remove(questId);
        }
    }

    private void HandleQuestStateChanged(int questId, QuestState state)
    {
        SetQuestLogState(questId, state);
        _npcManager.ChangeQuestState(questId, state);

        if (_QuestContents != null)
        {
            try
            {
                ref var content = ref _QuestContents.GetQuestContentById(questId);
                if (content.Publisher != content.Rewarder)
                {
                    _npcManager.UpdateNPCSign(content.Rewarder);
                }
            }
            catch {}
        }

        RefreshFinishableQuestList();
    }

    public bool ReceiveQuest(int questId)
    {
        if (_QuestContents == null)
        {
            Debug.LogError("QuestContentSO가 로드되지 않았습니다.");
            return false;
        }

        if (IsQuestInProgress(questId))
            return false;

        if (HasQuestState(questId, QuestState.Completed))
            return false;

        ref QuestContent content = ref _QuestContents.GetQuestContentById(questId);

        QuestInProgress quest = new QuestInProgress(in content);
        SynchronizePassiveObjectives(quest);
        SubscribeToQuestState(quest);
        progressingQuests.Add(quest);

        SetQuestLogState(questId, QuestState.InProgress);

        EnsureQuestRegisteredInNpcManager(questId, QuestState.InProgress);
        _npcManager.ChangeQuestState(questId, QuestState.InProgress);

        if (content.Publisher != content.Rewarder)
        {
            _npcManager.UpdateNPCSign(content.Rewarder);
        }

        UpdateAvailableQuest();
        RefreshFinishableQuestList();
        DoRegisterQuestStateInNpcManager();

        SynchonizeAvailableQuestListToFungus();
        SynchonizeFinishableQuestListToFungus();
        SynchonizeProgressingQuestListToFungus();

        onQuestListChanged.OnNext(Unit.Default);

        return true;
    }

    public bool CompleteQuest(int questId)
    {
        int index = FindProgressingQuestIndex(questId);

        if (index < 0)
            return false;

        QuestInProgress quest = progressingQuests[index];

        if (!quest.IsCompleted)
        {
            Debug.Log($"Quest {questId}는 아직 완료 조건을 만족하지 않았습니다.");
            return false;
        }

        ref QuestContent content = ref _QuestContents.GetQuestContentById(questId);

        GiveReward(in content);

        UnsubscribeFromQuestState(questId);

        quest.Dispose();
        progressingQuests.RemoveAt(index);

        SetQuestLogState(questId, QuestState.Completed);

        EnsureQuestRegisteredInNpcManager(questId, QuestState.Completed);
        _npcManager.ChangeQuestState(questId, QuestState.Completed);

        if (content.Publisher != content.Rewarder)
        {
            _npcManager.UpdateNPCSign(content.Rewarder);
        }
        /* [수동 수주 전환] 정적 구조체 배열 O(N) 순회로 완료된 questId의 후속 연계 퀘스트들을 찾아 자동 수주 처리하던 부분을 비활성화합니다.
        int currentDay = ProgressManager.getPlayedDayOnGameSystem();
        if (_QuestReqs != null && _QuestReqs.questRequirements != null)
        {
            for (int i = 0; i < _QuestReqs.questRequirements.Length; i++)
            {
                ref var req = ref _QuestReqs.questRequirements[i];
                if (req.PrereqQuestId == questId)
                {
                    // 날짜 해금(완료날짜(오늘) + UnlockDate <= 오늘) 및 만료 조건 확인
                    if (currentDay >= currentDay + req.UnlockDate && (req.ExpiredDate == 0 || req.ExpiredDate > currentDay))
                    {
                        if (_QuestContents != null)
                        {
                            ref QuestContent nextContent = ref _QuestContents.GetQuestContentById(req.QuestId);
                            if (nextContent.QuestId != 0)
                            {
                                if (!IsQuestInProgress(req.QuestId) && !HasQuestState(req.QuestId, QuestState.Completed))
                                {
                                    QuestInProgress nextInProgress = new QuestInProgress(in nextContent);
                                    SubscribeToQuestState(nextInProgress);
                                    progressingQuests.Add(nextInProgress);
                                    SetQuestLogState(req.QuestId, QuestState.InProgress);

                                    EnsureQuestRegisteredInNpcManager(req.QuestId, QuestState.InProgress);
                                    _npcManager.ChangeQuestState(req.QuestId, QuestState.InProgress);
                                }
                            }
                        }
                    }
                }
            }
        }
        */

        UpdateAvailableQuest();
        RefreshFinishableQuestList();
        DoRegisterQuestStateInNpcManager();

        SynchonizeAvailableQuestListToFungus();
        SynchonizeFinishableQuestListToFungus();
        SynchonizeProgressingQuestListToFungus();

        onQuestListChanged.OnNext(Unit.Default);

        return true;
    }

    private int FindProgressingQuestIndex(int questId)
    {
        for (int i = 0; i < progressingQuests.Count; i++)
        {
            if (progressingQuests[i].QuestID == questId)
                return i;
        }

        return -1;
    }

    private void SetQuestLogState(int questId, QuestState state)
    {
        for (int i = 0; i < questLogs.Count; i++)
        {
            QuestLog log = questLogs[i];

            if (log.QuestId == questId)
            {
                log.State = state;
                if (state == QuestState.Completed)
                {
                    log.CompletedDay = ProgressManager.getPlayedDayOnGameSystem();
                }
                questLogs[i] = log;
                return;
            }
        }

        questLogs.Add(new QuestLog
        {
            QuestId = questId,
            State = state,
            Progress = 0,
            CompletedDay = (state == QuestState.Completed) ? ProgressManager.getPlayedDayOnGameSystem() : 0
        });
    }

    public void RefreshFinishableQuestList()
    {
        List<int> result = new List<int>();

        for (int i = 0; i < progressingQuests.Count; i++)
        {
            QuestInProgress quest = progressingQuests[i];

            if (quest.IsCompleted)
            {
                result.Add(quest.QuestID);
            }
        }

        finishableQuestList = result.ToArray();
    }

    public void SynchonizeAvailableQuestListToFungus()
    {
        UpdateAvailableQuest();
        Fungus.FungusEventBridge.setAvailableQuestId(ref availableQuestList);
        DoRegisterQuestStateInNpcManager();
    }

    public void SynchonizeFinishableQuestListToFungus()
    {
        RefreshFinishableQuestList();
        Fungus.FungusEventBridge.setFinishableQuestId(ref finishableQuestList);
        DoRegisterQuestStateInNpcManager();
    }

    public void SynchonizeProgressingQuestListToFungus()
    {
        int[] progressingQuestIds = progressingQuests.Select(q => q.QuestID).ToArray();
        Fungus.FungusEventBridge.setProgressingQuestId(ref progressingQuestIds);
        DoRegisterQuestStateInNpcManager();
    }

    private void GiveReward(in QuestContent content)
    {
        for (int i = 0; i < content.QuestRewards.Length; i++)
        {
            QuestReward reward = content.QuestRewards[i];

            switch (reward.RewardType)
            {
                case RewardType.Currency:
                    // TODO:
                    // _playerItemManager.AddMoney(reward.RewardAmount);
                    break;

                case RewardType.Item:
                    // TODO:
                        _playerItemManager.AddItem(ContainerType.INVENTORY, _itemManager.CreateItem(reward.RewardID, reward.RewardAmount));
                    break;

                default:
                    Debug.LogWarning($"처리되지 않은 보상 타입입니다. RewardType: {reward.RewardType}");
                    break;
            }
        }
    }

    public (NPCname publisher, NPCname rewarder) GetQuestNpcs(int questId)
    {
        if (_QuestContents == null) return (NPCname.None, NPCname.None);
        try
        {
            ref var content = ref _QuestContents.GetQuestContentById(questId);
            return (content.Publisher, content.Rewarder);
        }
        catch
        {
            return (NPCname.None, NPCname.None);
        }
    }

    public bool TryGetQuestContent(int questId, out QuestContent content)
    {
        if (_QuestContents == null)
        {
            content = default;
            return false;
        }
        try
        {
            content = _QuestContents.GetQuestContentById(questId);
            return true;
        }
        catch
        {
            content = default;
            return false;
        }
    }

    public int GetItemCount(int itemId)
    {
        if (_playerItemManager == null) return 0;
        var list = _playerItemManager.GetData.GetItemList(ContainerType.INVENTORY);
        if (list == null) return 0;

        int total = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item != null && item.Id == itemId)
            {
                total += item.Count;
            }
        }
        return total;
    }

    private void SynchronizePassiveObjectives(QuestInProgress quest)
    {
        if (_QuestContents == null || quest == null || quest.QuestObjectives == null) return;
        try
        {
            ref var content = ref _QuestContents.GetQuestContentById(quest.QuestID);
            for (int i = 0; i < quest.QuestObjectives.Length; i++)
            {
                var objInProgress = quest.QuestObjectives[i];
                if (objInProgress == null) continue;

                if (content.QuestObjectives != null && i < content.QuestObjectives.Length)
                {
                    var baseObj = content.QuestObjectives[i];
                    switch (baseObj.ContentType)
                    {
                        case QuestContentType.OwnItemSpecific:
                        case QuestContentType.SubmissionItem:
                            int itemCount = GetItemCount(baseObj.TargetID);
                            objInProgress.SynchronizeCount(itemCount);
                            break;
                    }
                }
            }
        }
        catch {}
    }

    private void ProcessDayChangeQuestStatus()
    {
        int currentDay = ProgressManager.getPlayedDayOnGameSystem();

        // 1. 진행 중인 퀘스트 중 만료일 지난 것 Failed 처리
        for (int i = progressingQuests.Count - 1; i >= 0; i--)
        {
            QuestInProgress quest = progressingQuests[i];
            if (_QuestReqs != null && _QuestReqs.questRequirements != null)
            {
                for (int j = 0; j < _QuestReqs.questRequirements.Length; j++)
                {
                    ref var req = ref _QuestReqs.questRequirements[j];
                    if (req.QuestId == quest.QuestID)
                    {
                        if (req.ExpiredDate != 0 && currentDay >= req.ExpiredDate)
                        {
                            Debug.LogWarning($"[QuestManager] 퀘스트 {quest.QuestID}의 기간이 만료되어 실패 처리됩니다.");
                            UnsubscribeFromQuestState(quest.QuestID);
                            quest.Dispose();
                            progressingQuests.RemoveAt(i);

                            EnsureQuestRegisteredInNpcManager(req.QuestId, QuestState.Failed);
                            SetQuestLogState(req.QuestId, QuestState.Failed);
                            _npcManager.ChangeQuestState(req.QuestId, QuestState.Failed);
                        }
                        break;
                    }
                }
            }
        }

        // 2. 수주 대기(Available) 퀘스트 중 만료일이 지난 것 Expired 처리
        if (_QuestReqs != null && _QuestReqs.questRequirements != null)
        {
            for (int i = 0; i < _QuestReqs.questRequirements.Length; i++)
            {
                ref var req = ref _QuestReqs.questRequirements[i];
                if (req.ExpiredDate != 0 && currentDay >= req.ExpiredDate)
                {
                    bool isLogged = false;
                    for (int j = 0; j < questLogs.Count; j++)
                    {
                        if (questLogs[j].QuestId == req.QuestId)
                        {
                            isLogged = true;
                            if (questLogs[j].State == QuestState.Available || questLogs[j].State == QuestState.Unknown)
                            {
                                EnsureQuestRegisteredInNpcManager(req.QuestId, QuestState.Expired);
                                SetQuestLogState(req.QuestId, QuestState.Expired);
                                _npcManager.ChangeQuestState(req.QuestId, QuestState.Expired);
                            }
                            break;
                        }
                    }
                    if (!isLogged)
                    {
                        EnsureQuestRegisteredInNpcManager(req.QuestId, QuestState.Expired);
                        SetQuestLogState(req.QuestId, QuestState.Expired);
                        _npcManager.ChangeQuestState(req.QuestId, QuestState.Expired);
                    }
                }
            }
        }
    }

    public QuestInProgress[] GetProgressingQuestSaveData()
    {
        return progressingQuests.ToArray();
    }

    public QuestLog[] GetQuestLogSaveData()
    {
        return questLogs.ToArray();
    }

    public void LoadQuestData(
        QuestInProgress[] loadedProgressingQuests,
        QuestLog[] loadedQuestLogs)
    {
        ClearProgressingQuests();

        questLogs.Clear();

        if (loadedQuestLogs != null)
            questLogs.AddRange(loadedQuestLogs);

        if (loadedProgressingQuests != null)
        {
            for (int i = 0; i < loadedProgressingQuests.Length; i++)
            {
                QuestInProgress quest = loadedProgressingQuests[i];

                if (quest == null)
                    continue;

                try
                {
                    ref QuestContent content = ref _QuestContents.GetQuestContentById(quest.QuestID);
                    quest.Rebind(in content);

                    SynchronizePassiveObjectives(quest);

                    SubscribeToQuestState(quest);
                    progressingQuests.Add(quest);
                    SetQuestLogState(quest.QuestID, QuestState.InProgress);
                }
                catch (Exception e)
                {
                    Debug.LogError($"로드된 퀘스트를 Rebind하는 데 실패했습니다. QuestID: {quest.QuestID}\n{e}");
                    quest.Dispose();
                }
            }
        }

        UpdateAvailableQuest();
        RefreshFinishableQuestList();
        DoRegisterQuestStateInNpcManager();

        onQuestListChanged.OnNext(Unit.Default);
    }

    private void ClearProgressingQuests()
    {
        foreach (var sub in questStateSubscriptions.Values)
        {
            sub.Dispose();
        }
        questStateSubscriptions.Clear();

        for (int i = 0; i < progressingQuests.Count; i++)
        {
            progressingQuests[i]?.Dispose();
        }

        progressingQuests.Clear();
    }

    public void Dispose()
    {
        disposableBag.Dispose();

        Fungus.FungusEventBridge.CallReceivedQuestId -= SynchonizeAvailableQuestListToFungus;
        Fungus.FungusEventBridge.CallReceivedQuestId -= SynchonizeFinishableQuestListToFungus;
        Fungus.FungusEventBridge.CallReceivedQuestId -= SynchonizeProgressingQuestListToFungus;

        ClearProgressingQuests();

        if (_QuestReqs != null)
            AddressableManager.ReleaseAsset<QuestRequirementSO>(_QuestReqs);

        if (_QuestContents != null)
            AddressableManager.ReleaseAsset<QuestContentSO>(_QuestContents);

        _QuestReqs = null;
        _QuestContents = null;
    }
}