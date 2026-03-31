using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    [SerializeField]
    GameObject slotPrefab;

    //Getter
    public ItemStorageData GetInvenData => _data;

    private void Awake()
    {
        this.Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }
}
