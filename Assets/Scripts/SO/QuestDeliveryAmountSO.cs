using System;
using UnityEngine;

[Serializable]
public struct QuestDeliveryAmounts
{
    [Min(0)] public int ScaleLv1;
    [Min(0)] public int ScaleLv2;
    [Min(0)] public int ScaleLv3;
    [Min(0)] public int ScaleLv4;

    public int GetAmount(QuestScaleLevel scaleLevel)
    {
        return scaleLevel switch
        {
            QuestScaleLevel.Lv1 => ScaleLv1,
            QuestScaleLevel.Lv2 => ScaleLv2,
            QuestScaleLevel.Lv3 => ScaleLv3,
            QuestScaleLevel.Lv4 => ScaleLv4,
            _ => throw new ArgumentOutOfRangeException(nameof(scaleLevel))
        };
    }
}

[CreateAssetMenu(fileName = "QuestDeliveryAmountSO", menuName = "Quest/QuestDeliveryAmountSO", order = 4)]
public class QuestDeliveryAmountSO : ScriptableObject
{
    [SerializeField] private QuestDeliveryAmounts[] amountsByQuestLevel =
        new QuestDeliveryAmounts[(int)QuestLevel.Lv10];

    public int GetAmount(QuestLevel questLevel, QuestScaleLevel scaleLevel)
    {
        if (questLevel == QuestLevel.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(questLevel));
        }

        int index = (int)questLevel - 1;

        if ((uint)index >= (uint)amountsByQuestLevel.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(questLevel));
        }

        return amountsByQuestLevel[index].GetAmount(scaleLevel);
    }
}
