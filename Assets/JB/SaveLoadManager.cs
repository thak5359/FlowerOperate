using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SaveLoadManager : MonoBehaviour, IInitializable
{
    private InventoryManager inventoryManager;
    private StorageManager storageManager;

    public static event Action<SaveDatas> OnLoadData;
    public SaveDatas saveData;
    private string SaveJson;

    void IInitializable.Initialize()
    {
        saveData = new SaveDatas(inventoryManager.GetData, storageManager.GetData);
    }

    [Inject]
    public void Construct(InventoryManager inven, StorageManager storage)
    {
        inventoryManager = inven;
        storageManager = storage;
        Debug.Log("의존성 주입 완료!");
    }

    public SaveDatas GetSaveDatas => this.saveData;

    public void Save()
    {
        // 1. 주입 확인 (로그가 떴다면 통과)
        if (inventoryManager == null) return;

        // 2. 인벤토리에 들어있는 '현재 인스펙터 값'을 직접 가져옵니다.
        // 이벤트(Initialize)가 실행되지 않았어도 인스펙터에 넣은 값은 _data에 있습니다.
        var currentData = inventoryManager.GetData;

        if (currentData == null)
        {
            Debug.LogError("InventoryManager의 _data가 null입니다.");
            return;
        }

        // 3. 저장용 바구니(saveData)를 '지금' 새로 만듭니다. 
        // 여기서 생성자에 currentData를 넣으면 인스펙터 값이 저장용 객체로 복사됩니다.
        saveData = new SaveDatas(currentData, storageManager.GetData);

        // 4. JSON 저장
        string path = Path.Combine(Application.dataPath, "Save.json");
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);

        Debug.Log($"[저장 성공] 파일 경로: {path}");
    }

    public void Load()
    {
        string path = Path.Combine(Application.dataPath, "Save.json");
        SaveJson = File.ReadAllText(path);
        saveData = JsonUtility.FromJson<SaveDatas>(SaveJson);
        OnLoadData?.Invoke(saveData);
    }
}

[Serializable]
public class SaveDatas
{
    [SerializeField]
    private ItemStorageData InvenData;
    [SerializeField]
    private ItemStorageData StorageData;
    [SerializeField]
    private ItemStorageData plotData;

    public ItemStorageData GetInvenData => this.InvenData;
    public ItemStorageData GetStorageData => this.StorageData;
    public ItemStorageData GetPlotData => this.plotData;

    public SaveDatas(ItemStorageData inventory, ItemStorageData storage)
    {
        this.InvenData = inventory;
        this.StorageData = storage;
    }
}
