using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemGenerateManager
{
    public static void GenItem(ushort id, short amount, short duration, byte grade)
    {
        GameObject obj = ObjectPool.GetObject(id.ToString());
        if (obj != null)
        {
            ItemDataContainer item = obj.GetComponent<ItemDataContainer>();
            if (item != null)
            {
                item.SetData(new ItemObjectData(id, amount, duration, grade));
            }
        }
    }
    public static void GenItem(ItemObjectData data)
    {
        GameObject obj = ObjectPool.GetObject(data.GetItemID.ToString());
        if (obj != null)
        {
            ItemDataContainer item = obj.GetComponent<ItemDataContainer>();
            if (item != null)
            {
                item.SetData(data);
            }
        }
    }
}