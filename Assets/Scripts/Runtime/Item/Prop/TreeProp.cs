using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

[MemoryPackable]
public partial struct TreeData : IPropData
{
    public Vector3 Position { get; private set; }
    public readonly int Id { get; init; }
    public int Duration { get; set; }

    public TreeData(Vector3 input_pos, int input_OreId, int input_Duration)
    {
        Position = input_pos;
        Id = input_OreId;
        Duration = input_Duration;
    }

    public TreeData(int input_OreID)
    {
        Position = default;
        Id = input_OreID;
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

    public override void OnDestroy()
    {
        if(treeData.Id != 0)
            ItemFactory.CreateItemPrefab(new GameItem(treeData.Id), treeData.Position);
    }
}
