using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : ItemStorageParent
{
    public ItemStorageData GetStorageData => _data;
    private void Awake()
    {
        Initialize();
    }


}
