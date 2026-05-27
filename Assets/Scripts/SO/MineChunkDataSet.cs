using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "MineChunkDataSetSO", menuName = "Dataset/MineChunks", order = 3)]
public class MineChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo - 1];
    }
}
