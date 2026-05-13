using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Entities;
using UnityEngine;
using VContainer.Unity;

public sealed class ItemManager : IAsyncStartable, IDisposable
{
    private const string BlobFolder = "Blobs";

    private const string ItemBaseBlobFile = "ItemBaseData.blob";
    private const string FlowerItemBlobFile = "FlowerItemData.blob";
    private const string GearItemBlobFile = "GearItemData.blob";
    private const string FertilizerItemBlobFile = "FertilizerItemData.blob";


    private BlobAssetReference<ItemBaseBlobDatas> _itemBaseDB;
    private BlobAssetReference<FlowerItemBlobDatas> _flowerDB;
    private BlobAssetReference<GearItemBlobDatas> _gearDB;
    private BlobAssetReference<FertilizerItemBlobDatas> _fertilizerDB;

    public bool IsInitialized =>
        //_gearDB.IsCreated &&
         // _flowerDB.IsCreated &&
         _itemBaseDB.IsCreated;
    public async UniTask StartAsync(CancellationToken cancellationToken)
    {
        _itemBaseDB = await LoadBlobAsync<ItemBaseBlobDatas>(
            ItemBaseBlobFile,
            cancellationToken
        );

        _flowerDB = await LoadBlobAsync<FlowerItemBlobDatas>(
            FlowerItemBlobFile,
            cancellationToken
        );

        _gearDB = await LoadBlobAsync<GearItemBlobDatas>(
            GearItemBlobFile,
            cancellationToken
        );

        _fertilizerDB = await LoadBlobAsync<FertilizerItemBlobDatas>(
            FertilizerItemBlobFile,
            cancellationToken
        );

        var accessor = new ItemDatabaseAccessor
        {
            ItemBaseDB = _itemBaseDB,
            FlowerDB = _flowerDB,
            GearDB = _gearDB,
            FertilizerDB = _fertilizerDB
        };

        GlobalItemDB.Initialize(accessor);

        Debug.Log("[ItemManager] 모든 Item Blob DB 로드 완료");
    }

    private static async UniTask<BlobAssetReference<T>> LoadBlobAsync<T>(
        string fileName,
        CancellationToken cancellationToken
    ) where T : unmanaged
    {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            BlobFolder,
            fileName
        );

        if (!File.Exists(path))
        {
            Debug.LogError($"[ItemManager] Blob 파일을 찾을 수 없습니다: {path}");
            return default;
        }

        byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return BlobAssetReference<T>.Create(bytes);
    }

    public GameItem CreateItem(int itemId, int count)
    {
        if (!GlobalItemDB.TryGetBase(itemId, out ItemBaseBlobData baseData))
        {
            Debug.LogError($"[ItemManager] 존재하지 않는 ItemId입니다. Id: {itemId}");
            return null;
        }

        return baseData.SubType switch
        {
            ItemSubType.Flower => new FlowerItem(itemId, count),
            ItemSubType.Equipment => new GearItem(itemId, count),
            ItemSubType.Fertilizer => new FertilizerItem(itemId, count),
            _ => new GameItem(itemId, count)
        };
    }

    public async UniTask<GameItem> CreateAndLoadItemAsync(int itemId, int count)
    {
        GameItem item = CreateItem(itemId, count);

        if (item == null)
            return null;

        await item.OnLoadAsync();
        return item;
    }

    public void Dispose()
    {
        GlobalItemDB.Clear();

        if (_itemBaseDB.IsCreated)
            _itemBaseDB.Dispose();

        if (_flowerDB.IsCreated)
            _flowerDB.Dispose();

        if (_gearDB.IsCreated)
            _gearDB.Dispose();

        if( _fertilizerDB.IsCreated)
            _fertilizerDB.Dispose();

        Debug.Log("[ItemManager] Item Blob DB 해제 완료");
    }
}




