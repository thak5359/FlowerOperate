using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public struct QuestProgressState
{
    public NPC Publisher;
    public QuestState State;
}

public class NPCManager : IInitializable
{
    [SerializedDictionary("NPC Enum", "NPC클래스")]
    private SerializedDictionary<NPC, NpcClass> NpcDict = new SerializedDictionary<NPC, NpcClass>();
   
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
        if(NpcDict.TryGetValue(state.Publisher, out NpcClass npc))
        {
            npc.ChangeSprite(state.State);
        }
        else
        {
            Debug.LogError("[Error] NPCManager : RegisterQuestState함수에 전달한 NPC Enum이 딕셔너리에 존재하지 않음.");
        }
    }

    public void ChangeQuestState(int id, QuestState state)
    {
        if(ReceivedQuestState.TryGetValue(id, out QuestProgressState ProgressState))
        {
            ProgressState.State = state;
            ReceivedQuestState[id] = ProgressState;
            NpcDict[ProgressState.Publisher].ChangeSprite(state);
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
            NpcDict[ProgressState.Publisher].ChangeSprite(QuestState.Unknown);
        }
        else
        {
            Debug.LogError("[Error] NPCManager : RemoveQuestState함수에 전달한 퀘스트ID가 딕셔너리에 존재하지 않음.");
        }
        ReceivedQuestState.Remove(id);
    }
}
