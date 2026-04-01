using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    //Getter
    public ItemStorageData GetInvenData => _data;

    private void OnEnable()
    {
        SaveLoadManager.OnLoadData += this.Initialize;
    }

    private void OnDisable()
    {
        SaveLoadManager.OnLoadData -= this.Initialize;
    }

    private void Initialize(SaveDatas save)
    {
        base.Initialize();
        _data = save.GetInvenData;
        Debug.Log("초기화 끝");
    }
}
