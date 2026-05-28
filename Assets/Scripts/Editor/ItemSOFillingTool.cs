using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

        // 베이스 SO 초기화 (새로운 데이터를 누적하여 채우므로 시작 시점에 비워줍니다)
        if (baseSO != null)
        {
            baseSO.setItems(new List<ItemBaseAuthoringData>());
            EditorUtility.SetDirty(baseSO);
        }

        bool anySuccess = false;

        foreach (ScriptableObject so in targetSOList)
        {
            if (so == null) continue;
            if (so == baseSO) continue; // baseSO는 개별 SO 데이터 채울 때 같이 채워지므로 직접 루프에서 처리하지 않습니다

            bool success = so switch
            {
                FlowerItemData flowerItemData => FillData(flowerItemData),
                GearItemData gearItemData => FillData(gearItemData),
                FertilizerItemData fertilizerItemData => FillData(fertilizerItemData),
                _ => false
            };

            if (success)
            {
                EditorUtility.SetDirty(so);
                anySuccess = true;
            }
        }

        if (anySuccess)
        {
            if (baseSO != null)
            {
                EditorUtility.SetDirty(baseSO);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "데이터 삽입 및 저장이 완료되었습니다.", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("알림", "삽입된 데이터가 없거나 처리에 실패했습니다.", "확인");
        }
    }

    private bool SelectCsv(string itemTypeOrHeader)
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
            string[] tempLines = csvText.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (tempLines.Length > 0)
            {
                string firstLine = tempLines[0].Trim().Replace("\uFEFF", ""); 
                if (firstLine == itemTypeOrHeader || firstLine.Contains(itemTypeOrHeader))
                {
                    lines = tempLines;
                    return true;
                }
            }
        }

        Debug.LogError($"CSV 파일 중 '{itemTypeOrHeader}' 타입을 찾을 수 없습니다.");
        return false;
    }

    public int4 GrowthDayInitialize(int growthDurationID)
    {
        switch (growthDurationID)
        {
            default:
                return int4.zero;
            case 1:
                return new int4(1, 1, 1, 1);
            case 2:
                return new int4(1, 1, 2, 1);
            case 3:
                return new int4(1, 1, 2, 2);
            case 4:
                return new int4(1, 1, 3, 2);
            case 5:
                return new int4(1, 1, 4, 2);
            case 6:
                return new int4(1, 1, 5, 2);
            case 7:
                return new int4(1, 1, 5, 3);
            case 8:
                return new int4(1, 1, 6, 3);
        }
    }
    
    private bool FillData(ItemBaseData itemBase)
    {
        if (lines == null || lines.Length < 2)
        {
            Debug.LogError("데이터를 채울 CSV 라인이 유효하지 않습니다.");
            return false;
        }

        // 기존 아이템들을 복사한 리스트를 준비하여 데이터를 덮어쓰지 않고 누적합니다.
        List<ItemBaseAuthoringData> temp = itemBase.Items != null 
            ? new List<ItemBaseAuthoringData>(itemBase.Items) 
            : new List<ItemBaseAuthoringData>();

        // 헤더가 "번호,ID..."로 시작하는 경우 1번줄부터 데이터임 (0번은 헤더)
        // 만약 첫줄이 "Gear" 같은 타입명이라면 2번줄부터 데이터임
        int startIdx = lines[0].Contains("ID") ? 1 : 2;

        for (int i = startIdx; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(',');
            if (line.Length < 3) continue;

            ItemBaseAuthoringData data = new ItemBaseAuthoringData();
            data.itemId = int.Parse(line[1].Trim());
            data.mainType = itemMainType;

            string idStr = line[1].Trim();
            int subTypeRaw = int.Parse(idStr.Length >= 3 ? idStr.Substring(0, 3) : idStr);
            data.subType = (ItemSubType)subTypeRaw;

            data.stackLimit = (itemMainType == ItemMainType.Equipment) ? 1 : 999;
            data.itemName = line[2].Trim();
            data.price = (itemMainType != ItemMainType.Equipment) ? int.Parse(line[line.Length - 1].Trim()) : 0;
            data.spriteAddress = (data.subType == ItemSubType.Flower) ? FillFlowerSpriteAddress(data.itemName) : null;

            temp.Add(data);
        }
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
            data.itemId = int.Parse(line[1].Trim());
            data.species = (FlowerSpecies)(int.Parse(line[3].Trim()));
            data.color = (FlowerColor)(int.Parse(line[4].Trim()));
            data.florio1 = (FlowerFlorio)(int.Parse(line[5].Trim()));
            data.florio2 = (FlowerFlorio)((int.TryParse(line[6].Trim(), out int value)) ? value : 0);
            data.growthDurationID = int.Parse(line[7].Trim());
            data.growthDuration = GrowthDayInitialize(data.growthDurationID);
            data.harvestAmount = int.Parse(line[8].Trim());

            temp.Add(data);
        }

        flowerItem.setFlowers(temp);
        return true;
    }

    private bool FillData(GearItemData gearItem)
    {
        itemMainType = ItemMainType.Equipment;
        // 새로운 Gear.csv 형식 대응 (헤더로 찾거나 "Gear"로 찾음)
        if (!SelectCsv("번호,ID,이름,종류,등급,최대내구도,효율,최대차징,기본범위,1차차징범위,2차차징범위,차징시간") && !SelectCsv("Gear")) 
            return false;

        FillData(baseSO);

        List<GearItemAuthoringData> temp = new();
        int startIdx = lines[0].Contains("ID") ? 1 : 2;

        for (int i = startIdx; i < lines.Length; i++)
        {
            string[] line = lines[i].Split(',');
            if (line.Length < 12) continue;

            GearItemAuthoringData data = new();
            data.itemId = int.Parse(line[1].Trim());
            data.gearType = ParseEnum<GearType>(line[3].Trim());
            
            string gradeStr = line[4].Trim();
            if (gradeStr == "Siver") gradeStr = "Silver"; // 오타 수정
            data.grade = ParseEnum<GearGrade>(gradeStr);

            data.maxDurability = ParseEnum<GearMaxDuration>(line[5].Trim());
            data.efficiency = ParseEnum<GearEfficiency>(line[6].Trim());
            data.maxCharge = ParseEnum<GearMaxCharge>(line[7].Trim());
            data.chargeTime = ParseEnum<GearChargeTime>(line[11].Trim());

            // 차징 영역 데이터 (기본, 1차, 2차)
            List<ChargeArea> areas = new();
            AddChargeArea(areas, line[8].Trim());  // 기본범위
            AddChargeArea(areas, line[9].Trim());  // 1차차징범위
            AddChargeArea(areas, line[10].Trim()); // 2차차징범위
            data.chargeAreas = areas.ToArray();

            temp.Add(data);
        }

        gearItem.setGears(temp);
        return true;
    }

    private T ParseEnum<T>(string value) where T : struct, System.Enum
    {
        if (string.IsNullOrEmpty(value) || value == "Unknown") return default;

        // "LV1", "LV 1" 등을 "Lv1" 형식으로 변환하여 Enum 파싱 시도
        string normalized = value.Replace(" ", "").ToLower();
        if (normalized.StartsWith("lv"))
        {
            normalized = "Lv" + normalized.Substring(2);
        }
        else
        {
            // 첫 글자만 대문자로 변환 (Hoe, Old 등)
            normalized = char.ToUpper(normalized[0]) + normalized.Substring(1);
        }

        if (System.Enum.TryParse<T>(normalized, false, out T result)) return result;
        if (System.Enum.TryParse<T>(value, true, out result)) return result;

        return default;
    }

    private void AddChargeArea(List<ChargeArea> list, string value)
    {
        if (string.IsNullOrEmpty(value) || value == "Unknown") return;

        // "Default/A2"와 같이 슬래시가 포함된 경우 마지막 요소를 사용
        string[] parts = value.Split('/');
        string target = parts[parts.Length - 1].Trim();

        if (System.Enum.TryParse<ChargeArea>(target, true, out ChargeArea result))
        {
            list.Add(result);
        }
    }

    private bool FillData(FertilizerItemData fertilizerItem)
    {
        itemMainType = ItemMainType.Usable;
        if (!SelectCsv("Fertilizer")) return false;

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

    private string FillFlowerSpriteAddress(string name)
    {
        string[] temp = name.Split(" ");
        switch (temp[0])
        {
            case "빨간":
                temp[0] = "Red";
                break;
            case "주황":
                temp[0] = "Orange";
                break;
            case "노란":
                temp[0] = "Yellow";
                break;
            case "분홍":
                temp[0] = "Pink";
                break;
            case "하얀":
                temp[0] = "White";
                break;
            case "초록":
                temp[0] = "Green";
                break;
            case "보라":
                temp[0] = "Purple";
                break;
            case "검은":
                temp[0] = "Black";
                break;
            case "파랑":
                temp[0] = "Blue";
                break;
            case "무지개":
                temp[0] = "Rainbow";
                break;

        }

        switch(temp[1])
        {
            case "거베라":
                temp[1] = "Gerbera";
                break;
            case "국화":
                temp[1] = "Chrysanthemum";
                break;
            case "델피늄":
                temp[1] = "Delphinium";
                break;
            case "라넌큘러스":
                temp[1] = "Ranunculus";
                break;
            case "리시안셔스":
                temp[1] = "Lisianthus";
                break;
            case "백합":
                temp[1] = "Lily";
                break;
            case "수국":
                temp[1] = "Hydrangea";
                break;
            case "아네모네":
                temp[1] = "Anemone";
                break;
            case "연꽃":
                temp[1] = "Lotus";
                break;
            case "작약":
                temp[1] = "Peony";
                break;
            case "장미":
                temp[1] = "Rose";
                break;
            case "카네이션":
                temp[1] = "Carnation";
                break;
            case "코스모스":
                temp[1] = "Cosmos";
                break;
            case "튤립":
                temp[1] = "Tulip";
                break;
            case "프리지아":
                temp[1] = "Freesia";
                break;
            case "해바라기":
                temp[1] = "Sunflower";
                break;
            case "히아신스":
                temp[1] = "Hyacinth";
                break;
            default:
                return null;
        }

        return temp[0] + '_' + temp[1];
    }
}
