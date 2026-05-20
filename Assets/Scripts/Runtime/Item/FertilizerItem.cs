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

    public override void OnLoadAsync(IPropData propData = default)
    {
        base.OnLoadAsync(propData);

        if (!GlobalItemDB.HasFertilizer(Id))
        {
            Debug.LogError($"[GearItem] GearDB 조회 실패. Id: {Id}");
            return;
        }

        ref FertilizerItemBlobData FertilizerData = ref GlobalItemDB.GetFertilizerRef(Id);

        FertilizerLevel = (int)FertilizerData.Level;
        FertilizerType = FertilizerData.FertilizerType;

    }



}
