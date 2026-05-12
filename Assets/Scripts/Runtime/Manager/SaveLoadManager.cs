using Fungus;
using UnityEngine;
using VContainer;

public class SaveLoadManager : MonoBehaviour
{
    private PlayerItemDataManager _storageManager;
    private PlotManager _plotManager;
    private ItemManager _itemManager;

    private string SAVE_FILE_NAME = "SaveData.bytes";
    public SaveDatas saveData;

    [Inject]
    public void Construct(
        PlayerItemDataManager storageParent,
        PlotManager plot,
        ItemManager itemManager
    )
    {
        _storageManager = storageParent;
        _plotManager = plot;
        _itemManager = itemManager;

        Debug.Log("SaveLoadManager 의존성 주입 완료");
    }

    private void SyncSaveData()
    {
        if (_storageManager == null || _plotManager == null)
            return;

        int day = (ProgressManager.getDay() != 0) ? ProgressManager.getPlayDay() : 0;

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

    public void Load(string file)
    {
        SAVE_FILE_NAME = NormalizeBinaryFileName(file);

        SaveDatas loadedData = FileDataHandler.LoadBinary<SaveDatas>(SAVE_FILE_NAME);

        if (loadedData == null)
        {
            Debug.LogWarning($"로드할 데이터가 없습니다: {SAVE_FILE_NAME}");
            return;
        }

        saveData = loadedData;

        if (_storageManager != null)
            _storageManager.Load(saveData);

        if (_plotManager != null)
            _plotManager.Load(saveData);

        Debug.Log("데이터 로드 및 통합 매니저 분배 완료");
    }

    private static string NormalizeBinaryFileName(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            return "SaveData.bytes";

        if (file.EndsWith(".bytes"))
            return file;

        if (file.EndsWith(".json"))
            return file.Replace(".json", ".bytes");

        return $"{file}.bytes";
    }

    public SaveDatas GetSaveDatas => saveData;
}