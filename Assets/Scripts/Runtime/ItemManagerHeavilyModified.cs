using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer.Unity;
using static Constant;

public struct ItemDataStatic
{
    public int ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public ItemDataStatic(int input_itemId, string input_ItemName,
        string input_Description, string input_SpriteAddress)
    {
        ItemId = input_itemId;
        ItemName  = input_ItemName;
        Description = input_Description;
        SpriteAddress = input_SpriteAddress;
    }
}

public struct UsableItemDataStatic 
{
    public int ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public int Duration;
    public int Power;
    public ChargeInfo chargeInfo;

    public UsableItemDataStatic(int input_itemId, string input_ItemName,
        string input_Description, string input_SpriteAddress, int input_Duration, int input_Power, ChargeInfo input_ChargeInfo)
    {
        ItemId = input_itemId;
        ItemName = input_ItemName;
        Description = input_Description;
        SpriteAddress = input_SpriteAddress;

        Duration = input_Duration;
        Power = input_Power;
        chargeInfo = input_ChargeInfo;
    }
}

public struct FlowerItemDataStatic
{
    public int ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public FixedString32Bytes Species;
    public FixedString32Bytes Color;
    public FixedString32Bytes Floro1;
    public FixedString32Bytes Floro2;

    bool isSeed;

    public FlowerItemDataStatic(int input_itemId, string input_ItemName, string input_Description, string input_SpriteAddress, 
        string input_species, string input_Color, string input_Floro1, string input_Floro2 = null, bool input_isSeed = false)
    {
        ItemId = input_itemId;
        ItemName = input_ItemName;
        Description = input_Description;
        SpriteAddress = input_SpriteAddress;

        Species = input_species;
        Color = input_Color;
        Floro1 = input_Floro1;
        Floro2 = input_Floro2;
        isSeed = input_isSeed;
    }

    public FlowerItemDataStatic(FlowerItemDataStatic copy)
    {
        ItemId = copy.ItemId;
        ItemName = copy.ItemName;
        Description = copy.Description;
        SpriteAddress = copy.SpriteAddress;

        Species = copy.Species;
        Color = copy.Color;
        Floro1 = copy.Floro1;
        Floro2 = copy.Floro2;
        isSeed = copy.isSeed;
    }

    public void SetItemName(string itemName) => this.ItemName = itemName;
    public void SetIsSeed(bool isSeed = false) => this.isSeed = isSeed;
    public void SetItemDescription(string description) => this.Description = description;
}



public class ItemManagerHeavilyModified : IStartable
{
    bool _isInitialized = false; // 초기화 완료 여부


    [Header("Data Sources")]
    // 이 리스트에 FlowerIdData, UsableIdData SO들을 다 넣으시면 됩니다.
    [SerializeField] private List<ItemIdData> itemIdDatas;

    [Header("Master Tables (Detail)")]
    [SerializeField] private FlowerDetailData flowerDetail;
    [SerializeField] private UsableDetailData usableDetail;

    // 핵심: Burst가 접근 가능한 고속 데이터 배열
    private NativeArray<ItemDataStatic> _nativeItemDB;
    private NativeArray<FlowerItemDataStatic> _nativeFlowerItemDB;
    private NativeArray<UsableItemDataStatic> _nativeUsableItemDB;

    void IStartable.Start()
    {
        InitializeNativeDatabase();
        _isInitialized = true;
    }

    private void InitializeNativeDatabase()
    {
        _nativeItemDB = new NativeArray<ItemDataStatic>(1001, Allocator.Persistent);
        foreach (var data in itemIdDatas)
        {
            switch (data.startId)
            {
                case 0:
                    {
                        var IDdata = data as UsableIdData;
                        bool flowControl = InsertUsableItemData(IDdata);
                        if (!flowControl)
                            continue;
                    }
                    break;
                case LAST_USABLE_ID:
                    {
                        bool flowerControl = InsertItemData(data);
                        if (!flowerControl)
                            continue;
                    }
                    break;
                case LAST_COMMON_ID:
                    {
                        var IDdata = data as FlowerIdData;
                        bool flowControl = InsertFlowerData(IDdata);
                        if (!flowControl)
                            continue;
                    }
                    break;
                default:
                    continue;
            }
        }

        _isInitialized = true;
    }


