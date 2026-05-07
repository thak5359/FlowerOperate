using Fungus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using static Constant;

public enum ContainerType
{
    INVENTORY,
    STORAGE,
    SELLING
}

[Serializable]
public class PlayerItemDataManager : MonoBehaviour
{
    [SerializeField]
    protected ItemInstantData _Data = new ItemInstantData();

    // 현재 활성화된 박스 번호 (기본값 0)
    [SerializeField] private int _currentBoxIndex = 0;
    public int CurrentBoxIndex { get => _currentBoxIndex; set => _currentBoxIndex = value; }

    //Getter
    public ItemInstantData GetData => _Data;

    void OnEnable()
    {
        GlobalEventManager.OnItemPickedUp += AddItem;
        GlobalEventManager.NextDay += CalculateMoney;
    }

    void OnDisable()
    {
        GlobalEventManager.OnItemPickedUp -= AddItem;
        GlobalEventManager.NextDay -= CalculateMoney;
    }

    protected virtual ContainerType currentType => ContainerType.INVENTORY;

    protected virtual void Initialize(ItemInstantData data)
    {
        _Data = data;
        GlobalEventManager.InvokeDataChanged();
    }

    public virtual void Load(SaveDatas saveDatas)
    {
        Initialize(saveDatas.GetItemData);
    }

    // 특정 박스나 인벤토리를 리셋
    protected void ResetData(ContainerType type, int boxNum = 0)
    {
        if (type == ContainerType.INVENTORY)
        {
            List<ItemObjectData> emptyList = new List<ItemObjectData>(new ItemObjectData[50]);
            _Data.SetItemList(type, emptyList);
        }
        else
        {
            var boxes = _Data.GetStorageBoxes;
        }
        GlobalEventManager.InvokeDataChanged();
    }

