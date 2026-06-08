using UnityEngine;

[System.Serializable]
public struct QuestRequirement
{
    public int QuestId;
    public int UnlockDate;       // 게임 내 해금 일차 (ProgressManager.Day 기준)
    public int ExpiredDate;      // 만료 일차 (0이면 무기한 등)
    public int PrereqQuestId;    // 선행 퀘스트 ID
    public QuestState PrereqQuestState; // 선행 퀘스트의 요구 상태 (보통은 Completed를 요구)
}



[CreateAssetMenu(fileName = "QuestRequirementSO", menuName = "Quest/QuestRequirementSO", order=1)]
public class QuestRequirementSO : ScriptableObject
{
    [SerializeField] public QuestRequirement[] questRequirements;

    public int GetValidRequirements(int currentDay, QuestRequirement[] resultBuffer, System.Collections.Generic.IReadOnlyList<QuestLog> questLogs)
    {
        int count = 0;

        for (int i = 0; i < questRequirements.Length; i++)
        {
            // 구조체 복사를 막기 위해 ref로 참조해서 읽어옵니다.
            ref QuestRequirement req = ref questRequirements[i];

            // 1. 만료 일자가 존재하고 이미 만료일자가 지났다면
            if (req.ExpiredDate != 0 && req.ExpiredDate <= currentDay)
            {
                // 이 퀘스트는 무효하므로 패스하고 다음 퀘스트를 검사
                continue;
            }

            // 2. 선행 퀘스트 조건이 있는 연계 퀘스트인 경우
            if (req.PrereqQuestId != 0)
            {
                if (questLogs == null)
                    continue;

                bool hasPrereqCompleted = false;
                int completedDay = 0;

                for (int j = 0; j < questLogs.Count; j++)
                {
                    var log = questLogs[j];
                    if (log.QuestId == req.PrereqQuestId && log.State == QuestState.Completed)
                    {
                        hasPrereqCompleted = true;
                        completedDay = log.CompletedDay;
                        break;
                    }
                }

                if (!hasPrereqCompleted)
                {
                    // 선행 퀘스트가 미완료되었으므로 제외
                    continue;
                }

                // 선행을 완료한 날짜 + 대기 일차(UnlockDate)가 현재 날짜보다 크면 아직 해금되지 않음
                if (currentDay < completedDay + req.UnlockDate)
                {
                    continue;
                }
            }
            else
            {
                // 3. 선행 퀘스트 조건이 없는 일반 퀘스트인 경우
                if (req.UnlockDate > currentDay)
                {
                    continue;
                }
            }

            // 4. 모든 조건을 통과했다면 버퍼에 담아
            if (count < resultBuffer.Length)
            {
                resultBuffer[count] = req;
                count++;
            }
        }

        // 최종적으로 몇 개를 찾았는지 개수를 반환합니다.
        return count;
    }

}