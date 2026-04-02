using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]

public class UsableIdData : ItemIdData
{
    [SerializeField] public List<int> durationIndex;
    [SerializeField] public List<int> powerIndex;
    [SerializeField] public List<int> chargeIndex;

    public int DuratIndex(int idx) => durationIndex[idx];
    public int ChargeIndex(int idx) => chargeIndex[idx];
    public int PowerIndex(int idx) => powerIndex[idx];


}