using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

using static GlobalItemDB;

public static class GlobalItemDB
{
    private static ItemDatabaseAccessor _accessor;

    private static readonly Dictionary<int, int> _baseIndexById = new();
    private static readonly Dictionary<int, int> _flowerIndexById = new();
    private static readonly Dictionary<int, int> _gearIndexById = new();
    private static readonly Dictionary<int, int> _fertilizerIndexById = new();


    public static bool IsInitialized => _accessor.IsInitialized;

    public static void Initialize(ItemDatabaseAccessor accessor)
    {
        _accessor = accessor;

        BuildIndexMaps();

        Debug.Log("[GlobalItemDB] 초기화 완료");
    }

    public static void Clear()
    {
        _baseIndexById.Clear();
        _flowerIndexById.Clear();
        _gearIndexById.Clear();
        _fertilizerIndexById.Clear();

        _accessor = default;

        Debug.Log("[GlobalItemDB] 초기화 해제");
    }

    private static void BuildIndexMaps()
    {
        _baseIndexById.Clear();
        _flowerIndexById.Clear();
        _gearIndexById.Clear();
        _fertilizerIndexById.Clear();

        BuildBaseIndexMap();
        BuildFlowerIndexMap();
        BuildGearIndexMap();
        BuildFertilizerIndexMap();

    }

    private static void BuildBaseIndexMap()
    {
        if (!_accessor.ItemBaseDB.IsCreated)
        {
            Debug.LogError("[GlobalItemDB] ItemBaseDB가 생성되지 않았습니다.");
            return;
        }

        ref BlobArray<ItemBaseBlobData> items = ref _accessor.ItemBaseDB.Value.Items;

        for (int i = 0; i < items.Length; i++)
        {
            int itemId = items[i].ItemId;

            if (!_baseIndexById.TryAdd(itemId, i))
            {
                Debug.LogError($"[GlobalItemDB] ItemBaseDB 중복 ItemId 발견: {itemId}");
            }
        }
    }

    private static void BuildFlowerIndexMap()
    {
        if (!_accessor.FlowerDB.IsCreated)
        {
            Debug.LogWarning("[GlobalItemDB] FlowerDB가 생성되지 않았습니다.");
            return;
        }

        ref BlobArray<FlowerItemBlobData> items = ref _accessor.FlowerDB.Value.Items;

        for (int i = 0; i < items.Length; i++)
        {
            int itemId = items[i].ItemId;

            if (!_flowerIndexById.TryAdd(itemId, i))
            {
                Debug.LogError($"[GlobalItemDB] FlowerDB 중복 ItemId 발견: {itemId}");
            }
        }
    }

    private static void BuildGearIndexMap()
    {
        if (!_accessor.GearDB.IsCreated)
        {
            Debug.LogWarning("[GlobalItemDB] GearDB가 생성되지 않았습니다.");
            return;
        }

        ref BlobArray<GearItemBlobData> items = ref _accessor.GearDB.Value.Items;

        for (int i = 0; i < items.Length; i++)
        {
            int itemId = items[i].ItemId;

            if (!_gearIndexById.TryAdd(itemId, i))
            {
                Debug.LogError($"[GlobalItemDB] GearDB 중복 ItemId 발견: {itemId}");
            }
        }
    }

    private static void BuildFertilizerIndexMap()
    {
        if (!_accessor.FertilizerDB.IsCreated)
        {
            Debug.LogWarning("[GlobalItemDB] FertilizerDB가 생성되지 않았습니다.");
            return;
        }

        ref BlobArray<FertilizerItemBlobData> items = ref _accessor.FertilizerDB.Value.Items;

        for (int i = 0; i < items.Length; i++)
        {
            int itemId = items[i].ItemId;

            if (!_fertilizerIndexById.TryAdd(itemId, i))
            {
                Debug.LogError($"[GlobalItemDB] FertilizerDB 중복 ItemId 발견: {itemId}");
            }
        }
    }

    // 수정할 위치: GlobalItemDB.cs 내부
    // 변경 이유: 구조체를 복사하는 기존의 TryGet 메서드들을 삭제하고, 존재 여부를 묻는 Has 메서드와 원본 메모리를 가리키는 GetRef 메서드로 분리했어요.

    // ==========================================
    // 기존의 TryGet... 메서드들을 전부 지우고 아래 코드로 교체해주세요!
    // ==========================================

    // ItemBase
    public static bool HasBase(int itemId)
    {
        return _accessor.ItemBaseDB.IsCreated && _baseIndexById.ContainsKey(itemId);
    }
    public static ref ItemBaseBlobData GetBaseRef(int itemId)
    {
        return ref _accessor.ItemBaseDB.Value.Items[_baseIndexById[itemId]];
    }

    // Flower
    public static bool HasFlower(int itemId)
    {
        return _accessor.FlowerDB.IsCreated && _flowerIndexById.ContainsKey(itemId);
    }
    public static ref FlowerItemBlobData GetFlowerRef(int itemId)
    {
        return ref _accessor.FlowerDB.Value.Items[_flowerIndexById[itemId]];
    }

    // Gear (이 녀석 때문에 시작된 최적화죠!)
    public static bool HasGear(int itemId)
    {
        return _accessor.GearDB.IsCreated && _gearIndexById.ContainsKey(itemId);
    }
    public static ref GearItemBlobData GetGearRef(int itemId)
    {
        return ref _accessor.GearDB.Value.Items[_gearIndexById[itemId]];
    }

    // Fertilizer
    public static bool HasFertilizer(int itemId)
    {
        return _accessor.FertilizerDB.IsCreated && _fertilizerIndexById.ContainsKey(itemId);
    }
    public static ref FertilizerItemBlobData GetFertilizerRef(int itemId)
    {
        return ref _accessor.FertilizerDB.Value.Items[_fertilizerIndexById[itemId]];
    }

    public static bool Exists(int itemId) => HasBase(itemId);

    public static ItemMainType GetMainType(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).MainType : ItemMainType.Unknown;
    }

    public static ItemSubType GetSubType(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).SubType : ItemSubType.Unknown;
    }

    public static int GetStackLimit(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).StackLimit : 0;
    }

    public static int GetPrice(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).RefundPrice : 0;
    }
    public static FixedString64Bytes GetItemName(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).ItemName : default;
    }
    public static FixedString128Bytes GetDescription(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).Description : default;
    }
    public static FixedString128Bytes GetSpriteAddress(int itemId)
    {
        return HasBase(itemId) ? GetBaseRef(itemId).SpriteAddress : default;
    }
}