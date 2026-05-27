using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "FieldChunkDataSetSO", menuName = "Dataset/FieldChunks", order = 4)]
public class FieldChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo];
    }
}
