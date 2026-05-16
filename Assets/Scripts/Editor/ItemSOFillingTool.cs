using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemSOFillingTool : EditorWindow
{
    [SerializeField] private List<ScriptableObject> targetSOList = new();
    [SerializeField] private ItemBaseData baseSO;
    [SerializeField] List<TextAsset> csvFile;

    private ItemMainType itemMainType;
    private string[] lines;

    [MenuItem("Tools/Item/Item Data SO Filling Tool")]
    static void OpenGUI()
    {
        GetWindow<ItemSOFillingTool>("Item Data SO Filling Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("SO Filling Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(8);

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty listProperty = serializedObject.FindProperty(nameof(targetSOList));
        SerializedProperty baseSoProperty = serializedObject.FindProperty(nameof(baseSO));
        SerializedProperty csvProperty = serializedObject.FindProperty(nameof(csvFile));

        EditorGUILayout.PropertyField(listProperty, new GUIContent("대상 SO 리스트"), true);
        EditorGUILayout.PropertyField(csvProperty, new GUIContent("csv데이터파일"), true);
        EditorGUILayout.PropertyField(baseSoProperty, new GUIContent("베이스 SO"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("데이터 삽입하기", GUILayout.Height(40)))
        {
            InsertStart();
        }
    }

    private void InsertStart()
    {
        if (targetSOList == null || targetSOList.Count == 0)
        {
            EditorUtility.DisplayDialog("경고", "대상 SO 리스트가 비어 있습니다.", "확인");
            return;
        }

        foreach (ScriptableObject so in targetSOList)
        {
            if (so == null) continue;

            bool success = so switch
            {
                ItemBaseData itemBaseData => FillData(itemBaseData),
                FlowerItemData flowerItemData => FillData(flowerItemData),
                GearItemData gearItemData => FillData(gearItemData),
                FertilizerItemData fertilizerItemData => FillData(fertilizerItemData),
                _ => false
            };
        }
    }

    private bool SelectCsv(string itemType)
    {
        if (csvFile == null || csvFile.Count == 0)
        {
            Debug.LogError("CSV 파일 리스트가 비어 있습니다.");
            return false;
        }

        foreach (TextAsset csvAsset in csvFile)
        {
            if (csvAsset == null) continue;

            string csvText = csvAsset.text;
            // 줄바꿈 기호가 혼합되어 있을 수 있으므로 공백 제거 옵션과 함께 분리
            string[] tempLines = csvText.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (tempLines.Length > 0)
            {
                // 첫 줄의 타입 이름 비교 (공백 및 보이지 않는 문자(BOM 등) 처리)
                string firstLine = tempLines[0].Trim().Replace("\uFEFF", ""); 
                if (firstLine == itemType)
                {
                    lines = tempLines;
                    return true;
                }
            }
        }

        Debug.LogError($"CSV 파일 중 '{itemType}' 타입을 찾을 수 없습니다.");
        return false;
    }

    private bool FillData(ItemBaseData itemBase)
    {
        if (lines == null || lines.Length < 3)
        {
            Debug.LogError("데이터를 채울 CSV 라인이 유효하지 않습니다. SelectCsv가 먼저 성공해야 합니다.");
            return false;
        }

        List<ItemBaseAuthoringData> temp = new();
        for (int i = 2; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(',');
            if (line.Length < 3) continue;

            ItemBaseAuthoringData data = new ItemBaseAuthoringData();
            data.itemId = int.Parse(line[1]);
            data.mainType = itemMainType;

            // 문자열의 세 번째 자리까지만 사용 (안전한 구현)
            string idStr = line[1].Trim();
            int subTypeRaw = int.Parse(idStr.Length >= 3 ? idStr.Substring(0, 3) : idStr);
            data.subType = (ItemSubType)subTypeRaw;

            data.stackLimit = (itemMainType == ItemMainType.Equipment) ? 1 : 999;
            data.itemName = line[2];
            data.price = int.Parse(line[line.Length - 1]);
            temp.Add(data);
        }
        temp.AddRange(itemBase.Items);
        itemBase.setItems(temp);
        return true;
    }

    private bool FillData(FlowerItemData flowerItem)
    {
        itemMainType = ItemMainType.Farm;
        if (!SelectCsv("Flower")) return false;
        
        FillData(baseSO);

        List<FlowerItemAuthoringData> temp = new List<FlowerItemAuthoringData>();
        for (int i = 2; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(',');
            if (line.Length < 9) continue;

            FlowerItemAuthoringData data = new FlowerItemAuthoringData();
            data.itemId = int.Parse(line[1]);
            data.species = (FlowerSpecies)(int.Parse(line[3]));
            data.color = (FlowerColor)(int.Parse(line[4]));
            data.florio1 = (FlowerFlorio)(int.Parse(line[5]));
            data.florio2 = (FlowerFlorio)((int.TryParse(line[6], out int value)) ? value : 0);
            data.growthDuration = int.Parse(line[7]);
            data.harvestAmount = int.Parse(line[8]);

            temp.Add(data);
        }

        flowerItem.setFlowers(temp);
        return true;
    }

    private bool FillData(GearItemData gearItem)
    {
        itemMainType = ItemMainType.Equipment;
        if (!SelectCsv("Gear")) return false;

        FillData(baseSO);

        List<GearItemAuthoringData> temp = new();
        for (int i = 2; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(",");
            if (line.Length < 7) continue;

            GearItemAuthoringData data = new();
            data.itemId = int.Parse(line[1]);
            data.gearType = (GearType)((int.Parse(line[0]) / 9) + 1);
            data.maxDurability = (GearMaxDuration)(int.Parse(line[3]));
            data.efficiency = (GearEfficiency)(int.Parse(line[4]));
            data.chargeTime = (GearChargeTime)(int.Parse(line[5]));
            data.maxCharge = (GearMaxCharge)(int.Parse(line[6]));
            temp.Add(data);
        }

        gearItem.setGears(temp);
        return true;
    }

    private bool FillData(FertilizerItemData fertilizerItem)
    {
        itemMainType = ItemMainType.Usable;
        SelectCsv("Fertilizer");

        FillData(baseSO);

        List<FertilizerItemAuthoringData> temp = new();
        for(int i = 2; i<lines.Length; i++)
        {
            string[] line = lines[i].Split(',');

            FertilizerItemAuthoringData data = new();
            data.itemId = (int.Parse(line[1]));
            data.gearType = (FertilizerType)int.Parse(line[2]);
            data.level = (FertilizerGrade)int.Parse(line[3]);
            temp.Add(data);
        }

        fertilizerItem.setFertilizers(temp);
        return true;
    }
}
