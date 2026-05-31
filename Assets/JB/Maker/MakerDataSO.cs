using MemoryPack;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial struct MakerData
{
    [MemoryPackInclude, SerializeField] private MakerType makerType;
    [MemoryPackInclude, SerializeField] private MakerTier makerTier;
    [MemoryPackInclude, SerializeField] private int2 makerSize;
    [MemoryPackInclude, SerializeField] private int2 IngredientRatio;
    [MemoryPackInclude, SerializeField] private int maxProduction;

    public MakerType GetMakerType => makerType;
    public MakerTier GetMakerTier => makerTier;
    public int2 GetMakerSize => makerSize;
    public int2 GetIngredientRatio => IngredientRatio;
    public int GetMaxProduction => maxProduction;

    public MakerData(MakerType makerType, MakerTier makerTier, int2 makerSize, int2 ingredientRatio, int maxProduction)
    {
        this.makerType = makerType;
        this.makerTier = makerTier;
        this.makerSize = makerSize;
        this.IngredientRatio = ingredientRatio;
        this.maxProduction = maxProduction;
    }
}
