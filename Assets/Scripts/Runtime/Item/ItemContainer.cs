using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer
{
    protected List<ItemDataContainer> itemList;
    private int slotsCount;

    //Getter
    public int GetSlotsCount => slotsCount;

    //Setter
    public void SetItemList(List<ItemDataContainer> itemList) => this.itemList = itemList;
    public void SetSlotsCount(int slotsCount) => this.slotsCount = slotsCount;

    private void Initialize()
    {
        if (itemList != null)
            itemList.Clear();
    }

    public virtual void Swap(int idx1, int idx2)
    {
        ItemDataContainer temp = itemList[idx1];
        itemList[idx1] = itemList[idx2];
        itemList[idx2] = temp;
    }

    public void AbandonItem()
    {

    }
}

[System.Serializable]
public class Inventory : ItemContainer
{
    protected virtual void AddItem(ItemDataContainer item)
    {
        if (!itemList.Contains(item))
        {
            itemList[GetSlotsCount + 1] = item;
            SetSlotsCount(GetSlotsCount + 1);
        }
        else
        {
            int idx = itemList.IndexOf(item);
            itemList[idx].AddAmount(item.GetAmount);
        }
    }
}

public class Storage : ItemContainer
{

}