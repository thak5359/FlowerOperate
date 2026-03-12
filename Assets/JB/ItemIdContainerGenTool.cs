using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemIdContainerGenTool : EditorWindow
{
    public ItemIdData baseIdData;
    public TextAsset csvFile;
    public DropDownMenu menu = DropDownMenu.Flower;

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
                    FlowerIdData SO = baseIdData as FlowerIdData;
                    OperateFunc(SO);
                    break;
                case DropDownMenu.Usable:
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

    private static void ClearSO(FlowerIdData SO)
    {
        SO.itemName.Clear();
        SO.speciesIndex.Clear();
        SO.colorIndex.Clear();
        SO.floroIndex.Clear();
        SO.floroIndex2.Clear();
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
}
