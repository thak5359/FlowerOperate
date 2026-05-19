using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChargeInfo
{
    [SerializeField] public float ChargeTime;
    [SerializeField] ChargeArea[] ChargeAreas;
    [SerializeField] ChargeArea ChargeAreaSwap;

    public ChargeInfo(float chargeTime, ChargeArea[] chargeAreas, ChargeArea input_ChargeAreaSwap = ChargeArea.Unknown)
    {
        ChargeTime = chargeTime;
        ChargeAreas = chargeAreas;
        ChargeAreaSwap = input_ChargeAreaSwap;
    }

    public void ReadValue()
    {
        Debug.Log($"chargeTime : {ChargeTime}, chargeAreas : {string.Join(", ", ChargeAreas)}, chargeAreaSwap : {ChargeAreaSwap}");
    }
}