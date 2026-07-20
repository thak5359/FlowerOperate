using System;
using UnityEngine;


[Serializable]
public struct OreDropData
{
    [Header("드롭 아이템 1")]
    [SerializeField] int DropItem1;

    [Header("드롭 아이템 2")]
    [SerializeField] int DropItem2;

    [Header("드롭 아이템 3 (Optional)")]
    [SerializeField] int DropItem3;

}

[CreateAssetMenu(fileName = "OreDropTable", menuName = "DropTable/OreDropTable")]
public class OreDropTable : ScriptableObject
{
    [SerializeField] OreDropData[] oreDropDatas;

    public ref OreDropData GetOreDrop(int areaNo)
    {
        if ( areaNo <= 0 || areaNo >= oreDropDatas.Length)
        {
            EasyDebug.LogError($"Invalid area number: {areaNo}. Valid range is 0 to {oreDropDatas.Length - 1}.");
            throw new IndexOutOfRangeException($"Invalid area number: {areaNo}. Valid range is 0 to {oreDropDatas.Length - 1}.");
        }
        return ref oreDropDatas[areaNo];
    }
}