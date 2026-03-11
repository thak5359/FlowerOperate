using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowerIdData", menuName = "FlowerData/IdData")]
public class FlowerIdData : ItemIdData
{
    [Header("꽃의 구성 인덱스 [품종 번호, 색상 번호, 꽃말 번호]")]
    [SerializeField] public int speciesIndex;
    [SerializeField] public int colorIndex;
    [SerializeField] public List<int> floroIndex; // 한 꽃에 복수의 꽃말이 존재함 > List로 관리

    public int SpeciesIndex => speciesIndex;
    public int ColorIndex => ColorIndex;
    public List<int> FloroIndex => floroIndex;
}

