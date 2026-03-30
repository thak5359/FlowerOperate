using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

public struct ItemDataStatic
{
    public int ItemId;
    public FixedString64Bytes ItemName;
    public FixedString64Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public int Power;
    public int Duration;
}

public struct UsableItemDataStatic 
{
    public int ChargeTime;
    public int MaxChargeCount;
}

public class ItemMangerHeavyModified 
{
    public static ItemManager Instance { get; private set; }

    // 핵심: Burst가 접근 가능한 고속 데이터 배열
    private NativeArray<ItemDataStatic> _nativeMasterDb;
    private bool _isInitialized = false;


    //private void InitializeNativeDatabase()
    //{
    //    // NativeArray 할당 (Persistent: 게임 끝날 때까지 유지)
    //    _nativeMasterDb = new NativeArray<ItemDataStatic>(1001, Allocator.Persistent);
    //    foreach (var data in _nativeMasterDb)
    //    {
    //        switch (data.ItemId)
    //        {
    //            case 0:
    //                {
    //                    var IDdata = data as UsableIdData;
    //                    bool flowControl = InsertDataToMasterDB(IDdata);
    //                    if (!flowControl)
    //                        continue;
    //                }
    //                break;
    //            case 40:
    //                {
    //                    bool flowerControl = InsertDataToMasterDB(data);
    //                    if (!flowerControl)
    //                        continue;
    //                }
    //                break;
    //            case 300:
    //                {
    //                    var IDdata = data as FlowerIdData;
    //                    bool flowControl = InsertDataToMasterDB(IDdata);
    //                    if (!flowControl)
    //                        continue;
    //                }
    //                break;
    //            default:
    //                continue;
    //        }
    //    }



    //    _isInitialized = true;
    //}

    // --- Item.cs에서 사용하는 '겉모습'은 그대로 유지 ---

    //#region 데이터 타입별 InsertDataToMasterDB 함수
    //private bool InsertDataToMasterDB(ItemIdData data) //사용 불가능 아이템
    //{
    //    if (data == null)
    //    {
    //        Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
    //        return false;
    //    }
    //    for (int i = 0; i < data.itemName.Count; i++)
    //    {
    //        MasterData masterData = new MasterData(data.ItemName(i));
    //        if (masterData != null)
    //            masterDb[data.startId + i] = masterData;
    //    }
    //    return true;
    //}
    //private bool InsertDataToMasterDB(UsableIdData data) //사용 가능 아이템
    //{
    //    if (data == null)
    //    {
    //        Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
    //        return false;
    //    }
    //    for (int i = 0; i < data.itemName.Count; i++)
    //    {
    //        UsableDataBase usable = new UsableDataBase(
    //            data.ItemName(i), usableDetail.Duration(data.DuratIndex(i))
    //            , usableDetail.Power(data.PowerIndex(i)), usableDetail.ChargeInfo(data.ChargeIndex(i)));
    //        if (usable != null)
    //            masterDb[data.startId + i] = usable;
    //    }
    //    return true;
    //}
    //private bool InsertDataToMasterDB(FlowerIdData data) //꽃 아이템
    //{
    //    if (data == null)
    //    {
    //        Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
    //        return false;
    //    }
    //    for (int i = 0; i < data.itemName.Count; i++)
    //    {
    //        FlowerDataBase flower = new FlowerDataBase(
    //            data.ItemName(i), flowerDetail.Species(data.SpeciesIndex(i)),
    //            flowerDetail.Color(data.ColorIndex(i)), flowerDetail.Floro(data.FloroIndex(i)), flowerDetail.Floro(data.FloroIndex2(i)));
    //        if (flower != null)
    //        {
    //            var seed = new FlowerDataBase(flower);
    //            seed.SetItemName(data.itemName[i] + " 씨앗");
    //            seed.SetIsSeed(true);

    //            masterDb[data.startId + (i * 2 + 1)] = flower;
    //            masterDb[data.startId + (i * 2)] = seed;

    //            if (seed != null)
    //                Debug.Log(seed.GetItemName);
    //        }
    //    }
    //    return true;
    //}
    //#endregion

    public string GetItemName(int id)
    {
        // 내부적으로는 Burst로 최적화된 정적 메서드를 호출합니다.
        return ItemSearchSystem.GetNameBurst(_nativeMasterDb, id).ToString();
    }

    public string GetItemDescription(int id)
    {
        return ItemSearchSystem.GetDescriptionBurst(_nativeMasterDb, id).ToString();
    }

    public string GetItemAddress(int id)
    {
        return ItemSearchSystem.GetAddressBurst(_nativeMasterDb, id).ToString();
    }

    void OnDestroy()
    {
        // NativeArray는 반드시 수동으로 해제해야 메모리 누수가 없습니다!
        if (_nativeMasterDb.IsCreated) _nativeMasterDb.Dispose();
    }
}


[BurstCompile]
public static class ItemSearchSystem
{
    [BurstCompile]
    public static FixedString64Bytes GetNameBurst(NativeArray<ItemDataStatic> db, int id)
    {
        if (id < 0 || id >= db.Length) return "Unknown";
        return db[id].ItemName;
    }

    [BurstCompile]
    public static FixedString64Bytes GetDescriptionBurst(NativeArray<ItemDataStatic> db, int id)
    {
        if (id < 0 || id >= db.Length) return "";
        return db[id].Description;
    }

    [BurstCompile]
    public static FixedString64Bytes GetAddressBurst(NativeArray<ItemDataStatic> db, int id)
    {
        if (id < 0 || id >= db.Length) return "";
        return db[id].SpriteAddress;
    }
}