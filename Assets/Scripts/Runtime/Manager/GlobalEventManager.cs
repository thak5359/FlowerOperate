using System;

public static class GlobalEventManager
{
    public static event Action<GameItem> OnItemPickedUp;
    public static event Action NextDay;
    public static event Action<string> OnLoadScene;

    public static void InvokeNextDay()
    {
        NextDay?.Invoke();
    }

    public static void InvokeItemPickedUp(GameItem data)
    {
        OnItemPickedUp?.Invoke(data);
    }

    public static void InvokeLoadScene(string fileName)
    {
        OnLoadScene?.Invoke(fileName);
    }
}
