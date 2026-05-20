public static class GearEnumExtensions
{
    /// <summary>
    /// GearEfficiency Enum의 할당된 정수 값을 가비지(GC) 없이 안전하게 반환합니다.
    /// </summary>
    public static int ToValue(this GearEfficiency efficiency)
    {
        return (int)efficiency;
    }
}