using System;
using UnityEngine;

[Serializable]
public struct TreeDropData
{
    [Header("드롭 아이템 ID")]
    [SerializeField] public int ItemID;
    [Header("드롭량 최소값")]
    [SerializeField] public int MinAmount;
    [Header("드롭량 최대값")]
    [SerializeField] public int MaxAmount;
}

[CreateAssetMenu(fileName = "TreeDropTable", menuName = "DropTable/TreeDropTable")]
public class TreeDropTable : ScriptableObject
{
    [SerializeField] private TreeDropData[] treeDropdatas;

    public ref TreeDropData GetDropData(TreeGrade grade, bool isStump)
    {
        if( grade == TreeGrade.Unknown )
        {
            EasyDebug.LogError($"Invalid TreeGrade: {grade}, Valid Range is 1 to {treeDropdatas.Length - 1}");
            throw new System.ArgumentOutOfRangeException(nameof(grade), $"Invalid TreeGrade: {grade}");
        }
        return ref treeDropdatas[grade.ToValue() - (isStump ? 0 : 1 )];
    }
}
