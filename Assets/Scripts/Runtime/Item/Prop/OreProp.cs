using Fungus;
using MemoryPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[MemoryPackable]
public partial struct OreData
{
    public Vector3 Position { get; private set; }
    public readonly int OreId { get; init; }
    public int Duration { get; set; }

    public OreData(Vector3 input_pos, int input_OreId, int input_Duration)
    {
        Position = input_pos;
        OreId = input_OreId;
        Duration = input_Duration;
    }

    public OreData(int input_OreID)
    {
        Position = default;
        OreId = input_OreID;
        Duration = 100;
    }
    public void SetPosition(Vector3 position) => Position = position;
}


[Serializable]
public class OreProp : Prop
{
    // 어떤 광물 종류, 광물 아이템... 파편.. 인벤토리에는 들어가지 않는 아이템 타입이고, 어떤 금속이고, 어떤 아이템을 가지고..
    private OreData _oreData = new(0);

    public ref OreData oreData => ref _oreData;

    public void OnEnable()
    {
        if (oreData.Position == default)
            oreData.SetPosition(this.transform.position);
    }

    // 2. 데미지 계산
    public FarmActionResult Damaged(int Damage)
    {
        try
        {
            
            Debug.Log($"Damaged has been called. Current Duration : {oreData.Duration}");
            oreData.Duration -= Damage;

            Debug.Log($"Current Duration : { oreData.Duration}");
            if (oreData.Duration <= 0)
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

    public void LoadFromData(OreData data)
    {
        this.transform.position = data.Position;

        _oreData = data;
    }
}