using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowerItemData", menuName = "ItemData/FlowerItemData")]
public class FlowerItemData : ScriptableObject
{
    [SerializeField] private List<FlowerItemAuthoringData> flowers = new();

    public IReadOnlyList<FlowerItemAuthoringData> Flowers => flowers;

    public int Count => flowers?.Count ?? 0;

    public FlowerItemAuthoringData Get(int index)
    {
        return flowers[index];
    }
}

[Serializable]
public struct FlowerItemAuthoringData
{
    [Header("ItemBaseData에 존재하는 ItemId")]
    public int itemId;

    [Header("꽃 구성")]
    public FlowerSpecie species;
    public FlowerColor color;
    public FlowerFlorio florio1;
    public FlowerFlorio florio2;

    [Header("재배 정보")]
    public int growthDuration;
    public int harvestAmount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FlowerItemBlobData
{
    public int ItemId;

    public FlowerSpecie Species;
    public FlowerColor Color;
    public FlowerFlorio Florio1;
    public FlowerFlorio Florio2;

    public int GrowthDuration;
    public int HarvestAmount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FlowerItemBlobDatas
{
    public BlobArray<FlowerItemBlobData> Items;
}