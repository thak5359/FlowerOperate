using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]

public class UsableIdData : ItemIdData
{
    [SerializeField] public int durationIndex;
    [SerializeField] public int chargeIndex;
    [SerializeField] public int powerIndex;

    public int DuratIndex => durationIndex;
    public int ChargeIndex => chargeIndex;
    public int PowerIndex => powerIndex;


}