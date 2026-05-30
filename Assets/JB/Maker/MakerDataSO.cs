using MemoryPack;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct MakerDataSO
{
    [SerializeField] private MakerType makerType;
    [SerializeField] private MakerTier makerTier;
    [SerializeField] private int2 makerSize;
    [SerializeField] private int2 IngredientRatio;
    [SerializeField] private int maxProduction;

    public MakerDataSO(MakerType makerType, MakerTier makerTier, int2 makerSize, int2 ingredientRatio, int maxProduction)
    {
        this.makerType = makerType;
        this.makerTier = makerTier;
        this.makerSize = makerSize;
        this.IngredientRatio = ingredientRatio;
        this.maxProduction = maxProduction;
    }
}
