using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakerBaseModel : MonoBehaviour
{
    [SerializeField] protected GameItem gameItem;
    [SerializeField] protected MakerData makerData;

    public virtual void SetGameItem(GameItem item)
    {
        switch(makerData.GetMakerType)
        {
            case MakerType.Seed:
                if(item.SubType == ItemSubType.Flower)
                    gameItem = item;
                break;
            case MakerType.Ingot:
                if(item.SubType == ItemSubType.MetalMaterial)
                    gameItem = item;
                break;
            case MakerType.Jewerly:
                if (item.SubType == ItemSubType.JewelryMaterial)
                    gameItem = item;
                break;
            case MakerType.Wood:
                if(item.SubType == ItemSubType.Wood)
                    gameItem = item;
                break;
            default:
                break;
        }
        return;
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
