using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakerBaseModel : MonoBehaviour
{
    [SerializeField] protected GameItem gameItem;
    [SerializeField] protected MakerData makerData;

    public virtual void SetGameItem(GameItem item)
    {
        gameItem = item;
    }

    public virtual void AddGameItem(GameItem item)
    {
        if (gameItem == null)
        {
            gameItem = item;
            return;
        }

        if (gameItem.Id != item.Id)
        {
            Debug.LogError("다른 종류의 아이템을 추가할 수 없습니다.");
            return;
        }

        gameItem.AddAmount(item.Count);
    }
}
