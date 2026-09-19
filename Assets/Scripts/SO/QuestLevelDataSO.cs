using System;
using UnityEngine;

[Serializable]
public struct QuestLevelData
{
    public int Scale;
    public int Experience;
    public int BaseRequestFee;
}

[CreateAssetMenu(fileName = "QuestLevelDataSO", menuName = "Quest/QuestLevelDataSO", order = 4)]
public class QuestLevelDataSO : ScriptableObject
{
    [SerializeField] private QuestLevelData[] levelData = Array.Empty<QuestLevelData>();

    public QuestLevelData GetData(QuestLevel questLevel)
    {
        if (questLevel == QuestLevel.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(questLevel));
        }

        int index = (int)questLevel - 1;

        if (levelData == null || (uint)index >= (uint)levelData.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(questLevel));
        }

        return levelData[index];
    }
}
