using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using VContainer.Unity;
using static Constant;


[BurstCompile]
public class ItemManagerHeavilyModified : IAsyncStartable, IDisposable
{
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

        GlobalItemDB.Instance.Data = new ItemDatabaseAccessor
        {
            ItemDB = _nativeItemDB,
            FlowerDB = _nativeFlowerItemDB,
            UsableDB = _nativeUsableItemDB,
            FlowerDetail = flowerDetail,
            UsableDetail = usableDetail
        };

        Debug.Log("<color=green>[Blob]</color> 모든 데이터 로드 완료!");
        //_isInitialized = true;
    }

    void IDisposable.Dispose()
    {
        if (_nativeItemDB.IsCreated) _nativeItemDB.Dispose();
        if (_nativeFlowerItemDB.IsCreated) _nativeFlowerItemDB.Dispose();
        if (_nativeUsableItemDB.IsCreated) _nativeUsableItemDB.Dispose();
        if(flowerDetail.IsCreated) flowerDetail.Dispose();
        if(usableDetail.IsCreated) usableDetail.Dispose();
    }

    private async UniTask LoadBlob<T> (string fileName, Action<BlobAssetReference<T>> assignActrion) where T : unmanaged
    {
       string path = Path.Combine(Application.streamingAssetsPath, BLOB_FOLDER, fileName);

        byte[] data = await File.ReadAllBytesAsync(path);
        assignActrion(BlobAssetReference<T>.Create(data));
    }
}


public struct ItemDatabaseAccessor
{
    public BlobAssetReference<ItemBlobDatas> ItemDB;
    public BlobAssetReference<FlowerItemBlobDatas> FlowerDB;
    public BlobAssetReference<UsableItemBlobDatas> UsableDB;
    public BlobAssetReference<FlowerDetailBlobDatas> FlowerDetail;
    public BlobAssetReference<UsableDetailBlobDatas> UsableDetail;

    public bool IsInitialized => ItemDB.IsCreated;
}

[BurstCompile]
public static class GlobalItemDB //: IJobParallelFor
{
    private struct Context { }

    // Burst와 Managed가 공유하는 메모리 영역에 데이터 접근을 제공하는 SharedStatic
    public static readonly SharedStatic<ItemDatabaseAccessor> Instance =
        SharedStatic<ItemDatabaseAccessor>.GetOrCreate<Context>();


    // 데이터 존재 여부 확인용
    public static bool IsInitialized => Instance.Data.IsInitialized;

    #region 공용 데이터 접근 (Managed & Burst 호환)

    [BurstCompile]
    public static void GetItemName(short id, out FixedString64Bytes name)
    {
        name = default;
        if (!IsInitialized) return;

        ref var db = ref Instance.Data;
        if (id >= 0 && id < Constant.COMMON_END_ID)
            name = db.UsableDB.Value.Items[id - Constant.USABLE_START_ID].ItemName;
        else if (id >= Constant.COMMON_END_ID && id < Constant.FLOWER_END_ID)
            name = db.ItemDB.Value.Items[id - Constant.COMMON_START_ID].ItemName;
        else
            name = db.FlowerDB.Value.Items[id - Constant.FLOWER_START_ID].ItemName;
    }

    [BurstCompile]
    public static void GetAddress(short id, out FixedString128Bytes address)
    {
        address = default;
        if (!IsInitialized) return;

        ref var db = ref Instance.Data;
        if (id >= 0 && id < Constant.COMMON_END_ID)
            address = db.UsableDB.Value.Items[id - Constant.USABLE_START_ID].SpriteAddress;
        else if (id >= Constant.COMMON_END_ID && id < Constant.FLOWER_END_ID)
            address = db.ItemDB.Value.Items[id - Constant.COMMON_START_ID].SpriteAddress;
        else
            address = db.FlowerDB.Value.Items[id - Constant.FLOWER_START_ID].SpriteAddress;
    }

    //  UI, Addressables용
    public static string GetAddressString(short id)
    {
        GetAddress(id, out var addr);
        return addr.IsEmpty ? null : addr.ToString();
    }
    #endregion

    #region 장비(Usable) 아이템 데이터 접근
    [BurstCompile]
    public static void GetDuration(short id, out short duration)
    {
        duration = -1;
        if (!IsInitialized || id < 0 || id > USABLE_END_ID) return;

        ref var db = ref Instance.Data;
        // ID에서 시작 오프셋을 빼서 정확한 배열 인덱스를 계산합니다.
        int indexInArray = id - USABLE_START_ID;
        byte detailIndex = db.UsableDB.Value.Items[indexInArray].durationIndex;
        duration = db.UsableDetail.Value.usableDetails[detailIndex].duration;
    }

    [BurstCompile]
    public static void GetPower(short id, out short power)
    {
        power = -1;
        if (!IsInitialized || id < 0 || id > USABLE_END_ID) return;

        ref var db = ref Instance.Data;
        int indexInArray = id - USABLE_START_ID;
        byte detailIndex = db.UsableDB.Value.Items[indexInArray].powerIndex;
        power = db.UsableDetail.Value.usableDetails[detailIndex].power;
    }

    [BurstCompile]
    public static void GetChargeInfo(short id, out ChargeInfo charge)
    {
        charge = default;
        if (!IsInitialized || id < 0 || id > USABLE_END_ID) return;

        ref var db = ref Instance.Data;
        int indexInArray = id - USABLE_START_ID;
        byte detailIndex = db.UsableDB.Value.Items[indexInArray].chargeIndex;
        charge = db.UsableDetail.Value.usableDetails[detailIndex].chargeInfo;
    }
    #endregion

    #region 꽃(Flower) 아이템 전용 데이터 접근
    [BurstCompile]
    public static void GetSpecies(short id, out FixedString64Bytes species)
    {
        species = default;
        if (!IsInitialized || id < COMMON_END_ID || id > FLOWER_END_ID) return;

        ref var db = ref Instance.Data;
        int indexInArray = id - FLOWER_START_ID;
        byte detailIndex = db.FlowerDB.Value.Items[indexInArray].speciesIndex;
        species = db.FlowerDetail.Value.flowerDetails[detailIndex].species;
    }

    [BurstCompile]
    public static void GetColor(short id, out FixedString32Bytes color)
    {
        color = default;
        if (!IsInitialized || id < COMMON_END_ID || id > FLOWER_END_ID) return;

        ref var db = ref Instance.Data;
        int indexInArray = id - FLOWER_START_ID;
        byte detailIndex = db.FlowerDB.Value.Items[indexInArray].colorIndex;
        color = db.FlowerDetail.Value.flowerDetails[detailIndex].color;
    }

    [BurstCompile]
    public static void GetFloro(short id, out FixedString32Bytes floro1, out FixedString32Bytes floro2)
    {
        floro1 = default;
        floro2 = default;
        if (!IsInitialized || id < COMMON_END_ID || id > FLOWER_END_ID) return;

        ref var db = ref Instance.Data;
        int indexInArray = id - FLOWER_START_ID;

        byte idx1 = db.FlowerDB.Value.Items[indexInArray].floroIndex;
        sbyte idx2 = db.FlowerDB.Value.Items[indexInArray].floroIndex2;

        floro1 = db.FlowerDetail.Value.flowerDetails[idx1].color;
        if (idx2 >= 0)
            floro2 = db.FlowerDetail.Value.flowerDetails[idx2].color;
    }
    #endregion
}