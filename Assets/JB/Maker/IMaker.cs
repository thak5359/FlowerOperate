// 수정 위치: 제작 결과의 비동기 계약에 UniTask를 사용해요.
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMaker
{
    public abstract UniTask<GameItem> ReturnGameItemAsync();
}
