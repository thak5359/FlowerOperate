using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using MemoryPack;

public partial class Prop : MonoBehaviour, IGameResource
{
    // 1. 인터페이스의 Id 구현
    [field: SerializeField] public int Id { get; protected set; }

    [field: SerializeField] public Sprite DisplaySprite { get; protected set;  }

    [field : SerializeField] public SpriteRenderer SpriteRenderer { get; protected set; }

    public virtual void Awake()
    {
        //creatre Unique ID
        Id = Guid.NewGuid().GetHashCode();
    }


    public virtual void OnDisable()
    {
        if (DisplaySprite != null) AddressableManager.ReleaseAsset(DisplaySprite);
    }

    public void SetId(int id)
    {
        this.Id = id;
    }

    // 수정 위치: IGameResource의 비동기 로드 계약을 구현해요.
    public virtual UniTask OnLoadAsync(IPropData propData)
    {
        this.Id = propData.ItemId;
        return UniTask.CompletedTask;
    }

    public virtual void OnDestroy() 
    {
        
    }
}
