using AYellowpaper.SerializedCollections;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public struct QuestProgressState
{
    public NPCname Publisher;
    public QuestState State;
}

public class NPCManager : IPostInitializable
{
    [SerializedDictionary("NPC Enum", "NPC클래스")]
    private SerializedDictionary<NPCname, NPC> NpcDict = new SerializedDictionary<NPCname, NPC>();
   
    [SerializedDictionary("QuestId", "[NPCname], [QuestProgressState]")]
    private SerializedDictionary<int, QuestProgressState> ReceivedQuestState =  new SerializedDictionary<int, QuestProgressState>();

    private ReactiveProperty<NPCname> npcName = new ReactiveProperty<NPCname>();
    private Subject<QuestProgressState> progress = new Subject<QuestProgressState>();
    NPC[] npcClassArr;

    CompositeDisposable disposables = new CompositeDisposable();

    void IPostInitializable.PostInitialize()
    {
        progress.Subscribe(state => SyncSprite(state)).AddTo(disposables);
        npcClassArr = GameObject.FindObjectsByType<NPC>(FindObjectsSortMode.None);
        foreach(var npc in npcClassArr)
        {
            NpcDict.Add(npc.npcName, npc);
        }
    }

    public void RegisterQuestState(int id, QuestProgressState state)
    {
        ReceivedQuestState.Add(id, state);
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
        ReceivedQuestState.Remove(id);
    }

    void SyncSprite(QuestProgressState state)
    {
        npcName.Value = state.Publisher;
        NpcDict[npcName.Value].ChangeQuestSign(state.State);
    }
}
