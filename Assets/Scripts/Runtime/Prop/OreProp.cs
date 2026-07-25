// 수정 위치: 비동기 드롭 아이템 생성을 fire-and-forget 경계에서 실행해요.
using Cysharp.Threading.Tasks;
using Fungus;
using MemoryPack;
using System;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[MemoryPackable]
[Serializable]
public partial struct OreData : IPropData
{
    public Vector3 Position { get; private set; }
    [field: SerializeField]public int ItemId { get; set; }
    [field: SerializeField] public int Duration;

    public OreData(Vector3 input_pos, int input_OreId, int input_Duration)
    {
        Position = input_pos;
        ItemId = input_OreId;
        Duration = input_Duration;
    }

    public OreData(int input_OreID)
    {
        Position = default;
        ItemId = input_OreID;
        Duration = 100;
    }
    public void SetPosition(Vector3 position) => Position = position;
    

}


[Serializable]
public class OreProp : Prop
{
    // 어떤 광물 종류, 광물 아이템... 파편.. 인벤토리에는 들어가지 않는 아이템 타입이고, 어떤 금속이고, 어떤 아이템을 가지고..
    [SerializeField] private OreData _oreData = new(0);
    
    public ref OreData oreData => ref _oreData;

    public void OnEnable()
    {
        if (oreData.Position == default)
            oreData.SetPosition(this.transform.position);

        //oreData.ItemId = this.Id;
        oreData = new(this.gameObject.transform.localPosition, 402001, 3);

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

    
    private FarmActionResult Ruining()
    {
        try
        {
            Debug.Log("Runing has been called");


            if (oreData.ItemId != 0)
                // 수정 위치: 드롭 아이템 로드 완료 후 프리팹을 생성해요.
                ItemFactory.CreateItemPrefabAsync(oreData.ItemId, 1, oreData.Position).Forget();
            else
                Debug.Log("OreProp destroyed without item drop. ItemId is 0.");

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

    public override void OnDestroy()
    {
      
    }
}
