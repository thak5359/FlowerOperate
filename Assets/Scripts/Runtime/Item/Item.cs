using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    ItemObjectData data;

    public int GetItemID => data.GetItemID;
    public int GetAmount => data.GetAmount;
    public int GetDuration => data.GetDuration;
    public int GetGrade => data.GetGrade;
}

