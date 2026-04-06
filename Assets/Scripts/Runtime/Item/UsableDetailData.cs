using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[CreateAssetMenu(fileName = "UsableDetailData", menuName = "UsableData/DetailData")]

public class UsableDetailData : ItemDetailData
{
    [SerializeField] public List<short> durationList;
    [SerializeField] public List<byte> powerList;
    [SerializeField] public List<ChargeInfo> chargeInfoList = new List<ChargeInfo>();

    public short Duration(byte index) => durationList[index];
    public byte Power(byte index) => powerList[index];
    public ChargeInfo ChargeInfo(byte index) => chargeInfoList[index];
}

public struct UsableDetailBlobData
{
    public byte index;
    public short duration;
    public sbyte power;
    public ChargeInfo chargeInfo;
}

public struct UsableDetailBlobDatas
{
    public BlobArray<UsableDetailBlobData> usableDetails;
}