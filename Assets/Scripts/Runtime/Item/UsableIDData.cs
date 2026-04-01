using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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

public struct UsableItemBlobData
{
    public BlobString itemName;
    public BlobString description;
    public BlobString spriteAddress;

    public byte durationIndex;
    public byte powerIndex;
    public byte chargeIndex;
}

public struct UsableItemBlobDatas
{
    public BlobArray<UsableItemBlobData> usableItems;
}