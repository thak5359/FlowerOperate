using MemoryPack;
using System;
using System.Collections.Generic;
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
public class PlayerOwnItemDataManager : MonoBehaviour
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
        GlobalEventManager.NextDay += CalculateMoneyInSellingBox;
    }

    void OnDisable() // IDisposable로 전환
    {
        GlobalEventManager.OnItemPickedUp -= AddItem;
        GlobalEventManager.NextDay -= CalculateMoneyInSellingBox;
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
            _Data.SetItemList(type, ItemInstantData.CreateEmptyItemList(50));
        }
        else if (type == ContainerType.SELLING)
        {
            _Data.SetItemList(type, ItemInstantData.CreateEmptyItemList(50));
        }
        else
        {
            _Data.SetItemList(type, ItemInstantData.CreateEmptyItemList(50), boxNum);
        }
        GlobalEventManager.InvokeDataChanged();
    }

    public virtual void Swap(ContainerType startPoint, ContainerType endPoint,
    int startIdx, int endIdx, int startBoxNum = 0, int endBoxNum = 0)
    {
        _Data.SwapItem(startPoint, endPoint, startIdx, endIdx, startBoxNum, endBoxNum);
        GlobalEventManager.InvokeDataChanged();
        Debug.Log($"{startPoint}[{startIdx}] <-> {endPoint}[{endIdx}] 아이템 스왑 (Box: {startBoxNum}/{endBoxNum})");
    }

    public virtual void EngraftItem(GameItem a, GameItem b)
    {
        if (!ItemInstantData.CanStack(a, b))
            return;

        int amountToMove = Mathf.Min(a.GetRemainStackSpace(), b.Count);

        a.Count += amountToMove;
        b.Count -= amountToMove;

        GlobalEventManager.InvokeDataChanged();
    }

    public void Sort() => Sort(currentType, _currentBoxIndex);

    public void Sort(ContainerType type, int boxNum = 0)
    {
        _Data.SortList(type, boxNum);

        var list = _Data.GetItemList(type, boxNum);
        if (list == null) return;

        for (int i = 0; i < list.Count - 1; i++)
        {
            EngraftItem(list[i], list[i + 1]);

            if (ItemInstantData.IsEmpty(list[i + 1]))
                list[i + 1] = null;
        }
        _Data.SortList(type, boxNum);
        GlobalEventManager.InvokeDataChanged();
    }

    protected virtual void AddItem(GameItem item) => AddItem(currentType, item, _currentBoxIndex);

    public virtual void AddItem(ContainerType type, GameItem item, int boxNum = 0)
    {
        _Data.AddItem(type, item, boxNum);
        GlobalEventManager.InvokeDataChanged();
    }

    public bool RemoveItem(ContainerType type, int id, byte grade, int count, int boxNum = 0)
    {
        if (!HasItem(type, id, grade, count, boxNum)) return false;

        var list = _Data.GetItemList(type, boxNum);
        int remainingToRemove = count;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item.Id == id && item.GetGrade == grade)
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

    public void CalculateMoneyInSellingBox()
    {
        var sellingBox = _Data.GetItemList(ContainerType.SELLING);
        if (sellingBox == null) return;

        int totalMoney = sellingBox
            .Where(item => !ItemInstantData.IsEmpty(item))
            .Sum(item => GlobalItemDB.GetPrice((short)item.Id) * item.Count);

        _Data.SetItemList(ContainerType.SELLING, ItemInstantData.CreateEmptyItemList(50));
        _Data.AddMoney(totalMoney);

        GlobalEventManager.InvokeDataChanged();
        Debug.Log($"하루가 지나 판매 완료. 총 수익: {totalMoney}골드");
    }
}

