using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class GearItem : GameItem
{

    [MemoryPackInclude]public int CurrentDurability { get; set; }
    [MemoryPackIgnore]public GearGrade Grade { get; set; }
    [MemoryPackIgnore] public GearType GearType { get; private set; }
    [MemoryPackIgnore] public GearMaxDuration MaxDurability { get; private set; }
    [MemoryPackIgnore] public GearEfficiency Efficiency { get; private set; }
    [MemoryPackIgnore] public ChargeInfo ChargeInfo { get; private set; }

    [MemoryPackConstructor]
    protected GearItem()
    {
    }

    public GearItem(int id, int count, GearGrade input_Grade = GearGrade.Old, GearMaxDuration input_MaxDuration = GearMaxDuration.Lv1) : base(id, count)
    {
     Grade = input_Grade;
        MaxDurability = input_MaxDuration;

        CurrentDurability = (int)MaxDurability;
    }

    public override void OnLoadAsync(IPropData propData = default)
    {
        base.OnLoadAsync(propData);

        if (!GlobalItemDB.HasGear(Id))
        {
            Debug.LogError($"[GearItem] GearDB 조회 실패. Id: {Id}");
            return;
        }


        ref GearItemBlobData gearData = ref  GlobalItemDB.GetGearRef(Id);

        GearType = gearData.GearType;
        MaxDurability = gearData.MaxDuration;
        Efficiency = gearData.Efficiency;

        ChargeInfo = new ChargeInfo(GearValueConverter.ToSeconds(gearData.ChargeTime),gearData.ChargeAreas.ToArray());

    }

    public void repair()
    {
        CurrentDurability = (int)MaxDurability;
    }

}
/// <summary>
/// Enum으로 float 값을 적용할 수 없어서 별도의 전환 기능을 만듦. 내부 수치가 변동될 경우 이 함수도 수정
/// </summary>
public static class GearValueConverter
{
    public static float ToSeconds(GearChargeTime chargeTime)
    {
        return (int)chargeTime * 0.25f;
    }
}