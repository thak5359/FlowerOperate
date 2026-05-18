using System;
using System.Collections.Generic;
using UnityEngine;
using static Constant;

public class PlaneStatus : ScriptableObject
{
    [SerializeField] private List<ChunkData> chunks { get; set; } = new List<ChunkData>();
    public IReadOnlyList<ChunkData> Chunks() => chunks;

    public ChunkData GetChunk(int index)
    {
        if (index < 0 || index >= chunks.Count)
        {
            throw new IndexOutOfRangeException($"Index {index} is out of range for chunks list.");
        }
        return chunks[index];
    }


    public ChunkLevel GetChunkLevel(int index)
    {
        if (index < 0 || index >= chunks.Count)
        {
            throw new IndexOutOfRangeException($"Index {index} is out of range for chunks list.");
        }
        return chunks[index].ChunkLevel;
    }

    public bool TryGetChunkIndexByWorldPosition(Vector3 worldPosition, out int chunkIndex)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i].ContainsWorldPosition(worldPosition))
            {
                chunkIndex = i;
                return true;
            }
        }

        chunkIndex = -1;
        return false;
    }
}


[Serializable]
public struct ChunkData
{
    [Header("개방 여부")] public bool IsOpened;
    [Header("청크 레벨")] public ChunkLevel ChunkLevel;
    [Header("개방 여부")] public Vector3 StartPoint;

    public bool ContainsWorldPosition(Vector3 worldPosition)
    {
        return worldPosition.x >= StartPoint.x &&
               worldPosition.x < StartPoint.x + CHUNK_SIZE &&
               worldPosition.z >= StartPoint.z &&
               worldPosition.z < StartPoint.z + CHUNK_SIZE;
    }

    public FarmActionResult ChunkLevelUp(uint input_lv)
    {
        try
        {
            if (input_lv == 0)
            {
                throw new Exception("Input level must be greater than 0.");
            }

            ChunkLevel.Next<ChunkLevel>();
            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            return new FarmActionResult(FarmActionResult.ResultType.Error, e.Message);
        }
    }

    public FarmActionResult UnlockChunk()
    {
        try
        {
            if (IsOpened == true)
            {
                throw new Exception("Chunk is already opened.");
            }

            IsOpened = true;
            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            return new FarmActionResult(FarmActionResult.ResultType.Error, e.Message);
        }
    }

}