using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class IngotMaker : MakerBaseModel, IMaker
{
    // 아이템 생성 제작 비동기 대기
    public async UniTask<GameItem> ReturnGameItemAsync()
    {
        if (gameItem == null || gameItem.Count < makerData.GetIngredientRatio.x)
        {
            Debug.LogError("반환할 아이템이 없습니다.");
            return null;
        }
        GameItem itemToReturn = await ItemFactory.CreateItemAsync(gameItem.Id + 999, 0);
        int2 ingredientRatio = makerData.GetIngredientRatio;
        while(gameItem.Count >= ingredientRatio.x)
        {
            gameItem.SubCount(ref ingredientRatio.x);
            itemToReturn.Count += ingredientRatio.y;
        }
        return itemToReturn;
    }
}
