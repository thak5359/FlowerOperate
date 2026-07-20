using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct FlowerDropData
{
    [Header("드롭 아이템 ID")]
    [SerializeField] public int ItemID;
    [Header("드롭량 최소값")]
    [SerializeField] public int MinAmount;
    [Header("드롭량 최대값")]
    [SerializeField] public int MaxAmount;
}


[CreateAssetMenu(fileName = "FlowerDropTable", menuName = "DropTable/FlowerDropTable", order = 1)]
public class FlowerDropTable : ScriptableObject
{
    [SerializeField] private FlowerDropData[] flowerDropdatas;

    /// <summary>
    /// 꽃 종류를 넣으면 해당 꽃 종류의 최소/최대 드롭 수량을 반환 (Unknown 예외처리 필요)
    /// </summary>
    /// <param name="species">드롭 데이터를 조회할 꽃 종류</param>
    /// <returns>해당 꽃 종류의 최소/최대 드롭 수량</returns>
    public ref FlowerDropData GetDropData(FlowerSpecies species)
    {
        return ref flowerDropdatas[species.ToValue()];
    }
}