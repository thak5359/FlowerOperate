// 수정 위치: 프로젝트 내의 Editor 폴더(없으면 생성)에 "ChunkDataImporter.cs" 라는 이름으로 새 C# 스크립트를 생성하고 아래 코드를 덮어씌워 주세요.
// Assets/Editor/ChunkDataImporter.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics; // int4를 사용한다고 가정했습니다.

public class ChunkDataImporter : EditorWindow
{
    // 파트너가 사용하시는 ChunkData 임시 파싱 구조체
    private class ParsedChunk
    {
        public int ChunkId;
        public ChunkType Type;
        public ChunkGrade Grade;
        public string ContiguousChunkRaw; // "2,3,4," 등의 문자열
    }

    [MenuItem("Tools/성화당/CSV로 Chunk SO 생성하기")]
    public static void ImportChunkCSV()
    {
        // 1. CSV 파일 선택
        string path = EditorUtility.OpenFilePanel("상점 테이블 - 청크 데이터셋 선택", "Assets", "csv");
        if (string.IsNullOrEmpty(path)) return;

        // 2. CSV 읽기 및 파싱
        string[] lines = File.ReadAllLines(path);
        List<ParsedChunk> allChunks = new List<ParsedChunk>();

        // CSV의 쉼표(,)를 분리하되, 따옴표("") 안의 쉼표는 무시하는 정규식
        Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        // 헤더 및 불필요한 상단 데이터(ScriptableObject 정의 등) 건너뛰기
        // 실제 데이터가 시작되는 시점을 ChunkId가 숫자로 파싱되는지로 판별합니다.
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = csvParser.Split(lines[i]);

            // 데이터 열이 부족하거나, ChunkId(1번 인덱스)가 정수로 변환되지 않으면 헤더/설명글로 간주하고 스킵
            if (row.Length < 5 || !int.TryParse(row[1], out int chunkId)) continue;

            ParsedChunk data = new ParsedChunk
            {
                ChunkId = chunkId,
                Type = Enum.TryParse(row[2].Trim(), out ChunkType type) ? type : ChunkType.Unknonwn,
                Grade = Enum.TryParse("Lv" + row[3].Trim(), out ChunkGrade grade) ? grade : ChunkGrade.Unknown,
                ContiguousChunkRaw = row[4].Replace("\"", "").Trim() // 따옴표 제거
            };

            allChunks.Add(data);
        }

        // 3. 타입별 분류 및 ID 내림차순 정렬
        var groupedChunks = allChunks
            .Where(c => c.Type != ChunkType.Unknonwn) // Unknown 제외
            .GroupBy(c => c.Type)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(c => c.ChunkId).ToList() // ID 내림차순 정렬
            );

        // 4. 저장할 기본 경로 설정 (없으면 폴더 생성)
        string savePath = "Assets/Resources/ChunkDataSets";
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(savePath)) AssetDatabase.CreateFolder("Assets/Resources", "ChunkDataSets");

        // 5. SO 생성
        CreateAndSaveSO<FarmChunkDataSet>(ChunkType.Farm, groupedChunks, savePath, "FarmChunkDataSetSO");
        CreateAndSaveSO<ForestChunkDataSet>(ChunkType.Forest, groupedChunks, savePath, "ForestChunkDataSetSO");
        CreateAndSaveSO<FieldChunkDataSet>(ChunkType.Field, groupedChunks, savePath, "FieldChunkDataSetSO");
        CreateAndSaveSO<MineChunkDataSet>(ChunkType.Mine, groupedChunks, savePath, "MineChunkDataSetSO");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=cyan>[성화당]</color> 파트너! CSV 데이터를 성공적으로 변환하여 4개의 SO를 생성했어요.");
    }

    private static void CreateAndSaveSO<T>(ChunkType targetType, Dictionary<ChunkType, List<ParsedChunk>> dict, string path, string fileName) where T : ScriptableObject
    {
        if (!dict.ContainsKey(targetType)) return;

        List<ParsedChunk> chunks = dict[targetType];

        // SO 인스턴스 생성
        T so = ScriptableObject.CreateInstance<T>();

        // [field: SerializeField] private ChunkData[] DataList; 에 데이터 넣기
        // 필드가 private이므로 SerializedObject를 사용하여 안전하게 할당합니다.
        SerializedObject serializedSO = new SerializedObject(so);
        SerializedProperty dataListProp = serializedSO.FindProperty("<DataList>k__BackingField");

        // 만약 [field: SerializeField] 방식이 아니라 일반 변수명이라면 "DataList"로 시도
        if (dataListProp == null) dataListProp = serializedSO.FindProperty("DataList");

        if (dataListProp != null)
        {
            dataListProp.ClearArray();
            dataListProp.arraySize = chunks.Count;

            for (int i = 0; i < chunks.Count; i++)
            {
                SerializedProperty elementProp = dataListProp.GetArrayElementAtIndex(i);

                // 각 요소의 프로퍼티 찾기 및 값 할당
                elementProp.FindPropertyRelative("ChunkId").intValue = chunks[i].ChunkId;

                // Enum 값 할당
                elementProp.FindPropertyRelative("ChunkType").enumValueIndex = (int)chunks[i].Type;
                elementProp.FindPropertyRelative("ChunkGrade").enumValueIndex = (int)chunks[i].Grade;

                // int4 구조체 파싱 및 할당 (ex: "2,3,4," -> x:2, y:3, z:4, w:0)
                int[] parsedInts = ParseInt4String(chunks[i].ContiguousChunkRaw);
                SerializedProperty contiguousProp = elementProp.FindPropertyRelative("ContiguousChunk");
                if (contiguousProp != null)
                {
                    // Unity.Mathematics.int4 구조에 맞게 x, y, z, w 할당
                    contiguousProp.FindPropertyRelative("x").intValue = parsedInts.Length > 0 ? parsedInts[0] : 0;
                    contiguousProp.FindPropertyRelative("y").intValue = parsedInts.Length > 1 ? parsedInts[1] : 0;
                    contiguousProp.FindPropertyRelative("z").intValue = parsedInts.Length > 2 ? parsedInts[2] : 0;
                    contiguousProp.FindPropertyRelative("w").intValue = parsedInts.Length > 3 ? parsedInts[3] : 0;
                }
            }
            serializedSO.ApplyModifiedProperties();
        }

        // 에셋으로 저장
        string fullPath = $"{path}/{fileName}.asset";
        AssetDatabase.CreateAsset(so, fullPath);
    }

    // "2,3,4," 형태의 문자열을 잘라서 최대 4개의 int 배열로 반환하는 헬퍼 함수
    private static int[] ParseInt4String(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return new int[4];

        string[] splits = raw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        int[] result = new int[4];

        for (int i = 0; i < Mathf.Min(splits.Length, 4); i++)
        {
            if (int.TryParse(splits[i].Trim(), out int val))
            {
                result[i] = val;
            }
        }
        return result;
    }
}