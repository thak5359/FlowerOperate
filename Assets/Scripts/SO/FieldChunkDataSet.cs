using UnityEngine;

[CreateAssetMenu(fileName = "FieldChunkDataSetSO", menuName = "Dataset/FieldChunks", order = 4)]
public class FieldChunkDataSet : ScriptableObject
{
    [field: SerializeField] private ChunkData[] DataList;

    public ref ChunkData getChunk(ref int chunkNo)
    {
        return ref DataList[chunkNo - 1];
    }

    public int GetLength()
    {
        return DataList.Length;
    }

}
