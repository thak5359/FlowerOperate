using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class GlobalEventManager
{
    public static event Action<ItemObjectData> OnItemPickedUp;
    public static event Action<ItemObjectData> OnItemUsed;

    public static void InvokeItemPickedUp(ItemObjectData data)
    {
        OnItemPickedUp?.Invoke(data);
    }
}
