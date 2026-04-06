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


public struct ItemDataStatic
{
    public short ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public ItemDataStatic(short input_itemId, FixedString64Bytes input_ItemName,
        FixedString128Bytes input_Description, FixedString64Bytes input_SpriteAddress)
    {
        ItemId = input_itemId;
        ItemName  = input_ItemName;
        Description = input_Description;
        SpriteAddress = input_SpriteAddress;
    }
}

public struct UsableItemDataStatic 
{
    public short ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public short Duration;
    public sbyte Power;
    public ChargeInfo chargeInfo;

    public UsableItemDataStatic(short input_itemId, FixedString64Bytes input_ItemName,
        FixedString128Bytes input_Description, FixedString64Bytes input_SpriteAddress,
        short input_Duration, sbyte input_Power, ChargeInfo input_ChargeInfo)
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
    public short ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public FixedString32Bytes Species;
    public FixedString32Bytes Color;
    public FixedString32Bytes Floro1;
    public FixedString32Bytes Floro2;

    bool isSeed;

    public FlowerItemDataStatic(short input_itemId, FixedString64Bytes input_ItemName,
        FixedString128Bytes input_Description, FixedString64Bytes input_SpriteAddress,
        FixedString32Bytes input_species, FixedString32Bytes input_Color,
        FixedString32Bytes input_Floro1, FixedString32Bytes input_Floro2 = default,
        bool input_isSeed = false)
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

    public void SetItemName(FixedString64Bytes itemName) => this.ItemName = itemName;
    public void SetIsSeed(bool isSeed = false) => this.isSeed = isSeed;
    public void SetItemDescription(FixedString128Bytes description) => this.Description = description;
}



public class ItemManagerHeavilyModified : IAsyncStartable, IDisposable
{
    bool _isInitialized = false; // 초기화 완료 여부

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
        _isInitialized = true;
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

    public async UniTask LoadFlowerEncyclopedia()
    {
        int count = FLOWER_END_ID - FLOWER_START_ID + 1; // 예: 700개

        // 1. 필요한 메모리 할당 (NativeArray)
        var targetIds = new NativeArray<short>(count, Allocator.TempJob);
        var outNames = new NativeArray<FixedString64Bytes>(count, Allocator.TempJob);
        var outDescs = new NativeArray<FixedString128Bytes>(count, Allocator.TempJob);
        var outAddrs = new NativeArray<FixedString128Bytes>(count, Allocator.TempJob);

        // 2. 대상 ID 채우기 (여기는 메인 스레드에서 한 번만 수행)
        for (int i = 0; i < count; i++)
        {
            targetIds[i] = (short)(FLOWER_START_ID + i);
        }

        // 3. Job 설정
        ItemEncyclopediaJob job = new ItemEncyclopediaJob
        {
            targetItemIds = targetIds,
            CommonDB = _nativeItemDB,
            FlowerDB = _nativeFlowerItemDB,
            UsableDB = _nativeUsableItemDB,
            OutNames = outNames,
            OutDescriptions = outDescs,
            OutSpriteAddresses = outAddrs
        };
        try
        {

            // 4. Job 실행 (700번의 Execute를 병렬로 돌려라!)
            JobHandle handle = job.Schedule(count, 64); // 64개씩 묶어서 스레드에 배분

            // 5. 완료 대기 (비동기로 기다리기)
            //await handle.ToUniTask();
            await handle.WaitAsync(PlayerLoopTiming.Update);

            // 6. 결과 활용 (이제 outNames 등을 사용해 UI 리스트 생성)
            // ... UI 생성 로직 ...
        }
        finally
        {
            // 7. 메모리 해제
            targetIds.Dispose();
            outNames.Dispose();
            outDescs.Dispose();
            outAddrs.Dispose();
        }
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
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id].speciesIndex;
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
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index = db1.Value.Items[id].colorIndex;
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
            Debug.LogError($"<color=red>[ItemSearchSystem]</color> GetDurationBurst: Invalid ID {id}");
            return;
        }
        else
        {
            byte index1 = db1.Value.Items[id].floroIndex;
            sbyte index2 = db1.Value.Items[id].floroIndex2;
            floro1 = db2.Value.flowerDetails[index1].color;
            if( index2 >= 0)
                floro2 = db2.Value.flowerDetails[index2].color;
            else floro2 = default;
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