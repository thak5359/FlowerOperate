using MemoryPack;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

[MemoryPackable]
public partial struct TreeData : IPropData
{
    public Vector3 Position { get; private set; }
    public readonly int ItemId { get; init; }
    public int Duration { get; set; }

    public TreeData(Vector3 input_pos, int input_OreId, int input_Duration)
    {
        Position = input_pos;
        ItemId = input_OreId;
        Duration = input_Duration;
    }

    public TreeData(int input_OreID)
    {
        Position = default;
        ItemId = input_OreID;
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



    public FarmActionResult Damaged(int Damage)
    {
        try
        {

            Debug.Log($"Damaged has been called. Current Duration : {treeData.Duration}");
            treeData.Duration -= Damage;

            Debug.Log($"Current Duration : {treeData.Duration}");
            if (treeData.Duration <= 0)
            {
                return Ruining();
            }
            else
            {
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Ruining Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "Ruining_EXCEPTION");
        }
    }

    // 3. 죽음 & 아이템 뱉기 ( 추후0)


    // TODO 아이템 뱉는 로직 만들기
    private FarmActionResult Ruining()
    {
        try
        {
            Debug.Log("Runing has been called");




            Destroy(this.gameObject);

            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            Debug.Log($"Ruining Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "Ruining_EXCEPTION");
        }
    }

    public void LoadFromData(TreeData data)
    {
        this.transform.position = data.Position;

        _treeData = data;
    }

}
