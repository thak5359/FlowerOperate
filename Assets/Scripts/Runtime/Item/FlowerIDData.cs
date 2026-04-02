using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowerIdData", menuName = "FlowerData/IdData")]
public class FlowerIdData : ItemIdData
{
    [Header("꽃의 구성 인덱스 [품종 번호, 색상 번호, 꽃말 번호]")]
    [SerializeField] public List<byte> speciesIndex;
    [SerializeField] public List<byte> colorIndex;
    [SerializeField] public List<byte> floroIndex; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리
    [SerializeField] public List<sbyte> floroIndex2; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리

    public byte SpeciesIndex(byte i) => speciesIndex[i];
    public byte ColorIndex(byte i) => colorIndex[i];
    public byte FloroIndex(byte i) => floroIndex[i];
    public sbyte FloroIndex2(sbyte i) => floroIndex2[i];
}

public struct FlowerItemBlobData
{
    public short ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString128Bytes SpriteAddress;

    public byte speciesIndex;
    public byte colorIndex;
    public byte floroIndex;
    public sbyte floroIndex2;
}

public struct FlowerItemBlobDatas
{
    public BlobArray<FlowerItemBlobData> Items;
}
