using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SaveLoadManager : MonoBehaviour
{
    private InventoryManager _inventoryManager;
    private StorageManager _storageManager;
    private PlotManager _plotManager;

    private const string SAVE_FILE_NAME = "SaveData.json";
    public SaveDatas saveData;

    [Inject]
    public void Construct(InventoryManager inven, StorageManager storage, PlotManager plot)
    {
        _inventoryManager = inven;
        _storageManager = storage;
        _plotManager = plot;
        Debug.Log("의존성 주입 완료!");
    }

    private void SyncSaveData()
    {
        if (_inventoryManager == null || _storageManager == null || _plotManager == null) return;
        
        // 저장 전 동기화 호출
        _inventoryManager.SyncItemState();
        _storageManager.SyncItemState();
        _plotManager.SyncItemState();

        // 참조가 아닌 값(리스트 복사)을 넘겨서 데이터 오염 방지
        saveData = new SaveDatas(
            ProgressManager.Instance.getDay(),
            CloneData(_inventoryManager.GetData), 
            CloneData(_storageManager.GetData), 
            CloneData(_plotManager.GetData), 
            new List<PlotData>(_plotManager.GetPlots)
        );
    }

    // ItemStorageData를 깊은 복사하는 헬퍼 함수
    private ItemStorageData CloneData(ItemStorageData original)
    {
        ItemStorageData clone = new ItemStorageData();
        clone.SetSlotsCount(original.GetSlotsCount);
        if (original.GetList != null)
        {
            clone.SetItemList(new List<ItemObjectData>(original.GetList));
        }
        return clone;
    }

    public void Save()
    {
        SyncSaveData();
        
        if (saveData == null)
        {
            Debug.LogError("저장할 데이터가 생성되지 않았습니다.");
            return;
        }

        FileDataHandler.SaveJson(saveData, SAVE_FILE_NAME);
    }

    public void Load()
    {
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
    [SerializeField] private int playDay;
    [SerializeField] private ItemStorageData invenData;
    [SerializeField] private ItemStorageData storageData;
    [SerializeField] private ItemStorageData plotItemData;
    [SerializeField] private List<PlotData> plotData;

    public ItemStorageData GetInvenData => invenData;
    public ItemStorageData GetStorageData => storageData;
    public ItemStorageData GetPlotItemData => plotItemData;
    public List<PlotData> GetPlotData => plotData;

    public SaveDatas(int day, ItemStorageData inventory, ItemStorageData storage, ItemStorageData plotItem, List<PlotData> plot)
    {
        this.playDay = day;
        this.invenData = inventory;
        this.storageData = storage;
        this.plotItemData = plotItem;
        this.plotData = plot;
    }
}
