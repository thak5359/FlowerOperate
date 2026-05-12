using Unity.Entities;

public struct ItemDatabaseAccessor
{
    public BlobAssetReference<ItemBaseBlobDatas> ItemBaseDB;
    public BlobAssetReference<FlowerItemBlobDatas> FlowerDB;
    public BlobAssetReference<GearItemBlobDatas> GearDB;

    public bool IsInitialized =>
        ItemBaseDB.IsCreated &&
        FlowerDB.IsCreated &&
        GearDB.IsCreated;
}