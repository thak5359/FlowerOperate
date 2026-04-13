using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[CreateAssetMenu(fileName = "UsableDetailData", menuName = "UsableData/DetailData")]

public class UsableDetailData : ItemDetailData
{
    [SerializeField] public List<short> durationList;
    [SerializeField] public List<short> powerList;
    [SerializeField] public List<ChargeInfo> chargeInfoList = new List<ChargeInfo>();

    public short Duration(byte index) => durationList[index];
    public short Power(byte index) => powerList[index];
    public ChargeInfo ChargeInfo(byte index) => chargeInfoList[index];
}

public struct UsableDetailBlobData
{
    public byte index;
    public short duration;
    public short power;
    public ChargeInfo chargeInfo;
}

public struct UsableDetailBlobDatas
{
    public BlobArray<UsableDetailBlobData> usableDetails;
}