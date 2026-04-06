using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    [SerializeField] List<ItemDataContainer> slotList;
    [SerializeField] GameObject slotObject;

    private void Awake()
    {
        slotList = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
    }

    private void OnEnable()
    {
        SaveLoadManager.OnLoadData += this.Load;
    }

    private void OnDisable()
    {
        SaveLoadManager.OnLoadData -= this.Load;
    }

    private void Load(SaveDatas saveDatas)
    {
        base.Initialize(StorageType.INVEN, saveDatas.GetInvenData, slotObject, ref slotList);
    }
}
