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

    //public override async void Load(SaveDatas saveDatas)
    //{
    //    // base.Initialize를 통해 _data와 slotList(ref)를 초기화
    //    base.Initialize(saveDatas.GetStorageData);
    //    await RefreshUI();
    //}
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
        //if (_StorageData.GetList != null)
        //{
        //    _StorageData.SetItemList(slotList);
        //}
    }
}
