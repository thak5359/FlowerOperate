using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Threading.Tasks;

public class StorageManager : ItemStorageParent
{
    // 실제 아이템 데이터를 직접 관리하는 리스트
    [SerializeField] List<ItemObjectData> slotList = new List<ItemObjectData>();

    // 자식 오브젝트에서 관리할 UI 슬롯 리스트
    [SerializeField] List<HotBarSlot> hotbarSlots = new List<HotBarSlot>();

    private void Awake()
    {
        // _data가 초기화되어 있다고 가정하고 슬롯 생성 및 초기화
        if (_data != null)
        {
            // 이미 생성된 자식이 있다면 리스트에 추가하고, 부족하면 생성
            hotbarSlots.AddRange(GetComponentsInChildren<HotBarSlot>());

            for (int i = hotbarSlots.Count; i < _data.GetSlotsCount; i++)
            {
                if (slotObject != null)
                {
                    GameObject go = Instantiate(slotObject, this.transform);
                    HotBarSlot slot = go.GetComponent<HotBarSlot>();
                    if (slot != null) hotbarSlots.Add(slot);
                }
            }
        }
    }

    public override async void Load(SaveDatas saveDatas)
    {
        // base.Initialize를 통해 _data와 slotList(ref)를 초기화
        base.Initialize(this, saveDatas.GetStorageData, null, ref slotList);
        await RefreshUI();
    }

    public async Task SortList()
    {
        if (slotList == null || slotList.Count == 0) return;

        // 1. 비어있지 않은 아이템만 추출
        List<ItemObjectData> items = new List<ItemObjectData>();
        foreach (var item in slotList)
        {
            if (item.GetItemID != 0) // ID가 0이 아니면 유효한 아이템으로 간주 (ItemObjectData 정의 기준)
                items.Add(item);
        }

        // 2. 같은 아이템들 합치기 (Stacking)
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = i + 1; j < items.Count; j++)
            {
                if (items[i].GetItemID == items[j].GetItemID && items[i].GetGrade == items[j].GetGrade)
                {
                    ItemObjectData itemI = items[i];
                    ItemObjectData itemJ = items[j];

                    base.EngraftItem(ref itemI, ref itemJ);

                    items[i] = itemI;
                    items[j] = itemJ;
                }
            }
        }

        // 3. 빈 아이템 제거 (합치기 결과 수량이 0이 된 경우 등)
        items.RemoveAll(x => x.GetAmount <= 0);

        // 4. 정렬 (ID 오름차순, 등급 내림차순, 개수 내림차순)
        items.Sort((a, b) =>
        {
            if (a.GetItemID != b.GetItemID)
                return a.GetItemID.CompareTo(b.GetItemID);
            if (a.GetGrade != b.GetGrade)
                return b.GetGrade.CompareTo(a.GetGrade);
            return b.GetAmount.CompareTo(a.GetAmount);
        });

        // 5. 원래 슬롯 크기에 맞춰 리스트 재구성
        int totalSlots = _data != null ? _data.GetSlotsCount : hotbarSlots.Count;
        List<ItemObjectData> newList = new List<ItemObjectData>(totalSlots);
        newList.AddRange(items);
        while (newList.Count < totalSlots)
        {
            newList.Add(default); // 기본값(빈 아이템)으로 채움
        }

        slotList = newList;
        
        if (_data != null)
            _data.SetItemList(slotList);

        await RefreshUI();
    }

    public async Task RefreshUI()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < slotList.Count)
            {
                // HotBarSlot은 Item 객체를 받으므로, ItemManager 등을 통해 변환이 필요할 수 있습니다.
                // 여기서는 ItemManager.GetItem(ItemObjectData)와 같은 함수가 있다고 가정하거나
                // HotBarSlot의 내부 데이터 구조를 고려하여 처리해야 합니다.
                // 사용자 요청대로 HotBarSlot은 수정하지 않으므로, 데이터 동기화 로직만 유지합니다.
                
                // 임시: HotBarSlot이 ItemObjectData를 직접 처리할 수 있도록 확장되거나,
                // StorageManager에서 Item 객체를 생성하여 전달하는 로직이 필요합니다.
                // 현재 코드 구조상 HotBarSlot의 ChangeItem(Item)을 호출하는 방식을 권장합니다.
                
                // 만약 ItemManager가 static으로 존재한다면:
                // hotbarSlots[i].ChangeItem(ItemManager.GetItem(slotList[i]));

                await hotbarSlots[i].ChangeItem(
                    new Item(slotList[i].GetItemID, slotList[i].GetAmount));
            }
            else
            {
                await hotbarSlots[i].ChangeItem(null);
            }
        }
    }

    public void SyncItemState()
    {
        if (_data != null)
        {
            _data.SetItemList(slotList);
        }
    }
}
