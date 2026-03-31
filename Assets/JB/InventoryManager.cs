using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    [SerializeField]
    GameObject slotPrefab;
    [SerializeField]
    GameObject savePrefab;

    //Getter
    public ItemStorageData GetInvenData => _data;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        SaveLoadManager.OnSaveData += Initialize;
    }

    private void OnDisable()
    {
        SaveLoadManager.OnSaveData -= Initialize;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _data = savePrefab.GetComponent<SaveLoadManager>().save.GetInvenData;
        Debug.Log("초기화 끝");
    }
}
