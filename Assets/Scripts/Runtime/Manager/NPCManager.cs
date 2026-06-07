using AYellowpaper.SerializedCollections;
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

public class NPCManager : IInitializable
{
    [SerializedDictionary("NPC Enum", "NPC클래스")]
    private SerializedDictionary<NPCname, NpcClass> NpcDict = new SerializedDictionary<NPCname, NpcClass>();
   
    [SerializedDictionary("QuestId", "[NPC], [QuestProgressState]")]
    private SerializedDictionary<int, QuestProgressState> ReceivedQuestState;

    NpcClass[] npcClassArr;

    void IInitializable.Initialize()
    {
        npcClassArr = GameObject.FindObjectsByType<NpcClass>(FindObjectsSortMode.None);
        foreach(var npc in npcClassArr)
        {
            NpcDict.Add(npc.npcName, npc);
        }
    }

    public void RegisterQuestState(int id, QuestProgressState state)
    {
        ReceivedQuestState.Add(id, state);
    }

    public void ChangeQuestState(int id, QuestState state)
    {
        if(ReceivedQuestState.TryGetValue(id, out QuestProgressState ProgressState))
        {
            ProgressState.State = state;
            ReceivedQuestState[id] = ProgressState;
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
}
