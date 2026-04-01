using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowerIdData", menuName = "FlowerData/IdData")]
public class FlowerIdData : ItemIdData
{
    [Header("꽃의 구성 인덱스 [품종 번호, 색상 번호, 꽃말 번호]")]
    [SerializeField] public List<int> speciesIndex;
    [SerializeField] public List<int> colorIndex;
    [SerializeField] public List<int> floroIndex; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리
    [SerializeField] public List<int> floroIndex2; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리

    public int SpeciesIndex(int i) => speciesIndex[i];
    public int ColorIndex(int i) => colorIndex[i];
    public int FloroIndex(int i) => floroIndex[i];
    public int FloroIndex2(int i) => floroIndex2[i];
}

public struct FlowerItemBlobData
{
    public BlobString itemName;
    public BlobString description;
    public BlobString spriteAddress;

    public byte speciesIndex;
    public byte colorIndex;
    public byte floroIndex;
    public byte floroIndex2;
}

public struct FlowerItemBlobDatas
{
    public BlobArray<FlowerItemBlobData> flowerItems;
}
