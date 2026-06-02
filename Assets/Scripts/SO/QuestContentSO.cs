using System;
using UnityEngine;



[SerializeField]
public struct Item
{

}


[SerializeField]
public struct QuestReward
{
    

}


[Serializable]
public struct QuestContent
{

    [SerializeField] public int questId;
    [SerializeField] public string QuestTitle;
    [SerializeField] public string QuestDescription;
    [SerializeField] public int rewardReputation;
    [SerializeField] public int rewardGold;

}

[CreateAssetMenu(fileName ="QuestContentSO", menuName = "Quest/QuestContentSO", order = 1)]
public class QuestContentSO : ScriptableObject
{
    [SerializeField] public QuestContent[] questContents;


}
