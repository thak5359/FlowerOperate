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

    private SaveDatas save;
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

    }
}

[Serializable]
public class SaveDatas
{
    [SerializeField]
    private ItemStorageData InvenData;
    [SerializeField]
    private ItemStorageData StorageData;

    public SaveDatas(ItemStorageData inventory, ItemStorageData storage)
    {
        this.InvenData = inventory;
        this.StorageData = storage;
    }
}
