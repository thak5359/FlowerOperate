using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ChunkData
{
    [Header("ID")]
    [SerializeField] public int ChunkId;

    [Header("상세 정보")]
    [SerializeField] public ChunkType Type;
    [SerializeField] public ChunkGrade Grade;

    [Header("인접해 있는 청크 번호")]
    [SerializeField] public int4 ContinguousChunk;

}
