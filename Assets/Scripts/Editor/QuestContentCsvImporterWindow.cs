// 수정 위치: Assets/Editor/QuestContentCsvImporterWindow.cs

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// QuestContent CSV 파일을 QuestContentSO로 변환하는 에디터 윈도우입니다.
/// </summary>
public sealed class QuestContentCsvImporterWindow : EditorWindow
{
    private TextAsset csvAsset;
    private QuestContentSO targetSo;

    /// <summary>
    /// QuestContent CSV Importer 윈도우를 엽니다.
    /// </summary>
    [MenuItem("Tools/Quest/Import Quest Content CSV")]
    public static void OpenWindow()
    {
        QuestContentCsvImporterWindow window =
            GetWindow<QuestContentCsvImporterWindow>("Quest Content Importer");

        window.minSize = new Vector2(520f, 220f);
        window.Show();
    }

    /// <summary>
    /// 에디터 윈도우 GUI를 그립니다.
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField("QuestContentSO CSV Importer", EditorStyles.boldLabel);

        EditorGUILayout.Space(8f);

        csvAsset = (TextAsset)EditorGUILayout.ObjectField(
            "CSV TextAsset",
            csvAsset,
            typeof(TextAsset),
            false
        );

        targetSo = (QuestContentSO)EditorGUILayout.ObjectField(
            "Target SO",
            targetSo,
            typeof(QuestContentSO),
            false
        );

        EditorGUILayout.HelpBox(
            "필수 컬럼: QuestId, QuestTitle, QuestDescription, Objectives, Rewards, Publisher, Rewarder\n" +
            "Objectives 예시: PlotSowing:101:5|PlotWatering:0:5\n" +
            "Rewards 예시: Currency:0:300|Item:2001:2\n" +
            "QuestContentSO.GetQuestContentById가 이진 탐색을 사용하므로 Import 시 QuestId 오름차순 정렬합니다.",
            MessageType.Info
        );

        EditorGUILayout.Space(8f);

