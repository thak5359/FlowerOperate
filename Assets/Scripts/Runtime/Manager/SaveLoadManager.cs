using Fungus;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using R3;
using System;
using AYellowpaper.SerializedCollections;

public class SaveLoadManager : IStartable, IDisposable
{
    private PlayerOwnItemDataManager _storageManager;
    private PlotManager _plotManager;
    private ChunkManager _chunkManager;
    private string SAVE_FILE_NAME = "SaveData.bytes";
    public SaveDatas saveData;
    private SerializedDictionary<int, PlotData> _plotDataCache = new();

    private SaveDatas loadedData;
    private bool isLoading = false;

    // 구독 해제를 관리할 가방
    private DisposableBag disposableBag = new();

    public SaveLoadManager()
    {
    }

    [Inject]
    public void Construct(
        PlayerOwnItemDataManager storageParent
    )
    {
        _storageManager = storageParent;
        if (_storageManager == null)
        {
            Debug.LogError("<color=red>SaveLoadManager: storageParent (PlayerOwnItemDataManager) is NULL during Construct!</color>");
        }
        else
        {
            Debug.Log($"SaveLoadManager: storageParent injected successfully. Type: {_storageManager.GetType().Name}");
        }

        // 의존성 주입 완료 후 구독
        _storageManager.InventoryRevisionChanged.Subscribe(_ => SyncSaveData(true)).AddTo(ref disposableBag);
        GlobalEventManager.OnNextDayObservable.Subscribe(_ => OnNextDayTransition()).AddTo(ref disposableBag);

        Debug.Log("SaveLoadManager 전역 의존성 주입 완료");
    }

    void IStartable.Start()
    {
        Debug.Log("SaveLoadManager: Start EntryPoint initialized (Lazy loading avoided).");
    }

    public void RegisterPlotManager(PlotManager plotManager)
    {
        _plotManager = plotManager;
        Debug.Log("SaveLoadManager: PlotManager registered successfully.");
    }

    public void RegisterChunkManager(ChunkManager chunkManager)
    {
        _chunkManager = chunkManager;
        Debug.Log("SaveLoadManager: ChunkManager registered successfully.");
    }

    public void Dispose()
    {
        disposableBag.Dispose();
    }

    private void SyncSaveData(bool syncPlotManager = true)
    {
        if (isLoading) return;

        if (syncPlotManager && _plotManager != null)
        {
            _plotManager.SyncItemState();
            _plotDataCache = new SerializedDictionary<int, PlotData>(_plotManager.GetPlotDataDict);
        }

        if (_storageManager == null)
        {
            Debug.Log("SaveLoadManager : storageManager가 null임");
            return;
        }

        int day = (ProgressManager.getDay() != 0) ? ProgressManager.getPlayedDayOnGameSystem() : 0;

        var farmChunks = _chunkManager != null ? _chunkManager.GetFarmChunkDatas : (saveData != null ? saveData.GetFarmChunkDatas : null);
        var fieldChunks = _chunkManager != null ? _chunkManager.GetFieldChunkDatas : (saveData != null ? saveData.GetFieldChunkDatas : null);
        var forestChunks = _chunkManager != null ? _chunkManager.GetForestChunkDatas : (saveData != null ? saveData.GetForestChunkDatas : null);
        var mineChunks = _chunkManager != null ? _chunkManager.GetMineChunkDatas : (saveData != null ? saveData.GetMineChunkDatas : null);

        saveData = new SaveDatas(
            day,
            _storageManager.GetData,
            _plotDataCache,
            _storageManager.GetData.GetMoney,
            _storageManager.GetData.GetReputation,
            farmChunks,
            fieldChunks,
            forestChunks,
            mineChunks
        );
    }

    public void Save(string file)
    {
        Save(file, true);
    }

    public void Save(string file, bool syncPlotManager)
    {
        SAVE_FILE_NAME = NormalizeBinaryFileName(file);

        SyncSaveData(syncPlotManager);

        if (saveData == null)
        {
            Debug.LogError("저장할 데이터가 생성되지 않았습니다.");
            return;
        }

        FileDataHandler.SaveBinary(saveData, SAVE_FILE_NAME);
        Debug.Log($"데이터 저장 완료: {SAVE_FILE_NAME}");
    }

    private void OnNextDayTransition()
    {
        // 1. 현재 씬에 PlotManager가 활성화되어 있다면, 밭 데이터를 최종 싱크하여 최신 상태로 캐시를 채웁니다.
        if (_plotManager != null)
        {
            _plotManager.SyncItemState();
            _plotDataCache = new SerializedDictionary<int, PlotData>(_plotManager.GetPlotDataDict);
        }

        // 2. 메모리 캐시 상의 모든 밭(PlotData)에 대해 GrowUp(하루 성장/시듦 연산)을 적용합니다.
        var keys = new System.Collections.Generic.List<int>(_plotDataCache.Keys);
        foreach (var key in keys)
        {
            var data = _plotDataCache[key];
            data.GrowUp();
            _plotDataCache[key] = data;
        }

        // 3. 성장이 완료된 데이터를 파일에 저장합니다. 이때 PlotManager의 밭 데이터 수집은 패스합니다(이미 메모리 캐시를 최신 성장 데이터로 업그레이드했기 때문).
        Save("SaveData", syncPlotManager: false);
    }

    public void Load(string file = null)
    {
        SAVE_FILE_NAME = NormalizeBinaryFileName(file);
        isLoading = true;
        try
        {
            if(!string.IsNullOrEmpty(SAVE_FILE_NAME))
                loadedData = FileDataHandler.LoadBinary<SaveDatas>(SAVE_FILE_NAME);

            saveData = (loadedData != null) ? loadedData : saveData;

            if (saveData == null)
            {
                Debug.LogWarning("SaveLoadManager: 로드할 세이브 데이터가 존재하지 않아 Load를 건너뜁니다.");
                return;
            }

            // 로드 성공 시 메모리 밭 캐시 데이터도 복구해 둡니다.
            _plotDataCache = new SerializedDictionary<int, PlotData>(saveData.GetPlotData);

            // 날짜 데이터 복구
            ProgressManager.LoadData(new ProgressManager.ProgressData(saveData.GetPlayDay));
            Debug.Log($"SaveLoadManager: 날짜 복원 완료 -> {saveData.GetPlayDay}일차");

            if (_storageManager != null)
                _storageManager.Load(saveData);

            if (_plotManager != null)
                _plotManager.Load(saveData);

            Debug.Log("데이터 로드 및 통합 매니저 분배 완료");
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string NormalizeBinaryFileName(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            return null;

        if (file.EndsWith(".bytes"))
            return file;

        if (file.EndsWith(".json"))
            return file.Replace(".json", ".bytes");

        return $"{file}.bytes";
    }

    public SaveDatas GetSaveDatas => saveData;
}