    public virtual void Swap(ContainerType startPoint, ContainerType endPoint, int startIdx, int endIdx, int startBoxNum = 0, int endBoxNum = 0)
    {
        _Data.SwapItem(startPoint, endPoint, startIdx, endIdx, startBoxNum, endBoxNum);
        GlobalEventManager.InvokeDataChanged();
        Debug.Log($"{startPoint}[{startIdx}] <-> {endPoint}[{endIdx}] 아이템 스왑 (Box: {startBoxNum}/{endBoxNum})");
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

    public void Sort() => Sort(currentType, _currentBoxIndex);

    public void Sort(ContainerType type, int boxNum = 0)
    {
        _Data.SortList(type, boxNum);

        var list = _Data.GetItemList(type, boxNum);
        for (int i = 0; i < list.Count - 1; i++)
        {
            ItemObjectData itemL = list[i];
            ItemObjectData itemR = list[i + 1];

            EngraftItem(ref itemL, ref itemR);

            list[i] = itemL;
            list[i + 1] = itemR;
        }
        _Data.SortList(type, boxNum);
        GlobalEventManager.InvokeDataChanged();
    }

    protected virtual void AddItem(ItemObjectData item) => AddItem(currentType, item, _currentBoxIndex);

    public virtual void AddItem(ContainerType type, ItemObjectData item, int boxNum = 0)
    {
        _Data.AddItem(type, item, boxNum);
        GlobalEventManager.InvokeDataChanged();
    }

    public bool RemoveItem(ContainerType type, ushort id, byte grade, int count, int boxNum = 0)
    {
        if (!HasItem(type, id, grade, count, boxNum)) return false;

        var list = _Data.GetItemList(type, boxNum);
        int remainingToRemove = count;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item.GetItemID == id && item.GetGrade == grade)
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

    public bool HasItem(ContainerType type, ushort id, byte grade, int count, int boxNum = 0)
    {
        var list = _Data.GetItemList(type, boxNum);
        if (list == null) return false;

        int totalAmount = list
            .Where(item => item.GetItemID == id && item.GetGrade == grade)
            .Sum(item => (int)item.GetAmount);
        return totalAmount >= count;
    }

    public void CalculateMoney()
    {
        var sellingBox = _Data.GetItemList(ContainerType.SELLING);
        if (sellingBox == null) return;

        int totalMoney = sellingBox.Sum(item => GlobalItemDB.GetPrice((short)item.GetItemID) * item.GetAmount);
        _Data.SetItemList(ContainerType.SELLING, new List<ItemObjectData>(new ItemObjectData[50]));
        _Data.AddMoney(totalMoney);

        GlobalEventManager.InvokeDataChanged();
        Debug.Log($"하루가 지나 판매 완료. 총 수익: {totalMoney}골드");
    } 
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct ItemInstantData
{
    [SerializeField] private int money;
    [SerializeField] private int reputation;
    [SerializeField] private List<ItemObjectData> invenList;
    [SerializeField] private List<StorageBox> storageBoxList;

    [NonSerialized] private List<ItemObjectData> sellingBox;

    // 인벤토리나 특정 창고 박스를 IList 형태로 반환 (배열과 리스트 공통 처리)
    public IList<ItemObjectData> GetItemList(ContainerType type, int boxNum = 0)
    {
        if (type == ContainerType.INVENTORY) return invenList;
        if (type == ContainerType.STORAGE) return storageBoxList[boxNum].BoxSlots;
        if (type == ContainerType.SELLING) return sellingBox;
        return null;
    }

    public int GetMoney => this.money;
    public int GetReputation => this.reputation;

    // // 하위 호환성을 위한 메서드 (기본 박스 혹은 인벤토리 리스트 반환)
    // public List<ItemObjectData> GetInvenList(ContainerType type)
    // {
    //     if (type == ContainerType.INVENTORY) return invenList;
    //     if (storageBoxList != null && storageBoxList.Count > 0) return storageBoxList[0].BoxSlots.ToList();
    //     return new List<ItemObjectData>();
    // }

    public List<StorageBox> GetStorageBoxes => storageBoxList;

    public void SetItemList(ContainerType type, List<ItemObjectData> itemList)
    {
        if (type == ContainerType.INVENTORY) invenList = itemList;
        else if (storageBoxList != null && storageBoxList.Count > 0) storageBoxList[0].SetBoxSlots(itemList.ToArray());
    }

    public void AddMoney(int money) => this.money += money;

    public void SwapItem(ContainerType startPoint, ContainerType endPoint, int startIdx, int endIdx, int startBoxNum = 0, int endBoxNum = 0)
    {
        IList<ItemObjectData> target1 = GetItemList(startPoint, startBoxNum);
        IList<ItemObjectData> target2 = GetItemList(endPoint, endBoxNum);

        if (target1 == null || target2 == null || target1.Count <= startIdx || target2.Count <= endIdx)
        {
            Debug.LogError("<b> [에러(Swap)] </b> 인덱스가 범위를 초과했거나 대상 리스트가 없음");
            return;
        }

        ItemObjectData temp = target1[startIdx];
        target1[startIdx] = target2[endIdx];
        target2[endIdx] = temp;
    }

    public void AddItem(ContainerType type, ItemObjectData item, int boxNum = 0)
    {
        IList<ItemObjectData> targetList = GetItemList(type, boxNum);
        if (targetList == null) return;

        // 1. 같은 아이템이 있고 겹칠 수 있는 슬롯 확인
        for (int i = 0; i < targetList.Count; i++)
        {
            var curItem = targetList[i];
            if (curItem.GetItemID == item.GetItemID && !curItem.CheckFull())
            {
                curItem.AddAmount(item.GetAmount);
                targetList[i] = curItem;
                return;
            }
        }

        // 2. 빈 슬롯 확인
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].GetItemID == 0)
            {
                targetList[i] = item;
                return;
            }
        }

        Debug.Log($"[{type} Box:{boxNum}] 슬롯 가득 참");
    }

    public void SortList(ContainerType type, int boxNum = 0)
    {
        var targetList = GetItemList(type, boxNum);
        if (targetList == null) return;

        var sorted = targetList
            .OrderByDescending(item => item.GetItemID != 0)
            .ThenBy(item => item.GetItemID)
            .ThenByDescending(item => item.GetAmount)
            .ToList();

        for (int i = 0; i < targetList.Count; i++)
        {
            targetList[i] = sorted[i];
        }
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct StorageBox
{
    [SerializeField] private string boxName;
    [SerializeField] private ItemObjectData[] boxSlots;

    public ItemObjectData[] BoxSlots => boxSlots;
    public string BoxName => boxName;

    public void SetBoxName(string boxName) => this.boxName = boxName;
    public void SetBoxSlots(ItemObjectData[] itemObjectDatas) => this.boxSlots = itemObjectDatas;
}