        using (new EditorGUI.DisabledScope(csvAsset == null))
        {
            if (GUILayout.Button("Import CSV"))
            {
                ImportCsv();
            }
        }
    }

    /// <summary>
    /// 선택된 CSV 파일을 읽어 QuestContentSO에 반영합니다.
    /// </summary>
    private void ImportCsv()
    {
        if (csvAsset == null)
        {
            Debug.LogError("QuestContent CSV TextAsset이 선택되지 않았습니다.");
            return;
        }

        QuestContentSO output = EnsureTargetSo();

        if (output == null)
            return;

        List<string[]> rows = ParseCsv(csvAsset.text);

        if (rows.Count <= 1)
        {
            Debug.LogError("CSV에 데이터 행이 없습니다.");
            return;
        }

        Dictionary<string, int> headerMap = BuildHeaderMap(rows[0]);
        List<QuestContent> contents = new List<QuestContent>();

        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            string[] row = rows[rowIndex];

            if (IsEmptyRow(row))
                continue;

            int lineNumber = rowIndex + 1;

            QuestContent content = new QuestContent
            {
                QuestId = ParseInt(GetCell(row, headerMap, "QuestId", true), "QuestId", lineNumber, true),
                QuestTitle = GetCell(row, headerMap, "QuestTitle", true),
                QuestDescription = GetCell(row, headerMap, "QuestDescription", false),
                QuestObjectives = ParseObjectives(GetCell(row, headerMap, "Objectives", false), lineNumber),
                QuestRewards = ParseRewards(GetCell(row, headerMap, "Rewards", false), lineNumber),
                Publisher = ParseNpc(GetCell(row, headerMap, "Publisher", false), lineNumber, "Publisher"),
                Rewarder = ParseNpc(GetCell(row, headerMap, "Rewarder", false), lineNumber, "Rewarder")
            };

            contents.Add(content);
        }

        contents.Sort(CompareQuestContent);

        Undo.RecordObject(output, "Import QuestContent CSV");
        output.questContents = contents.ToArray();

        EditorUtility.SetDirty(output);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"QuestContentSO Import 완료. Count: {contents.Count}");
    }

    /// <summary>
    /// Target SO가 없을 경우 새 QuestContentSO 에셋을 생성합니다.
    /// </summary>
    /// <returns>가져오기 대상 QuestContentSO입니다.</returns>
    private QuestContentSO EnsureTargetSo()
    {
        if (targetSo != null)
            return targetSo;

        string path = EditorUtility.SaveFilePanelInProject(
            "Create QuestContentSO",
            "QuestContentSO",
            "asset",
            "QuestContentSO 에셋을 저장할 위치를 선택하세요."
        );

        if (string.IsNullOrEmpty(path))
            return null;

        targetSo = CreateInstance<QuestContentSO>();

        AssetDatabase.CreateAsset(targetSo, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return targetSo;
    }

    /// <summary>
    /// QuestContent를 QuestId 오름차순으로 정렬합니다.
    /// </summary>
    /// <param name="left">왼쪽 비교 대상입니다.</param>
    /// <param name="right">오른쪽 비교 대상입니다.</param>
    /// <returns>정렬 비교 결과입니다.</returns>
    private static int CompareQuestContent(QuestContent left, QuestContent right)
    {
        return left.QuestId.CompareTo(right.QuestId);
    }

    /// <summary>
    /// Objectives 셀 문자열을 QuestObjective 배열로 변환합니다.
    /// </summary>
    /// <param name="raw">Objectives 셀 문자열입니다.</param>
    /// <param name="lineNumber">CSV 줄 번호입니다.</param>
    /// <returns>QuestObjective 배열입니다.</returns>
    private static QuestObjective[] ParseObjectives(string raw, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<QuestObjective>();

        string[] entries = raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        QuestObjective[] objectives = new QuestObjective[entries.Length];

        for (int i = 0; i < entries.Length; i++)
        {
            string entry = entries[i].Trim();
            string[] parts = entry.Split(':');

            if (parts.Length != 3)
            {
                throw new Exception(
                    $"Objectives 형식 오류. Line: {lineNumber}, Entry: {entry}, Expected: ContentType:TargetID:TargetAmount"
                );
            }

            QuestContentType contentType = ParseEnum(
                parts[0],
                QuestContentType.Unknown,
                "Objectives.ContentType",
                lineNumber
            );

            int targetId = ParseInt(parts[1], "Objectives.TargetID", lineNumber, true);
            int targetAmount = ParseInt(parts[2], "Objectives.TargetAmount", lineNumber, true);

            objectives[i] = CreateQuestObjective(contentType, targetId, targetAmount);
        }

        return objectives;
    }

    /// <summary>
    /// Rewards 셀 문자열을 QuestReward 배열로 변환합니다.
    /// </summary>
    /// <param name="raw">Rewards 셀 문자열입니다.</param>
    /// <param name="lineNumber">CSV 줄 번호입니다.</param>
    /// <returns>QuestReward 배열입니다.</returns>
    private static QuestReward[] ParseRewards(string raw, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<QuestReward>();

        string[] entries = raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        QuestReward[] rewards = new QuestReward[entries.Length];

        for (int i = 0; i < entries.Length; i++)
        {
            string entry = entries[i].Trim();
            string[] parts = entry.Split(':');

            if (parts.Length != 3)
            {
                throw new Exception(
                    $"Rewards 형식 오류. Line: {lineNumber}, Entry: {entry}, Expected: RewardType:RewardID:RewardAmount"
                );
            }

            RewardType rewardType = ParseEnum(
                parts[0],
                RewardType.Unknown,
                "Rewards.RewardType",
                lineNumber
            );

            int rewardId = ParseInt(parts[1], "Rewards.RewardID", lineNumber, true);
            int rewardAmount = ParseInt(parts[2], "Rewards.RewardAmount", lineNumber, true);

            rewards[i] = CreateQuestReward(rewardType, rewardId, rewardAmount);
        }

        return rewards;
    }

    /// <summary>
    /// QuestObjective 인스턴스를 생성합니다.
    /// private setter 자동 구현 프로퍼티의 backing field에 값을 주입합니다.
    /// </summary>
    /// <param name="contentType">퀘스트 목표 타입입니다.</param>
    /// <param name="targetId">목표 대상 ID입니다.</param>
    /// <param name="targetAmount">목표 수량입니다.</param>
    /// <returns>생성된 QuestObjective입니다.</returns>
    private static QuestObjective CreateQuestObjective(
        QuestContentType contentType,
        int targetId,
        int targetAmount)
    {
        QuestObjective objective = new QuestObjective();
        object boxed = objective;

        SetAutoPropertyBackingField(boxed, typeof(QuestObjective), "ContentType", contentType);
        SetAutoPropertyBackingField(boxed, typeof(QuestObjective), "TargetID", targetId);
        SetAutoPropertyBackingField(boxed, typeof(QuestObjective), "TargetAmount", targetAmount);

        return (QuestObjective)boxed;
    }

    /// <summary>
    /// QuestReward 인스턴스를 생성합니다.
    /// private setter 자동 구현 프로퍼티의 backing field에 값을 주입합니다.
    /// </summary>
    /// <param name="rewardType">보상 타입입니다.</param>
    /// <param name="rewardId">보상 ID입니다.</param>
    /// <param name="rewardAmount">보상 수량입니다.</param>
    /// <returns>생성된 QuestReward입니다.</returns>
    private static QuestReward CreateQuestReward(
        RewardType rewardType,
        int rewardId,
        int rewardAmount)
    {
        QuestReward reward = new QuestReward();
        object boxed = reward;

        SetAutoPropertyBackingField(boxed, typeof(QuestReward), "RewardType", rewardType);
        SetAutoPropertyBackingField(boxed, typeof(QuestReward), "RewardID", rewardId);
        SetAutoPropertyBackingField(boxed, typeof(QuestReward), "RewardAmount", rewardAmount);

        return (QuestReward)boxed;
    }

    /// <summary>
    /// 자동 구현 프로퍼티의 backing field에 값을 설정합니다.
    /// </summary>
    /// <param name="boxedStruct">박싱된 구조체 인스턴스입니다.</param>
    /// <param name="structType">구조체 타입입니다.</param>
    /// <param name="propertyName">프로퍼티 이름입니다.</param>
    /// <param name="value">설정할 값입니다.</param>
    private static void SetAutoPropertyBackingField(
        object boxedStruct,
        Type structType,
        string propertyName,
        object value)
    {
        string backingFieldName = $"<{propertyName}>k__BackingField";

        FieldInfo fieldInfo = structType.GetField(
            backingFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (fieldInfo == null)
        {
            throw new MissingFieldException(
                $"{structType.Name}에서 backing field를 찾을 수 없습니다. Field: {backingFieldName}"
            );
        }

        fieldInfo.SetValue(boxedStruct, value);
    }

    /// <summary>
    /// NPC 셀 값을 NPC 타입으로 변환합니다.
    /// NPC가 enum이면 문자열 또는 숫자 파싱을 지원하고, enum이 아니면 기본값을 반환합니다.
    /// </summary>
    /// <param name="raw">NPC 셀 문자열입니다.</param>
    /// <param name="lineNumber">CSV 줄 번호입니다.</param>
    /// <param name="columnName">컬럼명입니다.</param>
    /// <returns>변환된 NPC 값입니다.</returns>
    private static NPCname ParseNpc(string raw, int lineNumber, string columnName)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return default(NPCname);

        Type npcType = typeof(NPCname);

        if (!npcType.IsEnum)
        {
            Debug.LogWarning(
                $"NPC 타입이 enum이 아니므로 CSV에서 직접 파싱하지 않았습니다. Line: {lineNumber}, Column: {columnName}"
            );

            return default(NPCname);
        }

        try
        {
            return (NPCname)Enum.Parse(npcType, raw, true);
        }
        catch
        {
            throw new Exception($"NPC 변환 실패. Line: {lineNumber}, Column: {columnName}, Value: {raw}");
        }
    }

    /// <summary>
    /// CSV 텍스트를 행과 셀 배열로 파싱합니다.
    /// 따옴표로 감싼 쉼표와 줄바꿈을 지원합니다.
    /// </summary>
    /// <param name="csv">CSV 원문입니다.</param>
    /// <returns>파싱된 CSV 행 목록입니다.</returns>
    private static List<string[]> ParseCsv(string csv)
    {
        List<string[]> rows = new List<string[]>();
        List<string> row = new List<string>();
        StringBuilder cell = new StringBuilder();

        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                row.Add(cell.ToString());
                cell.Length = 0;
                continue;
            }

            if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    i++;

                row.Add(cell.ToString());
                cell.Length = 0;

                if (!IsEmptyRow(row.ToArray()))
                    rows.Add(row.ToArray());

                row.Clear();
                continue;
            }

            cell.Append(c);
        }

        row.Add(cell.ToString());

        if (!IsEmptyRow(row.ToArray()))
            rows.Add(row.ToArray());

        return rows;
    }

    /// <summary>
    /// CSV 헤더 행을 컬럼명과 인덱스 딕셔너리로 변환합니다.
    /// </summary>
    /// <param name="headers">CSV 헤더 행입니다.</param>
    /// <returns>헤더 이름과 컬럼 인덱스 딕셔너리입니다.</returns>
    private static Dictionary<string, int> BuildHeaderMap(string[] headers)
    {
        Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headers.Length; i++)
        {
            string header = CleanCell(headers[i]);

            if (!map.ContainsKey(header))
                map.Add(header, i);
        }

        return map;
    }

    /// <summary>
    /// 지정한 컬럼의 셀 값을 가져옵니다.
    /// </summary>
    /// <param name="row">CSV 데이터 행입니다.</param>
    /// <param name="headerMap">헤더 인덱스 딕셔너리입니다.</param>
    /// <param name="columnName">가져올 컬럼명입니다.</param>
    /// <param name="required">필수 컬럼 여부입니다.</param>
    /// <returns>셀 문자열입니다.</returns>
    private static string GetCell(
        string[] row,
        Dictionary<string, int> headerMap,
        string columnName,
        bool required)
    {
        int index;

        if (!headerMap.TryGetValue(columnName, out index))
        {
            if (required)
                throw new Exception($"필수 컬럼이 없습니다. Column: {columnName}");

            return string.Empty;
        }

        if (index < 0 || index >= row.Length)
            return string.Empty;

        return CleanCell(row[index]);
    }

    /// <summary>
    /// 문자열 셀에서 BOM과 양끝 공백을 제거합니다.
    /// </summary>
    /// <param name="cell">정리할 셀 문자열입니다.</param>
    /// <returns>정리된 셀 문자열입니다.</returns>
    private static string CleanCell(string cell)
    {
        if (cell == null)
            return string.Empty;

        return cell.Trim().Trim('\uFEFF');
    }

    /// <summary>
    /// 행이 비어 있는지 확인합니다.
    /// </summary>
    /// <param name="row">검사할 CSV 행입니다.</param>
    /// <returns>모든 셀이 비어 있으면 true를 반환합니다.</returns>
    private static bool IsEmptyRow(string[] row)
    {
        if (row == null || row.Length == 0)
            return true;

        for (int i = 0; i < row.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(row[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 문자열을 int로 변환합니다.
    /// </summary>
    /// <param name="raw">변환할 문자열입니다.</param>
    /// <param name="columnName">컬럼명입니다.</param>
    /// <param name="lineNumber">CSV 줄 번호입니다.</param>
    /// <param name="required">필수 값 여부입니다.</param>
    /// <returns>변환된 정수입니다.</returns>
    private static int ParseInt(
        string raw,
        string columnName,
        int lineNumber,
        bool required)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (required)
                throw new Exception($"필수 정수 값이 비어 있습니다. Line: {lineNumber}, Column: {columnName}");

            return 0;
        }

        int value;

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            throw new Exception($"정수 변환 실패. Line: {lineNumber}, Column: {columnName}, Value: {raw}");
        }

        return value;
    }

    /// <summary>
    /// 문자열을 enum 값으로 변환합니다.
    /// </summary>
    /// <typeparam name="TEnum">변환할 enum 타입입니다.</typeparam>
    /// <param name="raw">변환할 문자열입니다.</param>
    /// <param name="defaultValue">빈 값일 때 사용할 기본값입니다.</param>
    /// <param name="columnName">컬럼명입니다.</param>
    /// <param name="lineNumber">CSV 줄 번호입니다.</param>
    /// <returns>변환된 enum 값입니다.</returns>
    private static TEnum ParseEnum<TEnum>(
        string raw,
        TEnum defaultValue,
        string columnName,
        int lineNumber)
        where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        if (!typeof(TEnum).IsEnum)
            throw new Exception($"{typeof(TEnum).Name} 타입은 enum이 아닙니다.");

        TEnum value;

        if (!Enum.TryParse(raw, true, out value))
        {
            throw new Exception($"Enum 변환 실패. Line: {lineNumber}, Column: {columnName}, Value: {raw}");
        }

        return value;
    }
}

#endif