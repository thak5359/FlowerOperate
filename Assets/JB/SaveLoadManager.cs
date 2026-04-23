using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer;
using VContainer.Unity;

public class SaveLoadManager : MonoBehaviour
{
    private InventoryDataManager _inventoryManager;
    private StorageManager _storageManager;
    private PlotManager _plotManager;
    private ProgressManager _progressManager;

    private string SAVE_FILE_NAME = "SaveData.json";
    public SaveDatas saveData;


    [Inject]
    public void Construct(InventoryDataManager inven, StorageManager storage, PlotManager plot, 
        ProgressManager progress)
    {
        _inventoryManager = inven;
        _storageManager = storage;
        _plotManager = plot;
        _progressManager = progress;

        Debug.Log("의존성 주입 완료!");
    }

    private void SyncSaveData()
    {
        if (_inventoryManager == null || _storageManager == null || _plotManager == null) return;
        
        // 저장 전 동기화 호출
        _inventoryManager.SyncItemState();
        _storageManager.SyncItemState();
        _plotManager.SyncItemState();

        int day = (_progressManager != null) ? _progressManager.getDay() : 0;

        // 참조가 아닌 값(리스트 복사)을 넘겨서 데이터 오염 방지
        saveData = new SaveDatas(
            day,
            CloneData(_inventoryManager.GetData),
            CloneData(_storageManager.GetData),
            _plotManager.getPlotDataDict
        );
    }

    // ItemStorageData를 깊은 복사하는 헬퍼 함수
    private ItemInstantData CloneData(ItemInstantData original)
    {
        ItemInstantData clone = new ItemInstantData();
        clone.SetSlotsCount(original.GetSlotsCount);
        if (original.GetList != null)
        {
            clone.SetItemList(new List<ItemObjectData>(original.GetList));
        }
        return clone;
    }

    public void Save(string file)
    {
        SAVE_FILE_NAME = file;
        SyncSaveData();
        
        if (saveData == null)
        {
            Debug.LogError("저장할 데이터가 생성되지 않았습니다.");
            return;
        }

        FileDataHandler.SaveJson(saveData, SAVE_FILE_NAME);
    }

    public void Load(string file)
    {
        SAVE_FILE_NAME = file;
        SaveDatas loadedData = FileDataHandler.LoadJson<SaveDatas>(SAVE_FILE_NAME);
        
        if (loadedData != null)
        {
            saveData = loadedData;
            
            // 주입된 각 매니저의 Load 메서드를 직접 호출하여 데이터를 분배합니다.
            if (_inventoryManager != null) _inventoryManager.Load(saveData);
            if (_storageManager != null) _storageManager.Load(saveData);
            if (_plotManager != null) _plotManager.Load(saveData);
            
            Debug.Log("데이터 로드 및 분배 완료");
        }
    }

    public SaveDatas GetSaveDatas => this.saveData;
}

[Serializable]
public class SaveDatas
{
    [SerializeField] private string saveTime;
    [SerializeField] private int playDay;
    [SerializeField] private ItemInstantData invenData;
    [SerializeField] private ItemInstantData storageData;
    [SerializeField] private SerializedDictionary<int, PlotData> plotDataDict;

    public string GetSaveTime => saveTime;
    public ItemInstantData GetInvenData => invenData;
    public ItemInstantData GetStorageData => storageData;
    public SerializedDictionary<int, PlotData> GetPlotData => plotDataDict;
    
    public SaveDatas() { }

    public SaveDatas(int day, ItemInstantData inventory, ItemInstantData storage, SerializedDictionary<int, PlotData> plotData)
    {
        this.saveTime = DateTime.Now.ToString("yyyy/MM/dd \n HH : mm");
        this.playDay = day;
        this.invenData = inventory;
        this.storageData = storage;
        this.plotDataDict = plotData;
    }
}
