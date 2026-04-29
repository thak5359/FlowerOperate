using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Constant;

public enum ContainerType
{
    INVENTORY,
    STORAGE
}

[Serializable]
public class ItemStorageParent : MonoBehaviour
{
    [SerializeField]
    protected ItemInstantData _Data = new ItemInstantData();

    //Getter
    public ItemInstantData GetData => _Data;
    void OnEnable()
    {
        GlobalEventManager.OnItemPickedUp += AddItem;
    }

    void OnDisable()
    {
        GlobalEventManager.OnItemPickedUp -= AddItem;
    }

    // 상속받은 클래스에서 기본으로 사용할 타입을 지정 (예: InventoryDataManager는 INVENTORY)
    protected virtual ContainerType currentType => ContainerType.INVENTORY;

    protected virtual void Initialize(ItemInstantData data)
    {
        if (data.GetList(currentType) == null)
        {
            Debug.LogWarning($"{currentType} 데이터가 유효하지 않습니다.");
            return;
        }

        _Data = data;
        GlobalEventManager.InvokeDataChanged();
    }

    public virtual void Load(SaveDatas saveDatas)
    {
        Initialize(saveDatas.GetItemData);
    }

    protected void ResetData(ContainerType type)
    {
        int count = (_Data.GetList(type).Count != 0) ? _Data.GetSlotsCount : 50;
        if (count <= 0) count = 50;

        List<ItemObjectData> emptyList = new List<ItemObjectData>();
        for (int i = 0; i < count; i++) emptyList.Add(default);

        _Data.SetItemList(type, emptyList);
        GlobalEventManager.InvokeDataChanged();
    }

    public virtual void Swap(ContainerType startPoint, ContainerType endPoint, int idx1, int idx2)
    {
        _Data.SwapItem(startPoint, endPoint, idx1, idx2);
        GlobalEventManager.InvokeDataChanged();
        Debug.Log($"{startPoint}[{idx1}] <-> {endPoint}[{idx2}] 아이템 위치 스왑");
    }

    public virtual void EngraftItem(ref ItemObjectData a, ref ItemObjectData b)
    {
        if (a.CheckFull() || b.CheckEmpty() || a.GetItemID != b.GetItemID || a.GetGrade != b.GetGrade)
            return;

        int space = 100 - a.GetAmount;
        int amountToMove = Math.Min(space, (int)b.GetAmount);

        a.SetAmount((short)(a.GetAmount + amountToMove));
        b.SetAmount((short)(b.GetAmount - amountToMove));
        
        if (b.CheckEmpty())
            b = default;

        GlobalEventManager.InvokeDataChanged();
    }

    public void Sort() => Sort(currentType);

    public void Sort(ContainerType type)
    {
        Debug.Log($"{type} 아이템 정리 시작");
        _Data.SortList(type);
        
        var list = _Data.GetList(type);
        for (int i = 0; i < list.Count - 1; i++)
        {
            ItemObjectData itemL = list[i];
            ItemObjectData itemR = list[i + 1];

            EngraftItem(ref itemL, ref itemR);

            list[i] = itemL;
            list[i + 1] = itemR;
        }
        _Data.SortList(type);
        GlobalEventManager.InvokeDataChanged();
    }

    protected virtual void AddItem(ItemObjectData item) => AddItem(currentType, item);

    public virtual void AddItem(ContainerType type, ItemObjectData item)
    {
        _Data.AddItem(type, item);
        GlobalEventManager.InvokeDataChanged();
    }

    public bool RemoveItem(ContainerType type, ushort id, int count)
    {
        if (!HasItem(type, id, count)) return false;

        var list = _Data.GetList(type);
        int remainingToRemove = count;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item.GetItemID == id)
            {
                int toTake = Mathf.Min(remainingToRemove, item.GetAmount);
                item.SetAmount((short)(item.GetAmount - toTake));
                remainingToRemove -= toTake;

                list[i] = item.GetAmount <= 0 ? default : item;
                if (remainingToRemove <= 0) break;
            }
        }
        GlobalEventManager.InvokeDataChanged();
        return true;
    }

    public bool HasItem(ContainerType type, ushort id, int count)
    {
        int totalAmount = _Data.GetList(type)
            .Where(item => item.GetItemID == id)
            .Sum(item => (int)item.GetAmount);
        return totalAmount >= count;
    }
}

[Serializable]
public struct ItemInstantData
{
    [SerializeField] private List<ItemObjectData> storageList;
    [SerializeField] private List<ItemObjectData> invenList;
    [SerializeField] private int slotsCount;

    // Getter
    public List<ItemObjectData> GetList(ContainerType type) => (type == ContainerType.INVENTORY) ? invenList : storageList;
    public int GetSlotsCount => slotsCount;

    // Setter
    public void SetItemList(ContainerType type, List<ItemObjectData> itemList)
    {
        if (type == ContainerType.INVENTORY) invenList = itemList;
        else storageList = itemList;
    }

    public void SetSlotsCount(int slotsCount) => this.slotsCount = slotsCount;

    public void ClearList(ContainerType type) => GetList(type).Clear();

    public void SwapItem(ContainerType startPoint, ContainerType endPoint, int idx1, int idx2)
    {
        List<ItemObjectData> target1 = GetList(startPoint);
        List<ItemObjectData> target2 = GetList(endPoint);

        ItemObjectData temp = target1[idx1];
        target1[idx1] = target2[idx2];
        target2[idx2] = temp;
    }

    public void AddItem(ContainerType type, ItemObjectData item)
    {
        List<ItemObjectData> targetList = GetList(type);
        
        // 1. 같은 아이템이 있고 겹칠 수 있는 슬롯 확인
        int idx = targetList.FindIndex(curItem => curItem.GetItemID == item.GetItemID && !curItem.CheckFull());
        
        if (idx != -1)
        {
            var existingItem = targetList[idx];
            existingItem.AddAmount(item.GetAmount);
            targetList[idx] = existingItem;
            Debug.Log($"[{type}] 기존 슬롯에 합치기");
        }
        else 
        {
            // 2. 빈 슬롯 확인
            int emptyIdx = targetList.FindIndex(data => data.GetItemID == 0);
            if (emptyIdx != -1)
            {
                targetList[emptyIdx] = item;
                Debug.Log($"[{type}] 새 슬롯에 추가");
            }
            else
            {
                Debug.Log($"[{type}] 슬롯 가득 참");
            }
        }
    }

    public void SortList(ContainerType type)
    {
        var sortedList = GetList(type)
            .OrderByDescending(item => item.GetItemID != 0)
            .ThenBy(item => item.GetItemID)
            .ThenByDescending(item => item.GetAmount)
            .ToList();

        SetItemList(type, sortedList);
    }

    public bool IsFull(ContainerType type) => !GetList(type).Any(item => item.GetItemID == 0);
}
