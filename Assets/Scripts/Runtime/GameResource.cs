using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameResource
{
    int Id { get; }

    Sprite PropSprite { get; }
    /// <summary>
    /// Execute when SaveLoadManager Load Data. 
    /// </summary>
    public virtual void OnLoad()
    {
        //TODO : onload에서 자신의 위치, 스프라이트 받아오기
    }
}