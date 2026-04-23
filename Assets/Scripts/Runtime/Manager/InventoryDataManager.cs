using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDataManager : ItemStorageParent
{
    [SerializeField] List<ItemObjectData> slotList => _data.GetList;
    void OnEnable()
    {
        GlobalEventManager.OnItemPickedUp += AddItem;
    }

    void OnDisable()
    {
        GlobalEventManager.OnItemPickedUp -= AddItem;
    }
    public override void Load(SaveDatas saveDatas)
    {
        base.Initialize(saveDatas.GetInvenData);
    }
    public void SyncItemState()
    {
        _data.SetItemList(slotList);
    }
}
