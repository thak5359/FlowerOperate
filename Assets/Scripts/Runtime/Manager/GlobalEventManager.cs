using R3;
using System;

public static class GlobalEventManager
{
    private static readonly Subject<GameItem> OnItemPickedUp = new Subject<GameItem>();
    public static event Action NextDay;
    public static event Action<string> OnLoadScene;

    public static Observable<GameItem> OnItemPickedUpObservable => OnItemPickedUp;

    public static void InvokeNextDay()
    {
        NextDay?.Invoke();
    }

    public static void InvokeItemPickedUp(GameItem data)
    {
        OnItemPickedUp.OnNext(data);
    }

    public static void InvokeLoadScene(string fileName)
    {
        OnLoadScene?.Invoke(fileName);
    }
}
