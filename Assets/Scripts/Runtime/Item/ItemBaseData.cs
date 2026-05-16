using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemBaseData", menuName = "ItemData/BaseData")]
public class ItemBaseData : ScriptableObject
{
    [SerializeField] private List<ItemBaseAuthoringData> items = new();

    public IReadOnlyList<ItemBaseAuthoringData> Items => items;
    public void setItems(List<ItemBaseAuthoringData> items) => this.items = items;
    public int Count => items?.Count ?? 0;

    public ItemBaseAuthoringData Get(int index)
    {
        return items[index];
    }
}

[Serializable]
public struct ItemBaseAuthoringData
{
    [Header("식별자")]
    public int itemId;
    public int refundPrice;

    [Header("분류")]
    public ItemMainType mainType;
    public ItemSubType subType;

    [Header("스택")]
    public int stackLimit;

    [Header("기본 정보")]
    public string itemName;
    [TextArea] public string description;
    public string spriteAddress;
    public int price;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemBaseBlobData
{
    public int ItemId;

    public ItemMainType MainType;
    public ItemSubType SubType;

    public int StackLimit;

    public FixedString64Bytes ItemName;
    public FixedString128Bytes Description;
    public FixedString128Bytes SpriteAddress;

    public int RefundPrice;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemBaseBlobDatas
{
    public BlobArray<ItemBaseBlobData> Items;
}