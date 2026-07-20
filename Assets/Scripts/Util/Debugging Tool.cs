using Unity.Collections;
using UnityEngine;

public static class EasyDebug
{
    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogDebug(string message) { Debug.Log(message); }

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(string message) { Debug.LogError(message); }

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message) { Debug.LogWarning(message); }

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogAssertion(string message) { Debug.LogAssertion(message); }


    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogAssertion(FixedString128Bytes message) { Debug.LogAssertion(message); }
}