using System;
using UnityEngine;


[Serializable]
public struct GrassDropData
{
    [Header("드롭 아이템 ID")]
    [SerializeField] public int DropItemID;
    [Header("드롭량 최소값")]
    [SerializeField] public int MinAmount;
    [Header("드롭량 최대값")]
    [SerializeField] public int MaxAmount;
}



[CreateAssetMenu(fileName = "GrassDropTable", menuName = "DropTable/GrassDropTable")]
public class GrassDropTable : ScriptableObject
{
    [SerializeField] private GrassDropData[] GrassDropdatas;

    public ref GrassDropData GetGrassDropData(int ChunkNo)
    {
        if (ChunkNo < 0 || ChunkNo >= GrassDropdatas.Length)
        {
            EasyDebug.LogError($"Invalid ChunkNo: {ChunkNo}. Valid range is 0 to {GrassDropdatas.Length - 1}.");
            throw new IndexOutOfRangeException($"Invalid ChunkNo: {ChunkNo}. Valid range is 0 to {GrassDropdatas.Length - 1}.");
        }
        return ref GrassDropdatas[ChunkNo];
    }

}
