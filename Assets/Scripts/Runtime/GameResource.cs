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
    // 수정 위치: 리소스 로드 완료를 호출자가 기다릴 수 있도록 비동기 계약으로 변경해요.
    public UniTask OnLoadAsync(IPropData propData);
}


