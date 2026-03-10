using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "IdData", menuName = "ItemData/IdData")]
public class ItemIdData : ScriptableObject
{

    [Header("기본 정보")]
    public int itemID;
    [SerializeField] public List<string> itemName;
    [SerializeField] public List<string> description;
    [SerializeField] public List<string> spriteAddress;

    public int ItemID => itemID;
    public string ItemName(int i) => itemName[i];
    public string Description(int i) => description[i];
    public string Address(int i) => spriteAddress[i];
}

public class ItemDetailData : ScriptableObject
{

}

[CreateAssetMenu(fileName = "FlowerIdData", menuName = "FlowerData/IdData")]
public class FlowerIdData : ItemIdData
{
    [Header("꽃의 구성 인덱스 [품종 번호, 색상 번호, 꽃말 번호]")]
    [SerializeField] public int speciesIndex;
    [SerializeField] public int colorIndex;
    [SerializeField] public List<int> floroIndex; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리

    public int SpeciesIndex => speciesIndex;
    public int ColorIndex => ColorIndex;
    public List<int> FloroIndex => floroIndex;
}


[CreateAssetMenu(fileName = "FlowerData", menuName = "FlowerData/DetailData")]
public class FlowerDetailData : ItemDetailData
{
    [SerializeField] public List<string> speciesList;
    [SerializeField] public List<string> colorList;
    [SerializeField] public List<string> floroList;

    public string Species(int index) => speciesList[index];
    public string Color(int index) => colorList[index];
    public string Floro(int index) => floroList[index];
}


[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]

public class UsableIdData : ItemIdData
{
    [SerializeField] public int durationIndex;
    [SerializeField] public int chargeIndex;
    [SerializeField] public int powerIndex;

    public int DuratIndex => durationIndex;
    public int ChargeIndex => chargeIndex;
    public int PowerIndex => powerIndex;


}

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
