using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OreDropTable", menuName = "DropTable/OreDropTable")]
public class OreDropData : ScriptableObject
{
    [SerializeField]List<OreDrop> oreLists;

    public IReadOnlyList<OreDrop> OreLists => oreLists;

}


[Serializable]
public struct OreDrop
{








}

