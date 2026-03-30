using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InventoryManager : ItemStorageParent
{
    [SerializeField]
    GameObject slotPrefab;
    string InvenJson;

    private void Awake()
    {
        this.Initialize();
        InvenJson = JsonUtility.ToJson(_data);
        string path = Path.Combine(Application.dataPath, "Savedata.json");
        File.WriteAllText(path, InvenJson);
    }

    protected override void Initialize()
    {
        base.Initialize();
        foreach(var slot in this.GetComponentsInChildren<ItemDataContainer>())
        {
            _data.AddItem(slot.GetData);
        }
    }
}
