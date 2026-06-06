using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ItemFactory
{
    private static GameObject itemPrefab = Addressables.LoadAssetAsync<GameObject>("ExPrefab_Item").WaitForCompletion();

    public static GameItem CreateItem(int itemId, int count, FlowerGrade grade_F = FlowerGrade.Lv0, GearGrade grade_G = GearGrade.Old)
    {
        if (!GlobalItemDB.IsInitialized)
        {
            Debug.LogWarning("[ItemFactory] GlobalItemDB가 초기화되지 않아 기본 CommonItem을 생성합니다.");
            return new CommonItem(itemId, count);
        }

        if (!GlobalItemDB.HasBase(itemId))
        {
            Debug.LogError($"[ItemFactory] 존재하지 않는 ItemId입니다. Id: {itemId}");
            return new CommonItem(itemId, count);
        }

        ref ItemBaseBlobData baseData = ref GlobalItemDB.GetBaseRef(itemId);

        return baseData.SubType switch
        {
            ItemSubType.Flower => new FlowerItem(itemId, count, grade_F),
            ItemSubType.Seed => new FlowerItem(itemId, count, grade_F),
            ItemSubType.Equipment => new GearItem(itemId, count, grade_G),
            ItemSubType.Fertilizer => new FertilizerItem(itemId, count),
            _ => new CommonItem(itemId, count)
        };
    }

    public static void CreateItemPrefab(GameItem itemData, Vector3 position)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("Item prefab is not loaded.");
            return;
        }

        GameObject itemObject = Object.Instantiate(itemPrefab, position, Quaternion.identity);
        DropItemData dropItemData = itemObject.GetComponent<DropItemData>();

        if (dropItemData != null)
        {
            dropItemData.SetData(itemData);
        }
        else
        {
            Debug.LogError("DropItemData component is missing on the item prefab.");
        }
    }
}
