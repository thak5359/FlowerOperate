using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


[CreateAssetMenu(fileName = "UsableDetailData", menuName = "UsableData/DetailData")]

public class UsableDetailData : ItemDetailData
{
    [SerializeField] public List<int> durationList;
    [SerializeField] public List<int> powerList;
    [SerializeField] public List<ChargeInfo> chargeInfoList = new List<ChargeInfo>();

    public int Duration(int index) => durationList[index];
    public int Power(int index) => powerList[index];
    public ChargeInfo ChargeInfo(int index) => chargeInfoList[index];
}

public struct UsableDetailBlobData
{
    public short duration;
    public byte power;
    public ChargeInfo chargeInfo;
}

public struct UsableDetailBlobDatas
{
    public BlobArray<UsableDetailBlobData> usableDetails;
}