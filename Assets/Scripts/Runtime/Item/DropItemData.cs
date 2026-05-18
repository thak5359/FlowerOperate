using Cysharp.Threading.Tasks;
using Fungus;
using MemoryPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class DropItemData : MonoBehaviour
{
    [SerializeField]
    ItemObjectData data;
    [SerializeField]
    GameItem _data;
    [SerializeField]
    int waitMilliSeconds = 1000;

    public ItemObjectData GetData => this.data;
    public ref ItemObjectData GetDataRef => ref this.data;
    public ushort GetItemID => data.GetItemID;
    public short GetAmount => data.GetAmount;
    public short GetDuration => data.GetDuration;
    public byte GetGrade => data.GetGrade;

    public void SetData(ItemObjectData data) => this.data = data;
    public void AddAmount(short amount) => data.SetAmount((short)(GetAmount + amount));
    

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(this.gameObject);
        }
        DoOnTrigger().Forget();
    }

    private async UniTaskVoid DoOnTrigger()
    {
        await UniTask.Delay(waitMilliSeconds);
        Debug.Log("딜레이 끝!");
        GlobalEventManager.InvokeItemPickedUp(data);
    }
}

[MemoryPackable]
[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct ItemObjectData /// ItemInstantData
{
    [MemoryPackInclude, SerializeField] ushort itemID;
    [MemoryPackInclude, SerializeField] short Duration;
    [MemoryPackInclude, SerializeField] short amount;
    [MemoryPackInclude, SerializeField] byte grade;

    //게터
    public ushort GetItemID => itemID;
    public short GetAmount => amount;
    public short GetDuration => Duration;
    public byte GetGrade => grade;

    //세터
    public void SetItemID(ushort itemID) => this.itemID = itemID;
    public void SetAmount(short amount) => this.amount = amount;
    public void SetDuration(short Dur) => this.Duration = Dur;
    public void SetGrade(byte grade) => this.grade = grade;

    public void AddAmount(short amount)
    {
        if(this.amount + amount > Constant.MAX_COUNT_INVENTORY)
        {
            this.amount = (short)Constant.MAX_COUNT_INVENTORY;
            return;
        }
        this.amount += amount;
    }
    public bool CheckFull()
    {
        // 스택이 Full인지 Zero인지 판단하는 함수
        if(amount == Constant.MAX_COUNT_INVENTORY)
            return true;
        return false;
    }

    public bool CheckEmpty()
    {
        if(amount <= 0) 
            return true;
        return false;
    }


    public ItemObjectData(ushort itemID = 0, short amount = 0, short duration = 0, byte grade = 0)
    {
        this.itemID = itemID;
        this.amount = amount;
        Duration = duration;
        this.grade = grade;
    }
}

