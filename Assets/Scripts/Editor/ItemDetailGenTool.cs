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

public class ItemDetailGenTool : EditorWindow
{
    public ItemDetailData itemDetailData;
    public TextAsset csvFile;
    DropDownMenu menu = DropDownMenu.Flower;

    [MenuItem("Tools/DetailGenerator")]
    static void MyMenu()
    {
        GetWindow<ItemDetailGenTool>();
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

        if (itemDetailData is UsableDetailData usableSO)
        {
            DrawChargeInfoList(usableSO);
        }

        menu = (DropDownMenu)EditorGUILayout.EnumPopup("종류", menu);
        if (GUILayout.Button("생성"))
        {
            switch (menu)
            {
                case DropDownMenu.Flower:
                    var SO = itemDetailData as FlowerDetailData;
                    if (SO != null)
                        OperateFunc(SO);
                    else
                        Debug.Log("잘못된 설정");
                    break;
                case DropDownMenu.Usable:
                    var so = itemDetailData as UsableDetailData;
                    if (so != null)
                        OperateFunc(so);
                    else
                        Debug.Log("난리남");
                    break;
            }
        }

        
    }

    private void OperateFunc(FlowerDetailData SO)
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

    private void OperateFunc(UsableDetailData SO)
    {
        string[] lines = csvFile.ToString().Split('\n');
        foreach (string line in lines)
        {
            string[] data = line.Split(',');

            SO.durationList.Add(int.Parse(data[1]));
            SO.powerList.Add(int.Parse(data[2]));
            if (float.TryParse(data[3], out float Time))
            {
                for (int i = 0; i < 4; i++)
                {
                    ChargeInfo chargeInfo = new ChargeInfo(Time, i);
                    chargeInfo.ReadValue();
                    SO.chargeInfoList.Add(chargeInfo);
                }
            }
        }

        UsableDetailData usableDetailData = ScriptableObject.CreateInstance<UsableDetailData>();
        usableDetailData.durationList = SO.durationList;
        usableDetailData.powerList = SO.powerList;
        usableDetailData.chargeInfoList = SO.chargeInfoList;

        AssetDatabase.CreateAsset(usableDetailData, "Assets/ScriptableObjects/Gear/UsableDetailSO.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void DrawChargeInfoList(UsableDetailData so)
    {
        if (so.chargeInfoList == null || so.chargeInfoList.Count == 0)
        {
            EditorGUILayout.HelpBox("표시할 ChargeInfo 데이터가 없습니다.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("< Charge Info List >", EditorStyles.boldLabel);

        // 표 헤더 그리기
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("No.", GUILayout.Width(30));
        EditorGUILayout.LabelField("Time (sec)", GUILayout.Width(80));
        EditorGUILayout.LabelField("Charge Index", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        // 리스트 내용 출력
        for (int i = 0; i < so.chargeInfoList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");

            // 순번
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));

            // Time 수정 (직접 수정 가능하도록 floatField 사용)
            // 1. 임시 변수에 복사본 꺼내기
            ChargeInfo tempInfo = so.chargeInfoList[i];

            // 2. 복사본의 값 수정
            tempInfo.ChargeTime = EditorGUILayout.FloatField(tempInfo.ChargeTime, GUILayout.Width(80));
            tempInfo.maxChargeCount = EditorGUILayout.IntField(so.chargeInfoList[i].maxChargeCount, GUILayout.Width(100));

            // 3. 수정된 복사본을 다시 리스트에 덮어쓰기
            so.chargeInfoList[i] = tempInfo;

            // Index 수정

            // 삭제 버튼 (옵션)
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                so.chargeInfoList.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        // 변경사항이 있으면 저장 대상으로 마킹
        if (GUI.changed)
        {
            EditorUtility.SetDirty(so);
        }
    }
}
