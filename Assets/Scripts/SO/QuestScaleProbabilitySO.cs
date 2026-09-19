using System;
using UnityEngine;

[Serializable]
public struct QuestScaleProbability
{
    [Min(0)] public int Scale1Weight;
    [Min(0)] public int Scale2Weight;
    [Min(0)] public int Scale3Weight;
    [Min(0)] public int Scale4Weight;

    public QuestScaleLevel GetRandomScaleLevel()
    {
        if (Scale1Weight < 0 || Scale2Weight < 0 || Scale3Weight < 0 || Scale4Weight < 0)
        {
            throw new InvalidOperationException("확률 가중치는 음수일 수 없습니다.");
        }

        int totalWeight = Scale1Weight + Scale2Weight + Scale3Weight + Scale4Weight;

        if (totalWeight <= 0)
        {
            throw new InvalidOperationException("확률 가중치 합계는 0보다 커야 합니다.");
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);

        if (roll < Scale1Weight)
        {
            return QuestScaleLevel.Lv1;
        }

        roll -= Scale1Weight;

        if (roll < Scale2Weight)
        {
            return QuestScaleLevel.Lv2;
        }

        roll -= Scale2Weight;

        return roll < Scale3Weight
            ? QuestScaleLevel.Lv3
            : QuestScaleLevel.Lv4;
    }
}

[CreateAssetMenu(fileName = "QuestScaleProbabilitySO", menuName = "Quest/Probability/Scale", order = 3)]
public class QuestScaleProbabilitySO : ScriptableObject
{
    [SerializeField] private QuestScaleProbability[] probabilitiesByQuestLevel =
        Array.Empty<QuestScaleProbability>();

    public QuestScaleLevel GetRandomScaleLevel(QuestLevel questLevel)
    {
        return probabilitiesByQuestLevel[GetIndex(questLevel)].GetRandomScaleLevel();
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
