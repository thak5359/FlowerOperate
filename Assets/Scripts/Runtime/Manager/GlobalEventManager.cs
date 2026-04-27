using System;

public static class GlobalEventManager
{
    public static event Action<ItemObjectData> OnItemPickedUp;
    public static event Action OnDataChanged;   // 인벤토리, 창고, 플롯의 데이터 변경시 호출할 이벤트

    public static void InvokeItemPickedUp(ItemObjectData data)
    {
        OnItemPickedUp?.Invoke(data);
    }

    public static void InvokeDataChanged()
    {
        OnDataChanged?.Invoke();
    }

}
