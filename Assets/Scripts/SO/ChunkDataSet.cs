using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]

public struct ChunkData
{
    [Header("ID")]
    [SerializeField] public int ChunkNo;

    [Header("상세 정보")]
    [SerializeField] public ChunkType Type;
    [SerializeField] public ChunkGrade Grade;
    [SerializeField] Vector3 Pos;

    [Header("인접해 있는 청크 번호")]
    [SerializeField] int4 ContinguousChunk;
}

[CreateAssetMenu(fileName = "FarmChunkDataSetSO", menuName = "Dataset/FarmChunks", order = 1)]
public class FarmChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo + 1];
    }

}

[CreateAssetMenu(fileName = "ForestChunkDataSetSO", menuName = "Dataset/ForestChunks", order = 2)]
public class ForestChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo];
    }
}

[CreateAssetMenu(fileName = "MineChunkDataSetSO", menuName = "Dataset/MineChunks", order = 3)]
public class MineChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo];
    }
}



[CreateAssetMenu(fileName = "FieldChunkDataSetSO", menuName = "Dataset/FieldChunks", order = 4)]
public class FieldChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo];
    }
}