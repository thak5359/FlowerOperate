using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Variable
{
    None,
    Rose,
    Tulip,
    Hyacinth,
    Freesia,
    Cosmos,
    Carnation,
    Peony,
    Lily,
    Lisianthus,
    Ranunculus,
    Delphinium,
    Chrysanthemum,
    Gerbera
};

public enum FColor
{
    None,
    White,
    Blue,
    Orange,
    Red,
    Pink,
    Purple,
    Yellow,
    Black,
    Green,
    Rainbow
};

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class FlowerData : ScriptableObject
{
    [Header("Á¤º¸")]
    public Sprite Sprite;
    public string FlowerName;
    public Variable FlowerVar;
    public FColor FlowerColor;
    public int Amount = 0;
    public int Price;
}
