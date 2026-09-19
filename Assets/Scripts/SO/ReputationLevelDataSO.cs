using System;
using UnityEngine;

[Serializable]
public struct QuestLevelProbability
{
    public int CurrentLevelWeight;
    public int OneLevelBelowWeight;
    public int TwoLevelsBelowWeight;
}

[Serializable]
public struct ReputationLevelData
{
    public int RequiredExperienceForNextLevel;
    public int DailyQuestCount;
    public int MaxAcceptedQuestCount;
    public QuestLevel MinQuestLevel;
    public QuestLevel MaxQuestLevel;
    public QuestLevelProbability QuestLevelProbability;
}

[CreateAssetMenu(fileName = "ReputationLevelDataSO", menuName = "Quest/ReputationLevelDataSO", order = 3)]
public class ReputationLevelDataSO : ScriptableObject
{
    [SerializeField] private ReputationLevelData[] levelData = Array.Empty<ReputationLevelData>();

    public ReputationLevelData GetData(ReputationLevel reputationLevel)
    {
        return levelData[GetIndex(reputationLevel)];
    }

    public QuestLevel GetRandomQuestLevel(ReputationLevel reputationLevel)
    {
        ReputationLevelData data = GetData(reputationLevel);
        QuestLevelProbability probability = data.QuestLevelProbability;

        if (probability.CurrentLevelWeight < 0 ||
            probability.OneLevelBelowWeight < 0 ||
            probability.TwoLevelsBelowWeight < 0)
        {
            throw new InvalidOperationException($"{reputationLevel}의 퀘스트 레벨 가중치는 음수일 수 없습니다.");
        }

        int currentLevel = (int)data.MaxQuestLevel;

        if ((currentLevel < 2 && probability.OneLevelBelowWeight > 0) ||
            (currentLevel < 3 && probability.TwoLevelsBelowWeight > 0))
        {
            throw new InvalidOperationException($"{reputationLevel}에 존재하지 않는 하위 퀘스트 레벨 가중치가 설정되어 있습니다.");
        }

        int totalWeight =
            probability.CurrentLevelWeight +
            probability.OneLevelBelowWeight +
            probability.TwoLevelsBelowWeight;

        if (totalWeight <= 0)
        {
            throw new InvalidOperationException($"{reputationLevel}의 퀘스트 레벨 가중치 합계는 0보다 커야 합니다.");
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);

        if (roll < probability.CurrentLevelWeight)
        {
            return data.MaxQuestLevel;
        }

        roll -= probability.CurrentLevelWeight;

        if (roll < probability.OneLevelBelowWeight)
        {
            return (QuestLevel)(currentLevel - 1);
        }

        return (QuestLevel)(currentLevel - 2);
    }

    private int GetIndex(ReputationLevel reputationLevel)
    {
        if (reputationLevel == ReputationLevel.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(reputationLevel));
        }

        int index = (int)reputationLevel - 1;

        if (levelData == null || (uint)index >= (uint)levelData.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(reputationLevel));
        }

        return index;
    }
}
