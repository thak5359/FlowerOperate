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

    public SerializedDictionary<int, QuestProgressState> GetReceivedQuestState => ReceivedQuestState;
    CompositeDisposable disposables = new CompositeDisposable();

    public NPCManager()
    {
        progress.Subscribe(state => SyncSprite(state)).AddTo(disposables);
    }

    public void RegisterNPC(NPCname name, NPC npc)
    {
        NpcDict[name] = npc;

        // 등록 시 해당 NPC가 퍼블리셔인 활성화된 퀘스트의 최신 상태를 즉시 동기화
        foreach (var pair in ReceivedQuestState)
        {
            if (pair.Value.Publisher == name)
            {
                npc.ChangeQuestSign(pair.Value.State);
            }
        }
    }

    public void UnregisterNPC(NPCname name)
    {
        if (NpcDict.ContainsKey(name))
        {
            NpcDict.Remove(name);
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
            if (NpcDict.TryGetValue(ProgressState.Publisher, out var npc))
            {
                npc.ChangeQuestSign(QuestState.Unknown);
            }
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
        if (NpcDict.TryGetValue(state.Publisher, out var npc))
        {
            npc.ChangeQuestSign(state.State);
        }
    }
}