[MemoryPackable]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct ItemInstantData
{
    [MemoryPackInclude, SerializeField] private int money;
    [MemoryPackInclude, SerializeField] private int reputation;
    [MemoryPackInclude, SerializeField] private List<GameItem> invenList;
    [MemoryPackInclude, SerializeField] private List<StorageBox> storageBoxList;

    [MemoryPackIgnore] private List<GameItem> sellingBox;


    public static List<GameItem> CreateEmptyItemList(int size)
    {
        return Enumerable.Repeat<GameItem>(null, size).ToList();
    }

    public void EnsureSlotLists()
    {
        invenList ??= CreateEmptyItemList(50);
        sellingBox ??= new List<GameItem>();
        storageBoxList ??= new List<StorageBox>();
    }


    // 인벤토리나 특정 창고 박스를 IList 형태로 반환 (배열과 리스트 공통 처리)
    public IList<GameItem> GetItemList(ContainerType type, int boxNum = 0)
    {
        EnsureSlotLists();

        if (type == ContainerType.INVENTORY)
            return invenList;

        if (type == ContainerType.SELLING)
            return sellingBox;

        if (type == ContainerType.STORAGE)
        {
            if (boxNum < 0 || boxNum >= storageBoxList.Count)
            {
                Debug.LogError($"<b>[에러(GetItemList)]</b> StorageBox index out of range: {boxNum}");
                return null;
            }

            return storageBoxList[boxNum].BoxSlots;
        }

        return null;
    }

    public int GetMoney => this.money;
    public int GetReputation => this.reputation;
    public List<StorageBox> GetStorageBoxes => storageBoxList;



    public void SetItemList(ContainerType type, List<GameItem> itemList, int boxNum = 0)
    {
        EnsureSlotLists();

        if (type == ContainerType.INVENTORY)
        {
            invenList = itemList;
            return;
        }

        if (type == ContainerType.SELLING)
        {
            sellingBox = itemList;
            return;
        }

        if (type == ContainerType.STORAGE)
        {
            if (boxNum < 0 || boxNum >= storageBoxList.Count)
            {
                Debug.LogError($"<b>[에러(SetItemList)]</b> StorageBox index out of range: {boxNum}");
                return;
            }

            StorageBox box = storageBoxList[boxNum];
            box.SetBoxSlots(itemList);
            storageBoxList[boxNum] = box;
        }
    }

    public void AddMoney(int money) => this.money += money;

    public void SwapItem(
         ContainerType startPoint,
         ContainerType endPoint,
         int startIdx,
         int endIdx,
         int startBoxNum = 0,
         int endBoxNum = 0)
    {
        IList<GameItem> target1 = GetItemList(startPoint, startBoxNum);
        IList<GameItem> target2 = GetItemList(endPoint, endBoxNum);

        if (target1 == null || target2 == null ||
            startIdx < 0 || endIdx < 0 ||
            startIdx >= target1.Count || endIdx >= target2.Count)
        {
            Debug.LogError("<b>[에러(Swap)]</b> 인덱스가 범위를 초과했거나 대상 리스트가 없음");
            return;
        }

        (target1[startIdx], target2[endIdx]) = (target2[endIdx], target1[startIdx]);
    }

    public void AddItem(ContainerType type, GameItem item, int boxNum = 0)
    {
        if (IsEmpty(item))
            return;

        IList<GameItem> targetList = GetItemList(type, boxNum);
        if (targetList == null)
            return;

        // 1. 같은 아이템의 기존 스택에 먼저 합침.
        for (int i = 0; i < targetList.Count; i++)
        {
            GameItem curItem = targetList[i];

            if (!CanStack(curItem, item))
                continue;

            int moveAmount = Mathf.Min(curItem.GetRemainStackSpace(), item.Count);
            curItem.Count += moveAmount;
            item.Count -= moveAmount;

            if (item.Count <= 0)
                return;
        }

        // 2. 빈 슬롯에 남은 아이템을 넣음.
        for (int i = 0; i < targetList.Count; i++)
        {
            if (!IsEmpty(targetList[i]))
                continue;

            targetList[i] = item;
            return;
        }

        // 3. 인벤토리는 5x10 고정 크기라 임의 확장하지 않음.
        Debug.Log($"[{type} Box:{boxNum}] 슬롯 가득 참");
    }

    public void SortList(ContainerType type, int boxNum = 0)
    {
        IList<GameItem> targetList = GetItemList(type, boxNum);
        if (targetList == null)
            return;

        List<GameItem> sorted = targetList
            .OrderByDescending(item => !IsEmpty(item))
            .ThenBy(item => IsEmpty(item) ? int.MaxValue : item.Id)
            .ThenByDescending(item => IsEmpty(item) ? 0 : item.Count)
            .ToList();

        for (int i = 0; i < targetList.Count; i++)
            targetList[i] = sorted[i];
    }

    public static bool IsEmpty(GameItem item)
    {
        return item == null || item.Id <= 0 || item.Count <= 0;
    }

    public static bool CanStack(GameItem a, GameItem b)
    {
        return !IsEmpty(a)
            && !IsEmpty(b)
            && a.Id == b.Id
            && a.GetRemainStackSpace() > 0;
    }



}

[MemoryPackable]
[StructLayout(LayoutKind.Sequential)]
public partial struct StorageBox
{
    [MemoryPackInclude] private string boxName;
    [MemoryPackInclude] private ItemObjectData[] boxSlots;

    public ItemObjectData[] BoxSlots => boxSlots;
    public string BoxName => boxName;

    public void SetBoxName(string boxName) => this.boxName = boxName;
    public void SetBoxSlots(ItemObjectData[] itemObjectDatas) => this.boxSlots = itemObjectDatas;
}
