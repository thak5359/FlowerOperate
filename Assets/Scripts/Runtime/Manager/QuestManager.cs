using Cysharp.Threading.Tasks;
using Fungus;
using R3;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 세이브 데이터 저장용 및 퀘스트 진척도 관리용 구조체입니다
/// </summary>
public struct QuestLog
{
      public int QuestId;
    public QuestState State;
    public int Progress; // 퀘스트 진행 상황 (예: 물주기 10번 중 3번 완료)
}




// 1. 날짜에 따라 퀘스트를 부른다!  Clear!
// 2. 수주 가능한 퀘스트 목록을 전달한다! Clear!
// 3. 퀘스트 수주를 받으면 처리한다! 
// 3.1 퀘스트 수주 요청을 받는다 Clear!
// 3.2 SO에서 관련데이터를 읽어낸다 Clear!
// 3.3 ReactiveProperty에 수주 퀘스트 값 증가를 구독한다.
// 4. 퀘스트 완료 조건이 달성되면 Finishable 퀘스트 목록에 추가한다! 



public class QuestManager : IInitializable
{
    PlayerOwnItemDataManager _playerItemManager;

    QuestRequirementSO questRequirements;
    QuestContentSO _QuestContents;
    QuestContent cachedContents;

    QuestRequirement[] AvailabeQuests;


    int[] AvailableQuestList;
    int[] FinishableQuestList;


    [Inject]
    public void Construct(PlayerOwnItemDataManager input_POITDM)
    {
        _playerItemManager = input_POITDM;
    }

    public void Initialize()
    {
        questRequirements = AddressableManager.LoadAssetAsync<QuestRequirementSO>("QuestRequirementSO").GetAwaiter().GetResult();
        _QuestContents = AddressableManager.LoadAssetAsync<QuestContentSO>("QuestContentSO").GetAwaiter().GetResult();
        GlobalEventManager.OnNextDayObservable.Subscribe(_ => UpdateAvailableQuest()).AddTo(GlobalEventManager.disposables);

        UpdateAvailableQuest();

        Fungus.FungusEventBridge.CallReceivedQuestId += SynchonizeAvailableQuestListToFungus;
        //Fungus.FungusEventBridge.CallQuestReward
    }

    public void UpdateAvailableQuest()
    {
        if (questRequirements != null)
            questRequirements.GetValidRequirements(ProgressManager.getPlayedDayOnGameSystem(), AvailabeQuests);
    }

    public void SynchonizeAvailableQuestListToFungus()
    {
        
        

        for (int i = 0; i < AvailabeQuests.Length; i++)
        {
            AvailableQuestList[i] = AvailabeQuests[i].QuestId;
        }
        Fungus.FungusEventBridge.setAvailableQuestId(ref AvailableQuestList);
    }


    public void giveReward(int questID)
    {
        //  SO 에서 데이터 받아오기
        cachedContents = _QuestContents.GetQuestContentById(questID);
    }





}