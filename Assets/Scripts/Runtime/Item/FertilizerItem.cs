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

    // 수정: 기반 아이템 로드가 성공한 뒤에만 비료 전용 DB를 조회
    public override async UniTask OnLoadAsync(IPropData propData = default)
    {
        if (!await TryLoadBaseDataAsync())
            return;

        LoadFertilizerData();
    }

    // 수정: Blob ref 접근은 await가 없는 동기 구간으로 격리
    private void LoadFertilizerData()
    {
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
