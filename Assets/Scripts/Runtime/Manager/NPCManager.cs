using AYellowpaper.SerializedCollections;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using static Constant;

public struct QuestProgressState
{
    public NPCname Publisher;
    public QuestState State;

    public QuestProgressState(NPCname name, QuestState state)
    {
        this.Publisher = name;
        this.State = state;
    }
}

public class NPCManager
{
    [SerializedDictionary("NPC Enum", "NPC클래스")]
    private SerializedDictionary<NPCname, NPC> NpcDict = new SerializedDictionary<NPCname, NPC>();
   
    [SerializedDictionary("QuestId", "[NPCname], [QuestProgressState]")]
    private SerializedDictionary<int, QuestProgressState> ReceivedQuestState =  new SerializedDictionary<int, QuestProgressState>();

    private ReactiveProperty<NPCname> npcName = new ReactiveProperty<NPCname>();
    private Subject<QuestProgressState> progress = new Subject<QuestProgressState>();
    NPC[] npcClassArr;

    public SerializedDictionary<int, QuestProgressState> GetReceivedQuestState => ReceivedQuestState;
    CompositeDisposable disposables = new CompositeDisposable();

    void Init()
    {
      //Debug.Log($"<color=red>PostInitialize has been called. Scenename :  {SceneManager.GetActiveScene().name} </color>");   
        if (SceneManager.GetActiveScene().name == FARM_SCENE_NAME) // TODO :: NPC 매니저가 사용되는 씬 있으면 추가
        {
           
            progress.Subscribe(state => SyncSprite(state)).AddTo(disposables);
            npcClassArr = GameObject.FindObjectsByType<NPC>(FindObjectsSortMode.None);

            if(npcClassArr == null)
            {
                Debug.Log("npcClassArr is NULL");
                return;
            }
            foreach (var npc in npcClassArr)
            {
                NpcDict.Add(npc.npcName, npc);
            }
        }
    }

    public void RegisterQuestState(int id, QuestProgressState state)
    {
        if (ReceivedQuestState.ContainsKey(id))
        {
            return;
        }
        else
        {
            ReceivedQuestState.Add(id, state);
        }
        progress.OnNext(state);
    }

    public void ChangeQuestState(int id, QuestState state)
    {
        if(ReceivedQuestState.TryGetValue(id, out QuestProgressState ProgressState))
        {
            ProgressState.State = state;
            ReceivedQuestState[id] = ProgressState;
            progress.OnNext(ProgressState);
        }
        else
        {
            Debug.LogError("[Error] NPCManager : ChangeQuestState함수에 전달한 퀘스트ID가 딕셔너리에 존재하지 않음.");
        }
    }

    public void RemoveQuestState(int id)
    {
        if(ReceivedQuestState.TryGetValue(id, out QuestProgressState ProgressState))
        {
            NpcDict[ProgressState.Publisher].ChangeQuestSign(QuestState.Unknown);
        }
        else
        {
            Debug.LogError("[Error] NPCManager : RemoveQuestState함수에 전달한 퀘스트ID가 딕셔너리에 존재하지 않음.");
        }
        ReceivedQuestState.Remove(id);
    }

    void SyncSprite(QuestProgressState state)
    {
        Debug.Log($"<color=red>SyncSpritea has been called : {state} </color>");
        npcName.Value = state.Publisher;
        NpcDict[npcName.Value].ChangeQuestSign(state.State);
    }
}
