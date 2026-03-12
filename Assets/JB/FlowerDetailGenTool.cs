using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public enum DropDownMenu
{
    Flower,
    Usable
};

public class FlowerDetailGenTool : EditorWindow
{
    public ItemDetailData itemDetailData;
    public TextAsset csvFile;
    DropDownMenu menu = DropDownMenu.Flower;

    [MenuItem("Tools/DetailGenerator")]
    static void MyMenu()
    {
        GetWindow<FlowerDetailGenTool>();
    }

    private void OnGUI()
    {
        itemDetailData = (ItemDetailData)EditorGUILayout.ObjectField(
            "디테일 데이터",
            itemDetailData,
            typeof(ItemDetailData),
            false);

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "csv 데이터",
            csvFile,
            typeof(TextAsset),
            false);

        menu = (DropDownMenu)EditorGUILayout.EnumPopup("종류", menu);
        if (GUILayout.Button("생성"))
        {
            switch (menu)
            {
                case DropDownMenu.Flower:
                    var SO = itemDetailData as FlowerDetailData;
                    if (SO != null)
                        FlowerOperateFunc(SO);
                    else
                        Debug.Log("잘못된 설정");
                    break;
                case DropDownMenu.Usable:
                    Debug.Log("Usable");
                    break;
            }
        }
    }

    private void FlowerOperateFunc(FlowerDetailData SO)
    {
        ClearSOlist(SO);
        string[] lines = csvFile.ToString().Split('\n');

        foreach (string line in lines)
        {
            string[] data = line.Split(',');
            if (int.TryParse(data[0], out int speciesIdx))
                SO.speciesList.Add(data[1]);
            if (int.TryParse(data[4], out int colorIdx))
                SO.colorList.Add(data[5]);
            if (int.TryParse(data[8], out int floroIdx))
                SO.floroList.Add(data[9]);
        }
            
        FlowerDetailData flowerData = ScriptableObject.CreateInstance<FlowerDetailData>();
        flowerData.speciesList = SO.speciesList;
        flowerData.colorList = SO.colorList;
        flowerData.floroList = SO.floroList;

        AssetDatabase.CreateAsset(flowerData, "Assets/ScriptableObjects/Flower/FlowerDetailSO.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ClearSOlist(FlowerDetailData SO)
    {
        SO.speciesList.Clear();
        SO.colorList.Clear();
        SO.floroList.Clear();
    }

    private void FlowerOperateFunc(UsableDetailData SO)
    {
        string[] lines = csvFile.ToString().Split('\n');
        foreach (string line in lines)
        {
        }
    }
}
