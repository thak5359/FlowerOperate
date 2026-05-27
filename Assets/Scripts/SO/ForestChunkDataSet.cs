using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ForestChunkDataSetSO", menuName = "Dataset/ForestChunks", order = 2)]
public class ForestChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo];
    }
}
