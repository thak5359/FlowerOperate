using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public interface IManager
{
    public void Load(SaveDatas saveDatas);
}

[Serializable]
public class ItemStorageParent : MonoBehaviour, IManager
{
    [SerializeField]
    protected ItemStorageData _data = new ItemStorageData();
    [SerializeField]
    protected GameObject slotObject;

    //Getter
    public ItemStorageData GetData => _data;

    protected virtual void Initialize(ItemStorageParent storageParent, ItemStorageData data, GameObject slotObject, ref List<ItemObjectData> slotList)
    {
        if (data == null || data.GetList == null)
        {
            Debug.LogWarning("불러온 데이터가 유효하지 않습니다.");
            return;
        }

        // 1. 데이터 교체 (ResetData를 먼저 하면 안됨)
        _data = data;
        slotList = _data.GetList;

        Debug.Log($"{storageParent.name} 초기화 끝 (아이템 수: {_data.GetList.Count})");
    }

    public virtual void Load(SaveDatas saveDatas) { }

    protected void ResetData()
    {
        // 리스트를 완전히 비우는 것이 아니라, 슬롯 수만큼 기본값으로 채웁니다.
        int count = (_data != null) ? _data.GetSlotsCount : 50;
        if (count <= 0) count = 50;

        List<ItemObjectData> emptyList = new List<ItemObjectData>();
        for (int i = 0; i < count; i++) emptyList.Add(default);

        if (_data == null) _data = new ItemStorageData();
        _data.SetItemList(emptyList);
    }

    public virtual void Swap(int idx1, int idx2)
    {
        //슬롯의 아이템 스프라이트 변경 로직 넣어주세요.
        _data.SwapItem(idx1, idx2);
        Debug.Log($"{idx1}번과 {idx2}번 슬롯의 아이템 위치 스왑");
    }

    // 아이템 합치기 함수
    public virtual void EngraftItem(ref ItemObjectData a, ref ItemObjectData b)
    {
        if (a.CheckFull() || b.CheckEmpty())
            return;

        int space = 100 - a.GetAmount;
        int amountToMove = Math.Min(space, (int)b.GetAmount);

        a.SetAmount((short)(a.GetAmount + amountToMove));
        b.SetAmount((short)(b.GetAmount - amountToMove));
    }

    public void AbandonItem()
    {

    }

    protected virtual void AddItem(ItemObjectData item)
    {
        _data.AddItem(item);
        //슬롯의 숫자UI 변경 로직 넣어주세요.
    }

    protected List<ItemObjectData> LoadChangedDataList(List<ItemDataContainer> changedDataList)
    {
        List<ItemObjectData> tempOD = new List<ItemObjectData>();
        foreach (ItemDataContainer data in changedDataList)
        {
            tempOD.Add(data.GetData);
        }
        return tempOD;
    }
}

[Serializable]
public class ItemStorageData
{
    [SerializeField]
    private List<ItemObjectData> itemListData;
    [SerializeField]
    private int slotsCount = 50;

    //Getter
    public int GetSlotsCount => slotsCount;
    public List<ItemObjectData> GetList => itemListData;

    //Setter
    public void SetItemList(List<ItemObjectData> itemList) => this.itemListData = itemList;
    public void SetSlotsCount(int slotsCount) => this.slotsCount = slotsCount;

    public void ClearList() => this.itemListData.Clear();
    public void SwapItem(int idx1, int idx2)
    {
        ItemObjectData temp = itemListData[idx1];
        itemListData[idx1] = itemListData[idx2];
        itemListData[idx2] = temp;
    }

    public void AddItem(ItemObjectData item)
    {
        int idx = itemListData.FindIndex(curItem => curItem.GetItemID == item.GetItemID);
        // 인벤토리에 같은 ID의 아이템이 있을 때
        if (idx != -1)
        {
            if (itemListData[idx].CheckFull())
            {
                itemListData.Add(item);
                return;
            }
            itemListData[idx].AddAmount(item.GetAmount);
            Debug.Log("Same Item");
        }
        // 획득한 아이템이 인벤에 없던 아이템 & 빈 슬롯이 존재할 때
        else if (itemListData.Any(data => data.Equals(null)))
        {
            itemListData[GetSlotsCount] = item;
            SetSlotsCount(GetSlotsCount + 1);
            Debug.Log("새 아이템");
        }
        // 첫 획득에 슬롯이 꽉 차있을 때
        else
        {
            Debug.Log("슬롯 꽉 참");
        }
    }

    // 함수 정의: 매개변수 앞에 ref를 붙입니다.
    public void CombineItem(ref ItemObjectData start, ref ItemObjectData target)
    {
        // 계산을 위해 임시 변수를 활용하는 것이 안전합니다.
        int totalAmount = start.GetAmount + target.GetAmount;

        if (totalAmount > 100)
        {
            target.SetAmount(100);
            start.SetAmount((short)(totalAmount - 100));
        }
        else
        {
            target.SetAmount((short)totalAmount);
            start.SetAmount(0); // 합쳐졌으므로 시작 아이템은 0개가 되어야 함
        }
    }
}