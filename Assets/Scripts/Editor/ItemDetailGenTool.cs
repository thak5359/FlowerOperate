using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum DropDownMenu
{
    None,
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
            "디테일 데이터 (SO)",
            itemDetailData,
            typeof(ItemDetailData),
            false);

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "CSV 데이터 파일",
            csvFile,
            typeof(TextAsset),
            false);

        if (itemDetailData is UsableDetailData usableSO)
        {
            DrawChargeInfoList(usableSO);
        }

        menu = (DropDownMenu)EditorGUILayout.EnumPopup("종류", menu);
        
        if (GUILayout.Button("데이터 생성 및 SO 업데이트"))
        {
            if (itemDetailData == null || csvFile == null)
            {
                Debug.LogError("SO 또는 CSV 파일이 할당되지 않았습니다!");
                return;
            }

            switch (menu)
            {
                case DropDownMenu.Flower:
                    if (itemDetailData is FlowerDetailData flowerSO)
                        OperateFunc(flowerSO);
                    else
                        Debug.LogError("할당된 SO가 FlowerDetailData 형식이 아닙니다.");
                    break;
                case DropDownMenu.Usable:
                    if (itemDetailData is UsableDetailData usableSOData)
                        OperateFunc(usableSOData);
                    else
                        Debug.LogError("할당된 SO가 UsableDetailData 형식이 아닙니다.");
                    break;
            }
        }
    }

    private void OperateFunc(FlowerDetailData SO)
    {
        if (SO == null) return;

        // 실행 취소 지원 및 데이터 초기화
        Undo.RecordObject(SO, "Update Flower Detail Data");
        ClearSOlist(SO);
        
        // 줄바꿈 문자를 유연하게 처리 (\r\n 또는 \n)
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] data = line.Split(',');
            if (data.Length < 2) continue;

            // 1. 품종 파싱: 0번 인덱스가 숫자이고 1번 인덱스에 이름이 있는 경우
            if (int.TryParse(data[0].Trim(), out _) && data.Length > 1 && !string.IsNullOrWhiteSpace(data[1]))
            {
                SO.speciesList.Add(data[1].Trim());
            }

            // 2. 색상 파싱: 4번 인덱스가 숫자이고 5번 인덱스에 이름이 있는 경우
            if (data.Length > 5 && int.TryParse(data[4].Trim(), out _) && !string.IsNullOrWhiteSpace(data[5]))
            {
                SO.colorList.Add(data[5].Trim());
            }

            // 3. 꽃말 파싱: 9번 인덱스에 이름이 있는 경우
            if (data.Length > 9 && !string.IsNullOrWhiteSpace(data[9]))
            {
                SO.floroList.Add(data[9].Trim());
            }
        }

        // 변경사항 저장 및 에디터 갱신
        EditorUtility.SetDirty(SO);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"<color=green>Flower SO 업데이트 완료:</color> 품종 {SO.speciesList.Count}, 색상 {SO.colorList.Count}, 꽃말 {SO.floroList.Count}");
    }

    private static void ClearSOlist(FlowerDetailData SO)
    {
        SO.speciesList.Clear();
        SO.colorList.Clear();
        SO.floroList.Clear();
    }

    private void OperateFunc(UsableDetailData SO)
    {
        if (SO == null) return;

        Undo.RecordObject(SO, "Update Usable Detail Data");
        SO.durationList.Clear();
        SO.powerList.Clear();
        SO.chargeInfoList.Clear();

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] data = line.Split(',');
            if (data.Length < 3) continue;

            if (short.TryParse(data[1].Trim(), out short duration))
                SO.durationList.Add(duration);
            
            if (byte.TryParse(data[2].Trim(), out byte power))
                SO.powerList.Add(power);

            if (data.Length > 3 && float.TryParse(data[3].Trim(), out float time))
            {
                for (sbyte i = 0; i < 4; i++)
                {
                    SO.chargeInfoList.Add(new ChargeInfo(time, i));
                }
            }
        }

        EditorUtility.SetDirty(SO);
        AssetDatabase.SaveAssets();
        Debug.Log("<color=green>Usable SO 업데이트 완료.</color>");
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

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("No.", GUILayout.Width(30));
        EditorGUILayout.LabelField("Time (sec)", GUILayout.Width(80));
        EditorGUILayout.LabelField("Charge Index", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < so.chargeInfoList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));

            ChargeInfo tempInfo = so.chargeInfoList[i];
            tempInfo.ChargeTime = EditorGUILayout.FloatField(tempInfo.ChargeTime, GUILayout.Width(80));
            tempInfo.maxChargeCount = (sbyte)EditorGUILayout.IntField(so.chargeInfoList[i].maxChargeCount, GUILayout.Width(100));
            so.chargeInfoList[i] = tempInfo;

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                so.chargeInfoList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(so);
        }
    }
}
