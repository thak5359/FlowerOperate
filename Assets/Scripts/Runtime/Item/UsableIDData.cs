using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]
public class UsableIdData : ItemIdData
{
    [SerializeField] public List<byte> durationIndex;
    [SerializeField] public List<byte> powerIndex;
    [SerializeField] public List<byte> chargeIndex;


    public byte DuratIndex(byte idx) => durationIndex[idx];
    public byte ChargeIndex(byte idx) => chargeIndex[idx];
    public byte PowerIndex(byte idx) => powerIndex[idx];
}

public struct UsableItemBlobData
{
    public short ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;

    public byte durationIndex;
    public byte powerIndex;
    public byte chargeIndex;
}

public struct UsableItemBlobDatas
{
    public BlobArray<UsableItemBlobData> Items;
}