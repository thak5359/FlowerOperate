
using UnityEngine;

public static class ItemFactory
{
    //public static GameItem Create(int itemId, int count)
    //{
    //    if (!GlobalItemDB.TryGetBase(itemId, out var baseData))
    //    {
    //        Debug.LogError($"[ItemFactory] 존재하지 않는 ItemId입니다. Id: {itemId}");
    //        return null;
    //    }

    //    return baseData.SubType switch
    //    {
    //        ItemSubType.Equipment => new GearItem(itemId, count),
    //        ItemSubType.Flower => new FlowerItem(itemId, count),
    //        ItemSubType.Seed => new SeedItem(itemId, count),
    //        _ => new GameItem(itemId, count),
    //    };
    //}
}