    #region 데이터 타입별 InsertDataToMasterDB 함수
    private bool InsertItemData(ItemIdData data) //사용 불가능 아이템
    {
        if (data == null)
        {
            Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
            return false;
        }
        for (int i = 0; i < data.itemName.Count; i++)
        {
            ItemDataStatic itemData = new ItemDataStatic(i, data.ItemName(i), data.Description(i), data.Address(i));
            _nativeItemDB[data.startId + i] = itemData;
        }
        return true;
    }
    private bool InsertUsableItemData(UsableIdData data) //사용 가능 아이템
    {
        if (data == null)
        {
            Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
            return false;
        }
        for (int i = 0; i < data.itemName.Count; i++)
        {
            UsableItemDataStatic usable = new UsableItemDataStatic(
                i, data.ItemName(i), data.Description(i), data.Address(i),
                usableDetail.Duration(data.DuratIndex(i)), usableDetail.Power(data.PowerIndex(i)), usableDetail.ChargeInfo(data.ChargeIndex(i)
                ));

            _nativeUsableItemDB[data.startId + i] = usable;
        }
        return true;
    }
    private bool InsertFlowerData(FlowerIdData data) //꽃 아이템
    {
        if (data == null)
        {
            Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
            return false;
        }
        for (int i = 0; i < data.itemName.Count; i++)
        {
            FlowerItemDataStatic flower = new FlowerItemDataStatic(
                 i, data.ItemName(i), data.Description(i), data.Address(i),
                flowerDetail.Color(data.ColorIndex(i)), flowerDetail.Floro(data.FloroIndex(i)), flowerDetail.Floro(data.FloroIndex2(i)));

            var seed = new FlowerItemDataStatic(flower);
            seed.SetItemName(data.itemName[i] + " 씨앗");
            seed.SetIsSeed(true);

            _nativeFlowerItemDB[data.startId + (i * 2 + 1)] = flower;
            _nativeFlowerItemDB[data.startId + (i * 2)] = seed;

        }
        return true;
    }
       
    
    #endregion

    public string GetItemName(int id)
    {
        FixedString64Bytes result;
        ItemSearchSystem.GetNameBurst(in _nativeItemDB, in _nativeUsableItemDB, in _nativeFlowerItemDB, id, out result);
        return result.ToString();
    }

    public string GetItemDescription(int id)
    {
        FixedString128Bytes result;
        ItemSearchSystem.GetDescriptionBurst(in _nativeItemDB,in _nativeUsableItemDB, in _nativeFlowerItemDB, id, out result);
        return result.ToString();
    }

    public string GetItemAddress(int id)
    {
        FixedString128Bytes result;
        ItemSearchSystem.GetAddressBurst(in _nativeItemDB, in _nativeUsableItemDB, in _nativeFlowerItemDB, id, out result);
        return result.ToString();

    }

    public string GetUsableItemPower(int id)
    {
        return ItemSearchSystem.GetPowerBurst(in _nativeUsableItemDB, id).ToString();
    }

    public ChargeInfo GetUsabelItemChargeInfo(int id)
    {
        ChargeInfo result = new ChargeInfo();
        ItemSearchSystem.GetChargeInfo(in _nativeUsableItemDB, id, out result);
        return result;
    }

    public int GetUsableItemDuration(int id)
    {
        return ItemSearchSystem.GetDurationBurst(in _nativeUsableItemDB, id);
    }

    public string GetFlowerItemSpecies(int id)
    {
        FixedString32Bytes result;
        ItemSearchSystem.GetSpeciesBurst(in _nativeFlowerItemDB, id, out result);
        return result.ToString();
    }

    public string GetFlowerItemColor(int id)
    {
        FixedString32Bytes result;
        ItemSearchSystem.GetColorBurst(in _nativeFlowerItemDB, id, out result);
        return result.ToString();
    }

    public string GetFlowerItemFloro(int id, out string Floro2)
    {
        FixedString32Bytes result1;
        FixedString32Bytes result2;
         ItemSearchSystem.GetFloroBurst(in _nativeFlowerItemDB, id, out result1, out result2);

        if (result2 != null)
        {
            Floro2 = result2.ToString();
        }
        else
        {
            Floro2 = null;
        }

        Floro2 = result2.ToString();
        return result1.ToString();

    }

    void OnDestroy()
    {
        // NativeArray는 반드시 수동으로 해제해야 메모리 누수가 없습니다!
        if (_nativeItemDB.IsCreated) _nativeItemDB.Dispose();
    }
}


[BurstCompile]
public static class ItemSearchSystem
{
    #region 공용 데이터 접근


    [BurstCompile]
    public static void GetNameBurst(in NativeArray<ItemDataStatic> db1,
        in NativeArray<UsableItemDataStatic> db2,
        in NativeArray<FlowerItemDataStatic> db3,
        int id, out FixedString64Bytes result)
    {
        if (id >= 0 && id < LAST_USABLE_ID)
            GetNameBurst(in db1, id, out result);
        else if (id >= LAST_USABLE_ID && id < LAST_COMMON_ID)
            GetNameBurst(in db2, id, out result);
        else if (id >= LAST_COMMON_ID && id < LAST_FLOWER_ID)
            GetNameBurst(in db3, id, out result);
        else result = default;

    }


