using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

[CreateAssetMenu(fileName = "IdData", menuName = "ItemData/IdData")]
public class ItemIdData : ScriptableObject
{

    [Header("기본 정보")]
    [SerializeField] public int startId;
    [SerializeField] public List<string> itemName;
    [SerializeField] public List<string> description;
    [SerializeField] public List<string> spriteAddress;



    public string ItemName(int i) => itemName[i];
    public string Description(int i) => description[i];
    public string Address(int i) => spriteAddress[i];
}


public struct ItemBlobData
{
    public int ItemId;
    public BlobString ItemName;
    public BlobString Description;
    public BlobString SpriteAddress;
}

public struct ItemBlobDatas
{
    public BlobArray<ItemBlobData> Items;
}


public class ItemDetailData : ScriptableObject
{

}


[System.Serializable]
public struct ChargeInfo
{
    public float ChargeTime;
    public int maxChargeCount;

    public ChargeInfo(float time, int count)
    {
        ChargeTime = time;
        maxChargeCount = count;
    }

    public void ReadValue()
    {
        Debug.Log($"chargeTime : ${ChargeTime}, maxChargeCont : ${maxChargeCount}");
    }
}
