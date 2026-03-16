using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]

public class UsableIdData : ItemIdData
{
    [SerializeField] public int durationIndex;
    [SerializeField] public int powerIndex;
    [SerializeField] public int chargeIndex;

    public int DuratIndex => durationIndex;
    public int ChargeIndex => chargeIndex;
    public int PowerIndex => powerIndex;


}