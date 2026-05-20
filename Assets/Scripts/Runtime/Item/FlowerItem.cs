using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class FlowerItem : GameItem
{
    [MemoryPackInclude] public FlowerGrade Grade { get; private set; } = 0;
    [MemoryPackIgnore] public FlowerSpecies Species { get; private set; }
    [MemoryPackIgnore] public FlowerColor Color { get; private set; }
    [MemoryPackIgnore] public FlowerFlorio Florio1 { get; private set; }
    [MemoryPackIgnore] public FlowerFlorio Florio2 { get; private set; }
    [MemoryPackIgnore] public int GrowthDuration { get; private set; }
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

        if (!GlobalItemDB.HasFlower(Id))
        {
            Debug.LogError($"[FlowerItem] FlowerDB 조회 실패. Id: {Id}");
            return;
        }
        ref FlowerItemBlobData flowerData = ref GlobalItemDB.GetFlowerRef(Id);

        Species = flowerData.Species;
        Color = flowerData.Color;
        Florio1 = flowerData.Florio1;
        Florio2 = flowerData.Florio2;

        GrowthDuration = flowerData.GrowthDuration;
        HarvestAmount = flowerData.HarvestAmount;
    }
}