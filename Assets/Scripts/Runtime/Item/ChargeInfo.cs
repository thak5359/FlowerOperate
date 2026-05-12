using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChargeInfo
{
    public float ChargeTime;
    public sbyte MaxChargeCount;

    public ChargeInfo(float chargeTime, sbyte maxChargeCount)
    {
        ChargeTime = chargeTime;
        MaxChargeCount = maxChargeCount;
    }

    public void ReadValue()
    {
        Debug.Log($"chargeTime : {ChargeTime}, maxChargeCount : {MaxChargeCount}");
    }
}