using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ItemFactory
{
    private static GameObject itemPrefab = Addressables.LoadAssetAsync<GameObject>("ExPrefab_Item").WaitForCompletion();

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
