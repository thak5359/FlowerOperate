using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
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
        so.speciesIndex = new List<byte>();
        so.colorIndex = new List<byte>();
        so.floroIndex = new List<byte>();
        so.floroIndex2 = new List<sbyte>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i % 10 == 0) // Update progress bar every 10 lines to save performance
                EditorUtility.DisplayProgressBar("SO 생성 중", $"Flower 데이터 처리 중... ({i}/{lines.Length})", (float)i / lines.Length);

            string[] data = lines[i].Split(',');
            if (data.Length < 5) continue;

            so.itemName.Add(data[1].Trim());
            so.speciesIndex.Add(byte.TryParse(data[2], out byte s) ? s : (byte)0);
            so.colorIndex.Add(byte.TryParse(data[3], out byte c) ? c : (byte)0);
            so.floroIndex.Add(byte.TryParse(data[4], out byte f) ? f : (byte)0);

            if (data.Length > 5 && sbyte.TryParse(data[5], out sbyte f2))
                so.floroIndex2.Add(f2);
            else
                so.floroIndex2.Add(-1);
        }

        SaveAsset(so, "Assets/ScriptableObjects/Flower/FlowerIdData.asset");
    }

    private void CreateUsableSO(string[] lines)
    {
        UsableIdData so = ScriptableObject.CreateInstance<UsableIdData>();
        InitializeBaseLists(so);
        so.durationIndex = new List<byte>();
        so.powerIndex = new List<byte>();
        so.chargeIndex = new List<byte>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i % 10 == 0)
                EditorUtility.DisplayProgressBar("SO 생성 중", $"Usable 데이터 처리 중... ({i}/{lines.Length})", (float)i / lines.Length);

            string[] data = lines[i].Split(',');
            if (data.Length < 4) continue;

            so.itemName.Add(data[1].Trim());
            so.durationIndex.Add(byte.TryParse(data[2], out byte d) ? d : (byte)0);
            so.powerIndex.Add(byte.TryParse(data[3], out byte p) ? p : (byte)0);

            if (data.Length > 6)
                so.chargeIndex.Add(byte.TryParse(data[6], out byte ch) ? ch : (byte)0);
            else
                so.chargeIndex.Add((byte)0);
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
        so.itemName = new List<FixedString64Bytes>();
        so.description = new List<FixedString128Bytes>();
        so.spriteAddress = new List<FixedString64Bytes>();
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