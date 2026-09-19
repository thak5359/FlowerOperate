using System;
using UnityEngine;

[Serializable]
public struct QuestCountProbability
{
    [Min(0)] public int OneCountWeight;
    [Min(0)] public int TwoCountWeight;
    [Min(0)] public int ThreeCountWeight;

    public int GetRandomCount()
    {
        if (OneCountWeight < 0 || TwoCountWeight < 0 || ThreeCountWeight < 0)
        {
            throw new InvalidOperationException("확률 가중치는 음수일 수 없습니다.");
        }

        int totalWeight = OneCountWeight + TwoCountWeight + ThreeCountWeight;

        if (totalWeight <= 0)
        {
            throw new InvalidOperationException("확률 가중치 합계는 0보다 커야 합니다.");
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);

        if (roll < OneCountWeight)
        {
            return 1;
        }

        roll -= OneCountWeight;

        return roll < TwoCountWeight ? 2 : 3;
    }
}
