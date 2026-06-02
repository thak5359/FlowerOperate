using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using System;

[CreateAssetMenu(fileName = "New Quest List", menuName = "Quest/QuestListSO", order = 1)]
public class QuestListSO : ScriptableObject
{
    [SerializedDictionary("QuestID", "Block")]
    [SerializeField]
    SerializedDictionary<int, Block> questList = new SerializedDictionary<int, Block>();

    [SerializeField]
    private Block defaultBlock = new Block();
}
