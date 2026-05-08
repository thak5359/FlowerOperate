using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public int Id { get; protected set; }

    [SerializeField] public SpriteRenderer spriteRenderer;

    protected Sprite sprite;

    public virtual void Awake()
    {
        Id = Guid.NewGuid().GetHashCode(); // 고유한 ID 생성
    }

    public virtual void OnDisable()
    {
        if (sprite != null) AddressableManager.ReleaseAsset(sprite);
    }

}