    [BurstCompile]
    public static void GetNameBurst(in NativeArray<ItemDataStatic> db, int id, out FixedString64Bytes result)
    {
        result= db[id].ItemName;
    }
    [BurstCompile]
    public static void GetNameBurst(in NativeArray<UsableItemDataStatic> db, int id, out FixedString64Bytes result)
    {
        result =  db[id].ItemName;
    }
    [BurstCompile]
    public static void GetNameBurst(in NativeArray<FlowerItemDataStatic> db, int id, out FixedString64Bytes result)
    {
        result = db[id].ItemName;
    }

    [BurstCompile]
    public static void GetDescriptionBurst(in NativeArray<ItemDataStatic> db1, in NativeArray<UsableItemDataStatic> db2,
       in NativeArray<FlowerItemDataStatic> db3, int id, out FixedString128Bytes result)
    {
        if (id >= 0 && id < LAST_USABLE_ID)
            GetDescriptionBurst(in db1, id, out result);
        else if (id >= LAST_USABLE_ID && id < LAST_COMMON_ID)
            GetDescriptionBurst(in db2, id, out result);
        else if (id >= LAST_COMMON_ID && id < LAST_FLOWER_ID)
            GetDescriptionBurst(in db3, id, out result);
        else result = default;

    }
    [BurstCompile]
    public static void GetDescriptionBurst(in NativeArray<ItemDataStatic> db, int id, out FixedString128Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {  
            result = default;
            return;
        }
        result = db[id].Description;
    }
    [BurstCompile]
    public static void GetDescriptionBurst(in NativeArray<UsableItemDataStatic> db, int id, out FixedString128Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result = db[id].Description;
    }
    [BurstCompile]
    public static void GetDescriptionBurst(in NativeArray<FlowerItemDataStatic> db, int id, out FixedString128Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result = db[id].Description;
    }



    [BurstCompile]
    public static void GetAddressBurst(in NativeArray<ItemDataStatic> db1, in NativeArray<UsableItemDataStatic> db2,
        in NativeArray<FlowerItemDataStatic> db3, int id, out FixedString128Bytes result)
    {
        if (id >= 0 && id < LAST_USABLE_ID)
             GetAddressBurst(in db1, id, out result);
        else if (id >= LAST_USABLE_ID && id < LAST_COMMON_ID)
             GetAddressBurst(in db2, id, out result);
        else if (id >= LAST_COMMON_ID && id < LAST_FLOWER_ID)
             GetAddressBurst(in db3, id, out result);
        else result = default;
    }
    [BurstCompile]
    public static void GetAddressBurst(in NativeArray<ItemDataStatic> db, int id, out FixedString128Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result =  db[id].SpriteAddress;
    }
    [BurstCompile]
    public static void GetAddressBurst(in NativeArray<UsableItemDataStatic> db, int id, out FixedString128Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result = db[id].SpriteAddress;
    }
    [BurstCompile]
    public static void GetAddressBurst(in NativeArray<FlowerItemDataStatic> db, int id, out FixedString128Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result = db[id].SpriteAddress;
    }


    #endregion 

    [BurstCompile]
    public static int GetDurationBurst(in NativeArray<UsableItemDataStatic> db, int id)
    {
        if (id < 0 || id >= db.Length) return -1;
        return db[id].Duration;
    }
    [BurstCompile]
    public static int GetPowerBurst(in NativeArray<UsableItemDataStatic> db, int id)
    {
        if (id < 0 || id >= db.Length) return -1;
        return db[id].Power;
    }
    [BurstCompile]
    public static void GetChargeInfo(in NativeArray<UsableItemDataStatic>db, int id, out ChargeInfo result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = new ChargeInfo(-1, -1);
        }
        result = db[id].chargeInfo;
    }



    [BurstCompile]
    public static void GetSpeciesBurst(in NativeArray<FlowerItemDataStatic> db, int id, out FixedString32Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result = db[id].Species;

    }
    [BurstCompile]
    public static void GetColorBurst(in NativeArray<FlowerItemDataStatic> db, int id, out FixedString32Bytes result)
    {
        if (id < 0 || id >= db.Length)
        {
            result = default;
            return;
        }
        result = db[id].Color;

    }
    [BurstCompile]
    public static void GetFloroBurst(in NativeArray<FlowerItemDataStatic> db, int id, out FixedString32Bytes result1, out FixedString32Bytes result2)
    {
        if (id < LAST_COMMON_ID || id >= db.Length - LAST_COMMON_ID)
        {
            result1 = default;
            result2 = default;
            return;
        }

        // TODO : 꽃말이 한개인건 별도 표기 방식이 존재하는가? 있다면 ?. 연산자를 이용하기
        result1 =  db[id].Floro1;
        result2 = db[id].Floro2;
    }
}