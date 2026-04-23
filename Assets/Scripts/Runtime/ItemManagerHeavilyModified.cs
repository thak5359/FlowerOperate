using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer.Unity;
using static Constant;

public class ItemManagerHeavilyModified : IAsyncStartable, IDisposable
{
   // bool _isInitialized = false; // 초기화 완료 여부

    //  Burst가 접근 가능한 고속 데이터 배열
    private BlobAssetReference<ItemBlobDatas> _nativeItemDB;
    private BlobAssetReference<FlowerItemBlobDatas> _nativeFlowerItemDB;
    private BlobAssetReference<UsableItemBlobDatas> _nativeUsableItemDB;

    private BlobAssetReference<FlowerDetailBlobDatas> flowerDetail;
    private BlobAssetReference<UsableDetailBlobDatas> usableDetail;

    public async UniTask StartAsync(CancellationToken ct)
    {
        await UniTask.WhenAll(
            LoadBlob<ItemBlobDatas>(ITEM_BLOB, (res) => _nativeItemDB = res),
            LoadBlob<FlowerItemBlobDatas>(FLOWER_BLOB, (res) => _nativeFlowerItemDB = res),
            LoadBlob<UsableItemBlobDatas>(USABLE_BLOB, (res) => _nativeUsableItemDB = res),
            LoadBlob<FlowerDetailBlobDatas>(FLOWER_DETAIL_BLOB, (res) => flowerDetail = res),
            LoadBlob<UsableDetailBlobDatas>(USABLE_DETAIL_BLOB, (res) => usableDetail = res)
        );

        Debug.Log("<color=green>[Blob]</color> 모든 데이터 로드 완료!");
        //_isInitialized = true;
    }

    void IDisposable.Dispose()
    {
        if (_nativeItemDB.IsCreated) _nativeItemDB.Dispose();
        if (_nativeFlowerItemDB.IsCreated) _nativeFlowerItemDB.Dispose();
        if (_nativeUsableItemDB.IsCreated) _nativeUsableItemDB.Dispose();
    }

    private async UniTask LoadBlob<T> (string fileName, Action<BlobAssetReference<T>> assignActrion) where T : unmanaged
    {
       string path = Path.Combine(Application.streamingAssetsPath, BLOB_FOLDER, fileName);

        byte[] data = await File.ReadAllBytesAsync(path);
        assignActrion(BlobAssetReference<T>.Create(data));
    }

}

[BurstCompile]
public static class ItemSearchSystem 
{
    #region 공용 데이터 접근
    [BurstCompile]
    public static void GetItemNameBurst(
        in BlobAssetReference<UsableItemBlobDatas> usableDB,
        in BlobAssetReference<ItemBlobDatas> itemDB,
        in BlobAssetReference<FlowerItemBlobDatas> flowerDB,
        short id,
        out FixedString64Bytes name)
    {
        // 인덱스 테이블 로직: ID 범위에 따라 적절한 BLOB의 Value.Items[index]에 접근
        if (id >= 0 && id < COMMON_END_ID)
        {
            name = usableDB.Value.Items[id - USABLE_START_ID].ItemName;
        }
        else if ( id >= COMMON_END_ID && id < FLOWER_END_ID)
        {
            name = itemDB.Value.Items[id - COMMON_START_ID].ItemName;
        }
        else
        {
            name = flowerDB.Value.Items[id- FLOWER_START_ID].ItemName;
        }
    }
    [BurstCompile]
    public static void GetDescriptionBurst(
       in BlobAssetReference<UsableItemBlobDatas> usableDB,
       in BlobAssetReference<ItemBlobDatas> itemDB,
       in BlobAssetReference<FlowerItemBlobDatas> flowerDB,
       short id,
       out FixedString128Bytes name)
    {
        // 인덱스 테이블 로직: ID 범위에 따라 적절한 BLOB의 Value.Items[index]에 접근
        if (id >= 0 && id < COMMON_END_ID)
        {
            name = usableDB.Value.Items[id- USABLE_START_ID].Description;
        }
        else if (id >= COMMON_END_ID && id < FLOWER_END_ID)
        {
            name = itemDB.Value.Items[id - COMMON_START_ID].Description;
        }
        else
        {
            name = flowerDB.Value.Items[id - FLOWER_START_ID].Description;
        }
    }
    [BurstCompile]
    public static void GetAddressBurst(
   in BlobAssetReference<ItemBlobDatas> db1,
   in BlobAssetReference<FlowerItemBlobDatas> db2,
   in BlobAssetReference<UsableItemBlobDatas> db3,
   short id,
   out FixedString128Bytes name)
    {
        // 인덱스 테이블 로직: ID 범위에 따라 적절한 BLOB의 Value.Items[index]에 접근
        if (id >= 0 && id < COMMON_END_ID)
        {
            name = db3.Value.Items[id - USABLE_START_ID].SpriteAddress;
        }
        else if (id >= COMMON_END_ID && id < FLOWER_END_ID)
        {
            name = db1.Value.Items[id - COMMON_START_ID].SpriteAddress;
        }
        else
        {
            name = db2.Value.Items[id - FLOWER_START_ID].SpriteAddress;
        }
    }
    #endregion
    
    #region 사용 아이템 전용 데이터 접근

