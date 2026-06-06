using Cysharp.Threading.Tasks;
using MemoryPack;
using Unity.Mathematics;
using UnityEngine;

[MemoryPackable]
public partial class FlowerItem : GameItem
{
    [MemoryPackInclude] public FlowerGrade Grade { get; private set; } = 0;
    [MemoryPackIgnore] public FlowerSpecies Species { get; private set; }
    [MemoryPackIgnore] public FlowerColor Color { get; private set; }
    [MemoryPackIgnore] public FlowerFlorio Florio1 { get; private set; }
    [MemoryPackIgnore] public FlowerFlorio Florio2 { get; private set; }
    [MemoryPackIgnore] public int GrowthDurationID { get; private set; }
    [MemoryPackIgnore] public int4 GrowthDuration { get; private set; }
    [MemoryPackIgnore] public int HarvestAmount { get; private set; }

    [MemoryPackConstructor]
    protected FlowerItem()
    {
    }
    public FlowerItem(int id, int count, FlowerGrade grade = FlowerGrade.Lv0) : base(id, count)
    {
        Grade = grade;
        OnLoadAsync();
    }
    public override void OnLoadAsync(IPropData propData = default)
    {
        base.OnLoadAsync(propData);

        int lookupId = Id;
        if (SubType == ItemSubType.Seed)
        {
            lookupId = Id + 1000;
        }

        if (!GlobalItemDB.HasFlower(lookupId))
        {
            Debug.LogError($"[FlowerItem] FlowerDB 조회 실패. Id: {lookupId}");
            return;
        }
        ref FlowerItemBlobData flowerData = ref GlobalItemDB.GetFlowerRef(lookupId);

        Species = flowerData.Species;
        Color = flowerData.Color;
        Florio1 = flowerData.Florio1;
        Florio2 = flowerData.Florio2;

        GrowthDurationID = flowerData.GrowthDurationID;
        GrowthDuration = flowerData.GrowthDuration;
        HarvestAmount = flowerData.HarvestAmount;
    }
}