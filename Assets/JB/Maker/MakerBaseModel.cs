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

    protected virtual GameItem ReturnGameItem()
    {
        // GameItem itemToReturn = gameItem;
        // itemToReturn.Count = 0;
        // gameItem = null;
        // if(itemToReturn == null || itemToReturn.Count < makerData.GetIngredientRatio.x)
        // {
        //     Debug.LogError("반환할 아이템이 없습니다.");
        //     return null;
        // }
        // return null;
        // while(gameItem.Count >= makerData.GetIngredientRatio.x)
        // {
        //     // 아이템 수량빼기 로직 완성되면 해당 로직을 사용한 코드로 변경예정
        //     gameItem.Count -= makerData.GetIngredientRatio.x;
        //     itemToReturn.Count += makerData.GetIngredientRatio.y;
        // }
        // return itemToReturn;
        return null;
    }
}
