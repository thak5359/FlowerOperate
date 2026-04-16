using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class InventoryManager : ItemStorageParent
{
    [SerializeField] List<ItemObjectData> slotList;
    // 인벤토리 슬롯 리스트
    [SerializeField] List<HotBarSlot> slots = new();

    [Inject] private HotbarManager _hotbarManager;
    // Getter >  게터는 PascalCase로 작성하는 것이 C#의 관례야! SlotList면 충분해!
    public List<ItemObjectData> getSlotList => slotList;

    public void RefreshHotbarUI()
    {
        
    }



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
        base.Initialize( saveDatas.GetInvenData, ref slotList);
    }

    //TODO :: 아래 기능 UI 부분으로 옮기기

    /// <summary>
    /// 실제 UI 슬롯들의 데이터를 현재 데이터 리스트(_data)와 동기화합니다.
    /// </summary>
    public void RefreshUI()
    {
        if (_data == null || _data.GetList == null) return;

        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < _data.GetList.Count)
                slotList = _data.GetList;
        }
    }

    /// <summary>
    /// 저장 전, UI 슬롯의 실제 값을 데이터 리스트에 반영합니다. 
    /// </summary>
    
    public void SyncItemState()
    {
        _data.SetItemList(slotList);
    }
}
