using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FarmChunkDataSetSO", menuName = "Dataset/FarmChunks", order = 1)]
public class FarmChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;
    
    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo - 1];
    }
}
