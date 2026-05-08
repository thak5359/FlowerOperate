using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using VContainer;

public class SaveLoadManager : MonoBehaviour
{
    private PlayerItemDataManager _storageManager;
    private PlotManager _plotManager;
    private ProgressManager _progressManager;

    private string SAVE_FILE_NAME = "SaveData.json";
    public SaveDatas saveData;

    [Inject]
    public void Construct(PlayerItemDataManager storageParent, PlotManager plot, 
        ProgressManager progress)
    {
        _storageManager = storageParent;
        _plotManager = plot;
        _progressManager = progress;

        Debug.Log("SaveLoadManager 의존성 주입 완료 (ItemStorageParent 통합)");
    }

    private void SyncSaveData()
    {
        if (_storageManager == null || _plotManager == null) return;

        int day = (_progressManager != null) ? _progressManager.getDay() : 0;

        // ItemStorageParent가 관리하는 ItemInstantData(인벤토리+창고 포함)를 통째로 저장
        saveData = new SaveDatas(
            day,
            _storageManager.GetData,
            _plotManager.GetPlotDataDict
        );
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
        Debug.Log($"데이터 저장 완료: {SAVE_FILE_NAME}");
    }

    public void Load(string file)
    {
        SAVE_FILE_NAME = file;
        SaveDatas loadedData = FileDataHandler.LoadJson<SaveDatas>(SAVE_FILE_NAME);
        
        if (loadedData != null)
        {
            saveData = loadedData;
            
            // 통합된 데이터를 매니저들에게 분배
            if (_storageManager != null) _storageManager.Load(saveData);
            if (_plotManager != null) _plotManager.Load(saveData);
            
            Debug.Log("데이터 로드 및 통합 매니저 분배 완료");
        }
    }
    public SaveDatas GetSaveDatas => this.saveData;
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class SaveDatas
{
    [SerializeField] private string saveTime;
    [SerializeField] private int playDay;
    [SerializeField] private int money;
    [SerializeField] private int reputation;
    [SerializeField] private ItemInstantData itemData; // 인벤토리와 창고 리스트가 포함된 통합 구조체
    [SerializeField] private SerializedDictionary<int, PlotData> plotDataDict;



    public string GetSaveTime => saveTime;
    public int GetPlayDay => playDay;
    public ItemInstantData GetItemData => itemData;
    public SerializedDictionary<int, PlotData> GetPlotData => plotDataDict;
    
    public SaveDatas() { }

    public SaveDatas(int day, ItemInstantData itemData, SerializedDictionary<int, PlotData> plotData, int money = 0, int reputation = 0)
    {
        this.saveTime = DateTime.Now.ToString("yyyy/MM/dd \n HH : mm");
        this.playDay = day;
        this.itemData = itemData;
        this.plotDataDict = plotData;
        this.money = money;
        this.reputation = reputation;
    }
}
