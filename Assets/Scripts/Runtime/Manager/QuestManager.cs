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
public partial struct QuestLog
{
    public int QuestId;
    public QuestState State;
    public int Progress; // 퀘스트 진행 상황 (예: 물주기 10번 중 3번 완료)
    public int CompletedDay; // 퀘스트를 완료한 날짜 (미완료 시 0)
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



    #region Getter
    [MemoryPackIgnore]
    public int Progress => progress;
    [MemoryPackIgnore]
    public int Goal => goal;
    [MemoryPackIgnore]
    public bool IsCompleted => progress >= goal;
    [MemoryPackIgnore]
    public string ProgressString => $"{progress} / {goal}";

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
        Dispose();

        goal = baseData.TargetAmount;
        SubscribeR3(in baseData);

        if (progress < 0)
            progress = 0;

        if (progress > goal)
            progress = goal;
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
    }

    /// <summary>
    /// 돈이나 명성 등, 현재의 상태를 추적하기 위한 함수
    /// </summary>
    /// <param name="progress"></param>
    public void SynchronizeCount(int input_progress)
    {
        progress = input_progress;
    }
    #endregion


    public void Dispose()
    {
        bag.Dispose();
        bag = default;
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

    #region Getter
    [MemoryPackIgnore]
    public int QuestID => questID;
    [MemoryPackIgnore]
    public QuestState QuestState => questState;

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
    }

    public void Rebind(in QuestContent content)
    {
        Dispose();

        QuestRewards = content.QuestRewards;

        if (QuestObjectives == null ||
            QuestObjectives.Length != content.QuestObjectives.Length)
        {
            QuestObjectives = new QuestObjectiveInProgress[content.QuestObjectives.Length];

            for (int i = 0; i < QuestObjectives.Length; i++)
            {
                QuestObjectives[i] = new QuestObjectiveInProgress(in content.QuestObjectives[i]);
            }

            return;
        }

        for (int i = 0; i < QuestObjectives.Length; i++)
        {
            QuestObjectives[i].Rebind(in content.QuestObjectives[i]);
        }
    }
    public void Dispose()
    {
        if (QuestObjectives == null)
            return;

        for (int i = 0; i < QuestObjectives.Length; i++)
        {
            QuestObjectives[i]?.Dispose();
        }
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
    private QuestRequirementSO _QuestReqs;
    private QuestContentSO _QuestContents;

    private readonly List<QuestInProgress> progressingQuests = new();
    private readonly List<QuestLog> questLogs = new();

    private QuestRequirement[] availableQuestBuffer;
    private int[] availableQuestList = Array.Empty<int>();
    private int[] finishableQuestList = Array.Empty<int>();

    private DisposableBag disposableBag;

    public IReadOnlyList<QuestInProgress> ProgressingQuests => progressingQuests;
    public IReadOnlyList<QuestLog> QuestLogs => questLogs;

    public int[] AvailableQuestList => availableQuestList;
    public int[] FinishableQuestList => finishableQuestList;

    [Inject]
    public void Construct(PlayerOwnItemDataManager input_POITDM, NPCManager input_NPCM)
    {
        _playerItemManager = input_POITDM;
        _npcManager = input_NPCM;
    }
    public void Initialize()
    {
        InitAsync().Forget();
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
            .Subscribe(_ => UpdateAvailableQuest())
            .AddTo(ref disposableBag);

        UpdateAvailableQuest();
        DoRegisterQuestStateInNpcManager();

        Fungus.FungusEventBridge.CallReceivedQuestId += SynchonizeAvailableQuestListToFungus;
    }

    private void DoRegisterQuestStateInNpcManager()
    {
        foreach (int id in availableQuestList)
        {
            _npcManager.RegisterQuestState(id,
                new QuestProgressState(_QuestContents.GetQuestPublisher(id), QuestState.Available));
        }

        foreach (int id in finishableQuestList)
        {
            _npcManager.RegisterQuestState(id,
                new QuestProgressState(_QuestContents.GetQuestPublisher(id), QuestState.Finishable));
        }
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

        List<int> validQuestIds = new List<int>(rawCount);

        for (int i = 0; i < rawCount; i++)
        {
            QuestRequirement req = availableQuestBuffer[i];

            if (!CanReceiveQuest(req))
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
        progressingQuests.Add(quest);

        SetQuestLogState(questId, QuestState.InProgress);

        _npcManager.ChangeQuestState(questId, QuestState.InProgress);
        UpdateAvailableQuest();
        RefreshFinishableQuestList();

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

        quest.Dispose();
        progressingQuests.RemoveAt(index);

        SetQuestLogState(questId, QuestState.Completed);

        _npcManager.ChangeQuestState(questId, QuestState.Completed);
        // 정적 구조체 배열 O(N) 순회로 가비지 없이 완료된 questId의 후속 연계 퀘스트들을 찾아 자동 수주 처리
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
                                    progressingQuests.Add(nextInProgress);
                                    SetQuestLogState(req.QuestId, QuestState.InProgress);
                                }
                            }
                        }
                    }
                }
            }
        }

        UpdateAvailableQuest();
        RefreshFinishableQuestList();

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
    }

    public void SynchonizeFinishableQuestListToFungus()
    {
        RefreshFinishableQuestList();
        Fungus.FungusEventBridge.setAvailableQuestId(ref finishableQuestList);
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
                    // _playerItemManager.AddItem(reward.RewardID, reward.RewardAmount);
                    break;

                default:
                    Debug.LogWarning($"처리되지 않은 보상 타입입니다. RewardType: {reward.RewardType}");
                    break;
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
    }

    private void ClearProgressingQuests()
    {
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

        ClearProgressingQuests();

        if (_QuestReqs != null)
            AddressableManager.ReleaseAsset<QuestRequirementSO>(_QuestReqs);

        if (_QuestContents != null)
            AddressableManager.ReleaseAsset<QuestContentSO>(_QuestContents);

        _QuestReqs = null;
        _QuestContents = null;
    }
}