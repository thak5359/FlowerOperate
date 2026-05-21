using Cysharp.Threading.Tasks;
using Fungus;
using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MemoryPackable]
public partial class FertilizerItem : GameItem
{
    [MemoryPackIgnore] public FertilizerGrade FertilizerGrade { get; private set; }
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

        FertilizerGrade = FertilizerData.Level;
        FertilizerType = FertilizerData.FertilizerType;

    }



}
