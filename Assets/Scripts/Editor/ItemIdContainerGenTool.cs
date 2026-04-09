using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemIdContainerGenTool : EditorWindow
{
    public TextAsset csvFile;
    public DropDownMenu menu = DropDownMenu.None;

    [MenuItem("Tools/IndexSOGenerator")]
    static void Mymenu()
    {
        GetWindow<ItemIdContainerGenTool>("ID SO Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("CSV 데이터를 기반으로 ScriptableObject를 생성합니다.\n'Assets/ScriptableObjects/...' 경로에 생성됩니다.", MessageType.Info);

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "csv 파일",
            csvFile,
            typeof(TextAsset),
            false);

        menu = (DropDownMenu)EditorGUILayout.EnumPopup("종류", menu);

        EditorGUILayout.Space();

        if (GUILayout.Button("SO 생성", GUILayout.Height(30)))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("오류", "CSV 파일을 선택해주세요.", "확인");
                return;
            }

            ExecuteGeneration();
        }
    }

    private void ExecuteGeneration()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            
            string csvText = csvFile.text;
            string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length == 0)
            {
                Debug.LogWarning("CSV 파일이 비어 있습니다.");
                return;
            }

            switch (menu)
            {
                case DropDownMenu.Flower:
                    CreateFlowerSO(lines);
                    break;

                case DropDownMenu.Usable:
                    CreateUsableSO(lines);
                    break;

                default:
                    CreateEtcSO(lines);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SO 생성 중 오류 발생: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("오류", $"생성 중 오류가 발생했습니다: {e.Message}", "확인");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
    }

    private void CreateFlowerSO(string[] lines)
    {
        FlowerIdData so = ScriptableObject.CreateInstance<FlowerIdData>();
        InitializeBaseLists(so);
        so.speciesIndex = new List<int>();
        so.colorIndex = new List<int>();
        so.floroIndex = new List<int>();
        so.floroIndex2 = new List<int>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i % 10 == 0) // Update progress bar every 10 lines to save performance
                EditorUtility.DisplayProgressBar("SO 생성 중", $"Flower 데이터 처리 중... ({i}/{lines.Length})", (float)i / lines.Length);

            string[] data = lines[i].Split(',');
            if (data.Length < 5) continue;

            SO.speciesIndex.Add(byte.Parse(data[2]));
            SO.colorIndex.Add(byte.Parse(data[3]));
            SO.floroIndex.Add(byte.Parse(data[4]));
            if (sbyte.TryParse(data[5], out sbyte value))
                SO.floroIndex2.Add(value);
            else
                so.floroIndex2.Add(-1);
        }

        SaveAsset(so, "Assets/ScriptableObjects/Flower/FlowerIdData.asset");
    }

    private void CreateUsableSO(string[] lines)
    {
        UsableIdData so = ScriptableObject.CreateInstance<UsableIdData>();
        InitializeBaseLists(so);
        so.durationIndex = new List<int>();
        so.powerIndex = new List<int>();
        so.chargeIndex = new List<int>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i % 10 == 0)
                EditorUtility.DisplayProgressBar("SO 생성 중", $"Usable 데이터 처리 중... ({i}/{lines.Length})", (float)i / lines.Length);

            SO.durationIndex.Add(byte.Parse(data[2]));
            SO.powerIndex.Add(byte.Parse(data[3]));
            SO.chargeIndex.Add(byte.Parse(data[6]));
        }

        SaveAsset(so, "Assets/ScriptableObjects/Gear/UsableIdData.asset");
    }

    private void CreateEtcSO(string[] lines)
    {
        ItemIdData so = ScriptableObject.CreateInstance<ItemIdData>();
        InitializeBaseLists(so);

        for (int i = 0; i < lines.Length; i++)
        {
            if (i % 10 == 0)
                EditorUtility.DisplayProgressBar("SO 생성 중", $"Etc 데이터 처리 중... ({i}/{lines.Length})", (float)i / lines.Length);

            string[] data = lines[i].Split(',');
            if (data.Length < 2) continue;

            so.itemName.Add(data[1].Trim());
        }

        SaveAsset(so, "Assets/ScriptableObjects/Ore/EtcIdData.asset");
    }

    private void InitializeBaseLists(ItemIdData so)
    {
        so.itemName = new List<string>();
        so.description = new List<string>();
        so.spriteAddress = new List<string>();
    }

    private void SaveAsset(UnityEngine.Object asset, string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        AssetDatabase.CreateAsset(asset, path);
        Debug.Log($"SO 생성 완료: {path}");
    }
}
