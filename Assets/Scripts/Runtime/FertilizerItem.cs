using Cysharp.Threading.Tasks;
using Fungus;
using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MemoryPackable]
public partial class FertilizerItem : GameItem
{
     public int FertilizerLevel { get; set; }
    [MemoryPackIgnore] public FertilizerType FertilizerType { get; private set; }
    


    [MemoryPackConstructor]
    protected FertilizerItem()
    {
    }
    public FertilizerItem(int id, int count) : base(id, count)
    {
    }

    public override async UniTask OnLoadAsync()
    {
        await base.OnLoadAsync();

        if (!GlobalItemDB.TryGetFertilizer(Id, out FertilizerItemBlobData FertilizerData))
        {
            Debug.LogError($"[GearItem] GearDB 조회 실패. Id: {Id}");
            return;
        }


        FertilizerLevel = (int)FertilizerData.Level;
        FertilizerType = FertilizerData.FertilizerType;

    }



}
