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
    public string path = "Assets/JB/CSV/FlowerID.csv";

    [MenuItem("Tools/DetailGenerator")]
    static void MyMenu()
    {
        GetWindow<FlowerDetailGenTool>();
    }

    private void OnGUI()
    {
        DropDownMenu menu = DropDownMenu.Flower;
        itemDetailData = (ItemDetailData)EditorGUILayout.ObjectField(
            "디테일 데이터",
            itemDetailData,
            typeof(ItemDetailData),
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
        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            SO.speciesList.Add("");
            SO.colorList.Add("");
            SO.floroList.Add("");
        }
        foreach (string line in lines)
        {
            string[] data = line.Split(',');
            SO.speciesList[int.Parse(data[1])] = data[3];
            SO.colorList[int.Parse(data[1])] = data[6];
            SO.floroList[int.Parse(data[1])] = data[10];
        }
        FlowerDetailData flowerData = ScriptableObject.CreateInstance<FlowerDetailData>();
        flowerData.speciesList = SO.speciesList;
        flowerData.colorList = SO.colorList;
        flowerData.floroList = SO.floroList;
        
        AssetDatabase.Refresh();
    }

    private void FlowerOperateFunc(UsableDetailData SO)
    {
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
        }
    }
}
