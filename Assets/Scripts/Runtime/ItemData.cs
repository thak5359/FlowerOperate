using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "IdData", menuName = "ItemData/IdData")]
public class ItemIdData : ScriptableObject
{

    [Header("기본 정보")]
    [SerializeField] public List<string> itemName;
    [SerializeField] public List<string> description;
    [SerializeField] public List<string> spriteAddress;

    public string ItemName(int i) => itemName[i];
    public string Description(int i) => description[i];
    public string Address(int i) => spriteAddress[i];
}

public class ItemDetailData : ScriptableObject
{

}

public struct ChargeInfo
{
    public readonly float ChargeTime;
    public readonly int maxChargeCount;

    public ChargeInfo(float time, int count)
    {
        ChargeTime = time;
        maxChargeCount = count;
    }

    
}
