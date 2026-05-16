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

    public virtual async UniTask OnLoadAsync(IPropData propData)
    {
        this.Id = propData.Id;
    }
}