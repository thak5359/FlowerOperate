using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataContainer : MonoBehaviour
{
    [SerializeField]
    ItemObjectData data;

    public int GetItemID => data.GetItemID;
    public int GetAmount => data.GetAmount;
    public int GetDuration => data.GetDuration;
    public int GetGrade => data.GetGrade;

    public void SetData(ItemObjectData data) => this.data = data;

    public void AddAmount(int amount) => data.SetAmount(GetAmount + amount);
}

