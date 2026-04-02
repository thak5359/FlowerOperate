using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    [SerializeField] List<ItemDataContainer> slotList;
    [SerializeField] GameObject slotObject;

    private void Awake()
    {
        slotList = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
    }

    private void OnEnable()
    {
        SaveLoadManager.OnLoadData += this.Initialize;
    }

    private void OnDisable()
    {
        SaveLoadManager.OnLoadData -= this.Initialize;
    }

    private void Initialize(SaveDatas save)
    {
        base.Initialize();
        _data = save.GetInvenData;
        for (int i = 0; i < _data.GetList.Count; i++)
        {
            if (i >= 30)
            {
                var newSlot = Instantiate(slotObject, this.gameObject.transform);
                slotList.Add(newSlot.GetComponent<ItemDataContainer>());
            }
            slotList[i].SetData(_data.GetList[i]);
        }
        Debug.Log("초기화 끝");
    }
}
