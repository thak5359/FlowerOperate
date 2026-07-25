// 수정 위치: 정적 아이템 생성 경로도 GameItem 비동기 로드를 대기
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ItemFactory
{
    private static GameObject itemPrefab = Addressables.LoadAssetAsync<GameObject>("ExPrefab_Item").WaitForCompletion();

    public static async UniTask<GameItem> CreateItemAsync(int itemId, int count, FlowerGrade grade_F = FlowerGrade.Lv0, GearGrade grade_G = GearGrade.Old)
    {
        if (!GlobalItemDB.IsInitialized)
        {
            Debug.LogError("[ItemFactory] GlobalItemDB가 초기화되지 않아 아이템을 생성할 수 없습니다.");
            return null;
        }

        if (!GlobalItemDB.HasBase(itemId))
        {
            Debug.LogError($"[ItemFactory] 존재하지 않는 ItemId입니다. Id: {itemId}");
            return null;
        }

        // 수정: async 메서드가 await를 넘겨 ref local을 보존하지 않도록 값을 복사
        ItemBaseBlobData baseData = GlobalItemDB.GetBaseRef(itemId);

        GameItem item = baseData.SubType switch
        {
            ItemSubType.Flower => new FlowerItem(itemId, count, grade_F),
            ItemSubType.Seed => new FlowerItem(itemId, count, grade_F),
            ItemSubType.Equipment => new GearItem(itemId, count, grade_G),
            ItemSubType.Fertilizer => new FertilizerItem(itemId, count),
            _ => new CommonItem(itemId, count)
        };

        await item.OnLoadAsync();
        return item;
    }

    // 수정: 월드 아이템 프리팹에는 완전히 로드된 아이템만 전달
    public static async UniTask CreateItemPrefabAsync(
        int itemId,
        int count,
        Vector3 position,
        FlowerGrade grade_F = FlowerGrade.Lv0,
        GearGrade grade_G = GearGrade.Old)
    {
        GameItem itemData = await CreateItemAsync(itemId, count, grade_F, grade_G);
        if (itemData != null)
            CreateItemPrefab(itemData, position);
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
