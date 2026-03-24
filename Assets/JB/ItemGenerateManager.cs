using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerateManager : MonoBehaviour
{
    int id;
    int amount;
    int duration;
    int grade;

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

    public void GenItem()
    {
        Item item = ObjectPool.GetObject();
        item.SetData(new ItemObjectData(id, amount, duration, grade));
    }
}
