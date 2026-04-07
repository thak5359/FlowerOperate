using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerateManager : MonoBehaviour
{
    private static ItemGenerateManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(instance);
    }

    public static ItemGenerateManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public void GenItem(ushort id, short amount, short duration, byte grade)
    {
        ItemDataContainer item = ObjectPool.GetObject();
        item.SetData(new ItemObjectData(id, amount, duration, grade));
    }
}