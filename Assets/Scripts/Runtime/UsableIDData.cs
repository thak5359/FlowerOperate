using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]

public class UsableIdData : ItemIdData
{
    [SerializeField] public List<int> durationIndex;
    [SerializeField] public List<int> powerIndex;
    [SerializeField] public List<int> chargeIndex;

    public List<int> DuratIndex => durationIndex;
    public List<int> ChargeIndex => chargeIndex;
    public List<int> PowerIndex => powerIndex;


}