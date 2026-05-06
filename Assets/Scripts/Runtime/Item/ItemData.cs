using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using System.Runtime.InteropServices;

[CreateAssetMenu(fileName = "IdData", menuName = "ItemData/IdData")]
public class ItemIdData : ScriptableObject
{

    [Header("기본 정보")]
    [SerializeField] public short startId;
    [SerializeField] public List<FixedString64Bytes> itemName;
    [SerializeField] public List<FixedString128Bytes> description;
    [SerializeField] public List<FixedString64Bytes> spriteAddress;
    [SerializeField] public List<short> price;

    public ItemIdData() => this.startId = Constant.COMMON_START_ID;

    public FixedString64Bytes ItemName(byte i) => itemName[i];
    public FixedString128Bytes Description(byte i) => description[i];
    public FixedString64Bytes Address(byte i) => spriteAddress[i];
    public short Price(byte i) => price[i];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemBlobData
{
    public short ItemId;
    public FixedString64Bytes ItemName;      
    public FixedString128Bytes Description; 
    public FixedString64Bytes SpriteAddress;
    public short Price;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemBlobDatas
{
    public BlobArray<ItemBlobData> Items;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ItemDetailData : ScriptableObject
{
}

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChargeInfo
{
    public float ChargeTime;
    public sbyte maxChargeCount;

    public ChargeInfo(float time, sbyte count)
    {
        ChargeTime = time;
        maxChargeCount = count;
    }

    public void ReadValue()
    {
        Debug.Log($"chargeTime : ${ChargeTime}, maxChargeCont : ${maxChargeCount}");
    }
}