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
