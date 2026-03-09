using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "IdData", menuName = "ItemData/IdData")]
public class ItemIdData : ScriptableObject
{

    [Header("기본 정보")]
    private int itemID;
    [SerializeField] protected List<string> itemName;
    [SerializeField] protected List<string> description;
    [SerializeField] protected List<string> spriteAddress;

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
    [SerializeField] protected int speciesIndex;
    [SerializeField] protected int colorIndex;
    [SerializeField] protected List<int> floroIndex; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리

    public int SpeciesIndex => speciesIndex;
    public int ColorIndex => colorIndex;
    public List<int> FloroIndex => floroIndex;
}


[CreateAssetMenu(fileName = "FlowerData", menuName = "FlowerData/DetailData")]
public class FlowerDetailData : ItemDetailData
{
    [SerializeField] protected List<string> speciesList;
    [SerializeField] protected List<string> colorList;
    [SerializeField] protected List<string> floroList;

    public string Species(int index) => speciesList[index];
    public string Color(int index) => colorList[index];
    public string Floro(int index) => floroList[index];
}


[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]

public class UsableIdData : ItemIdData
{
    [SerializeField] protected int durationIndex;
    [SerializeField] protected int chargeIndex;
    [SerializeField] protected int powerIndex;

    public int DuratIndex => durationIndex;
    public int ChargeIndex => chargeIndex;
    public int PowerIndex => powerIndex;


}

[CreateAssetMenu(fileName = "UsableDetailData", menuName = "UsableData/DetailData")]

public class UsableDetailData : ItemDetailData
{
    [SerializeField] protected List<int> durationList;
    [SerializeField] protected List<int> powerList;
    [SerializeField] protected List<ChargeInfo> chargeInfoList;

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
