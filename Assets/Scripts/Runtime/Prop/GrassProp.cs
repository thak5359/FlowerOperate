// 수정 위치: 비동기 드롭 아이템 생성을 fire-and-forget 경계에서 실행해요.
using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial struct GrassData : IPropData
{
    public Vector3 Position { get; private set; }
    public readonly int ItemId { get; init; }
    [field: SerializeField] public int Duration;

    public GrassData(Vector3 input_pos, int input_OreId, int input_Duration)
    {
        Position = input_pos;
        ItemId = input_OreId;
        Duration = input_Duration;
    }

    public GrassData(int input_OreID)
    {
        Position = default;
        ItemId = input_OreID;
        Duration = 100;
    }
    public void SetPosition(Vector3 position) => Position = position;
    
    
    // ?
    public void OnDestroy()
    {
        if (ItemId != 0)
            // 수정 위치: 드롭 아이템 로드 완료 후 프리팹을 생성해요.
            ItemFactory.CreateItemPrefabAsync(ItemId, 1, Position).Forget();
    }
}

public class GrassProp : Prop
{
    // 어떤 광물 종류, 광물 아이템... 파편.. 인벤토리에는 들어가지 않는 아이템 타입이고, 어떤 금속이고, 어떤 아이템을 가지고..
    [SerializeField] private GrassData _grassData = new(0);

    public ref GrassData grassData => ref _grassData;

    public void OnEnable()
    {
        if (grassData.Position == default)
            grassData.SetPosition(this.transform.position);
    }

    // TODO : 아이템 뱉는 로직 만들기
    public FarmActionResult Reaping() // 수확
    {
        try
        {
                //TODO:: 인스턴스 만들고 자멸하는 함수 넣기
                return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            Debug.Log($"Reaping Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "REAPING_EXCEPTION");
        }
    }

    public void LoadFromData(GrassData data)
    {
        this.transform.position = data.Position;

        _grassData = data;
    }

    public override void OnDestroy()
    {
        if (grassData.ItemId != 0)
            // 수정 위치: 드롭 아이템 로드 완료 후 프리팹을 생성해요.
            ItemFactory.CreateItemPrefabAsync(grassData.ItemId, 1, grassData.Position).Forget();
    }
}
