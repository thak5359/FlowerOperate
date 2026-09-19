using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestItemTypeCountProbabilitySO", menuName = "Quest/Probability/ItemTypeCount", order = 1)]
public class QuestItemTypeCountProbabilitySO : ScriptableObject
{
    [SerializeField] private QuestCountProbability[] probabilitiesByQuestLevel =
        Array.Empty<QuestCountProbability>();

    public int GetRandomItemTypeCount(QuestLevel questLevel)
    {
        return probabilitiesByQuestLevel[GetIndex(questLevel)].GetRandomCount();
    }

    private int GetIndex(QuestLevel questLevel)
    {
        if (questLevel == QuestLevel.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(questLevel));
        }

        int index = (int)questLevel - 1;

        if ((uint)index >= (uint)probabilitiesByQuestLevel.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(questLevel));
        }

        return index;
    }
}
