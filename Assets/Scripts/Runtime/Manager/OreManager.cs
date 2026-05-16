using Cysharp.Threading.Tasks;
using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using VContainer.Unity;



[MemoryPackable]
public partial struct OreManagerData
{




}

public class OreManager: IAsyncStartable
{
    OreManagerData _oreManagerData;
    public ref OreManagerData OreManagerData => ref _oreManagerData;


    public async UniTask StartAsync(CancellationToken cancellationToken)
    {
        


    }




}
