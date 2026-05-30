using System;

public static class FungusEventBridge
{
    // Fungus에서 보낸 메시지를 외부 C# 클래스들이 구독할 수 있는 전역 이벤트예요.
    public static event Action<string> OnFungusMessageBroadcasted;

    public static void Broadcast(string message)
    {
        OnFungusMessageBroadcasted?.Invoke(message);
    }
}