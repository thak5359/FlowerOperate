using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowerIdData", menuName = "FlowerData/IdData")]
public class FlowerIdData : ItemIdData
{
    [Header("꽃의 구성 인덱스 [품종 번호, 색상 번호, 꽃말 번호]")]
    [SerializeField] public List<int> speciesIndex;
    [SerializeField] public List<int> colorIndex;
    [SerializeField] public List<int> floroIndex; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리
    [SerializeField] public List<int> floroIndex2; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리

    public List<int> SpeciesIndex => speciesIndex;
    public List<int> ColorIndex => ColorIndex;
    public List<int> FloroIndex => floroIndex;
    public List<int> FloroIndex2 => floroIndex2;
}

