using Fungus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : ItemStorageParent
{
    [SerializeField] List<ItemDataContainer> slotList;

    private void Awake()
    {
        //Initialize();
        
    }

    private void OnEnable()
    {
        SaveLoadManager.OnLoadData += Load;
    }

    private void OnDisable()
    {
        SaveLoadManager.OnLoadData -= Load;
    }

    private void Load(SaveDatas saveDatas)
    {
        base.Initialize(StorageType.STORAGE, saveDatas.GetStorageData, null, ref slotList);
    }

    public void SortList()
    {
        GetData.GetList.Sort((ItemObjectData a, ItemObjectData b) => {
            //2차 : ID 오름차순
            return a.GetItemID.CompareTo(b.GetItemID);
        });
    }
}
