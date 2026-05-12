using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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

    public static bool TryGetBase(int itemId, out ItemBaseBlobData data)
    {
        data = default;

        if (!_accessor.ItemBaseDB.IsCreated)
        {
            Debug.LogError("[GlobalItemDB] ItemBaseDB가 초기화되지 않았습니다.");
            return false;
        }

        if (!_baseIndexById.TryGetValue(itemId, out int index))
        {
            return false;
        }

        data = _accessor.ItemBaseDB.Value.Items[index];
        return true;
    }

    public static bool TryGetFlower(int itemId, out FlowerItemBlobData data)
    {
        data = default;

        if (!_accessor.FlowerDB.IsCreated)
        {
            Debug.LogError("[GlobalItemDB] FlowerDB가 초기화되지 않았습니다.");
            return false;
        }

        if (!_flowerIndexById.TryGetValue(itemId, out int index))
        {
            return false;
        }

        data = _accessor.FlowerDB.Value.Items[index];
        return true;
    }

    public static bool TryGetGear(int itemId, out GearItemBlobData data)
    {
        data = default;

        if (!_accessor.GearDB.IsCreated)
        {
            Debug.LogError("[GlobalItemDB] GearDB가 초기화되지 않았습니다.");
            return false;
        }

        if (!_gearIndexById.TryGetValue(itemId, out int index))
        {
            return false;
        }

        data = _accessor.GearDB.Value.Items[index];
        return true;
    }

    public static bool TryGetFertilizer(int itemId, out FertilizerItemBlobData data)
    {
        data = default;

        if (!_accessor.FertilizerDB.IsCreated)
        {
            Debug.LogError("[GlobalItemDB] GearDB가 초기화되지 않았습니다.");
            return false;
        }

        if (!_fertilizerIndexById.TryGetValue(itemId, out int index))
        {
            return false;
        }

        data = _accessor.FertilizerDB.Value.Items[index];
        return true;
    }


    public static bool Exists(int itemId)
    {
        return _baseIndexById.ContainsKey(itemId);
    }

    public static ItemMainType GetMainType(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.MainType
            : ItemMainType.Unknown;
    }

    public static ItemSubType GetSubType(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.SubType
            : ItemSubType.Unknown;
    }

    public static int GetStackLimit(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.StackLimit
            : 0;
    }

    public static int GetPrice(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.Price
            : 0;
    }

    public static FixedString64Bytes GetItemName(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.ItemName
            : default;
    }

    public static FixedString128Bytes GetDescription(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.Description
            : default;
    }

    public static FixedString128Bytes GetSpriteAddress(int itemId)
    {
        return TryGetBase(itemId, out var data)
            ? data.SpriteAddress
            : default;
    }
}