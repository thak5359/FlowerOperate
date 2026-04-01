using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataContainer : MonoBehaviour
{
    [SerializeField]
    ItemObjectData data;

    public ItemObjectData GetData => this.data;
    public ushort GetItemID => data.GetItemID;
    public byte GetAmount => data.GetAmount;
    public short GetDuration => data.GetDuration;
    public byte GetGrade => data.GetGrade;

    public void SetData(ItemObjectData data) => this.data = data;

    public void AddAmount(byte amount) => data.SetAmount((byte)(GetAmount + amount));
    
}

[System.Serializable]
public struct ItemObjectData
{
    [SerializeField] ushort itemID;
    [SerializeField] byte amount;
    [SerializeField] short Duration;
    [SerializeField] byte grade;

    //게터
    public ushort GetItemID => itemID;
    public byte GetAmount => amount;
    public short GetDuration => Duration;
    public byte GetGrade => grade;

    //세터
    public void SetItemID(ushort itemID) => this.itemID = itemID;
    public void SetAmount(byte amount) => this.amount = amount;
    public void SetDuration(short Dur) => this.Duration = Dur;
    public void SetGrade(byte grade) => this.grade = grade;

    public void AddAmount(byte amount) => this.amount += amount;

    public ItemObjectData(ushort itemID, byte amount, short duration, byte grade)
    {
        this.itemID = itemID;
        this.amount = amount;
        Duration = duration;
        this.grade = grade;
    }
}

