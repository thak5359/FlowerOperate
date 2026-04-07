using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : ItemStorageParent
{
    [SerializeField] List<ItemDataContainer> slotList;

    private void Awake()
    {
        if (slotList == null || slotList.Count == 0)
            slotList = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
    }

    public override void Load(SaveDatas saveDatas)
    {
        base.Initialize(this, saveDatas.GetStorageData, null, ref slotList);
        RefreshUI();
    }

    public void SortList()
    {
        if (_data == null || _data.GetList == null) return;

        // 1. 비어있지 않은 아이템만 추출하여 작업 리스트 생성
        List<ItemObjectData> items = new List<ItemObjectData>();
        foreach (var item in _data.GetList)
        {
            if (!item.CheckEmpty())
                items.Add(item);
        }

        // 2. 같은 아이템들 합치기 (Stacking)
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].CheckFull()) continue;

            for (int j = i + 1; j < items.Count; j++)
            {
                if (items[i].GetItemID == items[j].GetItemID && items[i].GetGrade == items[j].GetGrade)
                {
                    ItemObjectData itemI = items[i];
                    ItemObjectData itemJ = items[j];

                    base.EngraftItem(ref itemI, ref itemJ);

                    items[i] = itemI;
                    items[j] = itemJ;

                    if (items[i].CheckFull()) break;
                }
            }
        }

        // 3. 합치기 후 빈 아이템 제거
        items.RemoveAll(x => x.CheckEmpty());

        // 4. 정렬 (ID 오름차순, 등급 내림차순, 개수 내림차순)
        items.Sort((a, b) =>
        {
            if (a.GetItemID != b.GetItemID)
                return a.GetItemID.CompareTo(b.GetItemID);
            if (a.GetGrade != b.GetGrade)
                return b.GetGrade.CompareTo(a.GetGrade);
            return b.GetAmount.CompareTo(a.GetAmount);
        });

        // 5. 원래 슬롯 크기에 맞춰 데이터 리스트 재구성 (빈 슬롯 채우기)
        int totalSlots = slotList.Count > 0 ? slotList.Count : _data.GetList.Count;
        List<ItemObjectData> newList = new List<ItemObjectData>(totalSlots);
        newList.AddRange(items);
        while (newList.Count < totalSlots)
        {
            newList.Add(new ItemObjectData(default));
        }

        _data.SetItemList(newList);

        // 6. UI 갱신
        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < _data.GetList.Count)
                slotList[i].SetData(_data.GetList[i]);
            else
                slotList[i].SetData(default);
        }
    }

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
