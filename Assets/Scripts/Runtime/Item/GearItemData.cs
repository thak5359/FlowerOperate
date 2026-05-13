using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "GearItemData", menuName = "ItemData/GearItemData")]
public class GearItemData : ScriptableObject
{
    [SerializeField] private List<GearItemAuthoringData> gears = new();

    public IReadOnlyList<GearItemAuthoringData> Gears => gears;
    public void setGears(List<GearItemAuthoringData> gears) => this.gears = gears;
    public int Count => gears?.Count ?? 0;

    public GearItemAuthoringData Get(int index)
    {
        return gears[index];
    }
}

[Serializable]
public struct GearItemAuthoringData
{
    [Header("ItemBaseData에 존재하는 ItemId")]
    public int itemId;

    [Header("장비 분류")]
    public GearType gearType;

    [Header("장비 성능")]
    public GearMaxDuration maxDurability;
    public GearEfficiency efficiency;
    public GearChargeTime chargeTime;
    public GearMaxCharge maxCharge;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GearItemBlobData
{
    public int ItemId;

    public GearType GearType;

    public GearMaxDuration MaxDuration;
    public GearEfficiency Efficiency;
    public GearChargeTime ChargeTime;
    public GearMaxCharge MaxCharge;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GearItemBlobDatas
{
    public BlobArray<GearItemBlobData> Items;
}