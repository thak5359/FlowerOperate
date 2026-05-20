using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChargeInfo
{
    [SerializeField] public float chargeTime;
    [SerializeField] ChargeArea[] chargeAreas;

    public float ChargeTime => chargeTime;
    public ChargeArea[] ChargeAreas => chargeAreas;



    public ChargeInfo(float chargeTime, ChargeArea[] chargeAreas)
    {
        this.chargeTime = chargeTime;
        this.chargeAreas = chargeAreas;
    }

    public void ReadValue()
    {
        Debug.Log($"chargeTime : {chargeTime}, chargeAreas : {string.Join(", ", chargeAreas)}");
    }
}