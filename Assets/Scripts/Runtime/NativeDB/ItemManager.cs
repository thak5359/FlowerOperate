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

    // 수정 위치: 모든 소비자가 동일한 초기화 완료 신호를 기다리도록 공유 완료 소스를 제공해요.
    private readonly UniTaskCompletionSource _initializationSource = new();
    private bool _initializationStarted;

    public UniTask InitializationTask => _initializationSource.Task;

    public bool IsInitialized =>
        _itemBaseDB.IsCreated &&
        _flowerDB.IsCreated &&
        _gearDB.IsCreated &&
        _fertilizerDB.IsCreated &&
        GlobalItemDB.IsInitialized;

    // 수정 위치: VContainer 진입점도 중복 실행 방지 초기화 API 하나만 사용해요.
    public UniTask StartAsync(CancellationToken cancellationToken)
    {
        return InitializeAsync(cancellationToken);
    }

    // 수정 위치: 초기화 중복 실행을 막고 성공·실패 결과를 모든 대기자에게 전달해요.
    public async UniTask InitializeAsync(CancellationToken cancellationToken)
    {
        if (_initializationStarted)
        {
            await InitializationTask;
            return;
        }

        _initializationStarted = true;

        try
        {
            await LoadAllAsync(cancellationToken);
            _initializationSource.TrySetResult();
        }
        catch (Exception exception)
        {
            DisposeBlobDatabases();
            _initializationSource.TrySetException(exception);
            Debug.LogException(exception);
            throw;
        }
    }

    // 수정 위치: 모든 Blob이 생성된 경우에만 GlobalItemDB를 공개해요.
    private async UniTask LoadAllAsync(CancellationToken cancellationToken)
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

        if (!_itemBaseDB.IsCreated ||
            !_flowerDB.IsCreated ||
            !_gearDB.IsCreated ||
            !_fertilizerDB.IsCreated)
        {
            throw new InvalidDataException("[ItemManager] 하나 이상의 Item Blob DB 생성에 실패했습니다.");
        }

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
            throw new FileNotFoundException($"[ItemManager] Blob 파일을 찾을 수 없습니다: {path}", path);
        }

        byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        
        // BlobAssetReference.Write로 저장된 파일은 36바이트의 헤더를 포함합니다.
        // (32바이트 헤더 + 4바이트 패딩)
        const int headerSize = 36;
        if (bytes.Length <= headerSize)
        {
            throw new InvalidDataException($"[ItemManager] Blob 파일이 유효하지 않거나 비어 있습니다: {path}");
        }

        // 헤더를 제외한 실제 데이터 영역만 추출하여 Blob 생성
        byte[] data = new byte[bytes.Length - headerSize];
        Array.Copy(bytes, headerSize, data, 0, data.Length);
        
        return BlobAssetReference<T>.Create(data);
    }

    // 수정 위치: 생성한 아이템의 DB·스프라이트 로드 완료 후에만 반환해요.
    public async UniTask<GameItem> CreateItemAsync(int itemId, int count, FlowerGrade grade_F = FlowerGrade.Lv0, GearGrade grade_G = GearGrade.Old)
    {
        await InitializationTask;

        if (!GlobalItemDB.HasBase(itemId))
        {
            Debug.LogError($"[ItemManager] 존재하지 않는 ItemId입니다. Id: {itemId}");
            return null;
        }

        // 수정 위치: async 메서드가 await를 넘겨 ref local을 보존하지 않도록 값을 복사해요.
        ItemBaseBlobData baseData = GlobalItemDB.GetBaseRef(itemId);


        GameItem item = baseData.SubType switch
        {
            ItemSubType.Flower => new FlowerItem(itemId, count, grade_F),
            ItemSubType.Seed => new FlowerItem(itemId, count, grade_F),
            ItemSubType.Equipment => new GearItem(itemId, count, grade_G),
            ItemSubType.Fertilizer => new FertilizerItem(itemId, count),
            _ => new CommonItem(itemId, count)
        };

        await item.OnLoadAsync();
        return item;
    }

    public void Dispose()
    {
        GlobalItemDB.Clear();
        DisposeBlobDatabases();

        Debug.Log("[ItemManager] Item Blob DB 해제 완료");
    }

    // 수정 위치: 초기화 실패와 정상 종료가 같은 안전한 Blob 해제 경로를 사용해요.
    private void DisposeBlobDatabases()
    {
        if (_itemBaseDB.IsCreated)
            _itemBaseDB.Dispose();

        if (_flowerDB.IsCreated)
            _flowerDB.Dispose();

        if (_gearDB.IsCreated)
            _gearDB.Dispose();

        if( _fertilizerDB.IsCreated)
            _fertilizerDB.Dispose();
    }
}
