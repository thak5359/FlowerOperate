using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    List<SlotItem> slots = new List<SlotItem>();
    
    int minCapacity = 30;
    int curCapacity;

    public void AddItemOnSlot(int id)
    {
        for(int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemId == id)
            {
                slots[i] = new SlotItem(id, slots[i].Amount);
                return;
            }
        }
        slots.Add(new SlotItem(id, 0));
    }

    void RemoveItemOnSlot()
    {

    }

}
