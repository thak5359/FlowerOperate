using Fungus;
using UnityEngine;
using VContainer;

public class SaveLoadManager : MonoBehaviour
{
    private PlayerOwnItemDataManager _storageManager;
    private PlotManager _plotManager;
    private string SAVE_FILE_NAME = "SaveData.bytes";
    public SaveDatas saveData;

    private SaveDatas loadedData;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    [Inject]
    public void Construct(
        PlayerOwnItemDataManager storageParent,
        IPlotManager plotManager
    )
    {
        _storageManager = storageParent;
        _plotManager = (PlotManager)plotManager;

        if (_storageManager == null)
        {
            Debug.LogError("<color=red>SaveLoadManager: storageParent (PlayerOwnItemDataManager) is NULL during Construct!</color>");
        }
        else
        {
            Debug.Log($"SaveLoadManager: storageParent injected successfully. Type: {_storageManager.GetType().Name}");
        }

        if (_plotManager == null)
        {
            Debug.LogError("<color=red>SaveLoadManager: plotManager (IPlotManager) is NULL during Construct!</color>");
        }
        else
        {
            Debug.Log($"SaveLoadManager: plotManager injected successfully. Type: {_plotManager.GetType().Name}");
        }

        Debug.Log("SaveLoadManager 의존성 주입 완료");
    }

    private void SyncSaveData()
    {
        if (_storageManager == null || _plotManager == null)
        {
            Debug.Log("SaveLoadManager : 매니저 둘 중에 하나 null임");
            return;
        }

        int day = (ProgressManager.getDay() != 0) ? ProgressManager.getPlayedDayOnGameSystem() : 0;

        saveData = new SaveDatas(
            day,
            _storageManager.GetData,
            _plotManager.GetPlotDataDict
        );
    }

    public void Save(string file)
    {
        SAVE_FILE_NAME = NormalizeBinaryFileName(file);

        SyncSaveData();

        if (saveData == null)
        {
            Debug.LogError("저장할 데이터가 생성되지 않았습니다.");
            return;
        }

        FileDataHandler.SaveBinary(saveData, SAVE_FILE_NAME);
        Debug.Log($"데이터 저장 완료: {SAVE_FILE_NAME}");
    }

    public void Load(string file = null)
    {
        SyncSaveData();
        SAVE_FILE_NAME = NormalizeBinaryFileName(file);
        
        if(!string.IsNullOrEmpty(SAVE_FILE_NAME) || loadedData == null)
            loadedData = FileDataHandler.LoadBinary<SaveDatas>(SAVE_FILE_NAME);

        saveData = (loadedData != null) ? loadedData : saveData;

        if (_storageManager != null)
            _storageManager.Load(saveData);

        if (_plotManager != null)
            _plotManager.Load(saveData);

        Debug.Log("데이터 로드 및 통합 매니저 분배 완료");
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