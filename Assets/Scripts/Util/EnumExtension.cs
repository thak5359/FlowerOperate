using System;

public static class EnumExtensions
{



    public static T Prev<T>(this T value) where T : struct, Enum
    {
        int intValue = Convert.ToInt32(value);
        int nextValue = intValue - 1;

        // 해당 Enum 타입에 이전 숫자가 정의되어 있는지 안전하게 확인
        if (Enum.IsDefined(typeof(T), nextValue))
        {
            return (T)(object)nextValue;
        }
        // 이전 단계가 없다면 (최대 등급이라면) 현재 값을 그대로 유지
        return value;
    }

    /// <summary>
    ///  값이 연속된 Enum 타입에서 현재 값의 다음 값을 반환하는 확장 메서드입니다.
    /// </summary>
    public static T Next<T>(this T value) where T : struct, Enum
    {
        int intValue = Convert.ToInt32(value);
        int nextValue = intValue + 1;

        // 해당 Enum 타입에 다음 숫자가 정의되어 있는지 안전하게 확인
        if (Enum.IsDefined(typeof(T), nextValue))
        {
            return (T)(object)nextValue;
        }

        // 다음 단계가 없다면 (최대 등급이라면) 현재 값을 그대로 유지
        return value;
    }



    public static int ToValue(this GearEfficiency efficiency)
    {
        return (int)efficiency;
    }

    
    public static int ToValue(this FlowerSpecies species)
    {
        return (int)species;
    }


    public static int ToValue(this FertilizerGrade fertGrade)
    {
        return (int)fertGrade;
    }
    public static int ToValue(this FlowerGrade grade)
    {
        return (int)grade;
    }

    public static int ToValue(this GearGrade grade)
    {
        return (int)grade;
    }
    public static int ToValue(this OreType oreType)
    {
        return (int)oreType;
    }

    public static int ToValue(this TreeGrade grade)
    {
        return (int)grade;
    }

    public static int ToValue(this GrassGrade grade)
    {
        return (int)grade;
    }
}