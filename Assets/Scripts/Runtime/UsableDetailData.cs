using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "UsableDetailData", menuName = "UsableData/DetailData")]

public class UsableDetailData : ItemDetailData
{
    [SerializeField] public List<int> durationList;
    [SerializeField] public List<int> powerList;
    [SerializeField] public List<ChargeInfo> chargeInfoList;

    public int Duration(int index) => durationList[index];
    public int Power(int index) => powerList[index];
    public ChargeInfo ChargeInfo(int index) => chargeInfoList[index];
}
