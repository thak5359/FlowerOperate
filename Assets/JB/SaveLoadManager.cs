using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{

    [SerializeField]
    GameObject Inven;
    [SerializeField]
    GameObject Storage;

    public static event Action OnSaveData;
    public SaveDatas save;
    string SaveJson;

    private void Awake()
    {
        // GetComponent는 임시 코드. 추후 vcontainer를 활용하는 방향으로 수정 예정
        save = new SaveDatas(Inven.GetComponent<InventoryManager>().GetInvenData, Storage.GetComponent<StorageManager>().GetStorageData);
    }
    public void Save()
    {
        string path = Path.Combine(Application.dataPath, "Save.json");
        SaveJson = JsonUtility.ToJson(save, true);
        File.WriteAllText(path, SaveJson);
    }

    public void Load()
    {
        string path = Path.Combine(Application.dataPath, "Save.json");
        SaveJson = File.ReadAllText(path);
        save = JsonUtility.FromJson<SaveDatas>(SaveJson);
        OnSaveData?.Invoke();
    }
}

[Serializable]
public class SaveDatas
{
    [SerializeField]
    private ItemStorageData InvenData;
    [SerializeField]
    private ItemStorageData StorageData;

    public ItemStorageData GetInvenData => this.InvenData;
    public ItemStorageData GetStorageData => this.StorageData;

    public SaveDatas(ItemStorageData inventory, ItemStorageData storage)
    {
        this.InvenData = inventory;
        this.StorageData = storage;
    }
}
