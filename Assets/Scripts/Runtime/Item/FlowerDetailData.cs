using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[CreateAssetMenu(fileName = "FlowerData", menuName = "FlowerData/DetailData")]
public class FlowerDetailData : ItemDetailData
{
    [SerializeField] public List<FixedString64Bytes> speciesList;
    [SerializeField] public List<FixedString32Bytes> colorList;
    [SerializeField] public List<FixedString32Bytes> floroList;


    public FixedString64Bytes Species(byte index) => speciesList[index];
    public FixedString32Bytes Color(byte index) => colorList[index];
    public FixedString32Bytes Floro(sbyte index) => (index != -1) ? floroList[index] : null;
    
}

[StructLayout(LayoutKind.Sequential)]
public struct FlowerDetailBlobData
{
    public FixedString64Bytes species;
    public FixedString32Bytes color;
    public FixedString32Bytes floro;
    public FixedString32Bytes floro2;
}

[StructLayout(LayoutKind.Sequential)]
public struct FlowerDetailBlobDatas
{
    public BlobArray<FlowerDetailBlobData> flowerDetails;
}
