using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

public class MakerDataFillngTool : EditorWindow
{
    private MakerDataSet makerDataSet;
    private string csvPath = "Assets/JB/CSV/MakerDataSheet.csv";

    [MenuItem("Tools/Maker Data Filling Tool")]
    public static void ShowWindow()
    {
        GetWindow<MakerDataFillngTool>("Maker Data Filling Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Maker Data Filling Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        makerDataSet = (MakerDataSet)EditorGUILayout.ObjectField("Maker Data Set", makerDataSet, typeof(MakerDataSet), false);
        
        EditorGUILayout.BeginHorizontal();
        csvPath = EditorGUILayout.TextField("CSV Path", csvPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFilePanel("Select MakerDataSheet CSV", "Assets", "csv");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    csvPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    csvPath = selectedPath;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Fill Data from CSV", GUILayout.Height(30)))
        {
            FillData();
        }
    }

    private void FillData()
    {
        if (makerDataSet == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign MakerDataSet ScriptableObject first.", "OK");
            return;
        }

        string fullPath = csvPath;
        if (!Path.IsPathRooted(fullPath))
        {
            fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, csvPath);
        }

        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error", $"CSV file not found at path:\n{fullPath}", "OK");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(fullPath);
            List<MakerData> dataList = new List<MakerData>();

            // 0번째 라인은 헤더이므로 생략
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');
                if (values.Length < 7)
                {
                    Debug.LogWarning($"Line {i + 1} has insufficient columns: {line}");
                    continue;
                }

                // 1. 등급 파싱 (0번째 컬럼)
                string tierStr = values[0].Trim();
                MakerTier tier = MakerTier.Tier1;
                if (tierStr.Contains("1")) tier = MakerTier.Tier1;
                else if (tierStr.Contains("2")) tier = MakerTier.Tier2;
                else if (tierStr.Contains("3")) tier = MakerTier.Tier3;
                else if (tierStr.Contains("4")) tier = MakerTier.Tier4;
                else if (tierStr.Contains("5")) tier = MakerTier.Tier5;
                else
                {
                    Debug.LogWarning($"Unknown tier '{tierStr}' at line {i + 1}. Defaulted to Tier1.");
                }

                // 2. 메이커 종류 파싱 (1번째 컬럼)
                string nameStr = values[1].Trim();
                MakerType type = MakerType.Seed;
                if (nameStr.Contains("씨앗")) type = MakerType.Seed;
                else if (nameStr.Contains("주괴")) type = MakerType.Ingot;
                else if (nameStr.Contains("보석")) type = MakerType.Jewerly;
                else if (nameStr.Contains("목재")) type = MakerType.Wood;
                else if (nameStr.Contains("비료")) type = MakerType.Fertilizer;
                else
                {
                    Debug.LogWarning($"Unknown maker type in name '{nameStr}' at line {i + 1}. Defaulted to Seed.");
                }

                // 3. 메이커 크기 파싱 (2, 3번째 컬럼)
                int sizeX = 0, sizeY = 0;
                int.TryParse(values[2].Trim(), out sizeX);
                int.TryParse(values[3].Trim(), out sizeY);
                int2 size = new int2(sizeX, sizeY);

                // 4. 재료 비율 파싱 (4, 5번째 컬럼)
                int ratioA = 0, ratioB = 0;
                int.TryParse(values[4].Trim(), out ratioA);
                int.TryParse(values[5].Trim(), out ratioB);
                int2 ratio = new int2(ratioA, ratioB);

                // 5. 가격/수량 (maxProduction) 파싱 (6번째 컬럼)
                int maxProd = 0;
                int.TryParse(values[6].Trim(), out maxProd);

                // MakerDataSO 생성 및 리스트에 추가
                MakerData data = new MakerData(type, tier, size, ratio, maxProd);
                dataList.Add(data);
            }

            // ScriptableObject에 데이터 설정
            makerDataSet.SetMakerDataList(dataList.ToArray());
            EditorUtility.SetDirty(makerDataSet);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Successfully loaded {dataList.Count} maker data items into MakerDataSet.", "OK");
            Debug.Log($"[MakerDataFillingTool] Successfully imported {dataList.Count} items.");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"An error occurred during import:\n{ex.Message}", "OK");
            Debug.LogException(ex);
        }
    }
}
