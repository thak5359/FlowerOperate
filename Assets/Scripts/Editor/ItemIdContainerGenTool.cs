using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemIdContainerGenTool : EditorWindow
{
    public ItemIdData baseIdData;
    public TextAsset csvFile;
    public DropDownMenu menu = DropDownMenu.None;

    [MenuItem("Tools/IndexSOGenerator")]
    static void Mymenu()
    {
        GetWindow<ItemIdContainerGenTool>();
    }

    private void OnGUI()
    {
        baseIdData = (ItemIdData)EditorGUILayout.ObjectField(
            "기반 SO 에셋",
            baseIdData,
            typeof(ItemIdData),
            false);

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "csv 파일",
            csvFile,
            typeof(TextAsset),
            false);

        menu = (DropDownMenu)EditorGUILayout.EnumPopup("종류", menu);

        if (GUILayout.Button("SO 생성"))
        {
            switch (menu)
            {
                case DropDownMenu.Flower:
                    if (baseIdData is FlowerIdData flowerData) // 패턴 매칭 사용 (C# 7.0+)
                        OperateFunc(flowerData);
                    break;

                case DropDownMenu.Usable:
                    if (baseIdData is UsableIdData usableData)
                        OperateFunc(usableData);
                    break;

                default:
                    if (baseIdData != null)
                        OperateFunc(baseIdData);
                    else
                        Debug.LogWarning("베이스 아이디SO 없음"); // 에러는 경고나 에러 로그가 좋습니다.
                    break;
            }
        }
    }

    private void OperateFunc(FlowerIdData SO)
    {
        ClearSO(SO);

        string[] lines = csvFile.ToString().Split('\n');

        foreach (string line in lines)
        {
            string[] data = line.Split(',');

            SO.itemName.Add(data[1]);

            SO.speciesIndex.Add(int.Parse(data[2]));
            SO.colorIndex.Add(int.Parse(data[3]));
            SO.floroIndex.Add(int.Parse(data[4]));
            if (int.TryParse(data[5], out int value))
                SO.floroIndex2.Add(value);
            else
                SO.floroIndex2.Add(-1);
        }

        FlowerIdData flowerId = CreateSOAsset(SO);

        AssetDatabase.CreateAsset(flowerId, "Assets/ScriptableObjects/Flower/FlowerIdData.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void OperateFunc(UsableIdData SO)
    {
        if(SO.itemName.Count > 0)
            ClearSO(SO);

        string[] lines = csvFile.ToString().Split('\n');

        foreach(string line in lines)
        {
            string[] data = line.Split(',');

            SO.itemName.Add(data[1]);

            SO.durationIndex.Add(int.Parse(data[2]));
            SO.powerIndex.Add(int.Parse(data[3]));
            SO.chargeIndex.Add(int.Parse(data[6]));
        }

        UsableIdData usableId = CreateSOAsset(SO);

        AssetDatabase.CreateAsset(usableId, "Assets/ScriptableObjects/Gear/UsableIdData.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    private void OperateFunc(ItemIdData SO)
    {
        if (SO.itemName.Count > 0)
            ClearSO(SO);

        string[] lines = csvFile.ToString().Split('\n');

        foreach (string line in lines)
        {
            string[] data = line.Split(',');

            SO.itemName.Add(data[1]);
        }

        ItemIdData etcItemId = CreateSOAsset(SO);

        AssetDatabase.CreateAsset(etcItemId, "Assets/ScriptableObjects/Ore/EtcIdData.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private ItemIdData CreateSOAsset(ItemIdData SO)
    {
        ItemIdData temp = ScriptableObject.CreateInstance<ItemIdData>();
        temp.itemName = SO.itemName;
        temp.description = SO.description;
        temp.spriteAddress = SO.spriteAddress;
        return temp;
    }

    private static void ClearSO(ItemIdData SO)
    {
        SO.itemName.Clear();
        SO.description.Clear();
        SO.spriteAddress.Clear();
    }

    private static void ClearSO(FlowerIdData SO)
    {
        ClearSO(SO);
        SO.itemName.Clear();
        SO.speciesIndex.Clear();
        SO.colorIndex.Clear();
        SO.floroIndex.Clear();
        SO.floroIndex2.Clear();
    }

    private static void ClearSO(UsableIdData SO)
    {
        ClearSO(SO);
        SO.itemName.Clear();
        SO.durationIndex.Clear();
        SO.powerIndex.Clear();
        SO.chargeIndex.Clear();
    }


    private static FlowerIdData CreateSOAsset(FlowerIdData SO)
    {
        FlowerIdData temp = ScriptableObject.CreateInstance<FlowerIdData>();
        temp.itemName = SO.itemName;
        temp.speciesIndex = SO.speciesIndex;
        temp.colorIndex = SO.colorIndex;
        temp.floroIndex = SO.floroIndex;
        temp.floroIndex2 = SO.floroIndex2;
        return temp;
    }

    private static UsableIdData CreateSOAsset(UsableIdData SO)
    {
        UsableIdData temp = ScriptableObject.CreateInstance<UsableIdData>();
        temp.itemName = SO.itemName;
        temp.durationIndex = SO.durationIndex;
        temp.powerIndex = SO.powerIndex;
        temp.chargeIndex = SO.chargeIndex;
        return temp;
    }
}
