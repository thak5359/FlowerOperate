using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] Inventory inventory = new Inventory();
    public List<ItemDataContainer> slots;
    private int curSlotCount;
    private string invenJson;

    private void Awake()
    {
    }

    
}


