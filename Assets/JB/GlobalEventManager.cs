using System;

public static class GlobalEventManager
{
    public static event Action<ItemObjectData> OnItemPickedUp;
    public static event Action NextDay;

    public static void InvokeNextDay()
    {
        NextDay?.Invoke();
    }

    public static void InvokeItemPickedUp(ItemObjectData data)
    {
        OnItemPickedUp?.Invoke(data);
    }
}
