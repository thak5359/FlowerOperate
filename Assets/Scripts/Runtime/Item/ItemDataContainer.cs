using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataContainer : MonoBehaviour
{
    [SerializeField]
    ItemObjectData data;

    public ItemObjectData GetData => this.data;
    public ushort GetItemID => data.GetItemID;
    public sbyte GetAmount => data.GetAmount;
    public short GetDuration => data.GetDuration;
    public byte GetGrade => data.GetGrade;

    public void SetData(ItemObjectData data) => this.data = data;

    public void AddAmount(sbyte amount) => data.SetAmount((sbyte)(GetAmount + amount));
    
    
}

[System.Serializable]
public struct ItemObjectData
{
    [SerializeField] ushort itemID;
    [SerializeField] short Duration;
    [SerializeField] sbyte amount;
    [SerializeField] byte grade;

    //게터
    public ushort GetItemID => itemID;
    public sbyte GetAmount => amount;
    public short GetDuration => Duration;
    public byte GetGrade => grade;

    //세터
    public void SetItemID(ushort itemID) => this.itemID = itemID;
    public void SetAmount(sbyte amount) => this.amount = amount;
    public void SetDuration(short Dur) => this.Duration = Dur;
    public void SetGrade(byte grade) => this.grade = grade;

    public void AddAmount(sbyte amount) => this.amount += amount;
    public bool CheckFull()
    {
        // 스택이 Full인지 Zero인지 판단하는 함수
        if(amount == 100)
            return true;
        return false;
    }

    public ItemObjectData(ushort itemID, sbyte amount, short duration, byte grade)
    {
        this.itemID = itemID;
        this.amount = amount;
        Duration = duration;
        this.grade = grade;
    }
}

