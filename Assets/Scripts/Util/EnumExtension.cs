using System;

public static class EnumExtensions
{
    public static T Next<T>(this T value) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("T must be an enum type.");

        Array values = Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, value);

        if (index < 0)
            return value;

        int nextIndex = index + 1;

        if (nextIndex >= values.Length)
            return value;

        return (T)values.GetValue(nextIndex);
    }

    public static int ToValue(this GearEfficiency efficiency)
    {
        return (int)efficiency;
    }

    public static int ToValue(this FertilizerGrade fertGrade)
    {
        return (int)fertGrade;
    }
}