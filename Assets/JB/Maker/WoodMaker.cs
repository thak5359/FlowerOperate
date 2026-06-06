using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WoodMaker : MakerBaseModel, IMaker
{
    public GameItem ReturnGameItem()
    {
        if (gameItem == null || gameItem.Count < makerData.GetIngredientRatio.x)
        {
            Debug.LogError("반환할 아이템이 없습니다.");
            return null;
        }
        GameItem itemToReturn = ItemFactory.CreateItem(gameItem.Id + 1000, 0);
        int2 ingredientRatio = makerData.GetIngredientRatio;
        while(gameItem.Count >= ingredientRatio.x)
        {
            gameItem.SubCount(ref ingredientRatio.x);
            itemToReturn.Count += ingredientRatio.y;
        }
        return itemToReturn;
    }
}