using MemoryPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItem : IGameResource
{
    // 1. 인터페이스의 Id 구현
    [field: SerializeField] public int Id { get; protected set; }

    public int count { get; set; }


    [MemoryPackIgnore]
    [field: SerializeField] public Sprite PropSprite { get; protected set; }

    [MemoryPackIgnore]
    public ItemMainType MainType { get; protected set; }
    [MemoryPackIgnore]
    public ItemSubType SubType { get; protected set; }
    [MemoryPackIgnore]
    int StackLimit { get; init; }

    /// <summary>
    /// creatre Unique ID
    /// </summary>
    public virtual void Awake()
    {
        
        Id = Guid.NewGuid().GetHashCode();
    }

    public virtual void OnLoad()
    {
        ((IGameResource)this).OnLoad();
    }

}
