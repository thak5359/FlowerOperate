using R3;
using System;

public static class GlobalEventManager
{
    private static readonly Subject<GameItem> OnItemPickedUp = new Subject<GameItem>();
    private static readonly Subject<Unit> NextDay = new Subject<Unit>();
    private static readonly Subject<bool> InventoryFull = new Subject<bool>();
    public static event Action<string> OnLoadScene;

    public static Observable<bool> InventoryFullObservable => InventoryFull;
    public static void SetInventoryFull(bool isFull) => InventoryFull.OnNext(isFull);

    public static Observable<GameItem> OnItemPickedUpObservable => OnItemPickedUp;
    public static Observable<Unit> OnNextDayObservable => NextDay;

    public static CompositeDisposable disposables = new CompositeDisposable();

    public static void InvokeNextDay()
    {
        NextDay.OnNext(default);
    }

    public static void InvokeItemPickedUp(GameItem data)
    {
        OnItemPickedUp.OnNext(data);
    }

    public static void InvokeLoadScene(string fileName)
    {
        OnLoadScene?.Invoke(fileName);
    }

    //종료될 때 실행
    public static void DisposeAll()
    {
        disposables.Dispose();
        disposables = null;
    }
}
