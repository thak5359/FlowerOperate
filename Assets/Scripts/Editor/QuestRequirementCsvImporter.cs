#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// QuestRequirement CSV 파일을 QuestRequirementSO로 변환하는 에디터 윈도우입니다.
/// </summary>
public sealed class QuestRequirementCsvImporterWindow : EditorWindow
{
    private TextAsset csvAsset;
    private QuestRequirementSO targetSo;

    /// <summary>
    /// QuestRequirement CSV Importer 윈도우를 엽니다.
    /// </summary>
    [MenuItem("Tools/Quest/Import Quest Requirement CSV")]
    public static void OpenWindow()
    {
        QuestRequirementCsvImporterWindow window =
            GetWindow<QuestRequirementCsvImporterWindow>("Quest Requirement Importer");

        window.minSize = new Vector2(480f, 180f);
        window.Show();
    }

    /// <summary>
    /// 에디터 윈도우 GUI를 그립니다.
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.Space(8f);

        EditorGUILayout.LabelField("QuestRequirementSO CSV Importer", EditorStyles.boldLabel);

        EditorGUILayout.Space(8f);

        csvAsset = (TextAsset)EditorGUILayout.ObjectField(
            "CSV TextAsset",
            csvAsset,
            typeof(TextAsset),
            false
        );

        targetSo = (QuestRequirementSO)EditorGUILayout.ObjectField(
            "Target SO",
            targetSo,
            typeof(QuestRequirementSO),
            false
        );

        EditorGUILayout.HelpBox(
            "필수 컬럼: QuestId, UnlockDate, ExpiredDate, PrereqQuestId, PrereqQuestState\n" +
            "GetValidRequirements가 UnlockDate 오름차순을 전제로 하므로 Import 시 UnlockDate → QuestId 순으로 정렬합니다.",
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
    /// 선택된 CSV 파일을 읽어 QuestRequirementSO에 반영합니다.
    /// </summary>
    private void ImportCsv()
    {
        if (csvAsset == null)
        {
            Debug.LogError("QuestRequirement CSV TextAsset이 선택되지 않았습니다.");
            return;
        }

        QuestRequirementSO output = EnsureTargetSo();

        if (output == null)
            return;

        List<string[]> rows = ParseCsv(csvAsset.text);

        if (rows.Count <= 1)
        {
            Debug.LogError("CSV에 데이터 행이 없습니다.");
            return;
        }

        Dictionary<string, int> headerMap = BuildHeaderMap(rows[0]);
        List<QuestRequirement> requirements = new List<QuestRequirement>();

        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            string[] row = rows[rowIndex];

            if (IsEmptyRow(row))
                continue;

            int lineNumber = rowIndex + 1;

            QuestRequirement requirement = new QuestRequirement
            {
                QuestId = ParseInt(GetCell(row, headerMap, "QuestId", true), "QuestId", lineNumber, true),
                UnlockDate = ParseInt(GetCell(row, headerMap, "UnlockDate", true), "UnlockDate", lineNumber, true),
                ExpiredDate = ParseInt(GetCell(row, headerMap, "ExpiredDate", false), "ExpiredDate", lineNumber, false),
                PrereqQuestId = ParseInt(GetCell(row, headerMap, "PrereqQuestId", false), "PrereqQuestId", lineNumber, false),
                PrereqQuestState = ParseEnum(
                    GetCell(row, headerMap, "PrereqQuestState", false),
                    QuestState.Unknown,
                    "PrereqQuestState",
                    lineNumber
                )
            };

            requirements.Add(requirement);
        }

        requirements.Sort(CompareQuestRequirement);

        Undo.RecordObject(output, "Import QuestRequirement CSV");
        output.questRequirements = requirements.ToArray();

        EditorUtility.SetDirty(output);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"QuestRequirementSO Import 완료. Count: {requirements.Count}");
    }

    /// <summary>
    /// Target SO가 없을 경우 새 QuestRequirementSO 에셋을 생성합니다.
    /// </summary>
    /// <returns>가져오기 대상 QuestRequirementSO입니다.</returns>
    private QuestRequirementSO EnsureTargetSo()
    {
        if (targetSo != null)
            return targetSo;

        string path = EditorUtility.SaveFilePanelInProject(
            "Create QuestRequirementSO",
            "QuestRequirementSO",
            "asset",
            "QuestRequirementSO 에셋을 저장할 위치를 선택하세요."
        );

        if (string.IsNullOrEmpty(path))
            return null;

        targetSo = CreateInstance<QuestRequirementSO>();

        AssetDatabase.CreateAsset(targetSo, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return targetSo;
    }

    /// <summary>
    /// QuestRequirement를 UnlockDate 오름차순, QuestId 오름차순으로 정렬합니다.
    /// </summary>
    /// <param name="left">왼쪽 비교 대상입니다.</param>
    /// <param name="right">오른쪽 비교 대상입니다.</param>
    /// <returns>정렬 비교 결과입니다.</returns>
    private static int CompareQuestRequirement(QuestRequirement left, QuestRequirement right)
    {
        int dateCompare = left.UnlockDate.CompareTo(right.UnlockDate);

        if (dateCompare != 0)
            return dateCompare;

        return left.QuestId.CompareTo(right.QuestId);
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