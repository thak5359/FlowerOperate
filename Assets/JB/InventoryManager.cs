using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    [SerializeField] List<ItemDataContainer> slotList;

    private void Awake()
    {
        for(int i = slotList.Count; i < _data.GetSlotsCount; i++)
            Instantiate(slotObject, this.transform);
        if (slotList == null || slotList.Count == 0)
            slotList = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
    }

    public override void Load(SaveDatas saveDatas)
    {
        base.Initialize(this, saveDatas.GetInvenData, slotObject, ref slotList);
    }

    /// <summary>
    /// 실제 UI 슬롯들의 데이터를 현재 데이터 리스트(_data)와 동기화합니다.
    /// </summary>
    public void RefreshUI()
    {
        if (_data == null || _data.GetList == null) return;

        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < _data.GetList.Count)
                slotList[i].SetData(_data.GetList[i]);
            else
                slotList[i].SetData(default); // 빈 슬롯 처리
        }
    }

    /// <summary>
    /// 저장 전, UI 슬롯의 실제 값을 데이터 리스트에 반영합니다.
    /// </summary>
    public void SyncItemState()
    {
        List<ItemObjectData> currentStates = new List<ItemObjectData>();
        foreach (var slot in slotList)
        {
            currentStates.Add(slot.GetData);
        }
        _data.SetItemList(currentStates);
    }
}
