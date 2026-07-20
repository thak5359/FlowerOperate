using System;
using AYellowpaper.SerializedCollections;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.InteropServices;


[Serializable]
public struct GemStoneDropData
{
    [Header("드롭 아이템 ID")]
    [SerializeField] public int DropItemID;
    [Header("드롭량 최소값")]
    [SerializeField] public int MinAmount;
    [Header("드롭량 최대값")]
    [SerializeField] public int MaxAmount;
}


[CreateAssetMenu(fileName = "GemstoneDropTable", menuName = "DropTable/GemstoneDropTable")]
public class GemstoneDropTable : ScriptableObject
{
    [SerializeField] private GemStoneDropData[] gemstoneDropdatas;
    public ref GemStoneDropData GetGemStoneDropData(int ChunkNo)
    {
        return ref gemstoneDropdatas[ChunkNo];
    }
}


