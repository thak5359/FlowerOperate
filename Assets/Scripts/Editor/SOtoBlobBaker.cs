#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public class ItemBlobBaker : EditorWindow
{
    [SerializeField] private List<ScriptableObject> targetSOList = new();

    private string savePath = "Assets/StreamingAssets/Blobs";

    [MenuItem("Tools/Item/Bake Item Data To Blob")]
    public static void ShowWindow()
    {
        GetWindow<ItemBlobBaker>("Item Blob Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Blob Baker", EditorStyles.boldLabel);
        EditorGUILayout.Space(8);

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty listProperty = serializedObject.FindProperty(nameof(targetSOList));

        EditorGUILayout.PropertyField(listProperty, new GUIContent("대상 SO 리스트"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8);
        savePath = EditorGUILayout.TextField("저장 경로", savePath);

        EditorGUILayout.Space(16);

        if (GUILayout.Button("선택한 SO들을 Blob으로 굽기", GUILayout.Height(36)))
        {
            BakeAll();
        }
    }

    private void BakeAll()
    {
        if (targetSOList == null || targetSOList.Count == 0)
        {
            EditorUtility.DisplayDialog("경고", "대상 SO 리스트가 비어 있습니다.", "확인");
            return;
        }

        EnsureDirectory();

        int successCount = 0;

        foreach (ScriptableObject target in targetSOList)
        {
            if (target == null)
                continue;

            bool success = target switch
            {
                ItemBaseData itemBaseData => BakeItemBaseData(itemBaseData),
                FlowerItemData flowerItemData => BakeFlowerItemData(flowerItemData),
                GearItemData gearItemData => BakeGearItemData(gearItemData),
                FertilizerItemData fertilizerItemData => BakeFertilizerItemData(fertilizerItemData),
                _ => Unsupported(target)
            };

            if (success)
                successCount++;
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "완료",
            $"Blob 베이킹 완료\n성공: {successCount} / 대상: {targetSOList.Count}",
            "확인"
        );
    }

    private bool Unsupported(ScriptableObject target)
    {
        Debug.LogWarning($"[ItemBlobBaker] 지원하지 않는 SO 타입입니다. name: {target.name}, type: {target.GetType()}");
        return false;
    }

    private bool BakeItemBaseData(ItemBaseData so)
    {
        if (so == null)
            return false;

        ValidateItemBaseData(so);

        var builder = new BlobBuilder(Allocator.Temp);

        try
        {
            ref ItemBaseBlobDatas root = ref builder.ConstructRoot<ItemBaseBlobDatas>();
            BlobBuilderArray<ItemBaseBlobData> arrayBuilder = builder.Allocate(ref root.Items, so.Count);

            for (int i = 0; i < so.Count; i++)
            {
                ItemBaseAuthoringData source = so.Get(i);

                arrayBuilder[i] = new ItemBaseBlobData
                {
                    ItemId = source.itemId,
                    MainType = source.mainType,
                    SubType = source.subType,
                    StackLimit = source.stackLimit,
                    ItemName = ToFixedString64(source.itemName),
                    Description = ToFixedString128(source.description),
                    SpriteAddress = ToFixedString128(source.spriteAddress),
                    RefundPrice = source.price
                };
            }

            SaveToBlob<ItemBaseBlobDatas>(builder, so.name);
            Debug.Log($"<color=green>[ItemBlobBaker]</color> ItemBaseData 베이킹 완료: {so.name}");
            return true;
        }
        finally
        {
            builder.Dispose();
        }
    }

    private bool BakeFlowerItemData(FlowerItemData so)
    {
        if (so == null)
            return false;

        ValidateFlowerItemData(so);

        var builder = new BlobBuilder(Allocator.Temp);

        try
        {
            ref FlowerItemBlobDatas root = ref builder.ConstructRoot<FlowerItemBlobDatas>();
            BlobBuilderArray<FlowerItemBlobData> arrayBuilder = builder.Allocate(ref root.Items, so.Count);

            for (int i = 0; i < so.Count; i++)
            {
                FlowerItemAuthoringData source = so.Get(i);

                arrayBuilder[i] = new FlowerItemBlobData
                {
                    ItemId = source.itemId,
                    Species = source.species,
                    Color = source.color,
                    Florio1 = source.florio1,
                    Florio2 = source.florio2,
                    GrowthDuration = source.growthDuration,
                    HarvestAmount = source.harvestAmount
                };
            }

            SaveToBlob<FlowerItemBlobDatas>(builder, so.name);
            Debug.Log($"<color=green>[ItemBlobBaker]</color> FlowerItemData 베이킹 완료: {so.name}");
            return true;
        }
        finally
        {
            builder.Dispose();
        }
    }

    // 수정할 위치: ItemBlobBaker.cs 파일 내부의 BakeGearItemData 메서드 전체 교체
    // 변경 이유: BlobArray 내부에 또 다른 BlobArray(ChargeAreas)를 할당하기 위해 ref 참조 방식을 사용하고 배열 메모리를 직접 구워줍니다.

    private bool BakeGearItemData(GearItemData so)
    {
        if (so == null)
            return false;

        ValidateGearItemData(so);

        var builder = new BlobBuilder(Allocator.Temp);

        try
        {
            ref GearItemBlobDatas root = ref builder.ConstructRoot<GearItemBlobDatas>();
            BlobBuilderArray<GearItemBlobData> arrayBuilder = builder.Allocate(ref root.Items, so.Count);

            for (int i = 0; i < so.Count; i++)
            {
                GearItemAuthoringData source = so.Get(i);

                // 1. 배열의 요소를 참조(ref)로 가져옵니다. 
                // (내부 BlobArray 할당을 위해선 반드시 ref로 접근해야 메모리 오프셋이 안 깨져요!)
                ref GearItemBlobData element = ref arrayBuilder[i];

                // 2. 기본 데이터 매핑 (기존에 누락되었던 Grade도 추가했어요)
                element.ItemId = source.itemId;
                element.GearType = source.gearType;
                element.MaxDuration = source.maxDurability;
                element.Efficiency = source.efficiency;
                element.ChargeTime = source.chargeTime;
                element.MaxCharge = source.maxCharge;
                element.Grade = source.grade;

                // 3. ChargeAreas 내부 배열 메모리 할당 및 복사
                if (source.chargeAreas != null && source.chargeAreas.Length > 0)
                {
                    // 해당 element의 ChargeAreas 필드를 타겟으로 배열 크기만큼 메모리 할당
                    BlobBuilderArray<ChargeArea> chargeAreasBuilder = builder.Allocate(ref element.ChargeAreas, source.chargeAreas.Length);

                    for (int j = 0; j < source.chargeAreas.Length; j++)
                    {
                        chargeAreasBuilder[j] = source.chargeAreas[j];
                    }
                }
            }

            SaveToBlob<GearItemBlobDatas>(builder, so.name);
            Debug.Log($"<color=green>[ItemBlobBaker]</color> GearItemData 베이킹 완료: {so.name}");
            return true;
        }
        finally
        {
            builder.Dispose();
        }
    }
    private bool BakeFertilizerItemData(FertilizerItemData so)
    {
        if (so == null)
            return false;

        ValidateFertilizerItemData(so);

        var builder = new BlobBuilder(Allocator.Temp);

        try
        {
            ref FertilizerItemBlobDatas root = ref builder.ConstructRoot<FertilizerItemBlobDatas>();
            BlobBuilderArray<FertilizerItemBlobData> arrayBuilder = builder.Allocate(ref root.Items, so.Count);

            for (int i = 0; i < so.Count; i++)
            {
                FertilizerItemAuthoringData source = so.Get(i);

                arrayBuilder[i] = new FertilizerItemBlobData
                {
                    ItemId = source.itemId,
                    FertilizerType = source.gearType,
                    Level = source.level
                };
            }

            SaveToBlob<FertilizerItemBlobDatas>(builder, so.name);
            Debug.Log($"<color=green>[ItemBlobBaker]</color> FertilizerItemData 베이킹 완료: {so.name}");
            return true;
        }
        finally
        {
            builder.Dispose();
        }
    }

    private void SaveToBlob<T>(BlobBuilder builder, string fileName) where T : unmanaged
    {
        EnsureDirectory();

        string fullPath = Path.Combine(savePath, $"{fileName}.blob");
        BlobAssetReference<T>.Write(builder, fullPath, 1);

        Debug.Log($"<color=green>[Blob 저장]</color> {typeof(T).Name} -> {fullPath}");
    }

    private void EnsureDirectory()
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
    }

    private static FixedString64Bytes ToFixedString64(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        return new FixedString64Bytes(value);
    }

    private static FixedString128Bytes ToFixedString128(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        return new FixedString128Bytes(value);
    }

    private static void ValidateItemBaseData(ItemBaseData so)
    {
        HashSet<int> ids = new();

        for (int i = 0; i < so.Count; i++)
        {
            ItemBaseAuthoringData item = so.Get(i);

            if (item.itemId <= 0)
            {
                Debug.LogWarning($"[ItemBaseData] 유효하지 않은 ItemId. SO: {so.name}, Index: {i}, ItemId: {item.itemId}");
            }

            if (!ids.Add(item.itemId))
            {
                Debug.LogError($"[ItemBaseData] 중복 ItemId 발견. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (item.mainType == ItemMainType.Unknown)
            {
                Debug.LogWarning($"[ItemBaseData] MainType이 Unknown입니다. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (item.subType == ItemSubType.Unknown)
            {
                Debug.LogWarning($"[ItemBaseData] SubType이 Unknown입니다. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (item.stackLimit <= 0)
            {
                Debug.LogWarning($"[ItemBaseData] StackLimit이 0 이하입니다. SO: {so.name}, ItemId: {item.itemId}, StackLimit: {item.stackLimit}");
            }

            if (string.IsNullOrWhiteSpace(item.itemName))
            {
                Debug.LogWarning($"[ItemBaseData] ItemName이 비어 있습니다. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (string.IsNullOrWhiteSpace(item.spriteAddress))
            {
                Debug.LogWarning($"[ItemBaseData] SpriteAddress가 비어 있습니다. SO: {so.name}, ItemId: {item.itemId}");
            }
        }
    }

    private static void ValidateFlowerItemData(FlowerItemData so)
    {
        HashSet<int> ids = new();

        for (int i = 0; i < so.Count; i++)
        {
            FlowerItemAuthoringData item = so.Get(i);

            if (item.itemId <= 0)
            {
                Debug.LogWarning($"[FlowerItemData] 유효하지 않은 ItemId. SO: {so.name}, Index: {i}, ItemId: {item.itemId}");
            }

            if (!ids.Add(item.itemId))
            {
                Debug.LogError($"[FlowerItemData] 중복 ItemId 발견. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (item.growthDuration < 0)
            {
                Debug.LogWarning($"[FlowerItemData] GrowthDuration이 음수입니다. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (item.harvestAmount < 0)
            {
                Debug.LogWarning($"[FlowerItemData] HarvestAmount가 음수입니다. SO: {so.name}, ItemId: {item.itemId}");
            }
        }
    }

    private static void ValidateGearItemData(GearItemData so)
    {
        HashSet<int> ids = new();

        for (int i = 0; i < so.Count; i++)
        {
            GearItemAuthoringData item = so.Get(i);

            if (item.itemId <= 0)
            {
                Debug.LogWarning($"[GearItemData] 유효하지 않은 ItemId. SO: {so.name}, Index: {i}, ItemId: {item.itemId}");
            }

            if (!ids.Add(item.itemId))
            {
                Debug.LogError($"[GearItemData] 중복 ItemId 발견. SO: {so.name}, ItemId: {item.itemId}");
            }

            if (item.maxDurability <= 0)
            {
                Debug.LogWarning($"[GearItemData] MaxDurability가 0 이하입니다. SO: {so.name}, ItemId: {item.itemId}, MaxDurability: {item.maxDurability}");
            }
        }
    }

    private static void ValidateFertilizerItemData(FertilizerItemData so)
    {
        HashSet<int> ids = new();

        for (int i = 0; i < so.Count; i++)
        {
            FertilizerItemAuthoringData item = so.Get(i);

            if (item.itemId <= 0)
            {
                Debug.LogWarning($"[FertilizerItemData] 유효하지 않은 ItemId. SO: {so.name}, Index: {i}, ItemId: {item.itemId}");
            }

            if (!ids.Add(item.itemId))
            {
                Debug.LogError($"[FertilizerItemData] 중복 ItemId 발견. SO: {so.name}, ItemId: {item.itemId}");
            }
        }
    }
}

#endif