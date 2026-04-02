using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using static UnityEditor.Progress;

[Serializable]
public class ItemStorageParent : MonoBehaviour
{
    [SerializeField]
    protected ItemStorageData _data = new ItemStorageData();

    //Getter
    public ItemStorageData GetData => _data;

    protected virtual void Initialize()
    {
        if (_data.GetList != null)
            _data.ClearList();
        _data.SetItemList(new List<ItemObjectData>(new ItemObjectData[_data.GetSlotsCount]));
        _data.SetSlotsCount(0);
        
    }

    public virtual void Swap(int idx1, int idx2)
    {
        //슬롯의 아이템 스프라이트 변경 로직 넣어주세요.
        _data.SwapItem(idx1, idx2);
        Debug.Log($"{idx1}번과 {idx2}번 슬롯의 아이템 위치 스왑");
    }

    public virtual void Combine()
    {

    }

    public void AbandonItem()
    {

    }

    protected virtual void AddItem(ItemObjectData item)
    {
        _data.AddItem(item);
        //슬롯의 숫자UI 변경 로직 넣어주세요.
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
            itemListData.Add(item);
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
            start.SetAmount((sbyte)(totalAmount - 100));
        }
        else
        {
            target.SetAmount((sbyte)totalAmount);
            start.SetAmount(0); // 합쳐졌으므로 시작 아이템은 0개가 되어야 함
        }
    }
}