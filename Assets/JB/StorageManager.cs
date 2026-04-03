using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : ItemStorageParent
{
    private void Awake()
    {
        //Initialize();
        
    }

    public void SortList()
    {
        GetData.GetList.Sort((ItemObjectData a, ItemObjectData b) => {
            //2차 : ID 오름차순
            return a.GetItemID.CompareTo(b.GetItemID);
        });
    }
}
