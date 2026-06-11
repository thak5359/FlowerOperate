using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ItemGenTest : MonoBehaviour
{
    [Inject] private ItemManager itemManager;
    [Inject] private PlayerOwnItemDataManager _inven;
    public void OnClick()
    {
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201001, 1), new Vector3(1, 1, 20));
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201009, 1), new Vector3(1, 1, 20));
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201017, 1), new Vector3(1, 1, 20));
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201025, 1), new Vector3(1, 1, 20));
        this.gameObject.SetActive(false);
    }
}
