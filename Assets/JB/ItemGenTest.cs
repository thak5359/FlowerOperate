using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        _inven.GetData.AddMoney(1000000);
    }
}
