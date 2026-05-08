using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "UsableIdData", menuName = "UsableData/IdData")]
public class UsableIdData : ItemIdData
{
    [Header("장비 스탯")]
    [SerializeField] public List<byte> durationIndex;
    [SerializeField] public List<byte> powerIndex;
    [SerializeField] public List<byte> chargeIndex;

    public UsableIdData() => base.startId = Constant.USABLE_START_ID;
    public byte DuratIndex(byte idx) => durationIndex[idx];
    public byte ChargeIndex(byte idx) => chargeIndex[idx];
    public byte PowerIndex(byte idx) => powerIndex[idx];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UsableItemBlobData
{
    public short ItemId;
    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString64Bytes SpriteAddress;
    public short Price;

    public byte durationIndex;
    public byte powerIndex;
    public byte chargeIndex;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UsableItemBlobDatas
{
    public BlobArray<UsableItemBlobData> Items;
}