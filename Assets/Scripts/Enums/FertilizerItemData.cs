using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "FertilizerItemData", menuName = "ItemData/FertilizerItemData")]
public class FertilizerItemData : ScriptableObject
{
    [SerializeField] private List<FertilizerItemAuthoringData> fertilizers = new();

    public IReadOnlyList<FertilizerItemAuthoringData> Fertilizers => fertilizers;

    public int Count => fertilizers?.Count ?? 0;

    public FertilizerItemAuthoringData Get(int index)
    {
        return fertilizers[index];
    }
}

[Serializable]
public struct FertilizerItemAuthoringData
{
    [Header("ItemBaseData에 존재하는 ItemId")]
    public int itemId;

    [Header("비료 분류")]
    public FertilizerType gearType;

    [Header("비료 레벨")]
    public FertilizerGrade level;
}
public struct FertilizerItemBlobData
{
    public int ItemId;

    public FertilizerType FertilizerType;

    public FertilizerGrade Level;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FertilizerItemBlobDatas
{
    public BlobArray<FertilizerItemBlobData> Items;
}