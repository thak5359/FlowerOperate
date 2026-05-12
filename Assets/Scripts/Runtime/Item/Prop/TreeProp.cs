using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

[MemoryPackable]
public partial struct TreeData
{
    public Vector3 Position { get; private set; }
    public readonly int TreeId { get; init; }
    public int Duration { get; set; }

    public TreeData(Vector3 input_pos, int input_OreId, int input_Duration)
    {
        Position = input_pos;
        TreeId = input_OreId;
        Duration = input_Duration;
    }

    public TreeData(int input_OreID)
    {
        Position = default;
        TreeId = input_OreID;
        Duration = 100;
    }
    public void SetPosition(Vector3 position) => Position = position;
}




public class TreeProp : Prop, IGameResource
{

    private TreeData _treeData = new(0);

    public ref TreeData treeData => ref _treeData;

    public void OnEnable()
    {
        if (treeData.Position == default)
            treeData.SetPosition(this.transform.position);
    }


}
