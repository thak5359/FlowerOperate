using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemGenerateManager
{
    public static void GenItem(ushort id, short amount, short duration, byte grade)
    {
        ItemDataContainer item = ObjectPool.GetObject();
        item.SetData(new ItemObjectData(id, amount, duration, grade));
    }
    public static void GenItem(ItemObjectData data)
    {
        ItemDataContainer item = ObjectPool.GetObject();
        item.SetData(data);
    }
}