using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData")]
public class ItemIdData : ScriptableObject
{
    public int itemID;

    [Header("기본 정보")]
    [SerializeField] protected string itemName;
    [SerializeField] protected string description;
    [SerializeField] protected string spriteAddress;

    public string ItemName => itemName;
    public string Description => description;
    public string SpriteAddress => spriteAddress;



    [Header("상태 설정")]
    [SerializeField] protected bool isStackable; // 저장 여부만 기록  (최대 스택은 각 저장 위치에서)
    [SerializeField] protected bool canDump;
    [SerializeField] protected bool canSold;
    [SerializeField] protected int maxLevel;

    public bool IsStackable => isStackable;
    public bool CanDump => canDump;
    public bool CanSold => canSold;

    [Header("차징 설정")]
    [SerializeField] protected int maxCharging;
    [SerializeField] protected int chargeTime1st;
    [SerializeField] protected int chargeTime2nd;
    [SerializeField] protected int chargeTime3rd;


    public int MaxLevel => maxLevel;
    public int MaxCharging => maxCharging;
    public int ChargeTime1st => chargeTime1st;
    public int ChargeTime2nd => chargeTime2nd;
    public int ChargeTime3rd => chargeTime3rd;
}


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData")]
public class ItemDetailData: ScriptableObject
{
    public int itemID;

    [Header("기본 정보")]
    [SerializeField] protected string itemName;
    [SerializeField] protected string description;
    [SerializeField] protected string spriteAddress;

    public string ItemName => itemName;
    public string Description => description;
    public string SpriteAddress => spriteAddress;



    [Header("상태 설정")]
    [SerializeField] protected bool isStackable; // 저장 여부만 기록  (최대 스택은 각 저장 위치에서)
    [SerializeField] protected bool canDump;
    [SerializeField] protected bool canSold;
    [SerializeField] protected int maxLevel;

    public bool IsStackable => isStackable;
    public bool CanDump => canDump;
    public bool CanSold => canSold;

    [Header("차징 설정")]
    [SerializeField] protected int maxCharging;
    [SerializeField] protected int chargeTime1st;
    [SerializeField] protected int chargeTime2nd;
    [SerializeField] protected int chargeTime3rd;


    public int MaxLevel => maxLevel;
    public int MaxCharging => maxCharging;
    public int ChargeTime1st => chargeTime1st;
    public int ChargeTime2nd => chargeTime2nd;
    public int ChargeTime3rd => chargeTime3rd;
}