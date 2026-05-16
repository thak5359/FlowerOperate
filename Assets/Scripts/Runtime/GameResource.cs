using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IGameResource
{
    int Id { get; }

    Sprite DisplaySprite { get; }
    /// <summary>
    /// Execute when SaveLoadManager Load Data. 
    /// </summary>
    UniTask OnLoadAsync(SaveDatas save);
}