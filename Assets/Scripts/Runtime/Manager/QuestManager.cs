using Cysharp.Threading.Tasks;
using Fungus;
using R3;
using VContainer.Unity;

public class QuestManager : IInitializable
{
    QuestRequirementSO questRequirements;
    QuestContentSO questContents;
    QuestContent cachedContents;

    QuestRequirement[] AvailabeQuests;







    int[] AvailableQuestList;
    int[] FinishableQuestList;

    public void Initialize()
    {
        questRequirements = AddressableManager.LoadAssetAsync<QuestRequirementSO>("QuestRequirementSO").GetAwaiter().GetResult();
        questContents = AddressableManager.LoadAssetAsync<QuestContentSO>("QuestContentSO").GetAwaiter().GetResult();
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

        cachedContents = Quest








    }





}