    [BurstCompile]
    public static void GetDurationBurst(
        in BlobAssetReference<UsableItemBlobDatas> db1,
        in BlobAssetReference<UsableDetailBlobDatas> db2,
        short id, out short duration
        )
    {
        if (id < 0 || id > USABLE_END_ID )
        {
            duration = -1;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id].durationIndex;
            duration = db2.Value.usableDetails[index].duration;
        }
    }
    [BurstCompile]
    public static void GetPowerBurst(
       in BlobAssetReference<UsableItemBlobDatas> db1,
       in BlobAssetReference<UsableDetailBlobDatas> db2,
       short id, out short power
       )
    {
        if (id < 0 || id > USABLE_END_ID)
        {
            power = -1;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id].powerIndex;
            power = db2.Value.usableDetails[index].power;
        }
    }

    [BurstCompile]
    public static void GetChargeInfoBurst(
       in BlobAssetReference<UsableItemBlobDatas> db1,
       in BlobAssetReference<UsableDetailBlobDatas> db2,
       short id, out ChargeInfo power
       )
    {
        if (id < 0 || id > USABLE_END_ID)
        {
            power = default;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id].chargeIndex;
            power = db2.Value.usableDetails[index].chargeInfo;
        }
    }
    #endregion

    #region 꽃 아이템 전용 데이터 접근
    [BurstCompile]
    public static void GetSpeciesBurst(
      in BlobAssetReference<FlowerItemBlobDatas> db1,
      in BlobAssetReference<FlowerDetailBlobDatas> db2,
      short id, out FixedString64Bytes power
      )
    {
        if (id < COMMON_END_ID || id > FLOWER_END_ID)
        {
            power = default;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetSpeciesBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id - FLOWER_START_ID].speciesIndex;
            power = db2.Value.flowerDetails[index].species;
        }
    }

    [BurstCompile]
    public static void GetColorBurst(
     in BlobAssetReference<FlowerItemBlobDatas> db1,
     in BlobAssetReference<FlowerDetailBlobDatas> db2,
     short id, out FixedString32Bytes color
     )
    {
        if (id < COMMON_END_ID || id > FLOWER_END_ID)
        {
            color = default;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetColorBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id - FLOWER_START_ID].colorIndex;
            color = db2.Value.flowerDetails[index].color;
        }
    }

    [BurstCompile]
    public static void GetFloroBurst(
     in BlobAssetReference<FlowerItemBlobDatas> db1,
     in BlobAssetReference<FlowerDetailBlobDatas> db2,
     short id, out FixedString32Bytes floro1, out FixedString32Bytes floro2
     )
    {
        if (id < COMMON_END_ID || id > FLOWER_END_ID)
        {
            floro1 = default;
            floro2 = default;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetFloroBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index1 = db1.Value.Items[id - FLOWER_START_ID].floroIndex;
            sbyte index2 = db1.Value.Items[id - FLOWER_START_ID].floroIndex2;
            floro1 = db2.Value.flowerDetails[index1].color;
            if( index2 >= 0)
                floro2 = db2.Value.flowerDetails[index2].color;
            else floro2 = default;
        }
    }

    [BurstCompile]
    public static void GetGrowthDurationBurst(
      in BlobAssetReference<FlowerItemBlobDatas> db1,
      short id, out byte duration
      )
    {
        if (id < COMMON_END_ID || id > FLOWER_END_ID)
        {
            duration = default;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetGrowthDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            duration = db1.Value.Items[id - FLOWER_START_ID].growthDuration;
        }
    }

    [BurstCompile]
    public static void GetHarvestAmountBurst(
      in BlobAssetReference<FlowerItemBlobDatas> db1,
      short id, out byte amount
      )
    {
        if (id < COMMON_END_ID || id > FLOWER_END_ID)
        {
            amount = default;
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetHarvestAmountBurst: Invalid ID {id}");
            return;
        }
        else
        {
            amount = db1.Value.Items[id - FLOWER_START_ID].harvestAmount;
        }
    }
    #endregion

}


[BurstCompile ]
public struct ItemEncyclopediaJob : IJobParallelFor
{

    [ReadOnly] public NativeArray<short> targetItemIds;
    [ReadOnly] public BlobAssetReference<ItemBlobDatas> CommonDB;
    [ReadOnly] public BlobAssetReference<FlowerItemBlobDatas> FlowerDB;
    [ReadOnly] public BlobAssetReference<UsableItemBlobDatas> UsableDB;

    public NativeArray<FixedString64Bytes> OutNames;
    public NativeArray<FixedString128Bytes> OutDescriptions;
    public NativeArray<FixedString128Bytes> OutSpriteAddresses;

    public void Execute(int index)
    {

        short id = targetItemIds[index];

        // 우리가 만든 Burst 함수를 그대로 활용할 수 있습니다!
        ItemSearchSystem.GetItemNameBurst(UsableDB, CommonDB, FlowerDB,  id, out var name);
        ItemSearchSystem.GetDescriptionBurst(UsableDB, CommonDB, FlowerDB, id, out var desc);
        ItemSearchSystem.GetAddressBurst(CommonDB, FlowerDB, UsableDB, id, out var spriteAddr);

        OutNames[index] = name;
        OutDescriptions[index] = desc;
        OutSpriteAddresses[index] = spriteAddr;
    }
}