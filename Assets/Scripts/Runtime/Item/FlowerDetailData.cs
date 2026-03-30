using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FlowerData", menuName = "FlowerData/DetailData")]
public class FlowerDetailData : ItemDetailData
{
    [SerializeField] public List<string> speciesList;
    [SerializeField] public List<string> colorList;
    [SerializeField] public List<string> floroList;

    public string Species(int index) => speciesList[index];
    public string Color(int index) => colorList[index];
    public string Floro(int index) => (index != -1) ? floroList[index] : null;
}
