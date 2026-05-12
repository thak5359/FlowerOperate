using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class FlowerItem : GameItem
{
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

    public FlowerItem(int id, int count) : base(id, count)
    {
        OnLoadAsync().Forget();
    }
    public override async UniTask OnLoadAsync()
    {
        await base.OnLoadAsync();

        if (!GlobalItemDB.TryGetFlower(Id, out FlowerItemBlobData flowerData))
        {
            Debug.LogError($"[FlowerItem] FlowerDB 조회 실패. Id: {Id}");
            return;
        }

        Species = flowerData.Species;
        Color = flowerData.Color;
        Florio1 = flowerData.Florio1;
        Florio2 = flowerData.Florio2;

        GrowthDuration = flowerData.GrowthDuration;
        HarvestAmount = flowerData.HarvestAmount;
    }
}