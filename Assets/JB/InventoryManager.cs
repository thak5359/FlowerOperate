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
}
