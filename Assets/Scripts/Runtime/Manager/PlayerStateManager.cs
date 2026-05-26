using R3;

public class PlayerStateManager 
{
    /// <summary>
    /// 1. 차징 상태 (true/false)
    /// </summary>
    public readonly ReactiveProperty<bool> IsCharging = new(false);

    /// <summary>
    /// 2. 현재 선택된 핫바 레이어 (0~4)
    /// </summary>
    public readonly ReactiveProperty<int> CurrentHotbarLayer = new(0);

    /// <summary>
    /// 3. 현재 가리키고 있는 핫바 슬롯 인덱스 (0~9)
    /// </summary>
    public readonly ReactiveProperty<int> CurrentHotbarSlot = new(0);

    /// <summary>
    /// 4. 장비 기본 영역(1x1) 강제 사용 상태 (토글용)
    /// </summary>
    public readonly ReactiveProperty<bool> IsSwappingGearDefaultArea = new(false);
}